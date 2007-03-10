using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Card
{
    class AdvanceGoBackToCard : Card
    {
        public AdvanceGoBackToCard(int aId, int aWhere, bool aAdvance)
        {
            Id = aId;
            Where = aWhere;
            Advance = aAdvance;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            bool passedStart = Advance && aPlayer.Position > Where;
            aServer.SendMessage("advanceGoBackToCard", "cardId", Id, "player", aPlayer.Nickname,
                "srcPos", aPlayer.Position, "dstPos", Where, "advance", Advance, "passedStart", passedStart);
            aPlayer.Position = Where;
            if (passedStart)
                aPlayer.Money += 200;

            return true;
        }

        public int Where;
        public bool Advance;
    }
}
