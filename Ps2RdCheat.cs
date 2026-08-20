using System.Text;
using System.Text.RegularExpressions;

namespace PS2RDCHTManager;

public sealed record CheatValidationLine(
    int LineNumber,
    string Original,
    string? Normalized,
    bool IsValid,
    string Message);

public static class Ps2RdCheat
{
    private static readonly Regex HexPair =
        new(@"^\s*([0-9A-Fa-f]{8})\s+([0-9A-Fa-f]{8})\s*$", RegexOptions.Compiled);

    // PS2RD code types store EE addresses in 25 bits.
    // For a typed left word X-aaaaaaa, bits 27..25 must therefore be zero.
    private const uint AddressMask25 = 0x01FFFFFF;
    private const uint ReservedAddressBits = 0x0E000000;

    private sealed record ParsedLine(
        int SourceIndex,
        int LineNumber,
        string Original,
        string LeftText,
        string RightText,
        uint Left,
        uint Right)
    {
        public string Normalized => $"{LeftText} {RightText}";
        public char Type => LeftText[0];
    }

    public static IReadOnlyList<CheatValidationLine> Validate(string text)
    {
        string[] sourceLines = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        var parsed = new List<ParsedLine>();
        var output = new Dictionary<int, CheatValidationLine>();

        // First pass: basic syntax.
        for (int i = 0; i < sourceLines.Length; i++)
        {
            string original = sourceLines[i];

            if (string.IsNullOrWhiteSpace(original))
                continue;

            Match m = HexPair.Match(original);
            if (!m.Success)
            {
                output[i] = new CheatValidationLine(
                    i + 1,
                    original,
                    null,
                    false,
                    "Invalid syntax. Expected: XXXXXXXX YYYYYYYY (8 hex digits + 8 hex digits).");
                continue;
            }

            string leftText = m.Groups[1].Value.ToUpperInvariant();
            string rightText = m.Groups[2].Value.ToUpperInvariant();

            parsed.Add(new ParsedLine(
                i,
                i + 1,
                original,
                leftText,
                rightText,
                Convert.ToUInt32(leftText, 16),
                Convert.ToUInt32(rightText, 16)));
        }

        // Block-level encrypted detection.
        // If any line uses a code type that PS2RD marks as unused (A, B or F),
        // treat the whole cheat as a possibly encrypted block. Some encrypted
        // lines can accidentally look like valid RAW types when checked alone.
        bool possibleEncryptedBlock = parsed.Any(line =>
            line.Type is 'A' or 'B' or 'F');

        if (possibleEncryptedBlock)
        {
            foreach (ParsedLine line in parsed)
            {
                output[line.SourceIndex] = new CheatValidationLine(
                    line.LineNumber,
                    line.Original,
                    line.Normalized,
                    false,
                    "Possible encrypted cheat block: contains PS2RD-unused code type A/B/F; do not treat this block as RAW.");
            }

            return output
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .ToList();
        }

        // Second pass: validate the actual PS2RD structure.
        for (int p = 0; p < parsed.Count; p++)
        {
            ParsedLine line = parsed[p];

            // Multi-line types can consume the next line.
            if (output.ContainsKey(line.SourceIndex))
                continue;

            switch (line.Type)
            {
                case '0':
                    ValidateType0(line, output);
                    break;

                case '1':
                    ValidateType1(line, output);
                    break;

                case '2':
                    ValidateType2(line, output);
                    break;

                case '3':
                    ValidateType3(parsed, ref p, line, output);
                    break;

                case '4':
                    ValidateType4(parsed, ref p, line, output);
                    break;

                case '5':
                    ValidateType5(parsed, ref p, line, output);
                    break;

                case '6':
                    ValidateType6(parsed, ref p, line, output);
                    break;

                case '7':
                    ValidateType7(line, output);
                    break;

                case '9':
                    ValidateType9(line, output);
                    break;

                case 'C':
                    ValidateTypeC(line, output);
                    break;

                case 'D':
                    ValidateTypeD(line, parsed, p, output);
                    break;

                case 'E':
                    ValidateTypeE(line, output);
                    break;

                case '8':
                case 'A':
                case 'B':
                case 'F':
                    PossibleEncrypted(
                        line,
                        output,
                        $"Type {line.Type} is unused by PS2RD. Not valid PS2RD RAW; possibly encrypted.");
                    break;

                default:
                    PossibleEncrypted(
                        line,
                        output,
                        $"Unknown PS2RD code type {line.Type}. Not valid PS2RD RAW; possibly encrypted.");
                    break;
            }
        }

        return output
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();
    }

