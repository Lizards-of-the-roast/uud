#include "page.hpp"

#include <string>
#include <vector>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "net/game_client.hpp"
#include "net/matchmaking_client.hpp"
#include "scenes/common/ui_theme.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

static void Styled_Message(Widget_Context &w, const std::string &text, SDL_Color color) {
    auto style = theme::Label_Body();
    for (auto &s : style)
        s.text.color = color;
    w.styles.push(style);
    w.Label(text);
    w.styles.pop();
}

bool Menu_Matchmaking_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        static std::string status_text;
        static std::string error_text;
        static std::string queue_info_text;
        static bool was_active = false;
        static std::vector<Game::Preset_Deck_Info> preset_decks;
        static bool decks_fetched = false;
        static int selected_deck_idx = 0;
        static int player_elo = 0;

        if (!was_active) {
            if (state.offline || !matchmaking_client.In_Queue()) {
                status_text.clear();
                error_text.clear();
                queue_info_text.clear();
            }
            if (!state.offline) {
                if (!decks_fetched)
                    game_client.List_Preset_Decks();
                matchmaking_client.Get_Queue_Info();
            }
            was_active = true;
        }

        if (!state.offline) {
            if (auto result = game_client.Poll_Preset_Decks()) {
                if (result->success) {
                    preset_decks = result->decks;
                    decks_fetched = true;
                    selected_deck_idx = 0;
                } else {
                    error_text = result->error.empty() ? "Failed to fetch decks" : result->error;
                }
            }

            if (auto qi = matchmaking_client.Poll_Queue_Info()) {
                if (qi->success)
                    queue_info_text = "Players in queue: " + std::to_string(qi->info.queued_players);
                else
                    queue_info_text.clear();
            }

            if (auto join = matchmaking_client.Poll_Join()) {
                if (join->success) {
                    status_text = "In queue...";
                    error_text.clear();
                } else {
                    error_text = join->error.empty() ? "Failed to join queue" : join->error;
                    status_text.clear();
                }
            }

            if (auto update = matchmaking_client.Poll_Update()) {
                if (update->error.has_value()) {
                    error_text = *update->error;
                    status_text.clear();
                } else if (update->matched) {
                    state.current_game_id = update->game_id;
                    state.joined_via_matchmaking = true;
                    if (selected_deck_idx >= 0 && selected_deck_idx < static_cast<int>(preset_decks.size()))
                        state.selected_deck_name = preset_decks[selected_deck_idx].name;
                    state.scene = Scene::Match;
                    status_text.clear();
                    was_active = false;
                    return true;
                } else {
                    status_text = "Position: " + std::to_string(update->queue_position) +
                                  " | Wait: ~" + std::to_string(update->estimated_wait_seconds) +
                                  "s";
                    if (update->elo > 0)
                        player_elo = update->elo;
                }
            }
        }

        w.styles.push(theme::Label_Title());
        w.Label("Matchmaking");
        w.styles.pop();

        if (!queue_info_text.empty())
            Styled_Message(w, queue_info_text, theme::TEXT_INFO);
        if (player_elo > 0)
            Styled_Message(w, "Your ELO: " + std::to_string(player_elo), theme::TEXT_INFO);
        if (!error_text.empty())
            Styled_Message(w, error_text, theme::TEXT_ERROR);
        if (!status_text.empty())
            Styled_Message(w, status_text, theme::TEXT_INFO);

        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        if (w.Button("Play Local").flags & UI_SIG_LEFT_RELEASED) {
            state.current_game_id = "local";
            state.selected_deck_name.clear();
            state.scene = Scene::Match;
            status_text.clear();
            error_text.clear();
            was_active = false;
            return true;
        }

        if (!state.offline) {
            w.Spacer(UI_Size_Pixels(8));

            if (!matchmaking_client.In_Queue()) {
                if (preset_decks.empty()) {
                    w.styles.push(theme::Label_Body());
                    w.Label(decks_fetched ? "No preset decks available" : "Loading decks...");
                    w.styles.pop();
                } else {
                    w.styles.push(theme::Label_Body());
                    w.Label("Deck: " + preset_decks[selected_deck_idx].name +
                            " (" + std::to_string(preset_decks[selected_deck_idx].card_count) + " cards)");
                    w.styles.pop();

                    if (preset_decks.size() > 1) {
                        w.styles.push(theme::Button_Secondary());
                        if (w.Button("<< Prev Deck").flags & UI_SIG_LEFT_RELEASED)
                            selected_deck_idx = (selected_deck_idx - 1 + static_cast<int>(preset_decks.size())) % static_cast<int>(preset_decks.size());
                        if (w.Button("Next Deck >>").flags & UI_SIG_LEFT_RELEASED)
                            selected_deck_idx = (selected_deck_idx + 1) % static_cast<int>(preset_decks.size());
                        w.styles.pop();
                    }
                }

                w.Spacer(UI_Size_Pixels(8));
            }

            if (matchmaking_client.In_Queue()) {
                w.styles.push(theme::Button_Secondary());
                if (w.Button("Cancel Queue").flags & UI_SIG_LEFT_RELEASED) {
                    matchmaking_client.Leave_Queue();
                    status_text.clear();
                    error_text.clear();
                }
                w.styles.pop();
            } else {
                bool has_deck = !preset_decks.empty();
                if (has_deck) {
                    if (w.Button("Find Match").flags & UI_SIG_LEFT_RELEASED) {
                        error_text.clear();
                        status_text = "Joining queue...";
                        matchmaking_client.Join_Queue(preset_decks[selected_deck_idx].name);
                    }
                } else {
                    w.styles.push(theme::Button_Secondary());
                    w.Label("Select a deck to queue");
                    w.styles.pop();
                }
            }
        }

        w.styles.push(theme::Button_Secondary());
        if (w.Button("Back").flags & UI_SIG_LEFT_RELEASED) {
            if (matchmaking_client.In_Queue())
                matchmaking_client.Leave_Queue();
            status_text.clear();
            error_text.clear();
            queue_info_text.clear();
            player_elo = 0;
            was_active = false;
            decks_fetched = false;
            preset_decks.clear();
            tab = Menu_Tab::None;
        }
        w.styles.pop();
    }
    return false;
}
