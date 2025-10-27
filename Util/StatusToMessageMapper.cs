using CodenamesClient.Properties.Langs;
using AuthSv = CodenamesGame.AuthenticationService;

namespace CodenamesClient.Util
{
    public static class StatusToMessageMapper
    {
        public static string AuthCodeToMessage(AuthSv.StatusCode code)
        {
            switch (code)
            {
                case AuthSv.StatusCode.SERVER_ERROR:
                    return Lang.globalServerError;
                case AuthSv.StatusCode.SERVER_UNAVAIBLE:
                    return Lang.globalConnectionLost;
                default:
                    return Lang.globalUnknownServerError;
            }
        }
    }
}
