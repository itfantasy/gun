using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.gnbuffers
{
    public class GnParser
    {
        byte[] buffer;
        int offset;

        public GnParser(byte[] buffer, int offset)
        {
            this.buffer = buffer;
            this.offset = offset;
        }

        public byte Byte()
        {
            return this.buffer[this.offset++];
        }

        public short Short()
        {
            short ret = BitConverter.ToInt16(this.buffer, this.offset);
            this.offset += 2;
            return ret;
        }

        public int Int()
        {
            int ret = BitConverter.ToInt32(this.buffer, this.offset);
            this.offset += 4;
            return ret;
        }

        public long Long()
        {
            long ret = BitConverter.ToInt64(this.buffer, this.offset);
            this.offset += 8;
            return ret;
        }

        public string String()
        {
            int length = this.Int();
            string ret = System.Text.Encoding.UTF8.GetString(this.buffer, this.offset, length);
            this.offset += length;
            return ret;
        }

        public float Float()
        {
            float ret = BitConverter.ToSingle(this.buffer, this.offset);
            this.offset += 4;
            return ret;
        }

        public int[] Ints()
        {
            int length = this.Int();
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.Int();
            }
            return array;
        }

        public object[] Array()
        {
            int length = this.Int();
            object[] array = new object[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.Object();
            }
            return array;
        }

        public Dictionary<object, object> Hash()
        {
            int length = this.Int();
            Dictionary<object, object> hash = new Dictionary<object, object>();
            for (int i = 0; i < length; i++)
            {
                object k = this.Object();
                object v = this.Object();
                hash[k] = v;
            }
            return hash;
        }

        public object Native()
        {
            int length = this.Int();
            byte[] datas = new byte[length];
            Buffer.BlockCopy(this.buffer, this.offset, datas, 0, length);
            this.offset += length;
            return NativeFormatter.DeserializeWithBinary(datas);
        }

        public object Object()
        {
            byte c = this.Byte();
            switch (c)
            {
                case GnTypes.Byte:
                    return this.Byte();
                case GnTypes.Short:
                    return this.Short();
                case GnTypes.Int:
                    return this.Int();
                case GnTypes.Long:
                    return this.Long();
                case GnTypes.String:
                    return this.String();
                case GnTypes.Float:
                    return this.Float();
                case GnTypes.Ints:
                    return this.Ints();
                case GnTypes.Array:
                    return this.Array();
                case GnTypes.Hash:
                    return this.Hash();
                case GnTypes.Native:
                    return this.Native();
                default:
                    if (customTypeExtends.ContainsKey(c))
                    {
                        CustomType custom = customTypeExtends[c];
                        if (custom.gnDeserializeFunc != null)
                        {
                            return custom.gnDeserializeFunc(this);
                        }
                        else
                        {
                            int length = this.Int();
                            object ret = customTypeExtends[c].deserializeFunc(this.buffer);
                            this.offset += length;
                            return ret;
                        }
                    }    
                    return null;
            }
        }

        public bool OverFlow()
        {
            return this.offset >= this.buffer.Length;   
        }

        private static Dictionary<byte, CustomType> customTypeExtends = new Dictionary<byte, CustomType>();

        public static bool ExtendCustomType(Type type, byte bSign, DeserializeFunc func)
        {
            customTypeExtends[bSign] = new CustomType(type, bSign, null, func);
            return true;
        }

        public static bool ExtendCustomType(Type type, byte bSign, GnDeserializeFunc func)
        {
            customTypeExtends[bSign] = new CustomType(type, bSign, null, func);
            return true;
        }
    }
}
