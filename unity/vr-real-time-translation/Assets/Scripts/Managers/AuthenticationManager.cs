using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Amazon.Config;
using Amazon.Runtime;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;

public class AuthenticationManager : Singleton<AuthenticationManager>
{
    public static event EventHandler<AuthCompleteEventArgs> OnSignIn;
    public static event EventHandler<AuthCompleteEventArgs> OnSignUp;
    public static Action OnSignOut;

    [SerializeField] private AWSConfig _awsConfig;

    /// <summary>
    /// New user sign up request
    /// </summary>
    /// <param name="fullname"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static void SignUp(string fullname, string email, string username, string password)
    {
        _instance._SignUp(fullname, email, username, password);
    }

    private async Task _SignUp(string fullname, string email, string username, string password)
    {
        // construct cognito sign up request
        SignUpRequest request = new SignUpRequest()
        {
            ClientId = _awsConfig.Cognito.AppClientID,
            Username = username,
            Password = password
        };

        // add user attributes required by cognito config
        request.UserAttributes = new List<AttributeType>()
        {
            new AttributeType(){ Name = "name", Value = fullname },
            new AttributeType(){ Name = "preferred_username", Value = username },
            new AttributeType(){ Name = "email", Value = email }
        };

        // construct anonymous identity provider in proper region
        AmazonCognitoIdentityProviderClient provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), _awsConfig.Region);
        try
        {
            // send sign up request to cognito
            SignUpResponse response = await provider.SignUpAsync(request);
            Debug.Log("[AuthenticationManager] Sign Up Successful");

            // invoke successful sign up
            OnSignUp?.Invoke(this, new AuthCompleteEventArgs(true));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AuthenticationManager] Sign Up Failed: {e.Message}");

            // if request fails, invoke with failed and error message 
            OnSignUp?.Invoke(this, new AuthCompleteEventArgs(false, e.Message));
        }
    }

    /// <summary>
    /// Existing user sign in request
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static void SignIn(string username, string password)
    {
        _instance._SignIn(username, password);
    }

    private async Task _SignIn(string username, string password)
    {
        // construct anonymous identity provider in proper region
        AmazonCognitoIdentityProviderClient provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), _awsConfig.Region);

        // construct cognito user pool and cognito user objects to validate user
        CognitoUserPool userPool = new CognitoUserPool(_awsConfig.Cognito.UserPoolID, _awsConfig.Cognito.AppClientID, provider);
        CognitoUser user = new CognitoUser(username, _awsConfig.Cognito.AppClientID, userPool, provider);

        // construct sign in request
        InitiateSrpAuthRequest request = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        try
        {
            // send sign in request
            AuthFlowResponse response = await user.StartWithSrpAuthAsync(request);
            if (response.AuthenticationResult != null)
            {
                // update user object with response tokens
                user.SessionTokens = new CognitoUserSession(
                    response.AuthenticationResult.IdToken,
                    response.AuthenticationResult.AccessToken,
                    response.AuthenticationResult.RefreshToken,
                    System.DateTime.UtcNow,
                    System.DateTime.UtcNow.AddSeconds(response.AuthenticationResult.ExpiresIn)
                );
            }

            if (await CreateUser(user))
            {
                // invoke successful sign in
                OnSignIn?.Invoke(this, new AuthCompleteEventArgs(true));
            }
            else
            {
                OnSignIn?.Invoke(this, new AuthCompleteEventArgs(false, "Failed to get User Identity."));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AuthenticationManager] Sign In Failed: {e.Message}");

            // if request fails, invoke with failed and error message 
            OnSignIn?.Invoke(this, new AuthCompleteEventArgs(false, e.Message));
        }
    }

    /// <summary>
    /// user sign out request
    /// </summary>
    public static void SignOut()
    {
        _instance._SignOut();
    }

    private async void _SignOut()
    {
        if (Session.User != null)
        {
            // invoke cognito user sign out on all devices
            await Session.User.GlobalSignOutAsync();
            Debug.Log("[AuthenticationManager] Sign Out Successful");
        }

        // tear down session object
        Session.Dispose();
        OnSignOut?.Invoke();
    }

    private async Task<bool> CreateUser(CognitoUser user)
    {
        try
        {
            Credentials credentials = await GetIdentityCredentials(user);
            Session.Construct(user, credentials);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AuthenticationManager] Failed to construct user credentials: {e.Message}");
            return false;
        }
        return true;
    }

    private async Task<Credentials> GetIdentityCredentials(CognitoUser user)
    {
        var loginAttributes = new Dictionary<string, string>
        {
            { $"cognito-idp.{_awsConfig.Region.SystemName}.amazonaws.com/{_awsConfig.Cognito.UserPoolID}", user.SessionTokens.IdToken }
        };

        var credentials = user.GetCognitoAWSCredentials(_awsConfig.Cognito.IdentityPoolID, _awsConfig.Region);
        var identityClient = new AmazonCognitoIdentityClient(credentials, _awsConfig.Region);

        var request = new GetIdRequest()
        {
            IdentityPoolId = _awsConfig.Cognito.IdentityPoolID,
            Logins = loginAttributes
        };

        var response = await identityClient.GetIdAsync(request);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            Debug.LogError($"[AuthenticationManager] ID Request Failed. Status Code: {response.HttpStatusCode}");
            return null;
        }

        var credentialsResponse = await identityClient.GetCredentialsForIdentityAsync(response.IdentityId, loginAttributes);
        if (credentialsResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            Debug.LogError($"[AuthenticationManager] Credentials Request Failed. Status Code: {credentialsResponse.HttpStatusCode}");
            return null;
        }
        return credentialsResponse.Credentials;
    }
}

/// <summary>
/// Event handler for Authentication events
/// </summary>
public class AuthCompleteEventArgs : EventArgs
{
    public AuthCompleteEventArgs(bool result = false, string message = "")
    {
        this.result = result;
        this.message = message;
    }

    public bool result;
    public string message;
}