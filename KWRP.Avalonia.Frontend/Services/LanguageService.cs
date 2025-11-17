using KWRP.Backend.Services;
using KWRP.Frontend.Models.Localizer;

namespace KWRP.Frontend.Services
{
    public class LanguageService : ILanguageService
    {
        public LanguageService()
        {
        }


        public string CurrentLanguage => Localizer.Instance.Language;

        public string GetString(string key) => Localizer.Instance[key];

        public bool LoadLanguage(string language) => Localizer.Instance.LoadLanguage(language);
    }
}
