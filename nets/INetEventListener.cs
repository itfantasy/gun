using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.nets
{
    public interface INetEventListener
    {
        void OnConn();                       // 获得新链接时
	    void OnMsg(byte[] msg);            // 有新消息时
	    void OnClose();                      // 链接断开时
	    void OnError(error err);           // 链接异常时
    }
}
