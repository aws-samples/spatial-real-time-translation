using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoiceDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;

    private string _current;
    public string Current
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
        _dropdown.AddOptions(_voices);
        _dropdown.onValueChanged.AddListener(HandleDropdownChange);
    }

    public void HandleDropdownChange(int index)
    {
        SetCurrent(index);
    }

    private void SetCurrent(int index)
    {
        if (index >= 0 && index < _voices.Count)
        {
            _current = _voices[index];
        }
    }

    public struct VoiceCollection
    {

    }

    private static readonly List<string> _voices = new List<string>()
    {
        "Amy",
        "Bianca",
        "Brian",
        "Camila",
        "Carla",
        "Celine",
        "Chantal",
        "Conchita",
        "Emma",
        "Enrique",
        "Gabrielle",
        "Giorgio",
        "Hannah",
        "Hans",
        "Joanna",
        "Joey",
        "Kendra",
        "Kimberly",
        "Lea",
        "Lucia",
        "Lupe",
        "Marlene",
        "Mathieu",
        "Matthew",
        "Mia",
        "Miguel",
        "Mizuki",
        "Nicole",
        "Olivia",
        "Penelope",
        "Ricardo",
        "Russell",
        "Salli",
        "Seoyeon",
        "Takumi",
        "Vicki",
        "Vitoria",
        "Zhiyu",
    };
}