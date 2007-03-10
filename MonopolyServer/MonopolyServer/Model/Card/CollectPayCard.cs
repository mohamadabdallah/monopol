using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Card
{
    class CollectPayCard : Card
    {
        public CollectPayCard(int aId, int aAmount)
        {
            Id = aId;
            Amount = aAmount;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aServer.SendMessage("collectPayCard", "cardId", Id, "player", aPlayer.Nickname, "amount", Amount);
            aPlayer.Money += Amount;

            return true;
        }

        public int Amount;
    }
}
