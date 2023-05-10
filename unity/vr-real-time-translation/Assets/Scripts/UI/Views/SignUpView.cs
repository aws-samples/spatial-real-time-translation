using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class SignUpView : View
{
    [Header("Password Requirements")]
    [SerializeField] private PasswordSettings _passwordSettings;

    [Header("References")]
    [SerializeField] private TMP_InputField _fullnameField;
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _usernameField;
    [SerializeField] private TMP_InputField _passwordField;
    [SerializeField] private TMP_InputField _confirmPasswordField;
    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private Button _signUpButton;

    private bool IsValidInput
    {
        get
        {
            return _emailField.characterValidation == TMP_InputField.CharacterValidation.EmailAddress &&
            _passwordSettings.IsValid(_passwordField.text) &&
            _confirmPasswordField.text == _passwordField.text &&
            !string.IsNullOrEmpty(_fullnameField.text) &&
            !string.IsNullOrEmpty(_usernameField.text);
        }
    }

    private void Awake()
    {
        AuthenticationManager.OnSignUp += SignUpComplete;
    }

    public override void Activate()
    {
        base.Activate();
    }

    public void VerifyPasswords()
    {
        if (string.IsNullOrEmpty(_confirmPasswordField.text))
            return;


        if (_confirmPasswordField.text == _passwordField.text)
        {
            SetError("", false);
        }
        else
        {
            SetError("Passwords do not match", false);
        }
    }

    public void SignUp()
    {
        if (IsValidInput)
        {
            AuthenticationManager.SignUp(_fullnameField.text, _emailField.text, _usernameField.text, _passwordField.text);
        }
        else
        {
            SetError("Verify information before signing up");
        }
    }

    public void SignUpComplete(object sender, AuthCompleteEventArgs args)
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
        _fullnameField.textComponent.text = "";
        _usernameField.textComponent.text = "";
        _emailField.textComponent.text = "";
        _passwordField.textComponent.text = "";
        _confirmPasswordField.textComponent.text = "";
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