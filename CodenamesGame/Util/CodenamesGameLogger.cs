using log4net;

namespace CodenamesGame.Util
{
    public static class CodenamesGameLogger
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(CodenamesGameLogger));
    }
}
