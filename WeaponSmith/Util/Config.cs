using System.IO;

namespace WeaponSmith.Util;

public class Config
{
    private const string FileHeadersSection = "FileHeaders";
    private const string RecentFilesSection = "RecentFiles";

    private static readonly string[] DefaultFileHeaders =
    {
        "WEAPONFILE",
        "ATTACHMENTFILE",
        "ATTACHMENTUNIQUEFILE",
    };

    public List<string> FileHeaders { get; } = new();
    public List<string> RecentFiles { get; } = new();

    public static Config Load(string path)
    {
        var config = new Config();

        try
        {
            if (File.Exists(path))
            {
                var section = RecentFilesSection;

                foreach (var rawLine in File.ReadAllLines(path))
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                        continue;

                    if (line.StartsWith('[') && line.EndsWith(']'))
                    {
                        section = line[1..^1].Trim();
                        continue;
                    }

                    if (section.Equals(FileHeadersSection, StringComparison.OrdinalIgnoreCase))
                        config.FileHeaders.Add(line);
                    else if (section.Equals(RecentFilesSection, StringComparison.OrdinalIgnoreCase))
                        config.RecentFiles.Add(line);
                }
            }
        }
        catch
        {
            // Silently fall back to defaults if the config can't be read
        }

        if (config.FileHeaders.Count == 0)
        {
            config.FileHeaders.AddRange(DefaultFileHeaders);
        }

        return config;
    }

    public void Save(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = new List<string> { $"[{FileHeadersSection}]" };
        lines.AddRange(FileHeaders);
        lines.Add(string.Empty);
        lines.Add($"[{RecentFilesSection}]");
        lines.AddRange(RecentFiles);

        File.WriteAllLines(path, lines);
    }
}
