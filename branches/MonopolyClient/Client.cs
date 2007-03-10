﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Monopoly;

namespace MonopolyClient
{
    class Client
    {
        #region Private Variables

        Socket socket;

        #endregion
        #region Public Constructors

        public Client(IPAddress IPAdress, int aPort, string aNick)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAdress, aPort);
            
        }

        #endregion
    }
}