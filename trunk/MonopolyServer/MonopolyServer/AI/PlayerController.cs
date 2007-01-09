using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.AI
{
    abstract class PlayerController
    {
        public Board GameBoard;
        public Player MyPlayer;
        public Player[] Players;
        public Socket MySocket;

        public abstract int Bid(Property aProp, int aCurrentBid, int aAuctionTime);
        public abstract bool TenPercentOr200Dollars();
        public abstract bool BuyVisitedProperty(Property aProp);
        public abstract bool BuyOfferedProperty(Player aPlayer, Property aProp, int aPrice);
        public abstract bool UseGetOutOfJaiFreeCard();
        public abstract void FreeMove(int aDebt);
        public abstract bool GetOutOfJailFreeCardOffered(Player aPlayer, int aPrice);
        public abstract bool PayOrDrawCard();

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
}
