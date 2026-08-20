using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace PS2RDCHTManager;

public sealed record Ps2IsoInfo(string Serial, string ElfPath, byte[] ElfData, string SystemCnf);

public static class Ps2IsoReader
{
    private const int SectorSize = 2048;

    private sealed record IsoEntry(string Name, uint Extent, uint Size, bool IsDirectory);

    public static Ps2IsoInfo Read(string isoPath)
    {
        using var fs = new FileStream(isoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (fs.Length < 17L * SectorSize)
            throw new InvalidDataException("The file is too small to be a valid ISO9660 image.");

        byte[] pvd = ReadExact(fs, 16L * SectorSize, SectorSize);
        if (pvd[0] != 1 || Encoding.ASCII.GetString(pvd, 1, 5) != "CD001" || pvd[6] != 1)
            throw new InvalidDataException("No ISO9660 Primary Volume Descriptor was found. This image may not be a standard PS2 ISO.");

        var root = ParseDirectoryRecord(pvd, 156);
        if (root is null || !root.IsDirectory)
            throw new InvalidDataException("The ISO9660 root directory could not be read.");

        var systemEntry = FindEntryRecursive(fs, root.Extent, root.Size, "SYSTEM.CNF", maxDepth: 3)
            ?? throw new InvalidDataException("SYSTEM.CNF was not found in the ISO.");

        byte[] systemBytes = ReadExact(fs, (long)systemEntry.Extent * SectorSize, checked((int)systemEntry.Size));
        string systemCnf = DecodeSystemCnf(systemBytes);

        var bootMatch = Regex.Match(systemCnf,
            @"(?im)^\s*BOOT2\s*=\s*cdrom0:\\+([^;\r\n]+)(?:;\d+)?\s*$",
            RegexOptions.CultureInvariant);
        if (!bootMatch.Success)
            throw new InvalidDataException("SYSTEM.CNF was found, but no valid BOOT2=cdrom0:\\... entry was found.");

        string elfPath = bootMatch.Groups[1].Value.Trim().Replace('/', '\\');
        while (elfPath.StartsWith('\\')) elfPath = elfPath[1..];

        string serial = Path.GetFileName(elfPath);
        if (string.IsNullOrWhiteSpace(serial))
            throw new InvalidDataException("The PS2 executable name could not be determined from SYSTEM.CNF.");

        var elfEntry = FindByPath(fs, root, elfPath)
            ?? throw new InvalidDataException($"The boot ELF '{elfPath}' from SYSTEM.CNF was not found in the ISO.");
        if (elfEntry.IsDirectory)
            throw new InvalidDataException($"The BOOT2 path '{elfPath}' points to a directory instead of an ELF file.");
        if (elfEntry.Size > int.MaxValue)
            throw new InvalidDataException("The boot ELF is too large to analyze.");

        byte[] elfData = ReadExact(fs, (long)elfEntry.Extent * SectorSize, checked((int)elfEntry.Size));
        return new Ps2IsoInfo(serial, elfPath, elfData, systemCnf);
    }

    private static IsoEntry? FindByPath(FileStream fs, IsoEntry root, string path)
    {
        string[] parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        IsoEntry current = root;
        foreach (string part in parts)
        {
            if (!current.IsDirectory) return null;
            var entries = ReadDirectory(fs, current.Extent, current.Size);
            var next = entries.FirstOrDefault(e => NamesEqual(e.Name, part));
            if (next is null) return null;
            current = next;
        }
        return current;
    }

    private static IsoEntry? FindEntryRecursive(FileStream fs, uint extent, uint size, string target, int maxDepth)
    {
        foreach (var entry in ReadDirectory(fs, extent, size))
        {
            if (NamesEqual(entry.Name, target)) return entry;
        }
        if (maxDepth <= 0) return null;
        foreach (var entry in ReadDirectory(fs, extent, size))
        {
            if (!entry.IsDirectory || entry.Name is "." or "..") continue;
            var found = FindEntryRecursive(fs, entry.Extent, entry.Size, target, maxDepth - 1);
            if (found is not null) return found;
        }
        return null;
    }

    private static List<IsoEntry> ReadDirectory(FileStream fs, uint extent, uint size)
    {
        if (size > int.MaxValue) throw new InvalidDataException("ISO directory is too large.");
        byte[] data = ReadExact(fs, (long)extent * SectorSize, checked((int)size));
        var list = new List<IsoEntry>();
        int pos = 0;
        while (pos < data.Length)
        {
            int len = data[pos];
            if (len == 0)
            {
                pos = ((pos / SectorSize) + 1) * SectorSize;
                continue;
            }
            if (pos + len > data.Length) break;
            var entry = ParseDirectoryRecord(data, pos);
            if (entry is not null) list.Add(entry);
            pos += len;
        }
        return list;
    }

    private static IsoEntry? ParseDirectoryRecord(byte[] data, int offset)
    {
        if (offset < 0 || offset >= data.Length) return null;
        int len = data[offset];
        if (len < 34 || offset + len > data.Length) return null;

        uint extent = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 2, 4));
        uint size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 10, 4));
        bool isDir = (data[offset + 25] & 0x02) != 0;
        int nameLen = data[offset + 32];
        if (nameLen < 1 || offset + 33 + nameLen > offset + len) return null;

        string name;
        byte first = data[offset + 33];
        if (nameLen == 1 && first == 0) name = ".";
        else if (nameLen == 1 && first == 1) name = "..";
        else
        {
            name = Encoding.ASCII.GetString(data, offset + 33, nameLen);
            int semi = name.IndexOf(';');
            if (semi >= 0) name = name[..semi];
        }
        return new IsoEntry(name, extent, size, isDir);
    }

    private static bool NamesEqual(string isoName, string requested)
    {
        static string Clean(string s)
        {
            s = s.Trim();
            int semi = s.IndexOf(';');
            if (semi >= 0) s = s[..semi];
            return s.TrimEnd('.');
        }
        return string.Equals(Clean(isoName), Clean(requested), StringComparison.OrdinalIgnoreCase);
    }

    private static string DecodeSystemCnf(byte[] data)
    {
        int end = Array.IndexOf(data, (byte)0);
        if (end < 0) end = data.Length;
        return Encoding.ASCII.GetString(data, 0, end);
    }

    private static byte[] ReadExact(FileStream fs, long offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > fs.Length)
            throw new InvalidDataException("ISO contains an invalid extent or file size.");
        byte[] buffer = new byte[count];
        fs.Position = offset;
        int total = 0;
        while (total < count)
        {
            int read = fs.Read(buffer, total, count - total);
            if (read == 0) throw new EndOfStreamException("Unexpected end of ISO image.");
            total += read;
        }
        return buffer;
    }
}
