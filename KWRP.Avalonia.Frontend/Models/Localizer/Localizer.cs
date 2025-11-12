using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KWRP.Frontend.Models.Localizer
{
    public class Localizer : INotifyPropertyChanged
    {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string> m_Strings = null;

        public bool LoadLanguage(string language)
        {
            try
            {
                Language = language;

                Uri uri = new Uri($"avares://KWRP/Assets/i18n/{language}.json");
                if (AssetLoader.Exists(uri))
                {
                    using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8))
                    {
                        //m_Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                        m_Strings = JsonSerializer.Deserialize<Dictionary<string, string>>(sr.ReadToEnd());
                    }
                    Invalidate();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string Language { get; private set; }

        public string this[string key]
        {
            get
            {
                if (m_Strings != null && m_Strings.TryGetValue(key, out string res))
                    return res.Replace("\\n", "\n");

                return $"missing {Language}:{key}";
            }
        }

        public static Localizer Instance { get; set; } = new Localizer();
        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
