#include "matchmaking_client.hpp"

#include <iostream>
#include <thread>

#include "net_client.hpp"

Matchmaking_Client matchmaking_client;

void Matchmaking_Client::Join_Queue(const std::string &format, const std::string &deck_id) {
    if (in_queue_)
        return;
    in_queue_ = true;

    std::thread([this, format, deck_id]() {
        auto stub = net.Matchmaking();
        if (!stub) {
            Matchmaking_Join_Result result;
            result.success = false;
            result.error = "Not connected to server";
            join_results_.Push(result);
            in_queue_ = false;
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::JoinQueueRequest req;
        req.set_format(format);
        req.set_deck_id(deck_id);

        mtg::proto::JoinQueueResponse resp;
        grpc::Status status = stub->JoinQueue(&ctx, req, &resp);

        Matchmaking_Join_Result result;
        if (status.ok()) {
            result.success = true;
            result.queue_ticket = resp.queue_ticket();
            {
                std::lock_guard lock(ticket_mutex_);
                queue_ticket_ = resp.queue_ticket();
            }
            Start_Status_Stream();
        } else {
            result.success = false;
            result.error = status.error_message();
            in_queue_ = false;
        }

        join_results_.Push(result);
    }).detach();
}

void Matchmaking_Client::Start_Status_Stream() {
    stream_active_ = true;

    std::string ticket;
    {
        std::lock_guard lock(ticket_mutex_);
        ticket = queue_ticket_;
    }

    std::thread([this, ticket]() {
        auto stub = net.Matchmaking();
        if (!stub) {
            Matchmaking_Update update;
            update.error = true;
            update.error_message = "Not connected to server";
            update.matched = false;
            updates_.Push(update);
            in_queue_ = false;
            stream_active_ = false;
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::QueueStatusRequest req;
        req.set_queue_ticket(ticket);

        auto reader = stub->QueueStatus(&ctx, req);

        mtg::proto::QueueStatusResponse resp;
        while (stream_active_ && reader->Read(&resp)) {
            Matchmaking_Update update;
            update.matched = resp.matched();
            update.game_id = resp.game_id();
            update.queue_position = resp.queue_position();
            update.estimated_wait_seconds = resp.estimated_wait_seconds();
            update.error = false;
            updates_.Push(update);

            if (resp.matched()) {
                in_queue_ = false;
                stream_active_ = false;
                break;
            }
        }

        grpc::Status status = reader->Finish();
        if (!status.ok() && stream_active_) {
            Matchmaking_Update update;
            update.error = true;
            update.error_message = status.error_message();
            update.matched = false;
            updates_.Push(update);
        }
        in_queue_ = false;
        stream_active_ = false;
    }).detach();
}

void Matchmaking_Client::Leave_Queue() {
    Stop_Stream();

    std::string ticket;
    {
        std::lock_guard lock(ticket_mutex_);
        ticket = queue_ticket_;
    }

    if (ticket.empty())
        return;

    std::thread([this, ticket]() {
        auto stub = net.Matchmaking();
        if (!stub)
            return;

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::LeaveQueueRequest req;
        req.set_queue_ticket(ticket);

        mtg::proto::LeaveQueueResponse resp;
        stub->LeaveQueue(&ctx, req, &resp);

        {
            std::lock_guard lock(ticket_mutex_);
            queue_ticket_.clear();
        }
        in_queue_ = false;
    }).detach();
}

void Matchmaking_Client::Stop_Stream() {
    stream_active_ = false;
}

std::optional<Matchmaking_Join_Result> Matchmaking_Client::Poll_Join() {
    return join_results_.Poll();
}

std::optional<Matchmaking_Update> Matchmaking_Client::Poll_Update() {
    return updates_.Poll();
}

bool Matchmaking_Client::In_Queue() {
    return in_queue_;
}
