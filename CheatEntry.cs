namespace PS2RDCHTManager;

public sealed class CheatEntry
{
    public string Name { get; set; } = "New Cheat";
    public string Codes { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMasterCode { get; set; }
    public bool IsHeader { get; set; }

    public override string ToString() => Name;
}
