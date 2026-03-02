#include "auth_client.hpp"

#include <thread>

#include "net_client.hpp"
#include "token_store.hpp"

Auth_Client auth_client;

void Auth_Client::Login(const std::string &server, const std::string &username,
                        const std::string &password) {
    if (in_flight_)
        return;
    in_flight_ = true;

    std::thread([this, server, username, password]() {
        net.Connect(server);

        auto stub = net.Auth();
        Auth_Login_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Failed to connect to server";
            results_.Push(result);
            in_flight_ = false;
            return;
        }

        grpc::ClientContext ctx;
        mtg::proto::LoginRequest req;
        req.set_username(username);
        req.set_password(password);

        mtg::proto::LoginResponse resp;
        grpc::Status status = stub->Login(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            result.access_token = resp.access_token();
            result.refresh_token = resp.refresh_token();
            result.user_id = resp.user_id();
            result.username = username;

            net.Set_Token(resp.access_token());
            Token_Store::Save(resp.access_token(), resp.refresh_token(), server);
        } else {
            result.success = false;
            result.error = status.error_message();
            net.Disconnect();
        }

        results_.Push(result);
        in_flight_ = false;
    }).detach();
}

void Auth_Client::Register(const std::string &server, const std::string &username,
                           const std::string &email, const std::string &password) {
    if (in_flight_)
        return;
    in_flight_ = true;

    std::thread([this, server, username, email, password]() {
        net.Connect(server);

        auto stub = net.Auth();
        Auth_Register_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Failed to connect to server";
            results_.Push(result);
            in_flight_ = false;
            return;
        }

        grpc::ClientContext ctx;
        mtg::proto::RegisterRequest req;
        req.set_username(username);
        req.set_email(email);
        req.set_password(password);

        mtg::proto::RegisterResponse resp;
        grpc::Status status = stub->Register(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            result.user_id = resp.user_id();
        } else {
            result.success = false;
            result.error = status.error_message();
        }

        results_.Push(result);
        in_flight_ = false;
    }).detach();
}

void Auth_Client::Validate(const std::string &server, const std::string &token) {
    if (in_flight_)
        return;
    in_flight_ = true;

    std::thread([this, server, token]() {
        net.Connect(server);
        net.Set_Token(token);

        auto stub = net.Auth();
        Auth_Validate_Result result;
        if (!stub) {
            result.valid = false;
            results_.Push(result);
            in_flight_ = false;
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::ValidateTokenRequest req;
        req.set_token(token);

        mtg::proto::ValidateTokenResponse resp;
        grpc::Status status = stub->ValidateToken(&ctx, req, &resp);

        if (status.ok() && resp.valid()) {
            result.valid = true;
            result.user_id = resp.user_id();
            result.username = resp.username();
        } else {
            result.valid = false;
            net.Set_Token("");
            net.Disconnect();
            Token_Store::Clear();
        }

        results_.Push(result);
        in_flight_ = false;
    }).detach();
}

std::optional<Auth_Result> Auth_Client::Poll() {
    return results_.Poll();
}

bool Auth_Client::In_Flight() {
    return in_flight_;
}
