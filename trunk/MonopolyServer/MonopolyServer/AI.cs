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
        public Socket MySocket;
        
        public abstract int Bid(Property aProp, int aCurrentBid);
        public abstract bool TenPercentOr200Dollars();
        public abstract bool BuyVisitedProperty(Property aProp);
        public abstract bool BuyOfferedProperty(Player aPlayer, Property aProp, int aPrice);
        public abstract bool UseGetOutOfJaiFreeCard();
        public abstract void FreeMove(int aDebt);
        public abstract bool GetOutOfJailFreeCardOffered(Player aPlayer, int aPrice);
        public abstract bool DrawChanceOrPay10Dollars();

        protected void Mortgage(Property aProp)
        {
            SendMessage("mortgage", "propertyId", aProp.Id);
        }

        protected void Unmortgage(Property aProp)
        {
            SendMessage("unmortgage", "propertyId", aProp.Id);
        }

        protected void BuyHouses(City aCity, int aNum)
        {
            SendMessage("buyHouses", "fieldId", aCity.Id, "number", aNum);
        }

        protected void SellHouses(City aCity, int aNum)
        {
            SendMessage("sellHouses", "fieldId", aCity.Id, "number", aNum);
        }
        /*
        protected void Bid(int aOffer)
        {
            SendMessage("bid", "offer", aOffer);
        }
        */
        protected void OfferProperty(Property aProp, Player aPi, int aOffer)
        {
            SendMessage("offerProperty", "propertyId", aProp.Id, 
                "player", aPi.Nickname,
                "offer", aOffer);
        }

        protected void SendMessage(string aType, params object[] aAttributes)
        {
            if (aAttributes.Length % 2 != 0)
                throw new ArgumentException("Number of parameters should be odd");
            string s = "<" + aType + " ";

            for (int i = 0; i < aAttributes.Length - 1; i += 2)
                s += aAttributes[i] + "=\"" + Uri.EscapeDataString(aAttributes[i + 1].ToString()) + "\" ";
            
            s += "/>";
            
            GameServer.SendMessage(MySocket, s);
        }
        

    }

    class AIPlayerController : PlayerController
    {
        public AIPlayerController(Socket aSocket)
        {
            MySocket = aSocket;
        }

        int CalculatePropertyMaxPrice(Property aProp)
        {

            int myMaxPrice = aProp.Price;
            //je¿eli brakuje mi do monopolu to zap³acimy wiêcej
            bool neededForMonopoly = true;
            foreach (Property p in aProp.Group.Properties)
                if (p != aProp && p.Owner != MyPlayer)
                    neededForMonopoly = false;

            if (neededForMonopoly)
                myMaxPrice *= 2;

            int res = CalculateReserve();
            myMaxPrice = Math.Min(myMaxPrice, MyPlayer.Money - res);

            return myMaxPrice;
        }

        public override int Bid(Property aProp, int aCurrentBid)
        {
            int myMaxBid = CalculatePropertyMaxPrice(aProp);

            if (aCurrentBid + 20 <= myMaxBid && new Random().NextDouble() > 0.2)
                return aCurrentBid + 20;
            else
                return 0;
        }

        public override bool TenPercentOr200Dollars()
        {
            int taxBase = PlayerCapitalization(MyPlayer);
            int tax = taxBase / 10;

            return tax < 200;
        }

        public override void FreeMove(int aDebt)
        {
            if (aDebt > 0)
                PayDebt();
            else
            {


                int res = CalculateReserve();

                //Odkupujemy zastawione domy
                foreach (Property p in MyPlayer.OwnedProperties)
                    if (p.Mortgaged && MyPlayer.Money - p.UnmortgageValue > res)
                        Unmortgage(p);

                //Jak tylko mo¿emy kupywaæ domki to kupujemy
                foreach (Property p in MyPlayer.OwnedProperties)
                {
                    if (!p.Mortgaged && p.GetType() == typeof(City) && p.Group.Monopolist == MyPlayer)
                    {
                        City c = (City)p;
                        int num = (MyPlayer.Money - res) / c.PricePerHouse;
                        if (num > 0)
                            BuyHouses(c, num);
                    }
                }
            }


        }

        public override bool BuyVisitedProperty(Property property)
        {
            if (MyPlayer.Money > CalculateReserve())
                return true;
            else
                return false;
        }

        public override bool GetOutOfJailFreeCardOffered(Player aPlayer, int aPrice)
        {
            int res = CalculateReserve();
            return MyPlayer.Money - res - aPrice > 0 && aPrice < 50 + new Random().Next(0, 200);                
        }

        public override bool DrawChanceOrPay10Dollars()
        {
            return true;
        }

        public override bool BuyOfferedProperty(Player aPlayer, Property aProp, int aPrice)
        {
            int myMaxPrice = CalculatePropertyMaxPrice(aProp);
            return aPrice <= myMaxPrice && new Random().NextDouble() > 0.2;
        }

        public override bool UseGetOutOfJaiFreeCard()
        {
            return true;
        }

        void PayDebt()
        {
            //Pierw zastawiamy pojedyñcze dzia³ki
            foreach (Property p in MyPlayer.OwnedProperties)
                if (!p.Mortgaged && p.TotalValue == p.Price && p.Group.Monopolist == null)
                //nie zastawione i brak domków
                {
                    Mortgage(p);
                    if (MyPlayer.Money >= 0)
                        return;
                }


            //Nie pomog³o? Trudno, pozb¹dŸmy siê domków
            foreach (Property p in MyPlayer.OwnedProperties)
                if (!p.Mortgaged && p.GetType() == typeof(City))
                {
                    City c = (City)p;
                    int num = (-MyPlayer.Money) / (c.PricePerHouse / 2);
                    if (num > c.NumHouses)
                        num = c.NumHouses;
                    if (num > 0)
                        SellHouses(c, num);
                    if (MyPlayer.Money >= 0)
                        return;
                }

            //I zastawmy resztê nieruchomoœci
            foreach (Property p in MyPlayer.OwnedProperties)
                if (!p.Mortgaged)
                {
                    Mortgage(p);
                    if (MyPlayer.Money >= 0)
                        return;
                }



            //Jak to za ma³o to przegraliœmy
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
            //Staramy siê mieæ rezerwy w wysokoœci 1/10 œredniej kapitalizacji przeciwników, 
            //ale nie mniej ni¿ 200
            int cap = 0;
            foreach (Player p in Players)
                if(p != MyPlayer)
                    cap += PlayerCapitalization(p);
            double averageCap = cap / (Players.Length - 1.0);
            return Math.Max((int)(averageCap / 10.0), 200);
        }
    }

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
            GameServer.SendMessage(mSocket, s);
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
                }
            }

        }

    }



}