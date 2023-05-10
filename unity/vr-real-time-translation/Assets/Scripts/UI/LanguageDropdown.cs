using System;
using UnityEngine;
using TMPro;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    public EventHandler<Language> OnSelect;

    private Language _current;
    public Language Current
    {
        get
        {
            return _current;
        }
    }

    private void Awake()
    {
        if (_dropdown == null)
        {
            _dropdown = this.GetComponentInChildren<TMP_Dropdown>();
        }

        _dropdown.ClearOptions();
        foreach (var kv in LanguageUtils.LanguageLookup)
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData(kv.Value.DisplayName));
        }
        _dropdown.onValueChanged.AddListener(HandleDropdownChange);
    }

    public void HandleDropdownChange(int index)
    {
        if (index < 0 && index >= LanguageUtils.LanguageLookup.Count)
            return;

        SetCurrentLanguage(_dropdown.options[index].text);
    }

    private void SetCurrentLanguage(string name)
    {
        foreach (var kv in LanguageUtils.LanguageLookup)
        {
            if (kv.Value.DisplayName == name)
            {
                _current = kv.Value;
                break;
            }
        }

        if (OnSelect != null)
        {
            OnSelect.Invoke(this, _current);
        }
    }
}
