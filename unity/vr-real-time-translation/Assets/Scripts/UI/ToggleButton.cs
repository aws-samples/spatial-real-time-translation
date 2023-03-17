using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image), typeof(Button))]
public class ToggleButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _text;

    [Header("Settings")]
    [SerializeField] private string _activeText = "On";
    [SerializeField] private string _inactiveText = "Off";
    [SerializeField] private Color _activeColor = new Color(0, 1, 0);
    [SerializeField] private Color _inactiveColor = new Color(1, 0, 0);
    [SerializeField] private bool _activeOnStart = true;
    private bool _isActive;

    private void Awake()
    {
        if (_button == null)
        {
            _button = this.GetComponent<Button>();
        }

        if (_image == null)
        {
            _image = this.GetComponent<Image>();
        }

        if (_text == null)
        {
            _text = this.GetComponentInChildren<TMP_Text>();
        }

        _button.onClick.AddListener(() =>
        {
            Toggle(!_isActive);
        });
    }

    private void Start()
    {
        Toggle(_activeOnStart);
    }

    public void Toggle(bool state)
    {
        _isActive = state;
        if (state)
        {
            SetActive();
        }
        else
        {
            SetInactive();
        }
    }

    private void SetActive()
    {
        _image.color = _activeColor;
        _text.text = _activeText;
    }

    private void SetInactive()
    {
        _image.color = _inactiveColor;
        _text.text = _inactiveText;
    }
}
