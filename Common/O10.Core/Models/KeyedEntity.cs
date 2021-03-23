using O10.Core.Identity;

namespace O10.Core.Models
{
    public class KeyedEntity<T> where T : class, ISerializableEntity<T>
    {
        public KeyedEntity(T entity)
        {
            Entity = entity;
        }

        public IKey? Key { get; set; }
        public T? Entity { get; set; }
    }
}
