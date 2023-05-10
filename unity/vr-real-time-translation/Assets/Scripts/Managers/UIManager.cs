using UnityEngine;
using UnityEngine.Events;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private SignUpView _signUpView;
    [SerializeField] private SignInView _signInView;
    [SerializeField] private TranslateView _translateView;
    public UnityEvent OnStart;

    private static View _currentView;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        OnStart.Invoke();
    }

    /// <summary>
    /// sets current view and activates
    /// </summary>
    /// <param name="view"></param>
    public void SetView(View view)
    {
        _currentView = view;
        _currentView.Activate();
    }

    /// <summary>
    /// on sign out button pressed
    /// </summary>
    public void SignOut()
    {
        AuthenticationManager.SignOut();

        if (_currentView != null)
        {
            _currentView.Deactivate(false);
        }
        SetView(_signInView);
    }
}