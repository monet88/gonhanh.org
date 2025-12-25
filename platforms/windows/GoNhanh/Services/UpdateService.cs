using System.Net.Http;
using System.Text.Json;
using GoNhanh.Core;

namespace GoNhanh.Services;

/// <summary>
/// Checks GitHub for new releases
/// Similar to macOS UpdateChecker.swift
/// </summary>
public class UpdateService
{
    private const string ApiUrl = "https://api.github.com/repos/khaphanspace/gonhanh.org/releases/latest";
    private static readonly HttpClient _client = new();

    public string? LatestVersion { get; private set; }
    public string? ReleaseUrl { get; private set; }
    public bool UpdateAvailable { get; private set; }

    static UpdateService()
    {
        // GitHub API requires User-Agent
        _client.DefaultRequestHeaders.Add("User-Agent", "GoNhanh-Windows");
        _client.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Check GitHub for updates (async, non-blocking)
    /// </summary>
    public async Task CheckForUpdatesAsync()
    {
        try
        {
            var response = await _client.GetStringAsync(ApiUrl);
            var json = JsonDocument.Parse(response);

            var tagName = json.RootElement.GetProperty("tag_name").GetString();
            var htmlUrl = json.RootElement.GetProperty("html_url").GetString();

            if (tagName != null && htmlUrl != null)
            {
                LatestVersion = tagName.TrimStart('v');
                ReleaseUrl = htmlUrl;
                UpdateAvailable = IsNewerVersion(LatestVersion, AppMetadata.Version);
            }
        }
        catch (Exception ex)
        {
            // Silent fail - don't show update if check fails
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            UpdateAvailable = false;
        }
    }

    /// <summary>
    /// Compare semantic versions (major.minor.patch)
    /// </summary>
    private static bool IsNewerVersion(string latest, string current)
    {
        try
        {
            var latestParts = latest.Split('.').Select(int.Parse).ToArray();
            var currentParts = current.Split('.').Select(int.Parse).ToArray();

            // Compare major.minor.patch
            for (int i = 0; i < Math.Min(latestParts.Length, currentParts.Length); i++)
            {
                if (latestParts[i] > currentParts[i]) return true;
                if (latestParts[i] < currentParts[i]) return false;
            }

            return latestParts.Length > currentParts.Length;
        }
        catch
        {
            return false;
        }
    }
}
