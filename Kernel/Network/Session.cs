using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HabboEnvironment_R3.Kernel.Network
{
    public class Session
    {
        public Socket Socket
        {
            get;
            set;
        }

        public SocketAsyncEventArgs ReceiveEventArgs
        {
            get;
            set;
        }

        public int SendBytesRemainingCount
        {
            get;
            set;
        }

        public int BytesSentAlreadyCount
        {
            get;
            set;
        }

        public byte[] DataToSend
        {
            get;
            set;
        }

        public void OnConnectionClose()
        {

        }
    }
}
