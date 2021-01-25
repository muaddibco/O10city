namespace O10.Node.DataLayer.DataServices.Keys
{
	public class BlockTypeKey : IDataKey
    {
        public BlockTypeKey(ushort blockType)
        {
            BlockType = blockType;
        }

        public ushort BlockType { get; set; }
    }

    public class SingleByBlockTypeKey : BlockTypeKey
    {
        public SingleByBlockTypeKey(ushort blockType) : base(blockType)
        {
        }
    }

    public class SingleByBlockTypeAndHeight : BlockTypeKey
    {
        public SingleByBlockTypeAndHeight(ushort blockType, ulong height)
            : base(blockType)
        {
            Height = height;
        }

        public ulong Height { get; }
    }

    public class BlockTypeLowHeightKey : BlockTypeKey
    {
        public BlockTypeLowHeightKey(ushort blockType, ulong height)
            : base(blockType)
        {
            Height = height;
        }
        public ulong Height { get; set; }
    }
}
