using System;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParser
    {
        LedgerType LedgerType { get; }

        ushort BlockType { get; }

        PacketBase Parse(Memory<byte> source);
    }
}
