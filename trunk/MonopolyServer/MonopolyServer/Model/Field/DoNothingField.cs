using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Field
{
    class DoNothingField : Field
    {
        public override void ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
        }

        public DoNothingField(int aId)
        {
            Id = aId;
        }
    }
}
