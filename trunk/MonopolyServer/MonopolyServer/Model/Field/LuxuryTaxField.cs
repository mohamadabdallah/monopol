using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Field
{
    class LuxuryTaxField : Field
    {
        public override void ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aPlayer.Money -= 75;
            aServer.SendMessage("luxuryTax", "player", aPlayer.Nickname, "tax", 75);
        }

        public LuxuryTaxField(int aId)
        {
            Id = aId;
        }
    }
}
