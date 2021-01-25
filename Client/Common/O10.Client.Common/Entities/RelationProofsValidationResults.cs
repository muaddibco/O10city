using System.Collections.Generic;

namespace O10.Client.Common.Entities
{
    public class RelationProofsValidationResults
    {
        public RelationProofsValidationResults()
        {
            ValidationResults = new List<RelationProofValidationResult>();
        }

        public string ImageContent { get; set; }

        public bool IsImageCorrect { get; set; }

		public bool IsEligibilityCorrect { get; set; }

		public bool IsKnowledgeFactorCorrect { get; set; }

		public List<RelationProofValidationResult> ValidationResults { get; }
    }

    public class RelationProofValidationResult
    {
        public string RelatedAttributeOwner { get; set; }
        public string RelatedAttributeContent { get; set; }

        public bool IsRelationCorrect { get; set; }
    }
}
