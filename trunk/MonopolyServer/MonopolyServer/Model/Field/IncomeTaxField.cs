using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MonopolyServer.Model.Field
{
    class IncomeTaxField : Field
    {
        public override void ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            aServer.SendMessage("tenPercentOr200Dollars", "player", aPlayer.Nickname);

            for (; ; )
            {
                XmlElement msg;
                try
                {
                    if (aServer.ProcessGetNextMessageFrom(aPlayer, true, out msg)
                        && msg.LocalName == "tenPercentOr200Dollars")
                    {
                        switch (msg.GetAttribute("type"))
                        {
                            case "tenPercent":
                                int taxBase = aPlayer.Money;
                                foreach (Property.Property p in aPlayer.OwnedProperties)
                                    taxBase += p.TotalValue;

                                aPlayer.Money -= taxBase / 10;

                                aServer.SendMessage("incomeTax", "player", aPlayer.Nickname, "type",
                                    "tenPercent", "tax", taxBase / 10);

                                return;
                            case "200Dollars":
                                aPlayer.Money -= 200;

                                aServer.SendMessage("incomeTax", "player", aPlayer.Nickname, "type",
                                    "200Dollars", "tax", 200);
                                return;
                            default:
                                GameServer.GameServer.ProtocolError(aPlayer);
                                break;
                        }
                    }
                    else
                        GameServer.GameServer.ProtocolError(aPlayer);
                }
                catch (XmlException e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        public IncomeTaxField(int aId)
        {
            Id = aId;
        }
    }
}
