using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TranslateView : View
{
    [SerializeField] private TMP_Text _transcriptionText;
    [SerializeField] private ToggleButton _startButton;
    [SerializeField] private LanguageDropdown _sourceDropdown;
    [SerializeField] private LanguageDropdown _targetDropdown;
    [SerializeField] private VoiceDropdown _voiceDropdown;
    [SerializeField]
    private FontCollection _fonts = new FontCollection();
    public FontCollection Fonts
    {
        get { return _fonts; }
    }

    [System.Serializable]
    public struct FontCollection
    {
        public TMP_FontAsset Standard;
        public TMP_FontAsset Chinese;
        public TMP_FontAsset Japanese;
        public TMP_FontAsset Korean;
    }

    private Language _sourceLanguage
    {
        get
        {
            return _sourceDropdown.Current;
        }
    }

    private Language _targetLanguage
    {
        get
        {
            return _targetDropdown.Current;
        }
    }

    private string _targetVoice
    {
        get { return _voiceDropdown.Current; }
    }

    private bool _isTranslateActive = false;

    private void Awake()
    {
        TranslateManager.OnTranslate += HandleTranslate;
        TranslateManager.OnTranslateEnd += (object sender, System.EventArgs args) =>
        {
            _isTranslateActive = false;
            _startButton.Toggle(true);
            _transcriptionText.text = "";
        };

        _targetDropdown.OnSelect += HandleTargetLanguageSelect;
    }

    public void Translate()
    {
        if (_isTranslateActive)
        {
            Debug.Log("[TranslateView] End Translate");
            TranslateManager.End();
        }
        else
        {
            Debug.Log("[TranslateView] Begin Translate");
            TranslateManager.Begin(_sourceLanguage, _targetLanguage, _targetVoice);
        }
        _isTranslateActive = !_isTranslateActive;
    }

    private void HandleTranslate(object sender, TranslateEventArgs args)
    {
        _transcriptionText.text = args.message;
    }

    public void HandleTargetLanguageSelect(object sender, Language language)
    {
        TMP_FontAsset font = _fonts.Standard;
        switch (language.Translate)
        {
            case "zh":
                font = _fonts.Chinese;
                break;
            case "ja":
                font = _fonts.Japanese;
                break;
            case "ko":
                font = _fonts.Korean;
                break;
        }
        _transcriptionText.font = font;
    }
}
