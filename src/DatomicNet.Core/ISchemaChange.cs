using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DatomicNet.Core
{
    public interface ISchemaChange
    {
        long TransactionId { get; }
        IReadOnlyDictionary<Type, uint> RegisterTypes { get; }
        IReadOnlyDictionary<Type, uint> RegisterAggregates { get; }
        IReadOnlyDictionary<uint, Func<IEnumerable<Datom>, IEnumerable<Datom>>> MapEntityStreamForType { get; }
        bool RequiresReIndex { get; }
    }
}