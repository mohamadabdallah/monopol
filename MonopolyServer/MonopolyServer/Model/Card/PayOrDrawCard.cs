using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using MonopolyServer.Model.Field;

namespace MonopolyServer.Model.Card
{
    class PayOrDrawCard : Card
    {
        public int Amount;
        public PayOrDrawCard(int aId, int aAmount)
        {
            Amount = aAmount;
            Id = aId;
        }

        public override bool ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aServer.SendMessage("payOrDrawCard", "cardId", Id, "player", aPlayer.Nickname, "amount",
                Amount);


            XmlElement msg;
            if (aServer.ProcessGetNextMessageFrom(aPlayer, true, out msg)
                && msg.LocalName == "payOrDrawCard")
            {
                switch (msg.GetAttribute("decision"))
                {
                    case "pay":
                        aPlayer.Money -= Amount;
                        aServer.SendMessage("payOrDraw", "player", aPlayer.Nickname, "decision",
                            msg.GetAttribute("decision"));

                        break;
                    case "draw":
                        aServer.SendMessage("payOrDraw", "player", aPlayer.Nickname, "decision",
                            msg.GetAttribute("decision"));
                        ((ChanceCommunityChestField)aServer.GameBoard.Fields[aPlayer.Position]).ServerAction
                            (aPlayer, aServer, aDice1, aDice2);

                        break;
                    default:
                        GameServer.GameServer.ProtocolError(aPlayer);
                        break;
                }
            }
            else
                GameServer.GameServer.ProtocolError(aPlayer);

            return true;
        }
    }
}
