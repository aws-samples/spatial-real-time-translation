using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Amazon.Polly;
using Amazon.Polly.Model;

public class PollyService
{
    private readonly string _accessKeyId;
    private readonly string _secretKey;
    private readonly string _sessionToken;
    private readonly Amazon.RegionEndpoint _region;
    private VoiceId _voiceId;
    const string k_AudioDirectoryName = "PollyAudio";

    public PollyService(string accessKeyId, string secretKey, string sessionToken, Amazon.RegionEndpoint region, string voice)
    {
        _accessKeyId = accessKeyId;
        _secretKey = secretKey;
        _sessionToken = sessionToken;
        _region = region;
        _voiceId = VoiceId.FindValue(voice);

        string directoryPath = Path.Combine(Application.persistentDataPath, k_AudioDirectoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Calls Amazon Polly to convert text to speech
    /// </summary>
    /// <param name="message">text to convert</param>
    public async Task SynthesizeSpeech(string message, int sampleRate = 16000, EventHandler<SynthesizeSpeechEventArgs> callback = null)
    {
        try
        {
            using (var client = new AmazonPollyClient(_accessKeyId, _secretKey, _sessionToken, _region))
            {
                if (_voiceId == null)
                {
                    _voiceId = VoiceId.Emma;
                }

                var response = await client.SynthesizeSpeechAsync(
                    new SynthesizeSpeechRequest()
                    {
                        Text = message,
                        VoiceId = _voiceId,
                        OutputFormat = OutputFormat.Mp3,
                        SampleRate = sampleRate.ToString()
                    }
                );

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"[TranslateManager] Failed to get audio clip. Status code: {response.HttpStatusCode}");
                }

                if (response.AudioStream != null)
                {
                    string path = await CreateFile(response.AudioStream);
                    MainThreadDispatcher.ExecuteCoroutineOnMainThread(IERequestAudio(path, callback));
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"[TranslateManager] Speech Synthesis Failed. {e}");
        }
    }

    private IEnumerator IERequestAudio(string path, EventHandler<SynthesizeSpeechEventArgs> callback = null)
    {
        string localPath = $"file://{path}";
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(localPath, AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"{request.result}: {request.error}");
                    break;
                case UnityWebRequest.Result.Success:
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    if (callback != null)
                    {
                        callback.Invoke(this, new SynthesizeSpeechEventArgs(path, clip));
                    }
                    break;
            }
        }
    }

    private async Task<string> CreateFile(Stream audioStream)
    {
        string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        string filename = $"{dateTime}-polly-audio.mp3";

        string path = Path.Combine(Application.persistentDataPath, k_AudioDirectoryName, filename);
        using (FileStream fs = File.Create(path))
        {
            await audioStream.CopyToAsync(fs);
        }
        return path;
    }
}

public class SynthesizeSpeechEventArgs : EventArgs
{
    public SynthesizeSpeechEventArgs(string path, AudioClip clip)
    {
        this.Path = path;
        this.Clip = clip;
    }

    public string Path;
    public AudioClip Clip;
}