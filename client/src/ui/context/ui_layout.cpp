#include "core/defer.hpp"
#include "ui_context.hpp"

// LAYOUT //

void UI_Context::Layout_Calc_Standalone(void) {
    for (UI_Box *box : this->Layout_It())
        for (int i = 0; i < 2; i++)
            switch (box->size[i].type) {
                case UI_SIZE_PIXELS:
                    box->fixed_size[i] = box->size[i].value;
                    break;
                case UI_SIZE_TEXT_CONTENT:
                    int text_size[2];
                    TTF_GetTextSize(box->label, &text_size[0], &text_size[1]);
                    box->fixed_size[i] =
                        box->size[i].value + (float)text_size[i] + box->size[i].value;
                    break;
                default:
                    break;
            }
}
void UI_Context::Layout_Calc_Upwards_Dependent(void) {
    // % of parent
    for (UI_Box *box : this->Layout_It())
        for (int i = 0; i < 2; i++) {
            if (box->size[i].type != UI_SIZE_PERCENT_OF_PARENT)
                continue;

            for (UI_Box *p = box->parent; p; p = p->parent) {
                if (p->fixed_size[i] > 0.0f) {
                    box->fixed_size[i] = p->fixed_size[i] * box->size[i].value;
                    break;
                }
            }
        }

    // fit fb
    for (UI_Box *box : this->Layout_It())
        for (int i = 0; i < 2; i++) {
            if (box->size[i].type != UI_SIZE_FIT)
                continue;

            for (UI_Box *p = box->parent; p; p = p->parent) {
                if (p->fixed_size[i] <= 0.0f)
                    continue;

                if (p->child_layout_axis != i)
                    box->fixed_size[i] = p->fixed_size[i];

                break;
            }
        }

    // siblings get space first
    for (UI_Box *parent : this->Layout_It())
        for (int i = 0; i < 2; i++) {
            if (parent->child_layout_axis != i)
                continue;
            if (parent->fixed_size[i] <= 0.0f)
                continue;

            float used_by_non_fit = 0.0f;
            int fit_count = 0;

            for (UI_Box *child = parent->first_child; child; child = child->next_sibling) {
                if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                    continue;

                if (child->size[i].type == UI_SIZE_FIT)
                    fit_count += 1;
                else
                    used_by_non_fit += child->fixed_size[i];
            }

            if (fit_count <= 0)
                continue;

            const float remaining = SDL_max(0.0f, parent->fixed_size[i] - used_by_non_fit);
            const float each_fit = remaining / (float)fit_count;

            for (UI_Box *child = parent->first_child; child; child = child->next_sibling) {
                if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                    continue;
                if (child->size[i].type == UI_SIZE_FIT)
                    child->fixed_size[i] = each_fit;
            }
        }
}

void UI_Context::Layout_Calc_Downwards_Dependant(void) {
    for (int i = 0; i < 2; i++)
        for (UI_Box *leaf : this->leafs) {
            UI_Box *last_box = NULL;
            for (UI_Box *box = leaf; box; box = box->parent) {
                defer(last_box = box);
                switch (box->size[i].type) {
                    case UI_SIZE_CHILD_SUM: {
                        // Find the last downward dependant child, if it isnt the last node
                        // whose branch we traveled up then skip traversing that from this
                        // leaf and get the next so that we first solve all downward
                        // dependancies

                        UI_Box *last_downward_dependant_child = NULL;
                        for (UI_Box *child = box->last_child; child; child = child->prev_sibling) {
                            if (child->size[i].type & UI_SIZE_DOWNWARD_DEPENDENT)
                                last_downward_dependant_child = child;
                            if (child == last_box)
                                break;
                        }
                        if (last_downward_dependant_child &&
                            last_downward_dependant_child != last_box)
                            goto continue_leaf_loop;

                        float sum = 0;
                        for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                            if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                                continue;

                            if (i == child->child_layout_axis)
                                sum += child->fixed_size[i];
                            else
                                sum = SDL_max(sum, child->fixed_size[i]);
                        }
                        box->fixed_size[i] = sum;

                        break;
                    }
                    default:
                        break;
                }
            }
        continue_leaf_loop:;
        }
}

