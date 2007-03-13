using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using MonopolyServer.GameServer;
using MonopolyServer.AI;
using MonopolyServer.Model;
using System.Net;
using System.Xml;
using System.Diagnostics;
using MonopolyServer.Model.Property;

namespace MonopolyServer
{
    class TestGameClient
    {
        Socket mSocket;
        MessageQueue mMessageQueue;
        AIPlayerController mController;//
        

        public Board GameBoard
        {
            set
            {
                mController.GameBoard = value;
            }
        }

        public Player MyPlayer
        {
            set
            {
                mController.MyPlayer = value;
            }
        }

        public Player[] Players
        {
            set
            {
                mController.Players = value;
            }

            get
            {
                return mController.Players;
            }
        }



        void SendMessage(string aType, params object[] aAttributes)
        {
            if (aAttributes.Length % 2 != 0)
                throw new ArgumentException("Number of parameters should be odd");
            string s = "<" + aType + " ";

            for (int i = 0; i < aAttributes.Length - 1; i += 2)
                s += aAttributes[i] + "=\"" + Uri.EscapeDataString(aAttributes[i + 1].ToString()) + "\" ";

            s += "/>";
            GameServer.GameServer.SendMessage(mSocket, s);
        }

        public TestGameClient(IPAddress aAddress, int aPort, string aNick)
        {
            mSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream, ProtocolType.Tcp);
            mSocket.Connect(aAddress, aPort);
            mMessageQueue = new MessageQueue(mSocket);
            mController = new AIPlayerController(mSocket);

            SendMessage("setNick", "nick", aNick);
            
            
        }

        XmlElement GetNextMessage()
        {
            for (; ; )
            {
                while (mMessageQueue.Count == 0)
                    System.Threading.Thread.Sleep(100);

                while (mMessageQueue.Count != 0)
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        string s = mMessageQueue.Pop();
                        doc.LoadXml(s);
                        //doc.LoadXml(mMessageQueue.Pop());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Client Protocol error! (" + mController.MyPlayer.Nickname + ") "
                                    + new StackTrace(true));
                        break;
                    }

                    try
                    {
                        XmlElement e = (XmlElement)doc.FirstChild;
                        return (XmlElement)doc.FirstChild;
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Client Protocol error! (" + mController.MyPlayer.Nickname + ") "
                                    + new StackTrace(true));
                        break;
                    }
                }
            }
        }
        

        public void Run()
        {
            for (; ; )
            {
                XmlElement e = GetNextMessage();
                if (e.LocalName == "playerReady" && !mController.MyPlayer.Ready) //jak cz³owiek jest gotowy to ai te¿
                {
                    SendMessage("ready");
                    continue;
                }
                if(e.GetAttribute("player") != mController.MyPlayer.Nickname)
                    continue;
                switch (e.LocalName)
                {
                    case "tenPercentOr200Dollars":
                        if (mController.TenPercentOr200Dollars())
                            SendMessage("tenPercentOr200Dollars", "type", "tenPercent");
                        else
                            SendMessage("tenPercentOr200Dollars", "type", "200Dollars");
                        break;
                    case "freeMove":
                        //if (mController.MyPlayer.Money < 0)
                            //mController.PayDebt();
                        mController.FreeMove(int.Parse(e.GetAttribute("debtToPay")));
                        SendMessage("done");
                        break;
                    case "buyVisitedProperty":
                        bool dec = mController.BuyVisitedProperty((Property)
                            (mController.GameBoard.Fields[int.Parse(e.GetAttribute("fieldId"))]));
                        SendMessage("buyVisitedProperty", "buy", dec.ToString().ToLower());
                            
                        break;

                    case "propertyOffer":
                        dec = mController.BuyOfferedProperty(FindPlayer(e.GetAttribute("player")),
                            (Property)mController.GameBoard.Fields[int.Parse(e.GetAttribute("propertyId"))],
                            int.Parse(e.GetAttribute("offer")));
                        SendMessage("propertyOffer", dec.ToString().ToLower());
                        break;

                    case "payOrDrawCard":
                        dec = mController.PayOrDrawCard();
                        SendMessage("payOrDrawCard", "decision", dec.ToString().ToLower());
                        break;

                    case "getOutOfJailFreeCardOffer":
                        dec = mController.GetOutOfJailFreeCardOffered(FindPlayer(e.GetAttribute("player")),
                            int.Parse(e.GetAttribute("offer")));
                        SendMessage("propertyOffer", dec.ToString().ToLower());
                        break;

                    case "areYouReady":
                        SendMessage("ready");
                        break;

                }
            }

        }

        Player FindPlayer(string aName)
        {
            foreach (Player pi in Players)
                if (pi.Nickname == aName)
                    return pi;

            return null;
        }
    }
}