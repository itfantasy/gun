using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using itfantasy.gun.nets;
#if UNITY_EDITOR
using itfantasy.gun.nets.ws;
#endif
using itfantasy.gun.nets.kcp;
using itfantasy.gun.nets.tcp;
using itfantasy.gun.core.binbuf;

namespace itfantasy.gun
{
    public class test : INetEventListener
    {
        INetWorker netWorker;

        public void Run()
        {
            netWorker = new KcpNetWorker();
            netWorker.BindEventListener(this);
            netWorker.Connect("kcp://192.168.99.100:8802");
            while (true)
            {
                netWorker.Update();
                Thread.Sleep(10);
            }
        }

        public void OnConn()
        {
            Console.WriteLine("conn succeed!");
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    BinBuffer buf = new BinBuffer(4096);
                    buf.PushInt(666);
                    buf.PushString("almighty brother gang!!");
                    netWorker.Send(buf.Bytes());
                    Thread.Sleep(10);
                }
            })).Start();

        }

        public void OnMsg(byte[] msg)
        {
            Console.WriteLine("receive from server!");
            Console.WriteLine(msg.Length);
            BinParser par = new BinParser(msg, 0);
            Console.WriteLine(par.Int());
            Console.WriteLine(par.String());
        }

        public void OnClose()
        {

        }

        public void OnError(itfantasy.gun.error err)
        {
            Console.WriteLine(err.Error());
        }
    }

}
