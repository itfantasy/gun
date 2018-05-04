using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.gnbuffers
{
    public class GnTypes
    {
        public const byte Byte = (byte)'b';
        public const byte Short = (byte)'t';
        public const byte Int = (byte)'i';
        public const byte Long = (byte)'l';
        public const byte String = (byte)'s';
        public const byte Float = (byte)'f';
        public const byte Ints = (byte)'I';
        public const byte Array = (byte)'A';
        public const byte Hash = (byte)'H';
        public const byte Bool = (byte)'B';
        public const byte Native = (byte)'#';

        public static bool ExtendCustomType(Type type, byte bSign, SerializeFunc serializeFunc, DeserializeFunc deserializeFunc)
        {
            if (!CheckNativeTypes(bSign))
            {
                return false;
            }
            if (!GnBuffer.ExtendCustomType(type, bSign, serializeFunc))
            {
                return false;
            }
            if (!GnParser.ExtendCustomType(type, bSign, deserializeFunc))
            {
                return false;
            }
            return true;
        }

        public static bool GnExtendCustomType(Type type, byte bSign, GnSerializeFunc serializeFunc, GnDeserializeFunc deserializeFunc)
        {
            if (!CheckNativeTypes(bSign))
            {
                return false;
            }
            if (!GnBuffer.ExtendCustomType(type, bSign, serializeFunc))
            {
                return false;
            }
            if (!GnParser.ExtendCustomType(type, bSign, deserializeFunc))
            {
                return false;
            }
            return true;
        }

        private static bool CheckNativeTypes(byte bSign)
        {
            if (bSign == Byte ||
                bSign == Short ||
                bSign == Int ||
                bSign == Long ||
                bSign == String ||
                bSign == Float ||
                bSign == Ints ||
                bSign == Array ||
                bSign == Hash ||
                bSign == Native
                )
            {
                return false;
            }
            return true;
        }
    }

    public delegate byte[] SerializeFunc(object obj);
    public delegate object DeserializeFunc(byte[] data);

    public delegate void GnSerializeFunc(GnBuffer buffer, object obj);
    public delegate object GnDeserializeFunc(GnParser parser);

    public class CustomType
    {
        public Type type { get; set; }
        public byte bSign { get; set; }
        public SerializeFunc serializeFunc { get; set; }
        public DeserializeFunc deserializeFunc { get; set; }
        public GnSerializeFunc gnSerializeFunc { get; set; }
        public GnDeserializeFunc gnDeserializeFunc { get; set; }

        public CustomType(Type type, byte bSign, SerializeFunc serializeFunc, DeserializeFunc deserializeFunc)
        {
            this.type = type;
            this.bSign = bSign;
            this.serializeFunc = serializeFunc;
            this.deserializeFunc = deserializeFunc;
        }

        public CustomType(Type type, byte bSign, GnSerializeFunc serializeFunc, GnDeserializeFunc deserializeFunc)
        {
            this.type = type;
            this.bSign = bSign;
            this.gnSerializeFunc = serializeFunc;
            this.gnDeserializeFunc = deserializeFunc;
        }
    }
}
