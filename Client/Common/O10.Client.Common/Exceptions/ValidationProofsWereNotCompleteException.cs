using Newtonsoft.Json;
using System;
using O10.Client.Common.Properties;
using O10.Core.Serialization;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class ValidationProofsWereNotCompleteException : Exception
    {
        public ValidationProofsWereNotCompleteException() { }
        public ValidationProofsWereNotCompleteException(ValidationCriteriaDTO validationCriteria) : base(string.Format(Resources.ERR_VALIDATION_PROOFS_NOT_COMPLETE, JsonConvert.SerializeObject(validationCriteria, new ByteArrayJsonConverter()))) { }
        public ValidationProofsWereNotCompleteException(string schemeName) : base(string.Format(Resources.ERR_VALIDATION_PROOFS_NOT_COMPLETE, schemeName)) { }
        public ValidationProofsWereNotCompleteException(ValidationCriteriaDTO validationCriteria, Exception inner) : base(string.Format(Resources.ERR_VALIDATION_PROOFS_NOT_COMPLETE, JsonConvert.SerializeObject(validationCriteria, new ByteArrayJsonConverter())), inner) { }
        protected ValidationProofsWereNotCompleteException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
