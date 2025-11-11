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
    }
}
