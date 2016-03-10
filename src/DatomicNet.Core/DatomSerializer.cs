using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace DatomicNet.Core
{
    public interface IDatomSerializationStrategy
    {
        Func<T, IEnumerable<Datom>> GetSerializer<T>(Assembly assembly, Type type);
        Func<IEnumerable<Datom>, T> GetDeserializer<T>(Assembly assembly, Type type);
    }

    public static class DatomSerializationProvider<T>
    {
        private static Func<T, IEnumerable<Datom>> _serializer;
        private static Func<IEnumerable<Datom>, T> _deserializer;

        public static T Deserialize(TypeRegistry typeRegistry, IEnumerable<Datom> datoms)
        {
            if (_deserializer == null)
            {
                //_deserializer = 
            }
            if (_deserializer != null)
            {
                return _deserializer(datoms);
            }
            throw new InvalidOperationException($"No deserializer found for {typeof(T).FullName}");
        }

        public static IEnumerable<Datom> Serialize(TypeRegistry typeRegistry, T entity)
        {
            if (_serializer == null)
            {
                ///return _deserializer(datoms);
            }
            if (_serializer != null)
            {
                return _serializer(entity);
            }
            throw new InvalidOperationException($"No serializer found for {typeof(T).FullName}");
        }
    }

    public class DatomSerializer
    {
        public DatomSerializer(
                TypeRegistry typeRegistry,
                IDatomSerializationStrategy serializationStrategy
            )
        {
            _typeRegistry = typeRegistry;
        }

        private TypeRegistry _typeRegistry;

        private Dictionary<Type, IEnumerable<Func<object, IEnumerable<Datom>>>> _serializerCache;

        public Dictionary<Type, Func<IEnumerable<Datom>, object>> _deserializerCache;


    }
}
