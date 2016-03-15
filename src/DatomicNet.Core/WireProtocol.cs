//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;

//namespace DatomicNet.Core
//{
//    public enum DatomSelectFlags
//    {
//        Type = 1 << 1,
//        Identity = 1 << 2,
//        Parameter = 1 << 3,
//        TransactionId = 1 << 4,
//        Action = 1 << 5,
//        Value = 1 << 6,
//        Header = Type | Identity | Parameter | TransactionId | Action,
//        All = Header | Value
//    }

//    public static class IEnumerableExtensions {
//        public static IEnumerable<Datom> ToDatoms(this Stream stream)
//        {
//            var flagBuffer = new byte[2];
//            stream.Read(flagBuffer, 0, 2);
//            var flags = (DatomSelectFlags)Convert.ToUInt16(flagBuffer);

//            var selectType = (flags & DatomSelectFlags.Type) > 0;
//            var selectIdentity = (flags & DatomSelectFlags.Identity) > 0;
//            var selectParameter = (flags & DatomSelectFlags.Parameter) > 0;
//            var selectTransactionId = (flags & DatomSelectFlags.TransactionId) > 0;
//            var selectAction = (flags & DatomSelectFlags.Action) > 0;
//            var selectValue = (flags & DatomSelectFlags.Value) > 0;

//            while(stream.CanRead)
//            {
//                var Type = selectType ? ReadUInt(stream) : 0;
//                var Identity = selectIdentity ? ReadULong(stream) : 0;
//                var Parameter = selectParameter ? ReadUShort(stream) : (ushort)0;
//                var TransactionId = selectTransactionId ? ReadULong(stream) : 0;
//                var Action = selectAction ? (DatomAction)ReadUShort(stream) : DatomAction.Unknown;
//                var ValueLength = selectValue ? ReadInt(stream) : 0;
//                var Value = new byte[0];

//                if(selectValue)
//                {
//                    Value = new byte[ValueLength];
//                    stream.Read(Value, (int)stream.Position, ValueLength);
//                }

//                yield return new Datom(
//                        Type,
//                        Identity,
//                        Parameter,
//                        TransactionId,
//                        Action,
//                        Value
//                    );
//            }
//        }

//        public static void WriteToStream (this IEnumerable<Datom> datoms, DatomSelectFlags flags, Stream stream)
//        {
//            stream.Write(BitConverter.GetBytes((ushort)flags), 0, 2);

//            var selectType = (flags & DatomSelectFlags.Type) > 0;
//            var selectIdentity = (flags & DatomSelectFlags.Identity) > 0;
//            var selectParameter = (flags & DatomSelectFlags.Parameter) > 0;
//            var selectTransactionId = (flags & DatomSelectFlags.TransactionId) > 0;
//            var selectAction = (flags & DatomSelectFlags.Action) > 0;
//            var selectValue = (flags & DatomSelectFlags.Value) > 0;

//            var enumerator = datoms.GetEnumerator();
//            while (enumerator.MoveNext())
//            {
//                if (selectType) stream.Write(BitConverter.GetBytes(enumerator.Current.Type), (int)stream.Position, 4);
//                if (selectIdentity) stream.Write(BitConverter.GetBytes(enumerator.Current.Identity), (int)stream.Position, 8);
//                if (selectParameter) stream.Write(BitConverter.GetBytes(enumerator.Current.Parameter), (int)stream.Position, 2);
//                if (selectTransactionId) stream.Write(BitConverter.GetBytes(enumerator.Current.TransactionId), (int)stream.Position, 8);
//                if (selectAction) stream.Write(BitConverter.GetBytes((ushort)enumerator.Current.Action), (int)stream.Position, 2);
//                if (selectValue)
//                {
//                    stream.Write(BitConverter.GetBytes(enumerator.Current.Value.Length), (int)stream.Position, 4);
//                    stream.Write(enumerator.Current.Value, (int)stream.Position, enumerator.Current.Value.Length);
//                }
//            }
//        }


//        private static Guid ReadGuid(Stream stream)
//        {
//            var bytes = new byte[16];
//            stream.Read(bytes, (int)stream.Position, 16);
//            return new Guid(bytes);
//        }

//        private static ulong ReadULong(Stream stream)
//        {
//            var bytes = new byte[8];
//            stream.Read(bytes, (int)stream.Position, 8);
//            return Convert.ToUInt64(bytes);
//        }

//        private static long ReadLong(Stream stream)
//        {
//            var bytes = new byte[8];
//            stream.Read(bytes, (int)stream.Position, 8);
//            return Convert.ToInt64(bytes);
//        }

//        private static int ReadInt(Stream stream)
//        {
//            var bytes = new byte[4];
//            stream.Read(bytes, (int)stream.Position, 4);
//            return Convert.ToInt32(bytes);
//        }

//        private static uint ReadUInt(Stream stream)
//        {
//            var bytes = new byte[4];
//            stream.Read(bytes, (int)stream.Position, 4);
//            return Convert.ToUInt32(bytes);
//        }

//        private static short ReadShort(Stream stream)
//        {
//            var bytes = new byte[2];
//            stream.Read(bytes, (int)stream.Position, 2);
//            return Convert.ToInt16(bytes);
//        }

//        private static ushort ReadUShort(Stream stream)
//        {
//            var bytes = new byte[2];
//            stream.Read(bytes, (int)stream.Position, 2);
//            return Convert.ToUInt16(bytes);
//        }
//    }
//}

