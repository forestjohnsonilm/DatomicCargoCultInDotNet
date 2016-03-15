using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DatomicNet.Core
{
    public interface ISchemaChange
    {
        long TransactionId { get; }
        IReadOnlyDictionary<Type, ushort> RegisterTypes { get; }
        IReadOnlyDictionary<Type, ushort> RegisterAggregates { get; }
        IReadOnlyDictionary<ushort, Func<IEnumerable<Datom>, IEnumerable<Datom>>> MapEntityStreamForType { get; }
        bool RequiresReIndex { get; }
    }
}