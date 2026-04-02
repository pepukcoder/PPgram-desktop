using System;
using System.IO;
using System.Text.Json;
using PPgram.Logging;

namespace PPgram.Config;

public sealed class AppConfig
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 4433;
    public string Alpn { get; set; } = "ppproto/1.0";
    public string? ServerName { get; set; }
    public bool InsecureSkipCertificateValidation { get; set; } = true;

    public static AppConfig LoadOrCreate()
    {
        var configPath = GetConfigPath();

        try
        {
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(configPath))
            {
                var defaults = new AppConfig();
                var json = JsonSerializer.Serialize(defaults, JsonOptions);
                File.WriteAllText(configPath, json);
                AppLog.Info("Config", $"Created default config at {configPath}.");
                return defaults;
            }

            var raw = File.ReadAllText(configPath);
            var loaded = JsonSerializer.Deserialize<AppConfig>(raw, JsonOptions);
            if (loaded is null)
            {
                throw new InvalidDataException("Config JSON parsed to null.");
            }

            AppLog.Info("Config", $"Loaded config from {configPath}.");
            return loaded;
        }
        catch (Exception ex)
        {
            AppLog.Error("Config", $"Failed to load config at {configPath}; using defaults.", ex);
            return new AppConfig();
        }
    }

    private static string GetConfigPath()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, "PPgram", "config.json");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };
}
