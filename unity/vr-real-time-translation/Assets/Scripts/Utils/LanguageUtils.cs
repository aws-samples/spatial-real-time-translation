using System.Linq;
using System.Collections.Generic;

public struct Language
{
    public string DisplayName;
    public string Transcribe;
    public string Translate;
    public string Polly;
}

public static class LanguageUtils
{
    private static List<string> _displayNames;
    public static List<string> LanguageDisplayNames
    {
        get
        {
            if (_displayNames != null)
            {
                return _displayNames;
            }

            _displayNames = new List<string>();
            foreach (KeyValuePair<string, Language> kv in LanguageLookup)
            {
                Language lang = kv.Value;
                if (string.IsNullOrEmpty(lang.DisplayName))
                    continue;

                _displayNames.Add(lang.DisplayName);
            }
            return _displayNames;
        }
    }

    public static readonly Dictionary<string, Language> LanguageLookup = new Dictionary<string, Language>
    {
        {"chinese", new Language{
            DisplayName = "Chinese",
            Transcribe = "zh-CN",
            Translate = "zh",
            Polly = "cmn-CN"
        }},
        {"english-au", new Language{
            DisplayName = "English, Australian",
            Transcribe = "en-AU",
            Translate = "en",
            Polly = "en-AU"
        }},
        {"english-uk", new Language{
            DisplayName = "English, British",
            Transcribe = "en-GB",
            Translate = "en",
            Polly = "en-GB"
        }},
        {"english-us", new Language{
            DisplayName = "English, US",
            Transcribe = "en-US",
            Translate = "en",
            Polly = "en-US"
        }},
        {"french", new Language{
            DisplayName = "French",
            Transcribe = "fr-FR",
            Translate = "fr",
            Polly = "fr-FR"
        }},
        {"french-ca", new Language{
            DisplayName = "French, Canadian",
            Transcribe = "fr-CA",
            Translate = "fr-CA",
            Polly = "fr-CA"
        }},
        {"german", new Language{
            DisplayName = "German",
            Transcribe = "de-DE",
            Translate = "de",
            Polly = "de-DE"
        }},
        {"italian", new Language{
            DisplayName = "Italian",
            Transcribe = "it-IT",
            Translate = "it",
            Polly = "it-IT"
        }},
        {"japanese", new Language{
            DisplayName = "Japanese",
            Transcribe = "ja-JP",
            Translate = "ja",
            Polly = "ja-JP"
        }},
        {"korean", new Language{
            DisplayName = "Korean",
            Transcribe = "ko-KR",
            Translate = "ko",
            Polly = "ko-KR"
        }},
        {"portuguese-br", new Language{
            DisplayName = "Portuguese, Brazil",
            Transcribe = "pt-BR",
            Translate = "pt",
            Polly = "pt-BR"
        }},
        {"spanish", new Language{
            DisplayName = "Spanish",
            Transcribe = "es-US",
            Translate = "es",
            Polly = "es-US"
        }},
    };
}

