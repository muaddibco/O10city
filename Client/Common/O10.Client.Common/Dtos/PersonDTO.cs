using System;
using System.Collections.Generic;

namespace O10.Client.Common.Dtos
{
    public class PersonDTO
    {
        public Guid PersonId { get; set; }
        public string UserData { get; set; }
        public string Name { get; set; }

        public List<Guid> PersistedFaceIds { get; set; }
    }
}
