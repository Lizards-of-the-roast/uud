#pragma once

#include <memory>
#include <mutex>
#include <string>

#include "mtg/auth_service.grpc.pb.h"
#include "mtg/game_service.grpc.pb.h"
#include "mtg/matchmaking_service.grpc.pb.h"
#include <grpcpp/grpcpp.h>

struct Net_Client {
    void Connect(const std::string &address);
    void Disconnect();
    bool IsConnected();

    void Set_Token(const std::string &token);
    std::string Get_Token();

    void Attach_Auth(grpc::ClientContext &ctx);

    std::shared_ptr<mtg::proto::AuthService::Stub> Auth();
    std::shared_ptr<mtg::proto::GameService::Stub> Game();
    std::shared_ptr<mtg::proto::MatchmakingService::Stub> Matchmaking();

private:
    std::mutex mutex_;
    std::shared_ptr<grpc::Channel> channel_;
    std::shared_ptr<mtg::proto::AuthService::Stub> auth_stub_;
    std::shared_ptr<mtg::proto::GameService::Stub> game_stub_;
    std::shared_ptr<mtg::proto::MatchmakingService::Stub> matchmaking_stub_;
    std::string token_;
    std::string address_;
};

extern Net_Client net;
