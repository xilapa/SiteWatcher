using System.Text.Json.Serialization;

namespace SiteWatcher.Infra.Authorization.GoogleAuth;

public class GoogleTokenMetadata
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public ushort ExpiresIn { get; set; }

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scopes { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    public bool Success { get; set; }
}