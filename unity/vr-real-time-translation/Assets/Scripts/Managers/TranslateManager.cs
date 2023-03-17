using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Amazon.Config;
using NativeWebSocket;

public class TranslateManager : Singleton<TranslateManager>
{
    public static event EventHandler<EventArgs> OnTranslateBegin;
    public static event EventHandler<EventArgs> OnTranslateEnd;
    public static event EventHandler<TranslateEventArgs> OnTranslate;

    [SerializeField] private AWSConfig _awsConfig;
    [Header("Audio Settings")]
    [SerializeField] private AudioSource _microphoneAudioSource;
    [SerializeField] private AudioSource _pollyAudioSource;
    [SerializeField] private float _delayTime = 0.2f;

    const string k_MediaEncoding = "pcm";
    const int k_SampleRate = 16000;

    private Coroutine _audioEventCoroutine = null;
    private TranscribeService _transcribeService = null;
    private TranslateService _translateService = null;
    private PollyService _pollyService = null;
    private WebSocketService _websocketService = null;
    private static Queue<TranslateResource> _resourceQueue;
    private static TranslateResource _currentResource;
    private static Coroutine _playAudioCoroutine;

    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("[TranslateManager] Microphone access granted by user.");
        }
        else
        {
            Debug.LogError("[TranslateManager] Microphone access denied by user.");
        }
    }

    private void Update()
    {
        _websocketService?.Update();
    }

    private async void OnApplicationQuit()
    {
        await _instance._End();
    }

    public static void Begin(Language sourceLanguage, Language targetLanguage, string targetVoice)
    {
        _instance._Begin(sourceLanguage, targetLanguage, targetVoice);
    }

    private async void _Begin(Language sourceLanguage, Language targetLanguage, string targetVoice)
    {
        // start microphone
        _microphoneAudioSource.clip = Microphone.Start(null, false, 300, k_SampleRate);

        // create Transcribe service
        _transcribeService = new TranscribeService(
            Session.AccessKeyId,
            Session.SessionToken,
            Session.SecretKey
        );

        // create Translate service
        _translateService = new TranslateService(
            Session.AccessKeyId,
            Session.SecretKey,
            Session.SessionToken,
            _awsConfig.Region,
            sourceLanguage.Translate,
            targetLanguage.Translate
        );

        // create Polly service
        _pollyService = new PollyService(
            Session.AccessKeyId,
            Session.SecretKey,
            Session.SessionToken,
            _awsConfig.Region,
            targetVoice
        );

        _resourceQueue = new Queue<TranslateResource>();

        // generate signed url for Transcribe WebSocket Streaming
        string url = _transcribeService.GenerateUrl(sourceLanguage.Transcribe, k_MediaEncoding, k_SampleRate.ToString());
        Debug.Log($"[TranslateManager] Transcribe URL: {url}");

        // create WebSocket and connect
        _websocketService = new WebSocketService(url, HandleSocketOpen, HandleSocketClose, HandleSocketMessageComplete);
        await _websocketService.Connect();

        if (OnTranslateBegin != null)
        {
            OnTranslateBegin.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleSocketOpen()
    {
        _audioEventCoroutine = StartCoroutine(IESendEvent());
    }

    private void HandleSocketClose(WebSocketCloseCode closeCode)
    {
        if (_audioEventCoroutine != null)
        {
            StopCoroutine(_audioEventCoroutine);
        }
        End();
    }

    private IEnumerator IESendEvent()
    {
        int offset = 0;
        while (_websocketService != null)
        {
            yield return new WaitForSeconds(_delayTime);
            offset = CreateAudioEvent(offset);
        }
    }

    private int CreateAudioEvent(int offset)
    {
        int position = Microphone.GetPosition(null);
        int diff = position - offset;
        if (diff > 0)
        {
            float[] currentSample = new float[diff * _microphoneAudioSource.clip.channels];
            _microphoneAudioSource.clip.GetData(currentSample, offset);

            AudioClip newClip = AudioClip.Create("", currentSample.Length, _microphoneAudioSource.clip.channels, _microphoneAudioSource.clip.frequency, false);
            newClip.SetData(currentSample, 0);

            var payload = AudioUtils.CreateAudioEvent(AudioUtils.To16BitPCM(newClip));
            _websocketService.Send(payload);
        }
        return position;
    }

    // responds to websocket message received
    private void HandleSocketMessageComplete(WebSocketEventArgs args)
    {
        // parse transcribed message
        if (args.message is TranslateWebsocketMessage translateMessage)
        {
            var results = translateMessage.Transcript.Results;
            if (results != null && results.Count > 0)
            {
                if (results[0].IsPartial)
                    return;

                string message = results[0].Alternatives[0].Transcript;
                Debug.Log($"[TranslateManager] Transcription: {message}");

                // call amazon translate to translate message
                Translate(message);
            }
        }
    }

    private async void Translate(string message)
    {
        string translation = await _translateService.Translate(message);
        if (!string.IsNullOrEmpty(translation))
        {
            _resourceQueue.Enqueue(new TranslateResource
            {
                Transcription = message,
                Translation = translation,
            });

            SynthesizeSpeech(translation);
        }
    }

    private async void SynthesizeSpeech(string message)
    {
        await _pollyService.SynthesizeSpeech(message, callback: HandleSynthesizeSpeechComplete);
    }

    private void HandleSynthesizeSpeechComplete(object sender, SynthesizeSpeechEventArgs args)
    {
        if (_resourceQueue.TryPeek(out TranslateResource resource))
        {
            resource.AudioFilePath = args.Path;
            resource.AudioClip = args.Clip;
        }

        if (_playAudioCoroutine == null)
        {
            _playAudioCoroutine = StartCoroutine(IEPlayAudio());
        }
    }

    private IEnumerator IEPlayAudio()
    {
        while (_resourceQueue.Count > 0)
        {
            if (_pollyAudioSource.isPlaying)
            {
                yield return new WaitUntil(() => !_pollyAudioSource.isPlaying);
                if (_currentResource != null)
                {
                    DeleteFile(_currentResource.AudioFilePath);
                }
            }

            _currentResource = _resourceQueue.Dequeue();

            if (OnTranslate != null)
            {
                OnTranslate?.Invoke(this, new TranslateEventArgs(true, _currentResource.Translation));
            }
            _pollyAudioSource.PlayOneShot(_currentResource.AudioClip);

            yield return null;
        }
        _playAudioCoroutine = null;
    }

    private void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void End()
    {
        _instance._End();
    }

    private async Task _End()
    {
        if (_playAudioCoroutine != null)
        {
            StopCoroutine(_playAudioCoroutine);
        }

        // stop microphone
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        // stop polly audio
        if (_pollyAudioSource.isPlaying)
        {
            _pollyAudioSource.Stop();
            _pollyAudioSource.clip = null;
        }

        // delete any audio files
        if (_resourceQueue != null && _resourceQueue.Count > 0)
        {
            for (int i = _resourceQueue.Count - 1; i >= 0; i--)
            {
                var resource = _resourceQueue.Dequeue();
                DeleteFile(resource.AudioFilePath);
            }
            _resourceQueue.Clear();
        }

        // close the websocket
        if (_websocketService != null)
        {
            // send close event
            byte[] payload = AudioUtils.CreateAudioEvent(new byte[0]);
            await _websocketService.Send(payload);

            // teardown and dispose socket service
            _websocketService.OnMessageComplete -= HandleSocketMessageComplete;
            await _websocketService.Dispose();
        }

        OnTranslateEnd?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Event handler for Translate events
/// </summary>
public class TranslateEventArgs : EventArgs
{
    public TranslateEventArgs(bool result, string message = "")
    {
        this.result = result;
        this.message = message;
    }

    public bool result;
    public string message;
}

public class TranslateResource
{
    public TranslateResource() { }

    public string Transcription;
    public string Translation;
    public string AudioFilePath;
    public AudioClip AudioClip;
}