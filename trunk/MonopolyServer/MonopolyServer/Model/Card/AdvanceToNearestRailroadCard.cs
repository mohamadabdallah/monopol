using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.Model.Property;
using MonopolyServer.Model.Field;

namespace MonopolyServer.Model.Card
{
    class AdvanceToNearestRailroadCard : Card
    {
        public AdvanceToNearestRailroadCard(int aId)
        {
            Id = aId;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            for (int i = 0; i < 40; i++)
            {
                int j = (aPlayer.Position + i) % 40;
                Field.Field f = aServer.GameBoard.Fields[j];
                if (f.GetType() == typeof(RailRoad))
                {
                    RailRoad rr = (RailRoad)f;
                    aServer.SendMessage("advanceToNearestRailroadCard", "cardId", Id, "player", aPlayer.Nickname,
                            "fieldId", rr.Id);
                    if (rr.Owner == null)
                        rr.ServerAction(aPlayer, aServer, 0, 0);
                    else if (rr.Owner != aPlayer)
                    {
                        int rent = rr.CalculateRent(aDice1, aDice2) * 2;
                        aPlayer.Money -= rent;
                        rr.Owner.Money += rent;
                        aServer.SendMessage("advanceToNearestRailroadPayment", "srcPlayer", aPlayer.Nickname,
                            "dstPlayer", rr.Owner.Nickname, "amount", rent);
                    }

                    return true;
                }

            }

            throw new Exception("Internal error");
        }


    }

}
