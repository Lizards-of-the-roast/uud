#include "net_client.hpp"

#include <iostream>

Net_Client net;

void Net_Client::Connect(const std::string &address) {
    std::lock_guard lock(mutex_);
    address_ = address;
    token_.clear();

    // should probably make this a secure tunnel but it doesnt really matter for this project
    channel_ = grpc::CreateChannel(address, grpc::InsecureChannelCredentials());
    auth_stub_ = mtg::proto::AuthService::NewStub(channel_);
    game_stub_ = mtg::proto::GameService::NewStub(channel_);
    matchmaking_stub_ = mtg::proto::MatchmakingService::NewStub(channel_);

    std::cout << "net: connected to " << address << '\n';
}

void Net_Client::Disconnect() {
    std::lock_guard lock(mutex_);
    auth_stub_.reset();
    game_stub_.reset();
    matchmaking_stub_.reset();
    channel_.reset();
    token_.clear();
    address_.clear();
}

bool Net_Client::IsConnected() {
    std::lock_guard lock(mutex_);
    if (!channel_)
        return false;
    auto state = channel_->GetState(false);
    return state == GRPC_CHANNEL_READY || state == GRPC_CHANNEL_IDLE ||
           state == GRPC_CHANNEL_CONNECTING;
}

void Net_Client::Set_Token(const std::string &token) {
    std::lock_guard lock(mutex_);
    token_ = token;
}

std::string Net_Client::Get_Token() {
    std::lock_guard lock(mutex_);
    return token_;
}

void Net_Client::Attach_Auth(grpc::ClientContext &ctx) {
    std::lock_guard lock(mutex_);
    if (!token_.empty())
        ctx.AddMetadata("authorization", "Bearer " + token_);
}

std::shared_ptr<mtg::proto::AuthService::Stub> Net_Client::Auth() {
    std::lock_guard lock(mutex_);
    return auth_stub_;
}

std::shared_ptr<mtg::proto::GameService::Stub> Net_Client::Game() {
    std::lock_guard lock(mutex_);
    return game_stub_;
}

std::shared_ptr<mtg::proto::MatchmakingService::Stub> Net_Client::Matchmaking() {
    std::lock_guard lock(mutex_);
    return matchmaking_stub_;
}
