namespace O10.Client.DataLayer.AttributesScheme
{
    public class AttributeScheme
	{
		public AttributeScheme()
		{
			IsMultiple = false;
		}
		public string Name { get; set; }
		public string Description { get; set; }
		public AttributeValueType ValueType { get; set; }
		public bool IsMultiple { get; set; }
	}
}
