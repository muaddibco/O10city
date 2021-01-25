using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;

namespace O10.Transactions.Core.Parsers.Factories
{
	public abstract class BlockParsersRepositoryBase : IBlockParsersRepository
    {
        protected Dictionary<ushort, IBlockParser> _blockParsers;

        public BlockParsersRepositoryBase(IEnumerable<IBlockParser> blockParsers)
        {
            _blockParsers = new Dictionary<ushort, IBlockParser>();

            foreach (IBlockParser blockParser in blockParsers.Where(bp => bp.PacketType == PacketType))
            {
                if (!_blockParsers.ContainsKey(blockParser.BlockType))
                {
                    _blockParsers.Add(blockParser.BlockType, blockParser);
                }
            }
        }

        public abstract PacketType PacketType { get; }

        public IBlockParser GetInstance(ushort blockType)
        {
            if (!_blockParsers.ContainsKey(blockType))
            {
                throw new BlockTypeNotSupportedException(blockType, PacketType);
            }

            return _blockParsers[blockType];
        }
    }
}
