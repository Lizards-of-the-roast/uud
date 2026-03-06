#include "page.hpp"

#include <algorithm>
#include <cstring>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "net/card_catalog.hpp"
#include "scenes/common/ui_theme.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

static bool catalog_loaded = false;
static bool page_active = false;
static Deck current_deck;
static std::string deck_status;
static std::string deck_error;
static int selected_deck_index = -1;
static std::vector<std::string> saved_deck_names;

static void Refresh_Saved_Decks() {
    saved_deck_names = Deck_Store::List_Decks();
}

static void Add_Card_To_Deck(const std::string &name) {
    const Card_Entry *card = card_catalog.Find(name);
    if (!card)
        return;

    for (auto &entry : current_deck.cards) {
        if (entry.card_name == name) {
            if (!card->is_basic_land && entry.count >= 4) {
                deck_error = "Max 4 copies of " + name;
                return;
            }
            entry.count++;
            deck_error.clear();
            return;
        }
    }
    current_deck.cards.push_back({name, 1});
    deck_error.clear();
}

static void Remove_Card_From_Deck(const std::string &name) {
    for (auto it = current_deck.cards.begin(); it != current_deck.cards.end(); ++it) {
        if (it->card_name == name) {
            it->count--;
            if (it->count <= 0)
                current_deck.cards.erase(it);
            return;
        }
    }
}

static void Styled_Message(Widget_Context &w, const std::string &text, SDL_Color color) {
    auto style = theme::Label_Body();
    for (auto &s : style)
        s.text.color = color;
    w.styles.push(style);
    w.Label(text);
    w.styles.pop();
}

