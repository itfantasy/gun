using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.core.binbuf
{
    public class BinBuffer
    {
        byte[] buffer;
        int offset;

        public BinBuffer(int capacity)
        {
            this.buffer = new byte[capacity];
            this.offset = 0;
        }

        public void PushByte(byte value)
        {
            this.buffer[this.offset++] = value;
        }

        public void PushBool(bool value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushShort(short value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushInt(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushLong(long value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushString(string value)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(value);
            this.PushInt(buffer.Length);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushFloat(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, buffer.Length);
            this.offset += buffer.Length;
        }

        public void PushInts(int[] value)
        {
            this.PushInt(value.Length);
            foreach (int item in value)
            {
                this.PushInt(item);
            }
        }

        public void PushArray(Array value)
        {
            this.PushInt(value.Length);
            foreach(object item in value)
            {
                this.PushObject(item);
            }
        }

        public void PushHash(Dictionary<object, object> value)
        {
            this.PushInt(value.Count);
            foreach (KeyValuePair<object, object> kv in value)
            {
                this.PushObject(kv.Key);
                this.PushObject(kv.Value);
            }
        }

        public void PushHash(Dictionary<int, int> value)
        {
            this.PushInt(value.Count);
            foreach (KeyValuePair<int, int> kv in value)
            {
                this.PushObject(kv.Key);
                this.PushObject(kv.Value);
            }
        }

        public void PushNative(object value)
        {
            byte[] datas = NativeFormatter.SerializeToBinary(value);
            this.PushInt(datas.Length);
            Buffer.BlockCopy(buffer, 0, this.buffer, this.offset, datas.Length);
            this.offset += datas.Length;
        }

        public void PushObject(object value)
        {

            if(value == null)
            {
                this.PushByte(Types.Null);
                this.PushByte((byte)0);
                return;
            }
            Type type = value.GetType();
            if (type == typeof(byte))
            {
                this.PushByte(Types.Byte);
                this.PushByte((byte)value);
            }
            else if (type == typeof(bool))
            {
                this.PushByte(Types.Bool);
                this.PushBool((bool)value);
            }
            else if (type == typeof(short))
            {
                this.PushByte(Types.Short);
                this.PushShort((short)value);
            }
            else if (type == typeof(int))
            {
                this.PushByte(Types.Int);
                this.PushInt((int)value);
            }
            else if (type == typeof(long))
            {
                this.PushByte(Types.Long);
                this.PushLong((long)value);
            }
            else if (type == typeof(string))
            {
                this.PushByte(Types.String);
                this.PushString(value.ToString());
            }
            else if (type == typeof(float))
            {
                this.PushByte(Types.Float);
                this.PushFloat((float)value);
            }
            else if (value is Array)
            {
                if (type == typeof(int[]))
                {
                    this.PushByte(Types.Ints);
                    this.PushInts(value as int[]);
                }
                else
                {
                    this.PushByte(Types.Array);
                    this.PushArray(value as Array);
                }
            }
            else if (value is Dictionary<object, object>)
            {
                this.PushByte(Types.Hash);
                this.PushHash(value as Dictionary<object, object>);
            }
            else if (value is Dictionary<int, int>)
            {
                this.PushByte(Types.Hash);
                this.PushHash(value as Dictionary<int, int>);
            }
            else
            {
                if (customTypeExtends.ContainsKey(type))
                {
                    CustomType custom = customTypeExtends[type];
                    this.PushByte(custom.bSign);
                    if (custom.binSerializeFunc != null)
                    {
                        custom.binSerializeFunc(this, value);
                    }
                    else
                    {
                        byte[] datas = custom.serializeFunc(value);
                        this.PushInt(datas.Length);
                        Buffer.BlockCopy(datas, 0, this.buffer, this.offset, datas.Length);
                        this.offset += datas.Length;
                    }
                }
                else
                {
                    this.PushByte(Types.Native);
                    this.PushNative(value);
                }
            }
        }

        public byte[] Bytes()
        {
            byte[] buf = new byte[this.offset];
            Buffer.BlockCopy(this.buffer, 0, buf, 0, offset);
            return buf;
        }

        private static Dictionary<Type, CustomType> customTypeExtends = new Dictionary<Type, CustomType>();

        public static bool ExtendCustomType(Type type, byte bSign, SerializeFunc func)
        {
            customTypeExtends[type] = new CustomType(type, bSign, func, null);
            return true;
        }

        public static bool ExtendCustomType(Type type, byte bSign, BinSerializeFunc func)
        {
            customTypeExtends[type] = new CustomType(type, bSign, func, null);
            return true;
        }
    }
}
