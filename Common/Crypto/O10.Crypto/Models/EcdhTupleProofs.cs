﻿namespace O10.Crypto.Models
{
    public class EcdhTupleProofs
    {
        public byte[] Mask { get; set; }
        public byte[] AssetId { get; set; }
        public byte[] AssetIssuer { get; set; }
        public byte[] Payload { get; set; }
    }
}
