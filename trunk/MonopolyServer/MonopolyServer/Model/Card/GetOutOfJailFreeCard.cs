using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Card
{
    class GetOutOfJailFreeCard : Card
    {
        public GetOutOfJailFreeCard(int aId, CardType aType)
        {
            Id = aId;
            Type = aType;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aPlayer.GetOutOfJailFreeCards.Add(this);
            aServer.SendMessage("getOutOfJailCard", "cardId", Id, "player", aPlayer.Nickname);
            return false;
        }
    }
}
