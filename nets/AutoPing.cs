using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.nets
{
    class AutoPing
    {
        ConnState state;
        long lstick = 0;

        public AutoPing(INetWorker netWorker, INetEventListener eventListener)
        {
            this.state = new ConnState();
            this.state.worker = netWorker;
            this.state.listener = eventListener;
            this.state.ping = false;
            this.state.ts = 0;
        }

        public void Update()
        {
            long tick = DateTime.Now.Ticks;
            if (lstick <= 0)
            {
                lstick = tick;
            }
            else if (tick - lstick > 20000000)
            {
                lock (this.state)
                {
                    if (this.state.ping)
                    {
                        // conn time out
                        error err = errors.New("conn time out!");
                        this.state.listener.OnError(err);
                        this.state.listener.OnClose(err);
                        this.state.worker.Dispose();
                    }
                    else if (tick - this.state.ts > 20000000)
                    {
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("#ping");
                        this.state.worker.SendAsync(buffer);
                        this.state.ping = true;
                    }
                }
                lstick = tick;
            }
        }

        public void ResetConnState()
        {
            lock (this.state)
            {
                this.state.ping = false;
                this.state.ts = DateTime.Now.Ticks;
            }
        }
    }

    class ConnState
    {
        public INetWorker worker { get; set; }
        public INetEventListener listener { get; set; }
        public bool ping { get; set; }
        public long ts { get; set; }
    }
}
