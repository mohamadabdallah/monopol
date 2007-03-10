using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.Model.Property;

namespace MonopolyServer.Model.Card
{
    class AdvanceToNearestUtilityCard : Card
    {
        public AdvanceToNearestUtilityCard(int aId)
        {
            Id = aId;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            for (int i = 0; i < 40; i++)
            {
                int j = (aPlayer.Position + i) % 40;
                Field.Field f = aServer.GameBoard.Fields[j];
                if (f.GetType() == typeof(Utility))
                {
                    Utility u = (Utility)f;
                    aServer.SendMessage("advanceToNearestUtilityCard", "cardId", Id, "player", aPlayer.Nickname,
                            "fieldId", u.Id);
                    if (u.Owner == null)
                        u.ServerAction(aPlayer, aServer, 0, 0);
                    else if (u.Owner != aPlayer)
                    {
                        int r = new Random().Next(1, 6);
                        aPlayer.Money -= 10 * r;
                        u.Owner.Money += 10 * r;
                        aServer.SendMessage("advanceToNearestUtilityPayment", "srcPlayer", aPlayer.Nickname,
                            "dstPlayer", u.Owner.Nickname, "dice", r, "amount", 10 * r);
                    }

                    return true;
                }

            }

            throw new Exception("Internal error");
        }
    }

}
