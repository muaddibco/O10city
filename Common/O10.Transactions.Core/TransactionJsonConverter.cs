using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Transactions.Core
{
    public class TransactionJsonConverter : JsonConverter
    {
        private readonly LedgerType _ledgerType;

        public TransactionJsonConverter(LedgerType ledgerType)
        {
            _ledgerType = ledgerType;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TransactionBase).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return null;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