// NOTE: To be completely honest, I dont remember how this works
void UI_Context::Layout_Solve_Violation(void) {
    for (UI_Box *box : this->Layout_It())
        for (int i = 0; i < 2; i++) {
            // - fixup children sizes on non child layout axis
            if (i != box->child_layout_axis) {
                for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                    if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                        continue;
                    float violation = child->fixed_size[i] - box->fixed_size[i];
                    float fixup = SDL_clamp(violation, 0, child->fixed_size[i]);
                    if (fixup > 0)
                        child->fixed_size[i] -= fixup;
                }
            } else  // - fixup children sizes on child layout axis
            {
                // float total_allowed_size = box->fixed_size[i];
                float total_size = 0;
                float total_weighted_size = 0;
                for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                    if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                        continue;

                    total_size += child->fixed_size[i];
                    total_weighted_size += child->fixed_size[i] * (1 - child->size[i].strictness);
                }

                float violation = total_size - box->fixed_size[i];

                if (violation > 0 && total_weighted_size > 0) {
                    static thread_local std::vector<float> child_fixup;
                    child_fixup.assign(box->child_count, 0.0f);
                    int idx = 0;

                    for (UI_Box *child = box->first_child; child;
                         child = child->next_sibling, idx++) {
                        if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                            continue;

                        child_fixup[idx] = SDL_max(0.0f, child->fixed_size[i]);
                    }

                    idx = 0;
                    const float fixup_pct = violation / total_weighted_size;
                    for (UI_Box *child = box->first_child; child;
                         child = child->next_sibling, idx++) {
                        if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                            continue;

                        child->fixed_size[i] -= child_fixup[idx] * fixup_pct;
                    }
                }
            }

            // XXX: if allowOverFlow flag is added then fixup upwards relitve sizes

            for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                child->fixed_size[i] = SDL_max(child->fixed_size[i], child->min_size[i]);
            }
        }
}

static void Apply_Margin(Rect *r, UI_Margin margin) {
    r->x += margin.left;
    r->w -= margin.left;

    r->y += margin.top;
    r->h -= margin.top;

    r->w -= margin.right;
    r->h -= margin.bottom;
}

void UI_Context::Layout_Comp_Relative(void) {
    for (UI_Box *box : this->Layout_It())
        for (int i = 0; i < 2; i++) {
            float layout_position = 0;
            float bounds = 0;
            for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                float original_position = child->layout_box[i];

                if (!(child->flags & (UI_BOX_FLAG_FLOATING_X << i))) {
                    child->fixed_position[i] += layout_position;
                    if (i == box->child_layout_axis) {
                        layout_position += child->fixed_size[i];
                        bounds += child->fixed_size[i];
                    } else
                        bounds = SDL_max(bounds, child->fixed_size[i]);
                }

                // XXX: handle animation stuff in an alternate case
                // XXX: add view offset to below
                child->layout_box[i] =
                    box->layout_box[i] + box->offset[i] + child->fixed_position[i];
                if (!(child->flags & (UI_BOX_FLAG_FLOATING_X << i)))
                    child->layout_box[i] -= box->view_offset[i];

                child->layout_box[2 + i] = child->fixed_size[i];

                child->position_delta[i] = original_position - child->layout_box[i];
            }

            // Element alignment
            // NOTE: Second pass so that bounds is up to date but i could skip the
            // second pass and use last frames bounds
            for (UI_Box *child = box->first_child; child; child = child->next_sibling) {
                if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                    continue;

                float original_position = child->layout_box[i] + child->position_delta[i];
                float size = (i == box->child_layout_axis) ? bounds : child->layout_box[2 + i];
                switch (box->elem_align[i]) {
                    case UI_ALIGN_LEFT:
                        // Do nothing
                        break;
                    case UI_ALIGN_RIGHT:
                        child->fixed_position[i] += box->layout_box[2 + i] - size;
                        break;
                    case UI_ALIGN_CENTER:
                        child->fixed_position[i] += (box->layout_box[2 + i] - size) / 2;
                        break;
                }

                child->layout_box[i] = box->layout_box[i] + box->offset[i] +
                                       child->fixed_position[i] + box->view_offset[i];
                child->position_delta[i] = original_position - child->layout_box[i];
            }

            box->view_bounds[i] = bounds;

            // apply margin / offset
            box->area = box->layout_box;

            Apply_Margin(&box->area, box->margin);

            box->area.x += box->offset.x;
            box->area.y += box->offset.y;

            // cache nearest clipping ancestor
            if (box == &this->root || !box->parent) {
                box->has_clip_ancestor = false;
            } else if (box->parent->flags & UI_BOX_FLAG_CLIP) {
                if (box->parent->has_clip_ancestor)
                    box->clip_ancestor_rect =
                        box->parent->area.Intersection(box->parent->clip_ancestor_rect);
                else
                    box->clip_ancestor_rect = box->parent->area;

                box->has_clip_ancestor = true;
            } else {
                box->has_clip_ancestor = box->parent->has_clip_ancestor;
                if (box->has_clip_ancestor)
                    box->clip_ancestor_rect = box->parent->clip_ancestor_rect;
            }
        }
}

void UI_Context::Layout_Compute(void) {
    // Calculate "standalone" sizes:
    //     sizes that dont depend on any other widget
    this->Layout_Calc_Standalone();

    // Calculate "Upwards-dependant" sizes
    //     sizes that depend only on widgets above
    this->Layout_Calc_Upwards_Dependent();

    // Calculate "downwards-dependent" sizes
    //     sizes that depend only on widgets below
    this->Layout_Calc_Downwards_Dependant();

    // Solve violations
    //     ensure that content is not extening past parents boundries, use
    //     strictness
    this->Layout_Solve_Violation();

    // compute the relative positions
    this->Layout_Comp_Relative();
}
