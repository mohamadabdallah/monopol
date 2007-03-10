using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.GameServer;

namespace MonopolyServer.Model.Card
{
    class CollectPayEachPlayerCard : Card
    {
        public CollectPayEachPlayerCard(int aId, int aAmountPerPlayer)
        {
            AmountPerPlayer = aAmountPerPlayer;
            Id = aId;

        }
        public int AmountPerPlayer;

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aPlayer.Money -= AmountPerPlayer * aServer.ServerPlayers.Count;
            foreach (ServerPlayer sp in aServer.ServerPlayers)
                sp.Money += AmountPerPlayer;

            aServer.SendMessage("collectPayEachPlayerCard", "cardId", Id, "player", aPlayer.Nickname,
                "amountPerPlayer", AmountPerPlayer);

            return true;
        }
    }

}
