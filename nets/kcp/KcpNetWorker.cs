﻿
#define KCP_v2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

#if KCP_v1
using Kcp = KcpProject.v1;
#else // KCP_v2
using Kcp = KcpProject.v2;
#endif

using itfantasy.lmjson;

namespace itfantasy.gun.nets.kcp
{
    public class KcpNetWorker : INetWorker
    {
        Kcp.UdpSocket kcpsocket;
        INetEventListener eventListener;
        AutoPing ping;

        Queue<byte[]> msgQueue = new Queue<byte[]>();

        public KcpNetWorker()
        {
            this.msgQueue = new Queue<byte[]>();
        }

        public bool Update()
        {
            if (Connected)
            {
                this.kcpsocket.Update();
                if (this.ping != null)
                {
                    this.ping.Update();
                }
            }
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

        public void Connect(string url)
        {
            this.Dispose();

            string urlInfo = url.TrimStart(("kcp://").ToCharArray());
            string[] infos = urlInfo.Split(':');

#if KCP_v1
            this.kcpsocket = new KcpProject.v1.UdpSocket((KcpProject.v1.UdpSocket.cliEvent ev, byte[] buf, string err) =>
            {
                switch (ev)
                {
                    case KcpProject.v1.UdpSocket.cliEvent.Connected:
                        this.eventListener.OnConn();
                        break;
                    case KcpProject.v1.UdpSocket.cliEvent.ConnectFailed:
                        this.OnError(errors.New(err));
                        break;
                    case KcpProject.v1.UdpSocket.cliEvent.Disconnect:
                        this.OnError(errors.New(err));
                        break;
                    case KcpProject.v1.UdpSocket.cliEvent.RcvMsg:
                        this.OnReceive(buf);
                        break;
                }
            });
#else // KCP_v2
            this.kcpsocket = new KcpProject.v2.UdpSocket((byte[] buf) =>  {
                this.OnReceive(buf);
            },(err) => {
                this.OnError(errors.New(err));
            });
#endif
            this.kcpsocket.Connect(infos[0], ushort.Parse(infos[1]));
            this.doHandShake("");
#if KCP_v1
#else // KCP_v2
            this.eventListener.OnConn();
#endif
            this.ping = new AutoPing(this, this.eventListener);
        }

        private void OnReceive(byte[] buf)
        {
            this.ping.ResetConnState();
            if (buf[0] == 35)
            {
                string str = System.Text.Encoding.UTF8.GetString(buf);
                if (str == "#pong")
                {
                    return; // nothing to do but ResetConnState for AutoPing
                }
                else if (str == "#ping")
                {
                    SendAsync(System.Text.Encoding.UTF8.GetBytes("#pong")); // return the pong pck to server
                    return;
                }
            }
            lock (this.msgQueue)
            {
                this.msgQueue.Enqueue(buf);
            }
        }

        private void OnError(error err)
        {
            this.eventListener.OnError(err);
            this.eventListener.OnClose(err);
            this.Dispose();
        }

        public error Send(byte[] msg)
        {
            this.kcpsocket.Send(msg);
            return errors.nil;
        }

        public error SendAsync(byte[] msg, Action<bool> callback = null)
        {
            this.kcpsocket.Send(msg);
            if (callback != null)
            {
                callback.Invoke(true);
            }
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
            this.eventListener.OnClose(errors.nil);
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.kcpsocket != null)
            {
                this.kcpsocket.Close();
                this.kcpsocket = null;
                this.msgQueue.Clear();
                this.ping = null;
            }
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
                return this.kcpsocket != null && this.kcpsocket.Connected;
            }
        }
    }

}
