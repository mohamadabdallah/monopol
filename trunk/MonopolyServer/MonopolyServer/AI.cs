using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

namespace Monopoly
{

    abstract class PlayerController
    {
        public Board GameBoard;
        public Player MyPlayer;
        public Player[] Players;
        
        public abstract int Bid(Property property, int currentBid);
        public abstract bool TenPercentOr200Dollars();
        public abstract bool BuyVisitedProperty(Property property);
        //public abstract bool WishToBuy(Property property, int price);
        //public abstract bool UseGetOutOfJaiFreeCard();
        public abstract void PayDebt();
        public abstract void FreeMove();

        public void Mortgage(Property aProp)
        {
        }
        public void Unmortgage(Property aProp)
        {
        }
        public void BuyHouses(City aCity, int aNum)
        {
        }
        public void SellHouses(City aCity, int aNum)
        {
        }
        public void Bid(int aBid)
        {
        }
        public void OfferProperty(Property aProp, PlayerInfo aPi)
        {
        }
        

    }

    class AIPlayerController : PlayerController
    {
        public override int Bid(Property property, int currentBid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool TenPercentOr200Dollars()
        {
            int taxBase = PlayerCapitalization(MyPlayer);
            int tax = taxBase / 10;

            return tax < 200;
        }

        public override void FreeMove()
        {
            int res = CalculateReserve();
            
            //Odkupujemy zastawione domy
            foreach(Property p in MyPlayer.OwnedProperties)
                if(p.Mortgaged && MyPlayer.Money - p.UnmortgageValue > res)
                    Unmortgage(p);

            //Jak tylko mo¿emy kupywaæ domki to kupujemy
            foreach (Property p in MyPlayer.OwnedProperties)
            {
                if (!p.Mortgaged && p.GetType() == typeof(City))
                {
                    City c = (City)p;
                    int num = (MyPlayer.Money - res) / c.PricePerHouse;
                    if (num > 0)
                        BuyHouses(c, num);
                }
            }






        }

        public override bool BuyVisitedProperty(Property property)
        {
            return false;
            if (MyPlayer.Money > CalculateReserve())
                return true;
            else
                return false;
        }

        public override void PayDebt()
        {
            //Pierw zastawiamy pojedyñcze dzia³ki





        }


        int PlayerCapitalization(Player aPlayer)
        {
            int c = aPlayer.Money;
            foreach (Property p in aPlayer.OwnedProperties)
                c += p.TotalValue;

            return c;
        }

        int CalculateReserve()
        {
            //return 300;
            
            //Staramy siê mieæ rezerwy w wysokoœci 1/10 œredniej kapitalizacji przeciwników
            int cap = 0;
            foreach (Player p in Players)
                if(p != MyPlayer)
                    cap += PlayerCapitalization(p);
            double averageCap = cap / (Players.Length - 1.0);

            return (int)(averageCap / 10.0);
        }
    }

    class TestGameClient
    {
        Socket mSocket;
        MessageQueue mMessageQueue;
        AIPlayerController mController = new AIPlayerController();
        //string mName;

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
            GameServer.SendMessage(mSocket, s);
        }

        public TestGameClient(IPAddress aAddress, int aPort, string aNick)
        {
            mSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream, ProtocolType.Tcp);
            mSocket.Connect(aAddress, aPort);
            mMessageQueue = new MessageQueue(mSocket);

            SendMessage("setNick", "nick", aNick);
            //
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
                        if (mController.MyPlayer.Money < 0)
                            mController.PayDebt();
                        mController.FreeMove();
                        SendMessage("done");
                        break;
                    case "buyVisitedProperty":
                        bool dec = mController.BuyVisitedProperty((Property)
                            (mController.GameBoard.Fields[int.Parse(e.GetAttribute("fieldId"))]));
                        SendMessage("buyVisitedProperty", "buy", dec.ToString().ToLower());
                            
                        break;
                }
            }

        }

    }



}