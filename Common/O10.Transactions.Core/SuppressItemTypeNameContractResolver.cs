using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using O10.Crypto.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Transactions.Core
{
    public class SuppressItemTypeNameContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            var containerContract = contract as JsonContainerContract;
            if (containerContract != null)
            {
                if (containerContract.ItemTypeNameHandling == null)
                    containerContract.ItemTypeNameHandling = typeof(TransactionBase).IsAssignableFrom(objectType) ? TypeNameHandling.All : TypeNameHandling.None;
            }
            return contract;
        }
    }
}
