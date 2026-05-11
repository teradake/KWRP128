namespace KWRP.Backend.Enums
{
    public enum SystemLanguageType
    {
        Ja,
        En,
    }

    public static class SystemLangTypeExtensions
    {
        private readonly static Dictionary<SystemLanguageType, string> _langMap = new()
        {
            [SystemLanguageType.Ja] = "ja",
            [SystemLanguageType.En] = "en",
        };

        public static string ToKey(this SystemLanguageType type) => _langMap[type];
    }
}
