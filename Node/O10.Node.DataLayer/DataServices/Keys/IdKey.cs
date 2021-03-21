using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Node.DataLayer.DataServices.Keys
{
    public class IdKey : IDataKey
    {
        public long Id { get; }

        public IdKey(long id)
        {
            Id = id;
        }
    }
}
