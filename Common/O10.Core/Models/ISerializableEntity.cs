namespace O10.Core.Models
{
    public interface ISerializableEntity<T> where T: ISerializableEntity<T>
    {
        byte[] ToByteArray();

        string ToString();
    }
}
