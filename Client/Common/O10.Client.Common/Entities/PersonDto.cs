using System;
using System.Collections.Generic;

namespace O10.Client.Common.Entities
{
    public class PersonDto
    {
        public Guid PersonId { get; set; }
        public string UserData { get; set; }
        public string Name { get; set; }

        public List<Guid> PersistedFaceIds { get; set; }
    }
}
