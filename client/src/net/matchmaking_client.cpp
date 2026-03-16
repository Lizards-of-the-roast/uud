#include "matchmaking_client.hpp"

#include <iostream>
#include <thread>

#include "convert/proto_convert.hpp"
#include "mtg/matchmaking_service.grpc.pb.h"
#include "net_client.hpp"

using namespace Game;

Matchmaking_Client matchmaking_client;

void Matchmaking_Client::Join_Queue(const std::string &deck_id) {
    if (in_queue_.exchange(true))
        return;

    std::thread([this, deck_id]() {
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
            Queue_Status update;
            update.error = "Not connected to server";
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
            Queue_Status update = convert::From_Proto(resp);
            updates_.Push(update);

            if (resp.matched()) {
                in_queue_ = false;
                stream_active_ = false;
                break;
            }
        }

        grpc::Status status = reader->Finish();
        if (!status.ok() && stream_active_) {
            Queue_Status update;
            update.error = status.error_message();
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

void Matchmaking_Client::Get_Queue_Info() {
    std::thread([this]() {
        auto stub = net.Matchmaking();
        Queue_Info_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            queue_info_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::GetQueueInfoRequest req;

        mtg::proto::GetQueueInfoResponse resp;
        grpc::Status status = stub->GetQueueInfo(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            result.info.total_queued = resp.total_queued();
            result.info.queued_players = resp.queued_players();
        } else {
            result.success = false;
            result.error = status.error_message();
        }
        queue_info_results_.Push(result);
    }).detach();
}

void Matchmaking_Client::Stop_Stream() {
    stream_active_ = false;
}

std::optional<Matchmaking_Join_Result> Matchmaking_Client::Poll_Join() {
    return join_results_.Poll();
}

std::optional<Queue_Status> Matchmaking_Client::Poll_Update() {
    return updates_.Poll();
}

std::optional<Queue_Info_Result> Matchmaking_Client::Poll_Queue_Info() {
    return queue_info_results_.Poll();
}

bool Matchmaking_Client::In_Queue() {
    return in_queue_;
}
