using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using MonopolyServer.Model.Card;

namespace MonopolyServer.Model.Field
{
    class GoToJailField : Field
    {
        public override void ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            if (aPlayer.GetOutOfJailFreeCards.Count != 0)
            {
                aServer.SendMessage("useGetOutOfJailCard", "player", aPlayer.Nickname);
                for (; ; )
                {
                    XmlElement msg;
                    if (aServer.ProcessGetNextMessageFrom(aPlayer, true, out msg)
                        && msg.LocalName == "useGetOutOfJailCard")
                    {
                        switch (msg.GetAttribute("use"))
                        {
                            case "true":
                                GetOutOfJailFreeCard c = aPlayer.GetOutOfJailFreeCards[0];
                                aPlayer.GetOutOfJailFreeCards.Remove(c);
                                if (c.Type == Card.Card.CardType.Chance)
                                    aServer.GameBoard.ChancesUsed.Add(c);
                                else
                                    aServer.GameBoard.CommunityChestUsed.Add(c);

                                aServer.SendMessage("getOutOfJailCardUsed", "player", aPlayer.Nickname);
                                return;
                            //break;
                            case "false":
                                break;
                            default:
                                GameServer.GameServer.ProtocolError(aPlayer);
                                break;
                        }



                    }
                    else
                        GameServer.GameServer.ProtocolError(aPlayer);
                }
            }

            aPlayer.TurnsToLeaveJail = 3;
            aPlayer.Position = 10;
            aServer.SendMessage("goToJail", "player", aPlayer.Nickname);
        }

        public GoToJailField(int aId)
        {
            Id = aId;
        }
    }
}
