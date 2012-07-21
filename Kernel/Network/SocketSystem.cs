using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HabboEnvironment_R3.Kernel.Network
{
    public class SocketSystem
    {
        public Socket Socket
        {
            get;
            private set;
        }

        public TPool<SocketAsyncEventArgs> AcceptPool
        {
            get;
            private set;
        }

        public TPool<SocketAsyncEventArgs> ReceivePool
        {
            get;
            private set;
        }

        public TPool<SocketAsyncEventArgs> SendPool
        {
            get;
            private set;
        }

        public BufferPool BufferPool
        {
            get;
            private set;
        }

        public SemaphoreSlim PoolEnforcer
        {
            get;
            private set;
        }

        public ProcessBytes ByteProcessor
        {
            get;
            private set;
        }

        public delegate void ProcessBytes(Session Session, ref byte[] Bytes);

        public void Serialize(IPAddress IP, int Port, int Backlog, int SupportedAmount, ProcessBytes ByteProcessor)
        {
            this.ConstructSocket(IP, Port, Backlog);
            this.ConstructPooling(SupportedAmount);
            this.ByteProcessor = ByteProcessor;
        }

        public void ConstructSocket(IPAddress IP, int Port, int Backlog)
        {
            IPEndPoint EndPoint = new IPEndPoint(IP, Port);

            Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(EndPoint);
            Socket.Blocking = false;

            Socket.Listen(Backlog);
        }

        public void ConstructPooling(int SupportedAmount)
        {
            this.AcceptPool = new TPool<SocketAsyncEventArgs>(SupportedAmount);
            this.ReceivePool = new TPool<SocketAsyncEventArgs>(SupportedAmount);
            this.SendPool = new TPool<SocketAsyncEventArgs>(SupportedAmount);
            this.PoolEnforcer = new SemaphoreSlim(SupportedAmount, SupportedAmount);
            this.BufferPool = new BufferPool(SupportedAmount);
            this.BufferPool.PushAllReceivers(this, this.ReceivePool.PushAndHandleAll());
            this.BufferPool.PushAllSenders(this, this.SendPool.PushAndHandleAll());

            foreach (SocketAsyncEventArgs AcceptArgs in AcceptPool.PushAndHandleAll())
            {
                AcceptArgs.Completed += AcceptArgs_Completed;
            }

            StartAccept();
        }

        #region Accepting
        public void StartAccept()
        {
            SocketAsyncEventArgs AcceptArgs;

            if (AcceptPool.TryPop(out AcceptArgs))
            {
                PoolEnforcer.Wait();

                bool Trigger = Socket.AcceptAsync(AcceptArgs);

                if (!Trigger)
                {
                    HandleAccept(AcceptArgs);
                }
            }
        }

        public void AcceptArgs_Completed(object sender, SocketAsyncEventArgs AcceptArgs)
        {
            HandleAccept(AcceptArgs);
        }

        public void HandleAccept(SocketAsyncEventArgs AcceptArgs)
        {
            if (AcceptArgs.SocketError != SocketError.Success)
            {
                HandleBadAccept(AcceptArgs);
                return;
            }

            StartAccept();

            SocketAsyncEventArgs ReceiveArgs;

            if (ReceivePool.TryPop(out ReceiveArgs))
            {
                ReceiveArgs.UserToken = new Session();
                ((Session)ReceiveArgs.UserToken).Socket = AcceptArgs.AcceptSocket;
                ((Session)ReceiveArgs.UserToken).ReceiveEventArgs = ReceiveArgs;

                AcceptArgs.AcceptSocket = null;
                AcceptPool.Push(AcceptArgs);

                StartReceive(ReceiveArgs);
            }
            else
            {
                HandleAccept(AcceptArgs);
            }
        }

        public void HandleBadAccept(SocketAsyncEventArgs AcceptArgs)
        {
            AcceptArgs.AcceptSocket.Close();
            AcceptPool.Push(AcceptArgs);
        }
        #endregion

        #region Receive
        public void StartReceive(SocketAsyncEventArgs Args)
        {
            bool Trigger = ((Session)Args.UserToken).Socket.ReceiveAsync(Args);

            if (!Trigger)
            {
                ProcessReceive(Args);
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs Args)
        {
            if (Args.BytesTransferred > 0 && Args.SocketError == SocketError.Success)
            {
                byte[] Data = new byte[Args.BytesTransferred];

                Array.Copy(BufferPool.Buffer, Args.Offset, Data, 0, Args.BytesTransferred);

                ByteProcessor.Invoke((Args.UserToken as Session), ref Data);

                StartReceive(Args);
            }
            else
            {
                CloseClientSocket(Args);
                this.ReceivePool.Push(Args);
                this.PoolEnforcer.Release();
            } 
        }
        #endregion

        #region Send
        public void SendBytes(Session Session, byte[] Bytes)
        {
            SocketAsyncEventArgs Args;

            if (SendPool.TryPop(out Args))
            {
                Args.UserToken = Session;
                Session.DataToSend = Bytes;
                Session.SendBytesRemainingCount = Bytes.Length;
                Args.AcceptSocket = Session.Socket;

                StartSend(Args);
            }
        }

        public void StartSend(SocketAsyncEventArgs Args)
        {
            Session Session = (Session)Args.UserToken;

            if (Session.SendBytesRemainingCount <= BufferPool.BUF_SIZE)
            {
                Args.SetBuffer(Args.Offset, Session.SendBytesRemainingCount);
                Buffer.BlockCopy(Session.DataToSend, Session.BytesSentAlreadyCount, Args.Buffer, Args.Offset, Session.SendBytesRemainingCount);
            }
            else
            {
                Args.SetBuffer(Args.Offset, BufferPool.BUF_SIZE);
                Buffer.BlockCopy(Session.DataToSend, Session.BytesSentAlreadyCount, Args.Buffer, Args.Offset, BufferPool.BUF_SIZE);
            }

            bool Trigger = Args.AcceptSocket.SendAsync(Args);

            if (!Trigger)
            {
                ProcessSend(Args);
            } 
        }

        public void ProcessSend(SocketAsyncEventArgs Args)
        {
            Session Session = (Session)Args.UserToken;

            if (Args.SocketError == SocketError.Success)
            {
                Session.SendBytesRemainingCount = Session.SendBytesRemainingCount - Args.BytesTransferred;

                if (Session.SendBytesRemainingCount == 0)
                {
                    SendPool.Push(Args);
                }
                else
                {
                    Session.BytesSentAlreadyCount += Args.BytesTransferred;
                    StartSend(Args);
                }
            }
            else
            {
                CloseClientSocket(Args);
                SendPool.Push(Args);
            } 
        }
        #endregion

        public void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs Args)
        {
            Session Session = (Session)Args.UserToken;

            try
            {
                Session.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }

            Session.Socket.Close();
            Session.OnConnectionClose();
        }
    }
}
