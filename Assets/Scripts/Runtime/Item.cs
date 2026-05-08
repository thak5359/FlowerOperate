using MemoryPack;

[MemoryPackable]
public partial class Item 
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ItemMainType mainType { get; init; } = ItemMainType.Unknown;
    public ItemSubType subType { get; private set; } = ItemSubType.Unknown;

    public int PileLimit = 999;




}
