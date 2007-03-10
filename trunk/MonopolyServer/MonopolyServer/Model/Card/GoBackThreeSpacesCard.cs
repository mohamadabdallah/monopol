using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Card
{
    class GoBackThreeSpacesCard : Card
    {
        public GoBackThreeSpacesCard(int aId)
        {
            Id = aId;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            int dstPos = aPlayer.Position - 3;
            if (dstPos < 0)
                dstPos += 40;
            aServer.SendMessage("goBackThreeSpacesCard", "cardId", Id, "player", aPlayer.Nickname,
                "srcPos", aPlayer.Position, "dstPos", dstPos);
            aPlayer.Position = dstPos;
            return true;
        }
    }
}
