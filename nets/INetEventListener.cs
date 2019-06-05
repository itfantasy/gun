using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.nets
{
    public interface INetEventListener
    {
        void OnConn();
	    void OnMsg(byte[] msg);
	    void OnClose(error reason);
	    void OnError(error err);
    }
}
