using UnityEngine;
using System.Linq;

[System.Serializable]
public class PasswordSettings
{
    [SerializeField] private int _minimumLength = 12;
    public int MinimumLength
    {
        get { return _minimumLength; }
    }

    [SerializeField] private bool _includeUpperCase = true;
    public bool IncludeUpperCase
    {
        get { return _includeUpperCase; }
    }

    [SerializeField] private bool _includeNumber = true;
    public bool IncludeNumber
    {
        get { return _includeNumber; }
    }

    [SerializeField] private bool _includeSpecialCharacter = true;
    public bool IncludeSpecialCharacter
    {
        get { return _includeSpecialCharacter; }
    }

    public bool IsValid(string password)
    {
        return password.Length >= _minimumLength &&
        (_includeNumber == password.Any(char.IsDigit)) &&
        (_includeUpperCase == password.Any(char.IsUpper)) &&
        (_includeSpecialCharacter == (password.Any(char.IsSymbol) || password.Any(char.IsPunctuation)));
    }
}