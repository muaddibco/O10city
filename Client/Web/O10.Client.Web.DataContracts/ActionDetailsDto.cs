using System.Collections.Generic;

namespace O10.Client.Web.DataContracts
{
    public class ActionDetailsDto
    {
        public ActionTypeDto ActionType { get; set; }

        /// <summary>
        /// An expression that contains value identifying the item upon which action us executed. E.g., for document sigining, it is concatenation through pipe name of the document, hash of its body and height of the last change
        /// </summary>
        public string ActionItemKey { get; set; }

        public string AccountInfo { get; set; }

        public bool IsRegistered { get; set; }

        public string PublicKey { get; set; }
        public string PublicKey2 { get; set; }

        public string SessionKey { get; set; }

        public bool IsBiometryRequired { get; set; }

        public long PredefinedAttributeId { get; set; }

        public Dictionary<string, ValidationTypeDto>? RequiredValidations { get; set; }

        public Dictionary<string, List<string>>? PermittedRelations { get; set; }

        public HashSet<string>? ExistingRelations { get; set; }
    }
}
