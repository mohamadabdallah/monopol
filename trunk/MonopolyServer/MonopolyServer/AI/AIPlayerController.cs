using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer;
using MonopolyServer.Model.Property;
using MonopolyServer.Model;
using System.Net.Sockets;

namespace MonopolyServer.AI
{
    class AIPlayerController : PlayerController
    {
        public AIPlayerController(Socket aSocket)
        {
            MySocket = aSocket;
        }

        int CalculatePropertyMaxPrice(Model.Property.Property aProp)
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

        public override int Bid(Model.Property.Property aProp, int aCurrentBid, int aAuctionTime)
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
                        if (c.NumHouses + num > 5)
                            num = 5 - c.NumHouses;
                        if (num > 0)
                            BuyHouses(c, num);
                    }
                }
            }


        }

        public override bool BuyVisitedProperty(Model.Property.Property property)
        {
            if (MyPlayer.Money > CalculateReserve())
                return true;
            else
                return false;
        }

        public override bool GetOutOfJailFreeCardOffered(Model.Player aPlayer, int aPrice)
        {
            int res = CalculateReserve();
            return MyPlayer.Money - res - aPrice > 0 && aPrice < 50 + new Random().Next(0, 150);
        }

        public override bool PayOrDrawCard()
        {
            return false;
        }

        public override bool BuyOfferedProperty(Model.Player aPlayer, Model.Property.Property aProp, int aPrice)
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


        int PlayerCapitalization(Model.Player aPlayer)
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
                if (p != MyPlayer)
                    cap += PlayerCapitalization(p);
            double averageCap = cap / (Players.Length - 1.0);
            return Math.Max((int)(averageCap / 10.0), 200);
        }
    }
}
