using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace MonopolyServer.AI
{
    abstract class PlayerController
    {

        public Model.Board GameBoard;
        public Model.Player MyPlayer;
        public Model.Player[] Players;
        public Socket MySocket;

        public abstract int Bid(Model.Property.Property aProp, int aCurrentBid, int aAuctionTime);
        public abstract bool TenPercentOr200Dollars();
        public abstract bool BuyVisitedProperty(Model.Property.Property aProp);
        public abstract bool BuyOfferedProperty(Model.Player aPlayer, Model.Property.Property aProp, int aPrice);
        public abstract bool UseGetOutOfJaiFreeCard();
        public abstract void FreeMove(int aDebt);
        public abstract bool GetOutOfJailFreeCardOffered(Model.Player aPlayer, int aPrice);
        public abstract bool PayOrDrawCard();

        protected void Mortgage(Model.Property.Property aProp)
        {
            SendMessage("mortgage", "propertyId", aProp.Id);
        }

        protected void Unmortgage(Model.Property.Property aProp)
        {
            SendMessage("unmortgage", "propertyId", aProp.Id);
        }

        protected void BuyHouses(Model.Property.City aCity, int aNum)
        {
            SendMessage("buyHouses", "fieldId", aCity.Id, "number", aNum);
        }

        protected void SellHouses(Model.Property.City aCity, int aNum)
        {
            SendMessage("sellHouses", "fieldId", aCity.Id, "number", aNum);
        }
        /*
        protected void Bid(int aOffer)
        {
            SendMessage("bid", "offer", aOffer);
        }
        */
        protected void OfferProperty(Model.Property.Property aProp, Model.Player aPi, int aOffer)
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

            GameServer.GameServer.SendMessage(MySocket, s);
        }
        
    }
}