    private static bool TryTypedAddress(
        ParsedLine line,
        out uint address,
        out string error)
    {
        address = line.Left & AddressMask25;

        if ((line.Left & ReservedAddressBits) != 0)
        {
            error =
                $"address field is outside PS2RD's 25-bit EE range " +
                $"(encoded address bits must be 00000000-01FFFFFF; decoded low address is {address:X8})";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static void ValidateType0(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 0: {addressError}. Possibly encrypted.");
            return;
        }

        if ((line.Right & 0xFFFFFF00) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                "Not valid PS2RD type 0: an 8-bit write must use value 000000XX. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 0 (8-bit write), address {address:X8}, value {(line.Right & 0xFF):X2}.");
    }

    private static void ValidateType1(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 1: {addressError}. Possibly encrypted.");
            return;
        }

        if ((line.Right & 0xFFFF0000) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                "Not valid PS2RD type 1: a 16-bit write must use value 0000XXXX. Possibly encrypted.");
            return;
        }

        if ((address & 1) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 1: address {address:X8} is not aligned to 2. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 1 (16-bit write), address {address:X8}, value {(line.Right & 0xFFFF):X4}.");
    }

    private static void ValidateType2(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 2: {addressError}. Possibly encrypted.");
            return;
        }

        if ((address & 3) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 2: address {address:X8} is not aligned to 4. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 2 (32-bit write), address {address:X8}, value {line.Right:X8}.");
    }

    private static void ValidateType3(
        List<ParsedLine> parsed,
        ref int p,
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        // Type 3 stores the address in the right word as 0aaaaaaa.
        if (line.Right > AddressMask25)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 3: target address {line.Right:X8} is outside 00000000-01FFFFFF. Possibly encrypted.");
            return;
        }

        uint address = line.Right;
        uint command = line.Left;

        // 8-bit increment: 300000vv
        if ((command & 0xFFFFFF00) == 0x30000000)
        {
            Valid(line, output,
                $"Valid PS2RD RAW: type 3 (8-bit increment), address {address:X8}, amount {(command & 0xFF):X2}.");
            return;
        }

        // 8-bit decrement: 301000vv
        if ((command & 0xFFFFFF00) == 0x30100000)
        {
            Valid(line, output,
                $"Valid PS2RD RAW: type 3 (8-bit decrement), address {address:X8}, amount {(command & 0xFF):X2}.");
            return;
        }

        // 16-bit increment/decrement: 3020vvvv / 3030vvvv
        if ((command & 0xFFFF0000) == 0x30200000 ||
            (command & 0xFFFF0000) == 0x30300000)
        {
            if ((address & 1) != 0)
            {
                PossibleEncrypted(
                    line,
                    output,
                    $"Not valid PS2RD type 3: 16-bit target address {address:X8} is not aligned to 2. Possibly encrypted.");
                return;
            }

            string action =
                (command & 0xFFFF0000) == 0x30200000 ? "increment" : "decrement";

            Valid(line, output,
                $"Valid PS2RD RAW: type 3 (16-bit {action}), address {address:X8}, amount {(command & 0xFFFF):X4}.");
            return;
        }

        // 32-bit increment/decrement:
        // 30400000 0aaaaaaa
        // vvvvvvvv 00000000
        // or 30500000 ...
        if (command == 0x30400000 || command == 0x30500000)
        {
            if ((address & 3) != 0)
            {
                PossibleEncrypted(
                    line,
                    output,
                    $"Not valid PS2RD type 3: 32-bit target address {address:X8} is not aligned to 4. Possibly encrypted.");
                return;
            }

            if (!TryGetNext(parsed, p, out ParsedLine second))
            {
                Invalid(
                    line,
                    output,
                    "Incomplete PS2RD type 3: 32-bit increment/decrement requires a second line VVVVVVVV 00000000.");
                return;
            }

            if (second.Right != 0)
            {
                Invalid(
                    line,
                    output,
                    "Invalid PS2RD type 3: second line of 32-bit increment/decrement must end in 00000000.");
                Invalid(
                    second,
                    output,
                    "Invalid continuation for previous type-3 code; expected VVVVVVVV 00000000.");
                p++;
                return;
            }

            string action = command == 0x30400000 ? "increment" : "decrement";

            Valid(line, output,
                $"Valid PS2RD RAW: type 3 (32-bit {action}), address {address:X8}; next line contains amount.");
            Valid(second, output,
                $"Valid continuation: 32-bit {action} amount {second.Left:X8}.");
            p++;
            return;
        }

