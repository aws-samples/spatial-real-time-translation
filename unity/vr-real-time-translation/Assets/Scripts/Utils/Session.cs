using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity.Model;
using UnityEngine;

namespace Amazon.Config
{
    public static class Session
    {
        public static void Construct(CognitoUser user, Credentials credentials)
        {
            User = user;
            Credentials = credentials;
        }

        public static void Dispose()
        {
            User = null;
            Credentials = null;
        }

        public static CognitoUser User { get; private set; }
        private static Credentials Credentials;

        public static string SessionToken
        {
            get
            {
                if (Credentials == null)
                {
                    return "";
                }
                return Credentials.SessionToken;
            }
        }

        public static string AccessToken
        {
            get
            {
                if (User == null)
                {
                    return "";
                }
                return User.SessionTokens.AccessToken;
            }
        }

        public static string AccessKeyId
        {
            get
            {
                if (Credentials == null)
                {
                    return "";
                }
                return Credentials.AccessKeyId;
            }
        }

        public static string SecretKey
        {
            get
            {
                if (Credentials == null)
                {
                    return "";
                }
                return Credentials.SecretKey;
            }
        }
    }
}
