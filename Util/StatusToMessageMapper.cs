using CodenamesClient.Properties.Langs;

namespace CodenamesClient.Util
{
    public static class StatusToMessageMapper
    {
        public static string GetAuthServiceMessage(CodenamesGame.AuthenticationService.StatusCode code)
        {
            switch (code)
            {
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case CodenamesGame.AuthenticationService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
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
                case CodenamesGame.SessionService.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                case CodenamesGame.SessionService.StatusCode.SERVER_TIMEOUT:
                    return Lang.globalTimeoutError;
                default:
                    return Lang.globalUnknownServerError;
            }
        }
        
        public static string GetLobbyServiceMessage(LobbyOperationType operationType, CodenamesGame.LobbyService.StatusCode code)
        {
            switch (operationType)
            {
                case LobbyOperationType.CONNECT:
                    return GetLobbyConnectMessage(code);
                case LobbyOperationType.CREATE_PARTY:
                    return GetCreatePartyMessage(code);
                case LobbyOperationType.INVITE_TO_PARTY:
                    return GetInviteToPartyMessage(code);
                case LobbyOperationType.JOIN_PARTY:
                    return GetJoinPartyMessage(code);
            }
            return Lang.globalUnknownServerError;
        }

        private static string GetLobbyConnectMessage(CodenamesGame.LobbyService.StatusCode code)
        {
            if (code == CodenamesGame.LobbyService.StatusCode.UNAUTHORIZED)
            {
                return Lang.loginAlreadyLoggedInError;
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
                default:
                    return Lang.globalUnknownServerError;
            }
        }
    }
}
