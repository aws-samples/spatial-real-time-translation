using System;
using System.Threading.Tasks;
using UnityEngine;
using Amazon.Translate;
using Amazon.Translate.Model;

public class TranslateService
{
    private string _accessKeyId;
    private string _secretKey;
    private string _sessionToken;
    private Amazon.RegionEndpoint _region;
    private string _sourceLanguage;
    private string _targetLanguage;

    public TranslateService(string accessKeyId, string secretKey, string sessionToken, Amazon.RegionEndpoint region, string sourceLanguage, string targetLanguage)
    {
        _accessKeyId = accessKeyId;
        _secretKey = secretKey;
        _sessionToken = sessionToken;
        _region = region;
        _sourceLanguage = sourceLanguage;
        _targetLanguage = targetLanguage;
    }

    public async Task<string> Translate(string message)
    {
        try
        {
            using (var translateClient = new AmazonTranslateClient(_accessKeyId, _secretKey, _sessionToken, _region))
            {
                var response = await translateClient.TranslateTextAsync(
                    new TranslateTextRequest()
                    {
                        Text = message,
                        SourceLanguageCode = _sourceLanguage,
                        TargetLanguageCode = _targetLanguage
                    }
                );

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.LogError($"[TranslateManager] Failed to translate text. Status Code: {response.HttpStatusCode}");
                }

                Debug.Log($"[TranslateManager] Translation: {response.TranslatedText}");
                return response.TranslatedText;
            }
        }
        catch (Exception e)
        {
            throw new Exception($"[TranslateManager] Translation Failed. {e}");
        }
    }
}
