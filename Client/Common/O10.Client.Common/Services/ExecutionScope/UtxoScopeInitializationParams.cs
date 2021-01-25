using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Client.Common.Services
{
    public class UtxoScopeInitializationParams : ScopeInitializationParams
    {
        internal UtxoScopeInitializationParams()
        {

        }

        public byte[] SecretSpendKey { get; set; }
        public byte[] SecretViewKey { get; set; }
        public byte[] PwdSecretKey { get; set; }
    }
}
