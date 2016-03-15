using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DatomicNet.Core
{
   
    public class Datom
    {
        public ushort AggregateType { get; }
        public ulong AggregateIdentity { get; }
        public ushort Type { get; }
        public ulong Identity { get; }
        public ushort Parameter { get; }
        public uint ParameterArrayIndex { get; }
        public ulong TransactionId { get; }
        public DatomAction Action { get; }
        public byte[] Value { get; }

        public Datom(
                ushort aggregateTypeId,
                ulong aggregateId,
                ushort type,
                ulong identity,
                ushort parameter,
                uint parameterIndex,
                byte[] value,
                ulong transactionId,
                DatomAction action
            )
        {
            Type = type;
            AggregateType = aggregateTypeId;
            AggregateIdentity = aggregateId;
            Identity = identity;
            Parameter = parameter;
            ParameterArrayIndex = parameterIndex;
            Value = value;
            TransactionId = transactionId;
            Action = action;
        }

        public Datom(
                ushort type,
                ulong identity,
                ushort parameter,
                uint parameterIndex,
                byte[] value,
                ulong transactionId,
                DatomAction action
            ) : this((ushort)0, (ulong)0, type, identity, parameter, parameterIndex, value, transactionId, action)
        { }

        public Datom(
                ushort type,
                ulong identity,
                ushort parameter,
                byte[] value,
                ulong transactionId,
                DatomAction action
            ) : this((ushort)0, (ulong)0, type, identity, parameter, 0, value, transactionId, action)
        { }
    }

    public enum DatomAction
    {
        Unknown = 0,
        Assertion = 1,
        Retraction = 2
    }
}

