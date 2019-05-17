#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace itfantasy.gun.nets.ws
{
    public class WSNetWorker : INetWorker
    {
        WebSocket websocket;
        INetEventListener eventListener;

        Queue<MessageEventArgs> msgQueue
            = new Queue<MessageEventArgs>();

        public WSNetWorker()
        {
            this.msgQueue = new Queue<MessageEventArgs>();
        }

        public bool Update()
        {
            if (this.msgQueue.Count > 0)
            {
                MessageEventArgs e;
                lock (this.msgQueue)
                {
                    e = this.msgQueue.Dequeue();
                }
                this.eventListener.OnMsg(e.RawData);
                return true;
            }
            return false;
        }

        public void Connect(string url)
        {
            this.websocket = new WebSocket(url);
            this.websocket.Origin = "ws://127.0.0.1/cnt#cnt";
            this.websocket.OnOpen += websocket_OnConnect;
            this.websocket.OnMessage += websocket_OnMessage;
            this.websocket.OnClose += websocket_OnClose;
            this.websocket.OnError += websocket_OnError;
            this.websocket.ConnectAsync();
        }

        public error Send(byte[] msg)
        {
            if (msg.Length > 1015)
            {
                return errors.New("the max length of send buffer is 1015 !");
            }
            this.websocket.Send(msg);
            return errors.nil;
        }

        public error SendAsync(byte[] msg, Action<bool> callback = null)
        {
            if (msg.Length > 1015)
            {
                return errors.New("the max length of send buffer is 1015 !");
            }
            this.websocket.SendAsync(msg, callback);
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
            this.websocket.Close();
            this.msgQueue.Clear();
            this.websocket.OnOpen -= websocket_OnConnect;
            this.websocket.OnMessage -= websocket_OnMessage;
            this.websocket.OnClose -= websocket_OnClose;
            this.websocket.OnError -= websocket_OnError;
            this.websocket = null;
        }

        private void websocket_OnConnect(object sender, object e)
        {
            this.eventListener.OnConn();
        }

        private void websocket_OnMessage(object sender, MessageEventArgs e)
        {
            lock (this.msgQueue)
            {
                this.msgQueue.Enqueue(e);
            }
        }

        private void websocket_OnClose(object sender, CloseEventArgs e)
        {
            this.eventListener.OnClose();
        }

        private void websocket_OnError(object sender, ErrorEventArgs e)
        {
            this.eventListener.OnError(errors.New(e.Message));
        }

        public bool Connected
        {
            get
            {
                return this.websocket != null && this.websocket.IsAlive;
            }
        }
    }
}
#endif