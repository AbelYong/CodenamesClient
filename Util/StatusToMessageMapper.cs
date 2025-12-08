using CodenamesClient.Properties.Langs;

namespace CodenamesClient.Util
{
    public static class StatusToMessageMapper
    {
        public static string GetAuthServiceMessage(AuthOperationType operationType, CodenamesGame.AuthenticationService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.AuthenticationService.StatusCode.WRONG_DATA:
                    return Lang.profileNewPasswordNotValid;
                case CodenamesGame.AuthenticationService.StatusCode.NOT_FOUND:
                    return Lang.emailPasswordUpdateFailedAddressNotFound;
                case CodenamesGame.AuthenticationService.StatusCode.UNAUTHORIZED:
                    return GetUnauthorizedAuthMessage(operationType);
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.AuthenticationService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                case CodenamesGame.AuthenticationService.StatusCode.MISSING_DATA:
                    return Lang.signInErrorMissingData;
                default:
                    return Lang.globalUnknownServerError;
            }
        }

        private static string GetUnauthorizedAuthMessage(AuthOperationType operationType)
        {
            switch (operationType)
            {
                case AuthOperationType.AUTHENTICATION:
                    return Lang.profilePasswordErrorTypedPasswordIncorrect;
                case AuthOperationType.PASS_RESET:
                    return Lang.emailVerificationFailedAttemptsRemainingX;
                case AuthOperationType.PASS_UPDATE:
                    return Lang.profilePasswordErrorTypedPasswordIncorrect;
            }
            return Lang.globalUnknownServerError;
        }

        public static string GetUserServiceMessage(CodenamesGame.UserService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.UserService.StatusCode.UPDATED:
                    return Lang.profileUpdateSucessful;
                case CodenamesGame.UserService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.UserService.StatusCode.WRONG_DATA:
                    return Lang.profileCouldNotUpdateCheckData;
                case CodenamesGame.UserService.StatusCode.NOT_FOUND:
                    return Lang.globalErrorProfileNotFound;
                case CodenamesGame.UserService.StatusCode.UNALLOWED:
                    return Lang.profileErrorEmailOrUsernameInUse;
                case CodenamesGame.UserService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.UserService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.UserService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.UserService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }

        public static string GetEmailServiceMessage(CodenamesGame.EmailService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.EmailService.StatusCode.SERVER_ERROR:
                    return Lang.emailFailedToSendCodeToUser;
                case CodenamesGame.EmailService.StatusCode.UNALLOWED:
                    return Lang.emailCannotUseAddressAlreadyInUse;
                case CodenamesGame.EmailService.StatusCode.NOT_FOUND:
                    return Lang.emailConfirmationCodeExpiredOrRemoved;
                case CodenamesGame.EmailService.StatusCode.UNAUTHORIZED:
                    return Lang.emailVerificationFailedAttemptsRemainingX;
                case CodenamesGame.EmailService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.EmailService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.EmailService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.EmailService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }

        public static string GetSessionServiceMessage(CodenamesGame.SessionService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.SessionService.StatusCode.UNAUTHORIZED:
                    return Lang.loginAlreadyLoggedInError;
                case CodenamesGame.SessionService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.SessionService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.SessionService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.SessionService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.SessionService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }
        
        public static string GetLobbyServiceMessage(LobbyOperationType operationType, CodenamesGame.LobbyService.StatusCode code)
        {
            switch (operationType)
            {
                case LobbyOperationType.INTIALIZE:
                    return GetLobbyInitializeMessage(code);
                case LobbyOperationType.CREATE_PARTY:
                    return GetCreatePartyMessage(code);
                case LobbyOperationType.INVITE_TO_PARTY:
                    return GetInviteToPartyMessage(code);
                case LobbyOperationType.JOIN_PARTY:
                    return GetJoinPartyMessage(code);
            }
            return Lang.globalUnknownServerError;
        }

        private static string GetLobbyInitializeMessage(CodenamesGame.LobbyService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.LobbyService.StatusCode.UNAUTHORIZED:
                    return Lang.loginAlreadyLoggedInError;
                case CodenamesGame.LobbyService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }

        private static string GetCreatePartyMessage(CodenamesGame.LobbyService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.LobbyService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.LobbyService.StatusCode.UNALLOWED:
                    return Lang.lobbyErrorAlreadyPartyHost;
                case CodenamesGame.LobbyService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }

