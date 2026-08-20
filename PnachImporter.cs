using System.Text.RegularExpressions;

namespace PS2RDCHTManager;

public sealed class PnachImportResult
{
    public List<PnachCheat> Cheats { get; } = new();
    public int SkippedIopPatches { get; internal set; }
    public int UnparsedPatchLines { get; internal set; }
}

public sealed class PnachCheat
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Codes { get; } = new();

    public override string ToString() => Name;
}

public static partial class PnachImporter
{
    [GeneratedRegex(@"^\[(.+)\]\s*$")]
    private static partial Regex SectionRegex();

    [GeneratedRegex(
        @"^patch\s*=\s*(\d+)\s*,\s*([^,]+)\s*,\s*([0-9A-Fa-f]{1,8})\s*,\s*(extends|extended|byte|half|short|word)\s*,\s*([0-9A-Fa-f]{1,8})\s*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex PatchRegex();

    public static PnachImportResult ImportFile(string path) =>
        ImportText(File.ReadAllText(path));

    public static PnachImportResult ImportText(string text)
    {
        var result = new PnachImportResult();
        PnachCheat? current = null;

        foreach (string rawLine in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0)
                continue;

            Match section = SectionRegex().Match(line);
            if (section.Success)
            {
                current = new PnachCheat
                {
                    Name = NormalizeSectionName(section.Groups[1].Value)
                };

                result.Cheats.Add(current);
                continue;
            }

            if (current is null)
                continue;

            if (line.StartsWith("author=", StringComparison.OrdinalIgnoreCase))
                continue;

            if (line.StartsWith("description=", StringComparison.OrdinalIgnoreCase))
            {
                string description = line[(line.IndexOf('=') + 1)..].Trim();
                if (description.Length > 0)
                    current.Description = description;
                continue;
            }

            if (line.StartsWith("gsaspectratio=", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("gsinterlacemode=", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!line.StartsWith("patch=", StringComparison.OrdinalIgnoreCase))
                continue;

            Match patch = PatchRegex().Match(line);
            if (!patch.Success)
            {
                result.UnparsedPatchLines++;
                continue;
            }

            string cpu = patch.Groups[2].Value.Trim();
            if (!cpu.Equals("EE", StringComparison.OrdinalIgnoreCase))
            {
                result.SkippedIopPatches++;
                continue;
            }

            string address = patch.Groups[3].Value.ToUpperInvariant().PadLeft(8, '0');
            string type = patch.Groups[4].Value.ToLowerInvariant();
            string value = patch.Groups[5].Value.ToUpperInvariant().PadLeft(8, '0');

            address = type switch
            {
                "word" => "2" + address[1..],
                "half" or "short" => "1" + address[1..],
                "byte" => "0" + address[1..],
                "extended" or "extends" => address,
                _ => address
            };

            current.Codes.Add($"{address} {value}");
        }

        return result;
    }

    private static string NormalizeSectionName(string section)
    {
        string name = section.Replace('\\', ' ').Trim();
        return Regex.Replace(name, @"\s+", " ");
    }
}
