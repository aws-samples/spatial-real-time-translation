using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LabelField : MonoBehaviour
{
    private TMP_Text _text;

    private void Awake()
    {
        _text = this.GetComponent<TMP_Text>();
    }

    public void Activate(string label, string value)
    {
        _text.text = $"{label}: {value}";
    }

    public void Activate(string label, string[] values)
    {
        string value = "";
        for (int i = 0; i < values.Length; i++)
        {
            value += $"{values[i]}";
            if (i < values.Length - 1)
            {
                value += ", ";
            }
        }
        _text.text = $"{label}: {value}";
    }

    public void Disable()
    {
        _text.text = "";
    }
}
