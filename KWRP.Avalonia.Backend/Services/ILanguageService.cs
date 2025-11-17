namespace KWRP.Backend.Services
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        bool LoadLanguage(string language);
        string GetString(string key);
    }
}
