﻿using O10.Core.Identity;

namespace O10.Gateway.DataLayer.Services.Inputs
{
    public class StealthStoreInput : IncomingStoreInput
    {
        public IKey? KeyImage { get; set; }

		public IKey? DestinationKey2 { get; set; }
	}
}