bool Menu_Deck_Builder_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    if (!catalog_loaded) {
        card_catalog.Load_Default_Cards();
        Refresh_Saved_Decks();
        if (current_deck.name.empty())
            current_deck.name = "New Deck";
        catalog_loaded = true;
    }

    if (!page_active) {
        deck_status.clear();
        deck_error.clear();
        Refresh_Saved_Decks();
        page_active = true;
    }

    TTF_Font *font = state.font[paths::beleren_bold];

    DIV(&w) {
        UI_Box *root = ui.leafs.back();
        root->child_layout_axis = 1;
        root->elem_align = UI_ALIGN_CENTER;
        root->flags |= UI_BOX_FLAG_CLIP;
        theme::Apply_Panel(root, theme::Panel());

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        const float m = 4.0f;
        ui.margins.push({.left = m, .right = m, .top = m / 2.0f, .bottom = m / 2.0f});
        defer(ui.margins.pop());

        ui.sizes.push({UI_Size_Parent(0.95), UI_Size_Text(6)});
        defer(ui.sizes.pop());

        w.styles.push(theme::Label_Title());
        w.Label("Deck Builder").box->Text_Copy_Font(font, {.size=30});
        w.styles.pop();

        ui.sizes.push({UI_Size_Parent(0.95), UI_Size_Child()});
        DIV(&w) {
            UI_Box *name_row = ui.leafs.back();
            name_row->child_layout_axis = 0;

            //name_row->min_size.y = (float)(TTF_GetFontHeight(font)) + 6*2;
            ui.sizes.push({UI_Size_Fit(), UI_Size_Text(6)});
            defer (ui.sizes.pop());

            w.styles.push(theme::Label_Body());
            w.Label("Deck:");
            w.styles.pop();

            w.styles.push(theme::Textbox());
            UI_Signal name_sig = w.Textbox(current_deck.name);
            w.styles.pop();
            if (name_sig.box->label && name_sig.box->label->text)
                current_deck.name = name_sig.box->label->text;

            w.styles.push(theme::Label_Body());
            w.Label(std::to_string(current_deck.Total_Cards()) + "/60");
            w.styles.pop();
        }
        ui.sizes.pop();

        if (!deck_error.empty())
            Styled_Message(w, deck_error, theme::TEXT_ERROR);
        if (!deck_status.empty())
            Styled_Message(w, deck_status, theme::TEXT_SUCCESS);

        ui.sizes.push({UI_Size_Parent(0.95), UI_Size_Parent(0.5f)});
        DIV(&w) {
            UI_Box *columns = ui.leafs.back();
            columns->child_layout_axis = 0;

            ui.sizes.push({UI_Size_Parent(0.55), UI_Size_Parent(1.0)});
            DIV(&w) {
                UI_Box *catalog_panel = ui.leafs.back();
                catalog_panel->child_layout_axis = 1;
                catalog_panel->flags |= UI_BOX_FLAG_CLIP;
                theme::Apply_Panel(catalog_panel, theme::Panel_Inner());

                ui.sizes.push({UI_Size_Parent(0.95), UI_Size_Text(4)});
                defer(ui.sizes.pop());

                w.styles.push(theme::Label_Title());
                w.Label("Card Catalog");
                w.styles.pop();

                w.styles.push(theme::Textbox());
                UI_Signal search_sig = w.Textbox("", {}, std::string("card_search"));
                w.styles.pop();

                const char *search_text = (search_sig.box->label && search_sig.box->label->text)
                                              ? search_sig.box->label->text
                                              : "";

                auto results = card_catalog.Search(search_text);

                int max_cards = 999;//(int)(panel_height / 22.0f) - 2;
                if (max_cards < 3)
                    max_cards = 3;

                int count = 0;
                for (const auto *card : results) {
                    if (count >= max_cards)
                        break;

                    std::string display = card->name;
                    if (!card->mana_cost.empty())
                        display += " " + card->mana_cost;

                    UI_Signal card_btn = w.Button(display, {}, std::string("cat_" + card->name));
                    if (card_btn.flags & UI_SIG_LEFT_RELEASED)
                        Add_Card_To_Deck(card->name);

                    count++;
                }
            }
            ui.sizes.pop();

            ui.sizes.push({UI_Size_Parent(0.45), UI_Size_Parent(1.0)});
            DIV(&w) {
                UI_Box *deck_panel = ui.leafs.back();
                deck_panel->child_layout_axis = 1;
                deck_panel->flags |= UI_BOX_FLAG_CLIP;
                theme::Apply_Panel(deck_panel, theme::Panel_Inner());

                ui.sizes.push({UI_Size_Parent(0.95), UI_Size_Text(4)});
                defer(ui.sizes.pop());

                w.styles.push(theme::Label_Title());
                w.Label("Current Deck");
                w.styles.pop();

                w.styles.push(theme::Button_Secondary());
                for (const auto &entry : current_deck.cards) {
                    std::string display = std::to_string(entry.count) + "x " + entry.card_name;
                    UI_Signal entry_btn =
                        w.Button(display, {}, std::string("deck_" + entry.card_name));
                    if (entry_btn.flags & UI_SIG_LEFT_RELEASED)
                        Remove_Card_From_Deck(entry.card_name);
                }
                w.styles.pop();
            }
            ui.sizes.pop();
        }
        ui.sizes.pop();


        ui.sizes.push({UI_Size_Fit(), UI_Size_Child()});
        DIV(&w) {
            UI_Box *action_bar = ui.leafs.back();
            action_bar->child_layout_axis = 1;
            /*
            TODO: add a ceck for downward dependant + align_center
                  in the ui code somewhere
            */
            //action_bar->elem_align = UI_ALIGN_CENTER;

            ui.sizes.push({UI_Size_Fit(), UI_Size_Text(8)});
            defer(ui.sizes.pop());

            const float bm = 8.0f;
            ui.margins.push({.left = bm, .right = bm, .top = 0, .bottom = 0});
            defer(ui.margins.pop());

            if (w.Button("Save").flags & UI_SIG_LEFT_RELEASED) {
                if (current_deck.name.empty() || current_deck.name == "New Deck") {
                    deck_error = "Enter a deck name first";
                } else if (Deck_Store::Save(current_deck)) {
                    deck_status = "Deck saved!";
                    deck_error.clear();
                    Refresh_Saved_Decks();
                } else {
                    deck_error = "Failed to save deck";
                    deck_status.clear();
                }
            }

            if (!saved_deck_names.empty()) {
                w.styles.push(theme::Button_Secondary());
                if (w.Button("Load").flags & UI_SIG_LEFT_RELEASED) {
                    selected_deck_index = (selected_deck_index + 1) % (int)saved_deck_names.size();
                    auto loaded = Deck_Store::Load(saved_deck_names[selected_deck_index]);
                    if (loaded.has_value()) {
                        current_deck = *loaded;
                        deck_status = "Loaded: " + current_deck.name;
                        deck_error.clear();
                    }
                }
                w.styles.pop();
            }

            w.styles.push(theme::Button_Danger());
            if (w.Button("Clear").flags & UI_SIG_LEFT_RELEASED) {
                current_deck.cards.clear();
                deck_status.clear();
                deck_error.clear();
            }
            w.styles.pop();

            w.styles.push(theme::Button_Secondary());
            if (w.Button("Back").flags & UI_SIG_LEFT_RELEASED) {
                deck_status.clear();
                deck_error.clear();
                page_active = false;
                tab = Menu_Tab::None;
            }
            w.styles.pop();
        }
        ui.sizes.pop();
    }
    return false;
}
