using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

public class TranscribeService
{
    private const string k_Service = "transcribe";
    private const string k_Path = "/stream-transcription-websocket";
    private const string k_Scheme = "AWS4";
    private const string k_Algorithm = "HMAC-SHA256";
    private const string k_Terminator = "aws4_request";
    private const string k_HmacSha256 = "HMACSHA256";
    private const string k_Region = "us-east-1";
    private const string k_Expiration = "300";


    private readonly string _accessKeyId;
    private readonly string _sessionToken;
    private readonly string _secretKey;

    public TranscribeService(string accessKeyId, string sessionToken, string secretKey)
    {
        _accessKeyId = accessKeyId;
        _sessionToken = sessionToken;
        _secretKey = secretKey;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="languageCode"></param>
    /// <param name="mediaEncoding"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public string GenerateUrl(string languageCode, string mediaEncoding = "pcm", string sampleRate = "16000")
    {
        var host = $"transcribestreaming.{k_Region}.amazonaws.com:8443";
        var dateNow = DateTime.Now.ToUniversalTime();
        var dateString = dateNow.ToString("yyyyMMdd");
        var dateTimeString = dateNow.ToString("yyyyMMddTHHmmssZ");
        var credentialScope = $"{dateString}/{k_Region}/{k_Service}/{k_Terminator}";
        var query = GenerateQueryParams(dateTimeString, credentialScope, languageCode, mediaEncoding, sampleRate);
        var signature = GenerateSignature(languageCode, host, dateString, dateTimeString, credentialScope);
        return $"wss://{host}{k_Path}?{query}&X-Amz-Signature={signature}";
    }

    /// <summary>
    /// Creates and formats Transcribe URL Parameters
    /// </summary>
    /// <param name="dateTimeString">transcribe formatted DateTime.Now string</param>
    /// <param name="credentialScope">scope for aws region, service, and terminator</param>
    /// <param name="languageCode">transcribe language id (defualt en-US)</param>
    /// <param name="mediaEncoding">audio format</param>
    /// <param name="sampleRate">audio rate</param>
    private string GenerateQueryParams(string dateTimeString, string credentialScope, string languageCode, string mediaEncoding = "pcm", string sampleRate = "16000")
    {
        var credentials = $"{_accessKeyId}/{credentialScope}";
        var result = new Dictionary<string, string>
        {
            {"X-Amz-Algorithm", $"{k_Scheme}-{k_Algorithm}"},
            {"X-Amz-Credential", credentials},
            {"X-Amz-Date", dateTimeString},
            {"X-Amz-Expires", k_Expiration},
            {"X-Amz-Security-Token", _sessionToken},
            {"X-Amz-SignedHeaders", "host"},
            {"language-code", languageCode},
            {"media-encoding", mediaEncoding},
            {"sample-rate", sampleRate},
            // {"transfer-encoding", "chunked"}
        };
        return string.Join("&", result.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="host"></param>
    /// <param name="dateString"></param>
    /// <param name="dateTimeString"></param>
    /// <param name="credentialScope"></param>
    /// <returns></returns>
    private string GenerateSignature(string languageCode, string host, string dateString, string dateTimeString, string credentialScope)
    {
        var canonicalRequest = CanonicalizeRequest(languageCode, k_Path, host, dateTimeString, credentialScope);
        var canonicalRequestHashBytes = GetHash(canonicalRequest);

        // construct the string to be signed
        var stringToSign = new StringBuilder();
        stringToSign.AppendFormat("{0}-{1}\n{2}\n{3}\n", k_Scheme, k_Algorithm, dateTimeString, credentialScope);
        stringToSign.Append(ToHex(canonicalRequestHashBytes, true));

        var kha = KeyedHashAlgorithm.Create(k_HmacSha256);
        kha.Key = GetSigningKey(k_HmacSha256, _secretKey, dateString, k_Service);

        // generate the final signature for the request, place into the result
        var signature = kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString()));
        var signatureString = ToHex(signature, true);
        return signatureString;
    }

    private string CanonicalizeRequest(string languageCode, string path, string host, string dateTimeString, string credentialScope)
    {
        var canonicalRequest = new StringBuilder();
        canonicalRequest.AppendFormat("{0}\n", "GET");
        canonicalRequest.AppendFormat("{0}\n", path);
        canonicalRequest.AppendFormat("{0}\n", GenerateQueryParams(dateTimeString, credentialScope, languageCode));
        canonicalRequest.AppendFormat("{0}\n", $"host:{host}");
        canonicalRequest.AppendFormat("{0}\n", "");
        canonicalRequest.AppendFormat("{0}\n", "host");
        canonicalRequest.Append(ToHex(GetHash(""), true));
        return canonicalRequest.ToString();
    }

    private static string ToHex(byte[] data, bool lowercase)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
        }
        return sb.ToString();
    }

    private static byte[] GetSigningKey(string algorithm, string awsSecretAccessKey, string date, string service)
    {
        char[] ksecret = (k_Scheme + awsSecretAccessKey).ToCharArray();
        byte[] hashDate = ComputeKeyedHash(algorithm, Encoding.UTF8.GetBytes(ksecret), Encoding.UTF8.GetBytes(date));
        byte[] hashRegion = ComputeKeyedHash(algorithm, hashDate, Encoding.UTF8.GetBytes(k_Region));
        byte[] hashService = ComputeKeyedHash(algorithm, hashRegion, Encoding.UTF8.GetBytes(service));
        return ComputeKeyedHash(algorithm, hashService, Encoding.UTF8.GetBytes(k_Terminator));
    }

    private static byte[] ComputeKeyedHash(string algorithm, byte[] key, byte[] data)
    {
        var kha = KeyedHashAlgorithm.Create(algorithm);
        kha.Key = key;
        return kha.ComputeHash(data);
    }

    private static byte[] GetHash(string data)
    {
        return HashAlgorithm.Create("SHA-256").ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
