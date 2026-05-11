using KWRP.Backend.Enums;
using System.Configuration;
using System.Globalization;

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

        public static SystemLanguageType DefaultLanguage
        {
            get
            {
                var langKey = ConfigurationManager.AppSettings["DefaultLanguage"];
                if (langKey == null)
                {
                    // OSの言語が日本語ならJa, それ以外ならEnを返す
                    return GetLanguageFromOS();
                }

                return langKey.Trim().ToLowerInvariant() switch
                {
                    "ja" => SystemLanguageType.Ja,
                    "en" => SystemLanguageType.En,
                    _ => GetLanguageFromOS(),
                };
            }
        }


        private static SystemLanguageType GetLanguageFromOS()
        {
            var uiCulture = CultureInfo.CurrentUICulture;

            return uiCulture.TwoLetterISOLanguageName switch
            {
                "ja" => SystemLanguageType.Ja,
                _ => SystemLanguageType.En,
            };
        }

    }
}
