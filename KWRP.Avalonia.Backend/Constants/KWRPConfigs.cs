using System.Configuration;

namespace KWRP.Avalonia.Backend.Constants
{
    public static class KWRPConfigs
    {
        public static bool IsDevMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return ConfigurationManager.AppSettings["DevMode"] == "true";
#endif
            }
        }
    }
}
