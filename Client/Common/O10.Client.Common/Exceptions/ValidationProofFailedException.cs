using Newtonsoft.Json;
using System;
using O10.Client.Common.Properties;
using O10.Core.Serialization;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class ValidationProofFailedException : Exception
    {
        public ValidationProofFailedException() { }
        public ValidationProofFailedException(ValidationCriteriaDTO validationCriteria) : base(string.Format(Resources.ERR_VALIDATION_PROOF_FAILED, JsonConvert.SerializeObject(validationCriteria, new ByteArrayJsonConverter()))) { }
        public ValidationProofFailedException(string schemeName) : base(string.Format(Resources.ERR_VALIDATION_PROOF_FAILED, schemeName)) { }
        public ValidationProofFailedException(ValidationCriteriaDTO validationCriteria, Exception inner) : base(string.Format(Resources.ERR_VALIDATION_PROOF_FAILED, JsonConvert.SerializeObject(validationCriteria, new ByteArrayJsonConverter())), inner) { }
        protected ValidationProofFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
