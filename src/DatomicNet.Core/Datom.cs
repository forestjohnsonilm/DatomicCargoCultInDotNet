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
        public readonly uint Type;
        public readonly uint AggregateId;
        public readonly ulong Identity;
        public readonly ushort Parameter;
        public readonly ulong TransactionId;
        public readonly DatomAction Action;
        public readonly byte[] Value;

        public Datom(
                uint type,
                uint aggregateId,
                ulong identity,
                ushort parameter,
                ulong transactionId,
                DatomAction action,
                byte[] value
            )
        {
            Type = type;
            AggregateId = aggregateId;
            Identity = identity;
            Parameter = parameter;
            Value = value;
            TransactionId = transactionId;
            Action = action;
        }
    }

    public enum DatomAction
    {
        Unknown = 0,
        Assertion = 1,
        Retraction = 2
    }
}

