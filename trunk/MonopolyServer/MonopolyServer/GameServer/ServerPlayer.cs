using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace MonopolyServer.GameServer
{
    class ServerPlayer :Model.Player
    {
        public MessageQueue MyMessageQueue;
        public Socket Connection;
        public string SafeName
        {
            get
            {
                return Nickname != null ? Nickname : Connection.RemoteEndPoint.ToString();
            }
        }
    }
}
