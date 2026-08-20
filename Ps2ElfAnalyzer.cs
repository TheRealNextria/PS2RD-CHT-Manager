using System.Buffers.Binary;
using System.Text;

public sealed record AnalysisResult(string Type, int Candidates, uint Matches, uint TargetAddress, uint TargetData, int Segment);
public sealed record AnalysisReport(uint Crc, IReadOnlyList<AnalysisResult> Results);

public static class Ps2ElfAnalyzer
{
    private sealed class Segment
    {
        public uint Offset, VAddr, FileSize;
        public uint VirtualOffset => VAddr - Offset;
    }

    private sealed class FuncId
    {
        public required string Name;
        public required uint[] Pattern;
        public required uint[] Mask;
        public int Length;
        public int JalCount;
        public int JalScope;
        public string? JalTargetFunction;
        public int JalRelativeOffset;
        public int JalRelativeTolerance;
        public int TargetOffset;
        public int TargetJalOffset;
        public int Counter;
        public int JalCounter;
        public uint Address;
        public uint TargetAddress;
        public int Candidates;
        public uint Matches;
        public int ResultSegment;
    }

    public static AnalysisReport Analyze(string path)
    {
        return Analyze(File.ReadAllBytes(path));
    }

    public static AnalysisReport Analyze(byte[] elf)
    {
        if (elf.Length < 52 || elf[0] != 0x7F || elf[1] != (byte)'E' || elf[2] != (byte)'L' || elf[3] != (byte)'F')
            throw new InvalidDataException("File is not an ELF file.");
        if (elf[4] != 1) throw new InvalidDataException("Only 32-bit ELF files are supported.");
        if (elf[5] != 1) throw new InvalidDataException("Only little-endian PS2 ELF files are supported.");

        uint entry = U32(elf, 24);
        uint phoff = U32(elf, 28);
        uint shoff = U32(elf, 32);
        ushort phentsize = U16(elf, 42);
        ushort phnum = U16(elf, 44);
        ushort shentsize = U16(elf, 46);
        ushort shnum = U16(elf, 48);
        ushort shstrndx = U16(elf, 50);

        var segments = new List<Segment>();
        for (int n = 0; n < phnum; n++)
        {
            int p = CheckedOffset(phoff + (uint)(n * phentsize), 32, elf.Length);
            uint type = U32(elf, p);
            uint off = U32(elf, p + 4);
            uint vaddr = U32(elf, p + 8);
            uint filesz = U32(elf, p + 16);
            uint flags = U32(elf, p + 24);
            if (type == 1 && (flags & 1) != 0)
                segments.Add(new Segment { Offset = off, VAddr = vaddr, FileSize = filesz });
        }
        if (segments.Count == 0) throw new InvalidDataException("ELF has no executable LOAD segment.");
        segments.Sort((a,b) => a.Offset.CompareTo(b.Offset));

        var funcs = MakeFunctions();
        FindSymbols(elf, shoff, shentsize, shnum, shstrndx, funcs);

        var entrypoint = funcs.First(f => f.Name == "entrypoint");
        entrypoint.Address = entry;
        entrypoint.Candidates = -3;
        var main = funcs.First(f => f.Name == "main");
        if (main.Candidates == 0) main.Candidates = -4;

        uint crc = 0;
        int wordCount = elf.Length / 4;
        for (int wi = 0; wi < wordCount; wi++) crc ^= U32(elf, wi * 4);

        // Match executable code. This is a direct managed translation of the PS2RD state machine.
        foreach (var seg in segments.Select((s, idx) => (s, idx)))
        {
            uint start = seg.s.Offset;
            uint end = Math.Min((uint)elf.Length, seg.s.Offset + seg.s.FileSize);
            for (uint pos = start; pos + 4 <= end; pos += 4)
            {
                uint data = U32(elf, (int)pos);
                uint address = pos + seg.s.VirtualOffset;

                for (int k = 0; k < funcs.Count; k++)
                {
                    var f = funcs[k];
                    if (f.Candidates >= 0)
                    {
                        if (f.Counter < f.Length)
                        {
                            if (f.Length != 1 && (data & f.Mask[f.Counter]) == (f.Pattern[f.Counter] & f.Mask[f.Counter]))
                                f.Counter++;
                            else
                                f.Counter = 0;
                        }
                        else if (f.Counter < f.JalScope)
                        {
                            if (data == 0x03E00008)
                            {
                                f.Counter = 0;
                                f.JalCounter = 0;
                            }
                            else
                            {
                                f.Counter++;
                                if ((data & 0xFC000000) == 0x0C000000)
                                {
                                    f.JalCounter++;
                                    if (f.JalCounter == f.JalCount)
                                    {
                                        uint jalTarget = (data & 0x03FFFFFF) << 2;
                                        if (f.JalTargetFunction is null)
                                        {
                                            if (f.JalRelativeOffset != 0)
                                            {
                                                long diff = (long)jalTarget - address - ((long)f.JalRelativeOffset << 2);
                                                diff = Math.Abs(diff) >> 2;
                                                if ((ulong)diff <= (uint)f.JalRelativeTolerance)
                                                    MarkFound(f, address - (uint)((f.Counter - 1) << 2), AddSigned(address, f.TargetJalOffset << 2), seg.idx);
                                            }
                                            else
                                                MarkFound(f, address - (uint)((f.Counter - 1) << 2), AddSigned(address, f.TargetJalOffset << 2), seg.idx);
                                        }
                                        else
                                        {
                                            f.Address = address - (uint)((f.Counter - 1) << 2);
                                            f.TargetAddress = address; // verified after scan
                                            f.ResultSegment = seg.idx;
                                            f.Matches++;
                                        }
                                        f.Counter = 0;
                                        f.JalCounter = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            f.Counter = 0;
                            f.JalCounter = 0;
                        }

                        if (f.Counter == f.Length)
                        {
                            f.Candidates++;
                            if (f.JalCount == 0)
                            {
                                f.Address = address - (uint)((f.Counter - 1) << 2);
                                f.Matches++;
                                if (f.TargetOffset >= 0)
                                {
                                    f.TargetAddress = AddSigned(f.Address, f.TargetOffset << 2);
                                    f.ResultSegment = seg.idx;
                                }
                                f.Counter = 0;
                            }
                        }
                    }
                    else if (f.Candidates >= -2)
                    {
                        if (address >= f.Address + (uint)(f.Length << 2) &&
                            address < f.Address + (uint)(f.Length << 2) + (uint)(f.JalScope << 2))
                        {
                            f.ResultSegment = seg.idx;
                            if (f.TargetOffset < 0 && f.JalCount != 0)
                            {
                                if (data == 0x03E00008)
                                {
                                    f.Counter = 1;
                                    f.JalCounter = 0;
                                }
                                else if ((data & 0xFC000000) == 0x0C000000 && f.Counter == 0)
                                {
                                    f.JalCounter++;
                                    if (f.JalCounter == f.JalCount)
                                    {
                                        uint jalTarget = (data & 0x03FFFFFF) << 2;
                                        if (f.JalTargetFunction is null)
                                        {
                                            if (f.JalRelativeOffset != 0)
                                            {
                                                long diff = (long)jalTarget - address - ((long)f.JalRelativeOffset << 2);
                                                diff = Math.Abs(diff) >> 2;
                                                if ((ulong)diff <= (uint)f.JalRelativeTolerance)
                                                    f.TargetAddress = AddSigned(address, f.TargetJalOffset << 2);
                                            }
                                            else
                                                f.TargetAddress = AddSigned(address, f.TargetJalOffset << 2);
                                        }
                                        else
                                        {
                                            f.TargetAddress = address; // verify later
                                            f.ResultSegment = seg.idx;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (f.Candidates == -3) // entrypoint
                    {
                        if (f.Counter == 0 && address >= f.Address)
                        {
                            f.ResultSegment = seg.idx;
                            if ((data & 0xFC000000) == 0x0C000000) f.JalCounter++;
                            if (f.JalCounter == 3)
                            {
                                f.Counter = 1;
                                if (main.Candidates != -1 && address < ((data & 0x03FFFFFF) << 2))
                                {
                                    main.Address = (data & 0x03FFFFFF) << 2;
                                    main.Candidates = -2;
                                }
                            }
                            else if (((data & 0xFC000000) == 0x08000000 && !((((data & 0x03FFFFFF) << 2) < address) && (((data & 0x03FFFFFF) << 2) >= f.Address))) ||
                                     ((data & 0xFC000000) == 0 && ((data & 0x3F) == 9 || (data & 0x3F) == 8)))
                            {
                                f.Counter = 1;
                            }
                        }
                    }
                }
            }
        }

        // Verify JAL references (scePadRead/scePad2Read -> memcpy).
        foreach (var f in funcs)
        {
            if (f.Address == 0 || f.TargetAddress == 0 || f.JalCount == 0 || f.JalTargetFunction is null) continue;
            var targetFunc = funcs.First(x => x.Name == f.JalTargetFunction);
            var seg = segments[f.ResultSegment];
            long fileOff = (long)f.TargetAddress - seg.VirtualOffset;
            if (fileOff < 0 || fileOff + 4 > elf.Length) { f.TargetAddress = 0; continue; }
            uint jal = U32(elf, (int)fileOff);
            if (((jal & 0x03FFFFFF) << 2) == targetFunc.Address)
                f.TargetAddress = AddSigned(f.TargetAddress, f.TargetJalOffset << 2);
            else
                f.TargetAddress = 0;
        }

        var results = new List<AnalysisResult>();
        foreach (var f in funcs)
        {
            uint targetData = 0;
            if (f.TargetAddress != 0)
            {
                var seg = segments[f.ResultSegment];
                long fileOff = (long)f.TargetAddress - seg.VirtualOffset;
                if (fileOff >= 0 && fileOff + 4 <= elf.Length)
                    targetData = U32(elf, (int)fileOff);
                else
                    f.TargetAddress = 0;
            }
            results.Add(new AnalysisResult(f.Name, f.Candidates, f.Matches, f.TargetAddress, targetData, f.ResultSegment));
        }
        return new AnalysisReport(crc, results);
    }

    private static void MarkFound(FuncId f, uint address, uint targetAddress, int segment)
    {
        f.Address = address;
        f.TargetAddress = targetAddress;
        f.ResultSegment = segment;
        f.Matches++;
    }

    private static uint AddSigned(uint value, int delta) => unchecked((uint)((long)value + delta));

    private static List<FuncId> MakeFunctions() => new()
    {
        new FuncId { Name="memcpy", Length=10, JalCount=0, JalScope=0, TargetOffset=-1, TargetJalOffset=0,
            Pattern=new uint[]{0x0080402d,0x2cc20020,0x1440001c,0x0100182d,0x00a81025,0x3042000f,0x54400019,0x24c6ffff,0x0100382d,0x78a30000},
            Mask=Enumerable.Repeat(0xFFFFFFFFu,10).ToArray() },
        new FuncId { Name="sceSifSendCmd", Length=10, JalCount=1, JalScope=15, JalTargetFunction=null, JalRelativeOffset=-88, JalRelativeTolerance=8, TargetOffset=-1, TargetJalOffset=0,
            Pattern=new uint[]{0x00c0102d,0x00e0182d,0x0100582d,0x27bdfff0,0x0120502d,0x00a0302d,0xffbf0000,0x0040382d,0x0060402d,0x0160482d},
            Mask=Enumerable.Repeat(0xFFFFFFFFu,10).ToArray() },
        new FuncId { Name="scePadRead", Length=10, JalCount=2, JalScope=30, JalTargetFunction="memcpy", TargetOffset=-1, TargetJalOffset=0,
            Pattern=new uint[]{0x0080382d,0x24030070,0x2404001c,0x70e31818,0x00a42018,0x27bd0000,0x3c020000,0xffb00000,0xffbf0000,0x24420000},
            Mask=new uint[]{0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffffffff,0xffff0000,0xff000000,0xff000000,0xffff0000,0xffff0000} },
        new FuncId { Name="scePad2Read", Length=10, JalCount=3, JalScope=40, JalTargetFunction="memcpy", TargetOffset=-1, TargetJalOffset=0,
            Pattern=new uint[]{0x27bdffc0,0x24020330,0xffb10010,0x3c03003d,0x0080882d,0xffb20020,0x02222018,0x2466ff40,0xffbf0030,0x00a0902d},
            Mask=new uint[]{0xffff0000,0xffff0000,0xffff0000,0xffff0000,0xff000000,0xffff0000,0xff000000,0xffff0000,0xff000000,0xff000000} },
        new FuncId { Name="main", Length=1, JalCount=1, JalScope=100, JalTargetFunction=null, TargetOffset=-1, TargetJalOffset=0, Pattern=new uint[]{0}, Mask=new uint[]{0} },
        new FuncId { Name="entrypoint", Length=1, JalCount=0, JalScope=0, JalTargetFunction=null, TargetOffset=-1, TargetJalOffset=0, Pattern=new uint[]{0}, Mask=new uint[]{0} },
    };

    private static void FindSymbols(byte[] elf, uint shoff, ushort shentsize, ushort shnum, ushort shstrndx, List<FuncId> funcs)
    {
        if (shoff == 0 || shnum == 0 || shstrndx == 0xFFFF || shstrndx >= shnum) return;
        int shstr = CheckedOffset(shoff + (uint)(shstrndx * shentsize), 40, elf.Length);
        uint shstrOffset = U32(elf, shstr + 16);
        uint symOffset = 0, symSize = 0, strOffset = 0;
        for (int i = 0; i < shnum; i++)
        {
            int s = CheckedOffset(shoff + (uint)(i * shentsize), 40, elf.Length);
            uint nameOff = U32(elf, s);
            string name = ReadZ(elf, checked((int)(shstrOffset + nameOff)));
            if (name == ".symtab") { symOffset = U32(elf, s + 16); symSize = U32(elf, s + 20); }
            else if (name == ".strtab") strOffset = U32(elf, s + 16);
        }
        if (symOffset == 0 || symSize == 0 || strOffset == 0) return;

        int count = (int)(symSize / 16);
        for (int i = 0; i < count; i++)
        {
            int p = CheckedOffset(symOffset + (uint)(i * 16), 16, elf.Length);
            uint nameIdx = U32(elf, p);
            uint value = U32(elf, p + 4);
            uint size = U32(elf, p + 8);
            byte info = elf[p + 12];
            if ((info & 0x0F) != 2) continue;
            string name = ReadZ(elf, checked((int)(strOffset + nameIdx)));
            var f = funcs.FirstOrDefault(x => x.Name == name && x.Address == 0);
            if (f is null) continue;
            f.Address = value;
            f.Matches = 1;
            f.Candidates = -1;
            if (f.TargetOffset >= 0) f.TargetAddress = AddSigned(f.Address, f.TargetOffset << 2);
            else if (f.JalCount != 0) f.JalScope = (int)(size >> 2);
        }
    }

    private static string ReadZ(byte[] b, int p)
    {
        if ((uint)p >= b.Length) return "";
        int e = p;
        while (e < b.Length && b[e] != 0) e++;
        return Encoding.ASCII.GetString(b, p, e - p);
    }

    private static int CheckedOffset(uint offset, int needed, int length)
    {
        if (offset > int.MaxValue || (long)offset + needed > length) throw new InvalidDataException("ELF contains an invalid table offset.");
        return (int)offset;
    }
    private static ushort U16(byte[] b, int p) => BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p, 2));
    private static uint U32(byte[] b, int p) => BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p, 4));
}
