using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using itfantasy.gun.nets;
using itfantasy.lmjson;

namespace itfantasy.gun.nets.tcp
{
    public class TcpNetWorker : INetWorker
    {
        Socket tcpsocket;
        byte[] rcvbuf;

        INetEventListener eventListener;

        Queue<byte[]> msgQueue = new Queue<byte[]>();


        public void Connect(string url, string tag)
        {
            string urlInfo = url.TrimStart(("tcp://").ToCharArray());
            string[] infos = urlInfo.Split(':');

            tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpsocket.BeginConnect(infos[0], int.Parse(infos[1]), (ar) => {
                this.tcpsocket.EndConnect(ar);
                this.doHandShake("localhost");
                this.eventListener.OnConn();
                this.rcvbuf = new byte[4096];
                this.tcpsocket.BeginReceive(rcvbuf, 0, rcvbuf.Length, 0, (ar2) => {
                    int n = this.tcpsocket.EndReceive(ar2);
                    byte[] tmpbuf = new byte[n];
                    Buffer.BlockCopy(rcvbuf, 0, tmpbuf, 0, n);
                    lock (this.msgQueue)
                    {
                        this.msgQueue.Enqueue(tmpbuf);
                    }
                }, null);
            }, null);
        }

        public bool Update()
        {
            if (this.msgQueue.Count > 0)
            {
                byte[] e;
                lock (this.msgQueue)
                {
                    e = this.msgQueue.Dequeue();
                }
                this.eventListener.OnMsg(e);
                return true;
            }
            return false;
        }

        public error Send(byte[] msg)
        {
            int n = this.tcpsocket.Send(msg);
            if (n <= 0)
            {
                return errors.New("there's no datas have been sended!");
            }
            return errors.nil;
        }

        public error SendAsync(byte[] msg, Action<bool> callback = null)
        {
            this.tcpsocket.BeginSend(msg, 0, msg.Length, 0, (ar) => {
                if (callback != null)
                {
                    int n = this.tcpsocket.EndSend(ar);
                    callback.Invoke(n > 0);
                }
            }, null);
            return errors.nil;
        }

        public error BindEventListener(INetEventListener eventListener)
        {
            if (this.eventListener == null)
            {
                this.eventListener = eventListener;
                return errors.nil;
            }
            return errors.New("this net worker has binded an event listener!!");
        }

        public void Close()
        {
            this.tcpsocket.Close();
            this.tcpsocket = null;
            this.rcvbuf = null;
            this.msgQueue.Clear();
        }

        private void doHandShake(string origin)
        {
            JsonData jd = new JsonData();
            jd["Origin"] = origin;
            string json = JSON.Stringify(jd);
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(json);
            Send(buf);
        }

        public bool Connected
        {
            get
            {
                return tcpsocket != null && tcpsocket.Connected;
            }
        }
    }
}