        private static string GetInviteToPartyMessage(CodenamesGame.LobbyService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.LobbyService.StatusCode.NOT_FOUND:
                    return Lang.lobbyErrorPartyOrHostNotFound;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_DISCONNECT:
                    return Lang.lobbyErrorServiceConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.UNAUTHORIZED:
                    return Lang.lobbyErrorYouAreNotPartyHost;
                case CodenamesGame.LobbyService.StatusCode.UNALLOWED:
                    return Lang.lobbyErrorLobbyFull;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_UNREACHABLE:
                    return Lang.lobbyInfoFriendNotOnlineEmailSent;
                case CodenamesGame.LobbyService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }

        private static string GetJoinPartyMessage(CodenamesGame.LobbyService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.LobbyService.StatusCode.CLIENT_DISCONNECT:
                    return Lang.lobbyErrorServiceConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.MISSING_DATA:
                    return Lang.lobbyErrorProfileNotFoundWhenJoining;
                case CodenamesGame.LobbyService.StatusCode.NOT_FOUND:
                    return Lang.lobbyErrorLobbyCodeNotFound;
                case CodenamesGame.LobbyService.StatusCode.CONFLICT:
                    return Lang.lobbyErrorLobbyFull;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_UNREACHABLE:
                    return Lang.lobbyErrorPartyHostNotFound;
                case CodenamesGame.LobbyService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.LobbyService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.LobbyService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }

        public static string GetMatchmakingServiceMessage(CodenamesGame.MatchmakingService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.MatchmakingService.StatusCode.UNAUTHORIZED:
                    return Lang.lobbyAlreadyConnectedToMatchmakingService;
                case CodenamesGame.MatchmakingService.StatusCode.CLIENT_CANCEL:
                    return Lang.lobbyMatchCanceledByCompanion;
                case CodenamesGame.MatchmakingService.StatusCode.MISSING_DATA:
                    return Lang.lobbyMatchMissingData;
                case CodenamesGame.MatchmakingService.StatusCode.CONFLICT:
                    return Lang.lobbyCantStartPlayerBusy;
                case CodenamesGame.MatchmakingService.StatusCode.CLIENT_UNREACHABLE:
                    return Lang.lobbyCantStartCompanionDisconnect;
                case CodenamesGame.MatchmakingService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.MatchmakingService.StatusCode.CLIENT_TIMEOUT:
                    return Lang.lobbyMatchCanceledTimeout;
                case CodenamesGame.MatchmakingService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.MatchmakingService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.MatchmakingService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.MatchmakingService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }
        
        public static string GetMatchServiceMessage(CodenamesGame.MatchService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.MatchService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.MatchService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.MatchService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.MatchService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.MatchService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
            }
            return Lang.globalUnknownServerError;
        }

        public static string GetModerationMessage(CodenamesGame.ModerationService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.ModerationService.StatusCode.REPORT_CREATED:
                    return Lang.reportSuccess;
                case CodenamesGame.ModerationService.StatusCode.REPORT_DUPLICATED:
                    return Lang.reportDuplicate;
                case CodenamesGame.ModerationService.StatusCode.USER_KICKED_AND_BANNED:
                    return Lang.reportUserBanned;
                case CodenamesGame.ModerationService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.ModerationService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.ModerationService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.ModerationService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }

        public static string GetFriendServiceMessage(CodenamesGame.FriendService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.FriendService.StatusCode.FRIEND_REQUEST_SENT:
                    return Lang.friendRequestSentSuccess;
                case CodenamesGame.FriendService.StatusCode.FRIEND_ADDED:
                    return Lang.friendAddedSuccess;
                case CodenamesGame.FriendService.StatusCode.FRIEND_REMOVED:
                    return Lang.friendRemovedSuccess;
                case CodenamesGame.FriendService.StatusCode.FRIEND_REQUEST_REJECTED:
                    return Lang.friendRequestRejected;
                case CodenamesGame.FriendService.StatusCode.ALREADY_FRIENDS:
                    return Lang.friendErrorAlreadyFriends;
                case CodenamesGame.FriendService.StatusCode.CONFLICT:
                    return Lang.friendErrorOperationFailed;
                case CodenamesGame.FriendService.StatusCode.UNALLOWED:
                    return Lang.friendErrorSelfRequest;
                case CodenamesGame.FriendService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.FriendService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalServerTimeout;
                case CodenamesGame.FriendService.StatusCode.SERVER_UNREACHABLE:
                    return Lang.globalServerNotFound;
                case CodenamesGame.FriendService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.FriendService.StatusCode.CLIENT_ERROR:
                    return Lang.globalClientError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }
    }
}
