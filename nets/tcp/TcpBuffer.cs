using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.nets.tcp
{
    class TcpBuffer
    {
        public const int PCK_MIN_SIZE  = 6; // |--- header 4bytes ---|--- length 2 bytes ---|--- other datas --- ....
        public const int PCK_HEADER = 0x2123676f; // !#go

        int _count;
        int _offset;
        byte[] _buffer;

        public TcpBuffer(byte[] buf)
	    {
            _buffer = buf;
            _offset = 0;
            _count = 0;
	    }

	    public void Clear()
	    {
            _offset = 0;
            _count = 0;
            _buffer.Initialize();
	    }

	    public void Reset()
	    {
            Buffer.BlockCopy(_buffer, _offset, _buffer, 0, _count);
            _offset = 0;
	    }

        public byte[] buf
	    {
            get
            {
                return _buffer;
            }
	    }

        public int count
        {
            get
            {
                return _count;
            }
        }

        public int offet
        {
            get
            {
                return _offset;
            }
        }

        public int capcity
        {
            get
            {
                return _buffer.Length - _count;
            }
        }

	    public bool AddData(byte[] buffer, int count)
	    {
            if (_offset + count > _buffer.Length)
		    {
			    return false;
		    }
            Buffer.BlockCopy(buffer, 0, _buffer, _offset, count);
            _count += count;
		    return true;
	    }

        public void AddDataLen(int count)
        {
            _count += count;
        }

        public void DeleteData(int count)
	    {
            if (_count >= count)
            {
                _offset += count;
                _count -= count;
            }
	    }
        
    }
}
