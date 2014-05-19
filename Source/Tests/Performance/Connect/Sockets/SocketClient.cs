using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Connect.Sockets
{
    class SocketClient
    {
        public const int MessageSize = 4;
        volatile bool _pending = false;
        public volatile int Queued = 0;
        readonly WaitCallback _sendCoreHandler;
        public event EventHandler OnConnected;
        public long BytesTransfered = 0;
        public EventHandler<long> OnSend;

        public SocketClient(string host, int port)
        {
            var remoteEp = new DnsEndPoint(host, port);

            // Create a TCP/IP socket.
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            this._sendCoreHandler = new WaitCallback(SendCore);
            this.Args = new SocketAsyncEventArgs();
            Args.Completed += ArgsCallback;
            Args.SetBuffer(new byte[MessageSize], 0, MessageSize);
            for (int i = 0; i < MessageSize; i++)
            {
                Args.Buffer[i] = (byte)i;
            }

            Args.RemoteEndPoint = remoteEp;

            // setup port scalability
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, (SocketOptionName)0x3006, true);
        }

        public void ConnectAsync()
        {
            this._pending = true;
            Args.SetBuffer(0, 0);
            if (!Socket.ConnectAsync(Args))
            {
                ArgsCallback(null, Args);
            }
        }

        void ArgsCallback(object sender, SocketAsyncEventArgs e)
        {
            _pending = false;
            this.TerminateProcessOnError();
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                Args.SetBuffer(0, Args.Buffer.Length);
                if (this.OnConnected != null)
                {
                    this.OnConnected(this, EventArgs.Empty);
                }
            }
            else if (e.LastOperation == SocketAsyncOperation.Send)
            {
                Interlocked.Add(ref this.BytesTransfered, e.BytesTransferred);
                if (this.OnSend != null)
                {
                    this.OnSend(this, e.BytesTransferred);
                }

                this.SendCore(null);
            }
        }

        internal void Send()
        {
            this.Queued++;
            SendCore(null);
        }

        private void SendCore(object state)
        {
            if (this.Queued <= 0)
            {
                return;
            }

            if (!_pending)
            {
                lock (this.Socket)
                {
                    if (!_pending)
                    {
                        Queued--;
                        _pending = true;
                        if (!this.Socket.SendAsync(this.Args))
                        {
                            this._pending = false;
                            this.TerminateProcessOnError();
                            if (this.Queued > 0)
                            {
                                ThreadPool.UnsafeQueueUserWorkItem(this._sendCoreHandler, null);
                            }
                        }
                    }
                }
            }
        }

        private void TerminateProcessOnError()
        {
            if (this.Args.SocketError != SocketError.Success)
            {
                Console.WriteLine("Socket error = " + this.Args.SocketError);
                Environment.Exit(1);
            }
        }

        public SocketAsyncEventArgs Args { get; set; }

        public Socket Socket { get; set; }
    }
}
