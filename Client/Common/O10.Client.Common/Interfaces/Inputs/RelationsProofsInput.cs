namespace O10.Client.Common.Interfaces.Inputs
{
    public class RelationsProofsInput : RequestInput
    {
        public byte[] ImageHash { get; set; }

        public Relation[] Relations { get; set; }
    }
}
