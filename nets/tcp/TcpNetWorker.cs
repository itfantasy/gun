using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using itfantasy.gun.core.binbuf;
using itfantasy.gun.nets;
using itfantasy.lmjson;

namespace itfantasy.gun.nets.tcp
{
    public class TcpNetWorker : INetWorker
    {
        Socket tcpsocket;
        TcpBuffer rcvbuf;

        INetEventListener eventListener;

        Queue<byte[]> msgQueue = new Queue<byte[]>();

        public void Connect(string url)
        {
            string urlInfo = url.TrimStart(("tcp://").ToCharArray());
            string[] infos = urlInfo.Split(':');

            tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpsocket.BeginConnect(infos[0], int.Parse(infos[1]), (ar) => {
                this.tcpsocket.EndConnect(ar);
                this.doHandShake("");
                this.eventListener.OnConn();
                this.rcvbuf = new TcpBuffer(new byte[8192]);
                this.PostReceive();
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

        private void PostReceive()
        {
            this.tcpsocket.BeginReceive(this.rcvbuf.buf, 0, this.rcvbuf.capcity, 0, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int n = this.tcpsocket.EndReceive(ar);
                if (n > 0)
                {
                    this.rcvbuf.AddDataLen(n);
                    while (this.rcvbuf.count > TcpBuffer.PCK_MIN_SIZE)
                    {
                        BinParser parser = new BinParser(this.rcvbuf.buf, this.rcvbuf.offet);
                        int head = parser.Int();
                        if (head != TcpBuffer.PCK_HEADER)
                        {
                            this.rcvbuf.Clear();
                            break;
                        }
                        int length = (int)parser.Short();
                        if (length > this.rcvbuf.count)
                        {
                            break;
                        }
                        byte[] tmpbuf = new byte[length];
                        Buffer.BlockCopy(this.rcvbuf.buf, this.rcvbuf.offet + TcpBuffer.PCK_MIN_SIZE, tmpbuf, 0, length);
                        this.rcvbuf.DeleteData(length);
                        this.OnReceive(tmpbuf);
                    }
                    this.rcvbuf.Reset();
                }
                this.PostReceive();
            }
            catch (Exception e)
            {
                this.eventListener.OnError(errors.New(e.Message));
            }
        }

        private void OnReceive(byte[] buf)
        {
            if (buf[0] == 35)
            {
                string str = System.Text.Encoding.UTF8.GetString(buf);
                if (str == "#ping")
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

        public error Send(byte[] msg)
        {
            int length = msg.Length;
            BinBuffer buf = new BinBuffer(length + TcpBuffer.PCK_MIN_SIZE);
            buf.PushInt(TcpBuffer.PCK_HEADER);
            buf.PushShort((short)length);
            buf.PushBytes(msg);

            try
            {
                int n = this.tcpsocket.Send(buf.Bytes());
                if (n <= 0)
                {
                    return errors.New("there's no datas have been sended!");
                }
                return errors.nil;
            }
            catch(Exception e)
            {
                error err = errors.New(e.Message);
                this.eventListener.OnError(err);
                return err;
            }
        }

        public error SendAsync(byte[] msg, Action<bool> callback = null)
        {
            int length = msg.Length;
            BinBuffer buf = new BinBuffer(length + TcpBuffer.PCK_MIN_SIZE);
            buf.PushInt(TcpBuffer.PCK_HEADER);
            buf.PushShort((short)length);
            buf.PushBytes(msg);

            this.tcpsocket.BeginSend(buf.Bytes(), 0, msg.Length, 0, (ar) =>
            {
                if (callback != null)
                {
                    try
                    {
                        int n = this.tcpsocket.EndSend(ar);
                        callback.Invoke(n > 0);
                    }
                    catch (Exception e)
                    {
                        this.eventListener.OnError(errors.New(e.Message));
                    }
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
