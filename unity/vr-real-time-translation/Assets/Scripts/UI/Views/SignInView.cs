using TMPro;
using UnityEngine;
using Amazon.Config;

public class SignInView : View
{
    [SerializeField] private TMP_InputField _usernameField;
    [SerializeField] private TMP_InputField _passwordField;
    [SerializeField] private TMP_Text _errorText;

    private bool IsValidInput
    {
        get
        {
            return !string.IsNullOrWhiteSpace(_usernameField.textComponent.text) && _passwordField.textComponent.text.Length >= 12;
        }
    }

    private void Awake()
    {
        AuthenticationManager.OnSignIn += SignInComplete;
    }

    public override void Activate()
    {
        base.Activate();
    }

    public void SignIn()
    {
        if (IsValidInput)
        {
            SetError("", false);
            AuthenticationManager.SignIn(_usernameField.text, _passwordField.text);
        }
        else
        {
            SetError("Email and/or Password are invalid.");
        }
    }

    public void SignInComplete(object sender, AuthCompleteEventArgs args)
    {
        if (args.result)
        {
            Deactivate();
        }
        else
        {
            SetError(args.message);
        }
    }

    public override void Deactivate(bool useEvent = true)
    {
        _usernameField.text = "";
        _passwordField.text = "";
        base.Deactivate(useEvent);
    }

    private void SetError(string error, bool log = true)
    {
        _errorText.text = error;
        _errorText.ForceMeshUpdate();

        if (log)
        {
            Debug.LogError($"[SignIn] {error}");
        }
    }
}