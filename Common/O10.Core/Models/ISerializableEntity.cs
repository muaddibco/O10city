namespace O10.Core.Models
{
    public interface ISerializableEntity
    {
        byte[] ToByteArray();

        string ToString();
    }
}
