using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.nets
{
    public interface INetWorker
    {
	    void Connect(string url, string tag);
        error Send(byte[] msg);
        error SendAsync(byte[] msg, Action<bool> callback = null);
	    error BindEventListener(INetEventListener eventListener);
        void Close();
        void Update();
    }
}