        PossibleEncrypted(
            line,
            output,
            "Not valid PS2RD type 3: unknown increment/decrement subtype. Possibly encrypted.");
    }

    private static void ValidateType4(
        List<ParsedLine> parsed,
        ref int p,
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 4: {addressError}. Possibly encrypted.");
            return;
        }

        if ((address & 3) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 4: start address {address:X8} is not aligned to 4. Possibly encrypted.");
            return;
        }

        int count = (int)(line.Right >> 16);
        if (count == 0)
        {
            Invalid(line, output, "Invalid PS2RD type 4: serial-write count is 0.");
            return;
        }

        if (!TryGetNext(parsed, p, out ParsedLine second))
        {
            Invalid(
                line,
                output,
                "Incomplete PS2RD type 4: serial write requires a second line VVVVVVVV IIIIIIII.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 4 (32-bit serial write), start {address:X8}, count {count}; next line contains value/step.");
        Valid(
            second,
            output,
            $"Valid continuation: start value {second.Left:X8}, value step {second.Right:X8}.");
        p++;
    }

    private static void ValidateType5(
        List<ParsedLine> parsed,
        ref int p,
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint source, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 5: {addressError}. Possibly encrypted.");
            return;
        }

        if (!TryGetNext(parsed, p, out ParsedLine second))
        {
            Invalid(
                line,
                output,
                "Incomplete PS2RD type 5: copy-bytes requires a second line 0DDDDDDD 00000000.");
            return;
        }

        if (second.Left > AddressMask25 || second.Right != 0)
        {
            Invalid(
                line,
                output,
                "Invalid PS2RD type 5: second line must be destination address 00000000-01FFFFFF followed by 00000000.");
            Invalid(
                second,
                output,
                "Invalid continuation for previous type-5 copy-bytes code.");
            p++;
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 5 (copy bytes), source {source:X8}, byte count {line.Right}.");
        Valid(
            second,
            output,
            $"Valid continuation: destination {second.Left:X8}.");
        p++;
    }

    private static void ValidateType6(
        List<ParsedLine> parsed,
        ref int p,
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint baseAddress, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 6: {addressError}. Possibly encrypted.");
            return;
        }

        if ((baseAddress & 3) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 6: base-pointer address {baseAddress:X8} is not aligned to 4. Possibly encrypted.");
            return;
        }

        if (!TryGetNext(parsed, p, out ParsedLine second))
        {
            Invalid(
                line,
                output,
                "Incomplete PS2RD type 6: pointer write requires a second line containing size selector and offset.");
            return;
        }

        uint selector = second.Left;
        if (selector != 0x00000000 &&
            selector != 0x00010000 &&
            selector != 0x00020000)
        {
            Invalid(
                line,
                output,
                "Invalid PS2RD type 6: selector must be 00000000 (8-bit), 00010000 (16-bit), or 00020000 (32-bit).");
            Invalid(
                second,
                output,
                "Invalid continuation for previous type-6 pointer write.");
            p++;
            return;
        }

        if (selector == 0x00000000 && (line.Right & 0xFFFFFF00) != 0)
        {
            Invalid(line, output,
                "Invalid PS2RD type 6: 8-bit pointer write must use value 000000XX.");
            Invalid(second, output,
                "Continuation belongs to invalid previous type-6 code.");
            p++;
            return;
        }

        if (selector == 0x00010000 && (line.Right & 0xFFFF0000) != 0)
        {
            Invalid(line, output,
                "Invalid PS2RD type 6: 16-bit pointer write must use value 0000XXXX.");
            Invalid(second, output,
                "Continuation belongs to invalid previous type-6 code.");
            p++;
            return;
        }

        string size = selector switch
        {
            0x00000000 => "8-bit",
            0x00010000 => "16-bit",
            _ => "32-bit"
        };

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 6 ({size} pointer write), base pointer address {baseAddress:X8}.");
        Valid(
            second,
            output,
            $"Valid continuation: {size}, offset {second.Right:X8}.");
        p++;
    }

    private static void ValidateType7(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 7: {addressError}. Possibly encrypted.");
            return;
        }

        string operation;
        bool is16Bit;

        // 8-bit OR: 000000vv
        if ((line.Right & 0xFFFFFF00) == 0)
        {
            operation = "8-bit OR";
            is16Bit = false;
        }
        // 16-bit OR: 0010vvvv
        else if ((line.Right & 0xFFFF0000) == 0x00100000)
        {
            operation = "16-bit OR";
            is16Bit = true;
        }
        // 8-bit AND: 002000vv
        else if ((line.Right & 0xFFFFFF00) == 0x00200000)
        {
            operation = "8-bit AND";
            is16Bit = false;
        }
        // 16-bit AND: 0030vvvv
        else if ((line.Right & 0xFFFF0000) == 0x00300000)
        {
            operation = "16-bit AND";
            is16Bit = true;
        }
        // 8-bit XOR: 004000vv
        else if ((line.Right & 0xFFFFFF00) == 0x00400000)
        {
            operation = "8-bit XOR";
            is16Bit = false;
        }
        // 16-bit XOR: 0050vvvv
        else if ((line.Right & 0xFFFF0000) == 0x00500000)
        {
            operation = "16-bit XOR";
            is16Bit = true;
        }
        else
        {
            PossibleEncrypted(
                line,
                output,
                "Not valid PS2RD type 7: unknown boolean-operation value layout. Possibly encrypted.");
            return;
        }

        if (is16Bit && (address & 1) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 7: 16-bit target address {address:X8} is not aligned to 2. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 7 ({operation}), address {address:X8}.");
    }

    private static void ValidateType9(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type 9: {addressError}. Possibly encrypted.");
            return;
        }

        if ((address & 3) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type 9: hook address {address:X8} is not aligned to 4. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type 9 hook/mastercode, address {address:X8}, expected instruction {line.Right:X8}.");
    }

    private static void ValidateTypeC(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type C: {addressError}. Possibly encrypted.");
            return;
        }

        if ((address & 3) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type C: 32-bit comparison address {address:X8} is not aligned to 4. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type C (32-bit conditional), address {address:X8}.");
    }

    private static void ValidateTypeD(
        ParsedLine line,
        List<ParsedLine> parsed,
        int p,
        Dictionary<int, CheatValidationLine> output)
    {
        if (!TryTypedAddress(line, out uint address, out string addressError))
        {
            PossibleEncrypted(line, output,
                $"Not valid PS2RD type D: {addressError}. Possibly encrypted.");
            return;
        }

        string r = line.RightText;
        int test = HexNibble(r[2]);

        bool is16Bit = r[3] == '0';                 // nnt0vvvv
        bool is8Bit = r[3] == '1' && r[4] == '0' && r[5] == '0'; // nnt100vv

        if (test < 0 || test > 7 || (!is16Bit && !is8Bit))
        {
            PossibleEncrypted(
                line,
                output,
                "Not valid PS2RD type D: conditional bit-field layout is invalid. Possibly encrypted.");
            return;
        }

        if (is16Bit && (address & 1) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type D: 16-bit comparison address {address:X8} is not aligned to 2. Possibly encrypted.");
            return;
        }

        int count = Convert.ToInt32(r[..2], 16);
        if (count == 0)
            count = 1; // documented compatibility behavior

        int remaining = parsed.Count - p - 1;
        string warning = remaining < count
            ? $" Warning: conditional requests {count} following line(s), but only {remaining} raw line(s) remain in this cheat."
            : string.Empty;

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type D ({(is16Bit ? "16-bit" : "8-bit")} conditional), address {address:X8}, controls next {count} line(s).{warning}");
    }

    private static void ValidateTypeE(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output)
    {
        // Deprecated form:
        // E0nnvvvv taaaaaaa  (16-bit)
        // E1nn00vv taaaaaaa  (8-bit)
        string l = line.LeftText;
        string r = line.RightText;

        bool is16Bit = l[1] == '0';
        bool is8Bit = l[1] == '1' && l[4] == '0' && l[5] == '0';
        int test = HexNibble(r[0]);

        // After the test-condition nibble, the address still only has 25 bits,
        // so bits 27..25 of the remaining address field must be zero.
        bool addressFieldValid = (line.Right & ReservedAddressBits) == 0;
        uint address = line.Right & AddressMask25;

        if (test < 0 || test > 7 || (!is16Bit && !is8Bit) || !addressFieldValid)
        {
            PossibleEncrypted(
                line,
                output,
                "Not valid PS2RD type E: deprecated conditional bit-field/address layout is invalid. Possibly encrypted.");
            return;
        }

        if (is16Bit && (address & 1) != 0)
        {
            PossibleEncrypted(
                line,
                output,
                $"Not valid PS2RD type E: 16-bit comparison address {address:X8} is not aligned to 2. Possibly encrypted.");
            return;
        }

        Valid(
            line,
            output,
            $"Valid PS2RD RAW: type E deprecated {(is16Bit ? "16-bit" : "8-bit")} conditional, address {address:X8}.");
    }

    private static bool TryGetNext(
        List<ParsedLine> parsed,
        int currentIndex,
        out ParsedLine next)
    {
        if (currentIndex + 1 < parsed.Count)
        {
            next = parsed[currentIndex + 1];
            return true;
        }

        next = null!;
        return false;
    }

    private static int HexNibble(char c)
    {
        if (c is >= '0' and <= '9')
            return c - '0';

        if (c is >= 'A' and <= 'F')
            return c - 'A' + 10;

        return -1;
    }

    private static void Valid(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output,
        string message)
    {
        output[line.SourceIndex] = new CheatValidationLine(
            line.LineNumber,
            line.Original,
            line.Normalized,
            true,
            message);
    }

    private static void Invalid(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output,
        string message)
    {
        output[line.SourceIndex] = new CheatValidationLine(
            line.LineNumber,
            line.Original,
            line.Normalized,
            false,
            message);
    }

    private static void PossibleEncrypted(
        ParsedLine line,
        Dictionary<int, CheatValidationLine> output,
        string message)
    {
        output[line.SourceIndex] = new CheatValidationLine(
            line.LineNumber,
            line.Original,
            line.Normalized,
            false,
            message);
    }

    public static bool TryNormalizeCodeLine(string line, out string normalized)
    {
        Match m = HexPair.Match(line);

        if (m.Success)
        {
            normalized =
                $"{m.Groups[1].Value.ToUpperInvariant()} {m.Groups[2].Value.ToUpperInvariant()}";
            return true;
        }

        normalized = string.Empty;
        return false;
    }

    public static string BuildCht(string masterCode, IEnumerable<CheatEntry> cheats)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Master Code");
        sb.AppendLine(masterCode.Trim().ToUpperInvariant());

        foreach (CheatEntry cheat in cheats)
        {
            string name = string.IsNullOrWhiteSpace(cheat.Name)
                ? "Unnamed Cheat"
                : cheat.Name.Trim();

            List<CheatValidationLine> valid = Validate(cheat.Codes)
                .Where(x => x.IsValid && x.Normalized is not null)
                .ToList();

            if (valid.Count == 0)
                continue;

            sb.AppendLine(name);

            foreach (CheatValidationLine line in valid)
                sb.AppendLine(line.Normalized!);
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    public static List<CheatEntry> ParseCht(string text, out string? masterCode)
    {
        masterCode = null;
        var cheats = new List<CheatEntry>();
        CheatEntry? current = null;
        bool expectMaster = false;

        foreach (string raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            string line = raw.Trim();

            if (line.Length == 0)
                continue;

            if (TryNormalizeCodeLine(line, out string normalized))
            {
                if (expectMaster && normalized.StartsWith('9'))
                {
                    masterCode = normalized;
                    expectMaster = false;
                    continue;
                }

                current ??= new CheatEntry { Name = "Imported Cheat" };

                if (!cheats.Contains(current))
                    cheats.Add(current);

                current.Codes +=
                    (current.Codes.Length == 0 ? "" : Environment.NewLine) +
                    normalized;
            }
            else
            {
                if (line.Equals("Master Code", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("Mastercode", StringComparison.OrdinalIgnoreCase))
                {
                    expectMaster = true;
                    current = null;
                    continue;
                }

                // C++-style comments are descriptions belonging to the current cheat.
                if (line.StartsWith("//", StringComparison.Ordinal))
                {
                    if (current is not null)
                    {
                        string description = line[2..].Trim();

                        if (description.Length > 0)
                        {
                            current.Description +=
                                (current.Description.Length == 0 ? "" : Environment.NewLine) +
                                description;
                        }
                    }

                    continue;
                }

                // Script-style comments are valid PS2RD comments too.
                if (line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                expectMaster = false;
                current = new CheatEntry { Name = line };
                cheats.Add(current);
            }
        }

        return cheats;
    }
}
