namespace O10.Network.Communication
{
    public class OperationStatus<T>
    {
        public bool Succeeded { get; set; }
        public string Description { get; set; }
        public T Tag { get; set; }
    }
}
