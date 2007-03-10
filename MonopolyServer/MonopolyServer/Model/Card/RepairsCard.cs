using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.Model.Property;

namespace MonopolyServer.Model.Card
{
    class RepairsCard : Card
    {
        public RepairsCard(int aId, int aAmountPerHouse, int aAmountPerHotel)
        {
            AmountPerHouse = aAmountPerHouse;
            AmountPerHotel = aAmountPerHotel;
            Id = aId;
        }

        public int AmountPerHouse;
        public int AmountPerHotel;

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            int cost = 0;
            foreach (Property.Property p in aPlayer.OwnedProperties)
                if (p.GetType() == typeof(City))
                {
                    City c = (City)p;
                    cost += (c.NumHouses / 5) * 115;
                    cost += (c.NumHouses % 5) * 40;
                }

            aPlayer.Money -= cost;
            aServer.SendMessage("repairsCard", "cardId", Id, "player", aPlayer.Nickname, "cost", cost);



            return true;
        }
    }
}
