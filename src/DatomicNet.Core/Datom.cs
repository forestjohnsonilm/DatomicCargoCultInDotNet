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
        public readonly ushort AggregateType;
        public readonly ulong AggregateIdentity;
        public readonly ushort Type;
        public readonly ulong Identity;
        public readonly ushort Parameter;
        public readonly uint ParameterIndex;
        public readonly ulong TransactionId;
        public readonly DatomAction Action;
        public readonly byte[] Value;

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
            ParameterIndex = parameterIndex;
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

