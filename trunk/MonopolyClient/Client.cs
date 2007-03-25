using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MonopolyServer.GameServer;
using MonopolyServer.AI;
using MonopolyServer.Model;
using System.Xml;
using System.Diagnostics;
using MonopolyCommunicate;

namespace MonopolyClient
{
    public class Client
    {

        //zwroc uwage czy wszystko jest kompletne, zwlaszcza czy te moetody na dole maja dobre parametry
        //bo ja za bardzo nie wiem

        #region Private Variables

        Socket socket;
        MessageQueue queue;
        AIPlayerController controller;
        Board board;
        Communicate comunicate;


        #endregion

        #region Public Constructors

        public Client(IPAddress IPAdress, int aPort, string aNick)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAdress, aPort);

            queue = new MessageQueue(socket);

            SendMessage("setNick", "nick", aNick);
        }

        #endregion

        void SendMessage(string type, params object[] attributes)
        {
            if (attributes.Length % 2 != 0)
            {
                throw new ArgumentException("Number of parametrs should be odd.");
            }

            String s = "<" + type + " ";

            for (int i = 0; i < attributes.Length - 1; i += 2)
            {
                s += attributes[i] + "=\"" + Uri.EscapeDataString(attributes[i + 1].ToString()) + "\"";
            }

            s += "/>";

            GameServer.SendMessage(socket, s);
        }

        XmlElement GetNextMessage()
        {
            for (; ; )
            {
                while (queue.Count == 0)
                    System.Threading.Thread.Sleep(100);

                while (queue.Count != 0)
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        string s = queue.Pop();
                        doc.LoadXml(s);
                        //doc.LoadXml(mMessageQueue.Pop());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Client Protocol error! (" + controller.MyPlayer.Nickname + ") "
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
                        Console.WriteLine("Client Protocol error! (" + controller.MyPlayer.Nickname + ") "
                                    + new StackTrace(true));
                        break;
                    }
                }
            }
        }

        //tutaj jest zakomentowana ta metoda ktora ty napisale nie wiem jak
        //jej uzyc wiec moze to zrob

        
        XmlElement GetNextMessageNoWait()
        {
            if (queue.Count != 0)
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    string s = queue.Pop();
                    doc.LoadXml(s);
                    //doc.LoadXml(mMessageQueue.Pop());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client Protocol error! (" + controller.MyPlayer.Nickname + ") "
                                + new StackTrace(true));
                    return null;
                }

                try
                {
                    XmlElement e = (XmlElement)doc.FirstChild;
                    return (XmlElement)doc.FirstChild;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Client Protocol error! (" + controller.MyPlayer.Nickname + ") "
                                + new StackTrace(true));
                    return null;
                }
            }


            return null;
        } 

        public void Run()
        {

            //tutaj musisz wyrzucic fora//
           for(;;)
           {

               XmlElement e = GetNextMessage();

               //get welcome message from server

               if (e == null)
               {
                   //nic nie rob, nie ma komunikatu
               }

               #region Welcome

                if (e.LocalName == "welcome")
                {
                    XmlNodeList players = e.ChildNodes;

                    String nicks = "";

                    foreach (XmlNode node in players)
                    {
                        XmlAttributeCollection attr = node.Attributes;

                        foreach (XmlAttribute a in attr)
                        {
                            nicks += a.Value + "\n";
                        }
                    }
                    
                    //w zmiennej nicks sa niki ktore musisz wyswitlic po pdlaczeniu do serwera 

                    
                }
                #endregion

               #region NickOK

                if (e.LocalName == "nickOK")
                {
                   //tutaj musisz sobie wyswietlic komunikat ze nick jest OK
                }

                #endregion

               #region NickTaken

                if (e.LocalName == "nickTaken")
                {
                    //wyswietl komunikat ze nick jest juz zajety

                    //tutaj sobie musisz wstawic okienko do podania nowego nicka
                    String newNick = "dupa";

                    SendMessage("setNick", "nick", newNick);
                }

                #endregion

               #region NewPlayer

                if (e.LocalName == "newPlayer")
                {
                    String nick = e.Attributes[0].Value;
                    //tutaj wyswielasz komunikat ze dolaczyl nowy gracz i jego nick
                    
                }

                #endregion

               #region Chat

                if (e.LocalName == "chat")
                {
                    //tutaj masz nadawce wiadomosci
                    string from = e.Attributes[0].Value;

                   

                    //tutaj tresc - wyswietlasz ja sobie
                    //do wysylanai na na chat bedzie osobna metoda
                    string message = e.Attributes[1].Value;
                
                }

                #endregion

               #region Ready

                if (e.LocalName == "playerReady")
                {
                    //tutaj wyswietlasz komunikat ze gracz jest gotowy
                    string player = e.Attributes[0].Value;
  
                }

                #endregion

               #region AllReady

                if (e.LocalName == "allReady")
                {
                    //tutaj musisz wyswietlic komunikat ze wszyscy sa gotowi i ze zaczyna sie gra
                   
                }

                #endregion

               #region Jail

               //tutaj masz komunikat dotyczący pojscia do wiezeienia
               //musisz dodac sobie wyswietlanie na planszy

               if(e.LocalName == "jail")
               {
                   string player = e.Attributes[0].Value;
                   string dice1 = e.Attributes[1].Value;
                   string dice2 = e.Attributes[2].Value;
                   string freed = e.Attributes[3].Value;

               }

                #endregion

               #region Move

               //tutaj sobie ruch pionka obsluzysz

               if (e.LocalName == "move")
               {
                   string player = e.Attributes[0].Value;
                   string dice1 = e.Attributes[1].Value;
                   string dice2 = e.Attributes[2].Value;
                   string sourcePosition = e.Attributes[3].Value;
                   string destinationPosition = e.Attributes[4].Value;
                   string passedStart = e.Attributes[5].Value;

               }

               #endregion

               #region FreeMove

               if (e.LocalName == "freeMove")
               {
                   string player = e.Attributes[0].Value;
                   string debt = e.Attributes[1].Value;
                   double debtDouble = double.Parse(debt);
               }

               #endregion

               #region HousesBougth

               if (e.LocalName == "housesBougth")
               {
                   string player = e.Attributes[0].Value;
                   string fieldID = e.Attributes[1].Value;
                   string number = e.Attributes[2].Value;
                   string price = e.Attributes[3].Value;

                   double numberDouble = double.Parse(number);
                   double priceDouble = double.Parse(price);
               }

               #endregion

               #region HousesSold

               if (e.LocalName == "housesSold")
               {
                   string player = e.Attributes[0].Value;
                   string fieldID = e.Attributes[1].Value;
                   string number = e.Attributes[2].Value;
                   string price = e.Attributes[3].Value;

                   double numberDouble = double.Parse(number);
                   double priceDouble = double.Parse(price);
               }

               #endregion

               #region PropertyMortgaged

               if (e.LocalName == "propertyMortgaged")
               {
                   string propertyID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string mortgage = e.Attributes[2].Value;

                   double mortgageDouble = double.Parse(mortgage);
               }

               #endregion

               #region UnmortgageProperty

               if (e.LocalName == "propertyUnmortgaged")
               {
                   string propertyID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string mortgage = e.Attributes[2].Value;

                   double mortgageDouble = double.Parse(mortgage);
               }

               #endregion

               #region PropertyOffer

               if (e.LocalName == "propertyOffer")
               {
                   string offerer = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string offer = e.Attributes[2].Value;
                   string propertyID = e.Attributes[3].Value;

                   double offerDouble = double.Parse(offer);
               }

               #endregion

               #region OfferAccepted

               if (e.LocalName == "offerAccepted")
               {
                   string offerer = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string offer = e.Attributes[2].Value;
                   string propertyID = e.Attributes[3].Value;
                   string accepted = e.Attributes[4].Value;

                   double offerDouble = double.Parse(offer);
               }


               #endregion

               #region GetOutOfJailFreeCardOffer

               if (e.LocalName == "getOutOfJailFreeCardOffer")
               {
                   string offerer = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string offer = e.Attributes[2].Value;
                   string cardID = e.Attributes[3].Value;

                   double offerDouble = double.Parse(offer);
               }

               #endregion

               #region CardOfferAccepted

               if (e.LocalName == "cardOfferAccepted")
               {
                   string offerer = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string offer = e.Attributes[2].Value;
                   string cardID = e.Attributes[3].Value;
                   string accepted = e.Attributes[4].Value;

                   double offerDouble = double.Parse(offer);
               }

               #endregion

               #region Done

               if (e.LocalName == "done")
               {
                  string player = e.Attributes[0].Value;
                   //wyswietl komunikat ze gracz skonczyl
               }
               #endregion

               #region Bankruptcy

               if (e.LocalName == "bankruptcy")
               {
                   string player = e.Attributes[0].Value;
                   //wyswietl komunikat ze gracz zbankrutowal
               }

               #endregion

               #region GameOver

               if (e.LocalName == "gameOver")
               {
                   string winner = e.Attributes[0].Value;
                   //wyswietl zwyciezce
               }

               #endregion

               #region AreYouReady
               if (e.LocalName == "areYouReady")
               {
                   //no tutaj to trzeba wyswietlic komunikat ktory odpali metode Ready
               }
               #endregion

               #region UseGetOutOfJailCard
               if (e.LocalName == "useGetOutOfJailCard")
               {
                   //tutaj decyzja co do uzycia karty
                   //o co tu chodzi ??
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;

                   
               }
               #endregion

               #region GetOutOfJailCardUsed

               if (e.LocalName == "getOutOfJailCardUsed")
               {
                   string player = e.Attributes[0].Value;
               }

               #endregion

               #region GoToJail

               if (e.LocalName == "goToJail")
               {
                   string player = e.Attributes[0].Value;
               }

               #endregion

               #region TenPercentOr200Dollars

               if (e.LocalName == "tenPercentOr200Dollars")
               {
                   //tutaj odpowiedz na komunikat jest poprzez wywołanie metody tenPercentOr200Dollars
                   string player = e.Attributes[0].Value;
               }

               #endregion

               #region IncomeTax

               if (e.LocalName == "incomeTax")
               {
                   string player = e.Attributes[0].Value;
                   string type = e.Attributes[1].Value;
                   string tax = e.Attributes[2].Value;

                   double taxDouble = double.Parse(tax);
               }

               #endregion

               #region LuxuryTax

               if (e.LocalName == "luxuryTax")
               {
                   string player = e.Attributes[0].Value;
                   string tax = e.Attributes[1].Value;

                   double taxDouble = double.Parse(tax);
               }

               #endregion

               #region Rent

               if (e.LocalName == "rent")
               {
                   string owner = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string price = e.Attributes[2].Value;

                   double priceDouble = double.Parse(price);
               }

               #endregion

               #region BuyVisitedProperty

               if (e.LocalName == "buyVisitedProperty")
               {
                   string player = e.Attributes[0].Value;
                   string propertyID = e.Attributes[1].Value;
               }

               #endregion

               #region PropertyBought

               if (e.LocalName == "propertyBought")
               {
                   string player = e.Attributes[0].Value;
                   string fieldID = e.Attributes[1].Value;
                   string price = e.Attributes[2].Value;

                   double priceDouble = double.Parse(price);
               }

               #endregion

               #region Auction

               if (e.LocalName == "auction")
               {
                   string fieldID = e.Attributes[0].Value;

                   //tuaj gracze licytuja
               }

               #endregion

               #region AuctionTimer

               if (e.LocalName == "auctionTimer")
               {
                   string timer = e.Attributes[0].Value;
               }

               #endregion

               #region NoAuctionWinner

               if (e.LocalName == "noAuctionWinner")
               {
                   //komunikat ze nikt nie wygral
               }

               #endregion

               #region AuctionWinner

               if (e.LocalName == "auctionWinner")
               {
                   //po pierwsze nie wiem jak zrobic z tym free move
                   string player = e.Attributes[0].Value;
                   string bid = e.Attributes[1].Value;

                   double bidDouble = double.Parse(bid);
               }

               #endregion

               #region AdvanceGoBackToCard

               if (e.LocalName == "advanceGoBackToCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string sourcePosition = e.Attributes[2].Value;
                   string destinationPosition = e.Attributes[3].Value;
                   string advance = e.Attributes[4].Value;
                   string passedStart = e.Attributes[5].Value;
               }

               #endregion

               #region AdvanceToNearestRailroadCard

               if (e.LocalName == "advanceToNearestRailroadCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string fieldID = e.Attributes[2].Value;
               }

               #endregion

               #region AdvanceToNearestRailroadPayment

               if (e.LocalName == "advanceToNearestRailroadPayment")
               {
                   string srcPlayer = e.Attributes[0].Value;
                   string destinationPlayer = e.Attributes[1].Value;
                   string amount = e.Attributes[2].Value;

                   double amountDouble = double.Parse(amount);
               }

               #endregion

               #region AdvanceToNearestUtilityCard

               if (e.LocalName == "advanceToNearestUtilityCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string fieldID = e.Attributes[2].Value;
               }

               #endregion

               #region AdvanceToNearestUtilityPayment

               if (e.LocalName == "advanceToNearestUtilityPayment")
               {
                   string sourcePlayer = e.Attributes[0].Value;
                   string destinationPlayer = e.Attributes[1].Value;
                   string dice = e.Attributes[2].Value;
                   string amount = e.Attributes[3].Value;

                   double amountDouble = double.Parse(amount);
               }

               #endregion

               #region CollectPayCard

               if (e.LocalName == "collectPayCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string amount = e.Attributes[2].Value;

                   double amountDouble = double.Parse(amount);
               }

               #endregion

               #region CollectPayEachPlayerCard

               if (e.LocalName == "collectPayEachPlayerCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string amountPerPlayer = e.Attributes[2].Value;

                   double amountPerPlayerDouble = double.Parse(amountPerPlayer);
               }

               #endregion

               #region GetOutOfJailCard

               if (e.LocalName == "getOutOfJailCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
               }

               #endregion

               #region GoBackThreeSpacesCard

               if (e.LocalName == "goBackThreeSpacesCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string sourcePosition = e.Attributes[2].Value;
                   string destinationPosition = e.Attributes[3].Value;
               }

               #endregion

               #region PayOrDrawCard

               if (e.LocalName == "payOrDrawCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string amount = e.Attributes[2].Value;

                   double amountDouble = double.Parse(amount);

                   //no i tuaj trzeba podjac decyzje wiec uzywasz metody PayOrDrawCard
               }

               #endregion

               #region PayOrDraw

               if (e.LocalName == "payOrDraw")
               {
                   string player = e.Attributes[0].Value;
                   string decision = e.Attributes[1].Value;
               }

               #endregion

               #region RepairsCard

               if (e.LocalName == "repairsCard")
               {
                   string cardID = e.Attributes[0].Value;
                   string player = e.Attributes[1].Value;
                   string cost = e.Attributes[2].Value;

                   double costDouble = double.Parse(cost);
               }

               #endregion

           }
        }

        //musisz sobie zrobic message box z zapytanie czy jest gotowy
        public void Ready()
        {
            SendMessage("ready");
        }

        //kupuje domki
        public void BuyHouses(String cityID, int number)
        {
            SendMessage("buyHouses", "fieldId", cityID, "number", number);
        }

        //sprzedaje domki
        public void SellHouses(String cityID, int number)
        {
            SendMessage("sellHouses", "fieldId", cityID, "number", number);
        }

        public void Mortgage(string propertyID)
        {
            SendMessage("mortgage", "propertyId", propertyID);
        }

        public void Unmortgage(string propertyID)
        {
            SendMessage("unmortgage", "propertyId", propertyID);
        }

        public void OfferProperty(string propertyID, string player, double offer)
        {
            SendMessage("offerProperty", "propertyId", propertyID, "player", player, "offer", offer);
        }

        public void PropertyOffer(bool accepted)
        {
            SendMessage("propertyOffer", "accepted", accepted);
        }

        public void OfferGetOutOfJailFreeCard()
        {

        }

        public void Done()
        {
            SendMessage("done");
        }

        public void DoNothingField()
        {
            //obsluga przypadku gdzie nic sie nie dzieje
        }

        public void UseGetOutOfJailCard()
        {
        }

        public void TenPercentOr200Dollars(string type)
        {
            SendMessage("tenPercentOr200Dollars", "type", type);
        }

        public void BuyVisitedProperty(bool buy)
        {
            SendMessage("buyVisitedProperty", "buy", buy);
        }

        public void Bid(string player, double offer)
        {
            SendMessage("bid", "player", player, "offer", offer);
        }

        public void PayOrDrawCard(string decision)
        {
            SendMessage("payOrDrawCard", "decision", decision);
        }
    }
}