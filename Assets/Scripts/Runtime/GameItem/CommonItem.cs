using MemoryPack;
using System;

[MemoryPackable]
[Serializable]
public partial class CommonItem : GameItem
{
    [MemoryPackConstructor]
    protected CommonItem() : base()
    {
    }

    public CommonItem(int id, int count = 1) : base(id, count)
    {
    }
}
