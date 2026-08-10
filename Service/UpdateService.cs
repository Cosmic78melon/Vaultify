using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Vaultify.Service;
using System.Text.Json.Serialization;

public class UpdateService: IUpdateService
{
    private readonly HttpClient _httpClient = new();

    public async Task<GitHubReleaseData?> CheckUpdateInfoAsync()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Vaultify");
            return await _httpClient.GetFromJsonAsync<GitHubReleaseData>("https://api.github.com/repos/Cosmic78melon/Vaultify/releases/latest");
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    
}
public class GitHubReleaseData
{
    [JsonPropertyName("tag_name")] public string TagName { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("assets")] public List<GithubAssets> Assets { get; set; } = [];
}
public class GithubAssets
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}

