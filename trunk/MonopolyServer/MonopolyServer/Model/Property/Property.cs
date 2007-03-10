using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using MonopolyServer.GameServer;
using System.Diagnostics;
using System.Net.Sockets;

namespace MonopolyServer.Model.Property
{
    abstract class Property : Field.Field
    {
        public PropertyGroup Group;
        public Player Owner = null;
        public int Price;
        public bool Mortgaged = false;
        public virtual int TotalValue
        {
            get
            {
                return Price;
            }
        }

        public int MortgageValue
        {
            get
            {
                return Price / 2;
            }
        }
        public int UnmortgageValue
        {
            get
            {
                return (int)(1.1 * (Price / 2));
            }
        }
        public abstract int CalculateRent(int aDice1, int aDice2);
        public override void ServerAction(ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            XmlElement msg;
            if (Owner == null)
            {
                aServer.SendMessage("buyVisitedProperty", "player", aPlayer.Nickname, "fieldId",
                    Id);

                for (; ; )
                {
                    try
                    {
                        if (aServer.ProcessGetNextMessageFrom(aPlayer, true, out msg) &&
                            msg.LocalName == "buyVisitedProperty")
                        {
                            switch (msg.GetAttribute("buy"))
                            {
                                case "true":
                                    Owner = aPlayer;
                                    aPlayer.Money -= Price;
                                    aPlayer.OwnedProperties.Add(this);
                                    aServer.SendMessageToEveryone("<propertyBought player=\"" + aPlayer.Nickname +
                                        "\" fieldId=\"" + aPlayer.Position + "\" price=\"" + Price + "\"/>");

                                    //foreach (Property p in Group.Properties)
                                    //  p.Owner = aPlayer.MyPlayer;

                                    return;
                                case "false":
                                    aServer.SendMessage("auction", "fieldId", Id);
                                    Stopwatch sw = new Stopwatch();
                                    bool firstTimerFired = false;
                                    bool secondTimerFired = false;
                                    sw.Start();
                                    int highestBid = 0;
                                    ServerPlayer highestBidder = null;
                                    while (sw.ElapsedMilliseconds < 15000)
                                    {
                                        ServerPlayer src; XmlElement msg2;
                                        if (aServer.ProcessGetNextMessage(out src, out msg2, false))
                                        {
                                            if (msg2.LocalName != "bid")
                                            {
                                                GameServer.GameServer.ProtocolError(src);
                                                continue;
                                            }

                                            try
                                            {
                                                int offer = (int)uint.Parse(msg2.GetAttribute("offer"));

                                                if (offer > highestBid)
                                                {
                                                    highestBid = offer;
                                                    highestBidder = src;
                                                    aServer.SendMessage("bid", "player", src.Nickname,
                                                        "offer", offer);
                                                    firstTimerFired = false;
                                                    secondTimerFired = false;
                                                    sw.Reset();
                                                    sw.Start();
                                                }

                                            }
                                            catch (FormatException e)
                                            {
                                                Console.WriteLine(e);
                                            }
                                        }

                                        long time = sw.ElapsedMilliseconds;
                                        if (time > 5000 && !firstTimerFired)
                                        {
                                            aServer.SendMessage("auctionTimer", "timer", 1);
                                            firstTimerFired = true;
                                        }
                                        if (time > 10000 && !secondTimerFired)
                                        {
                                            secondTimerFired = true;
                                            aServer.SendMessage("auctionTimer", "timer", 2);
                                        }


                                    }

                                    if (highestBidder != null)
                                    {
                                        aServer.SendMessage("auctionWinner",
                                            "player", highestBidder.Nickname,
                                            "bid", highestBid);
                                        Owner = highestBidder;
                                        highestBidder.Money -= highestBid;
                                        highestBidder.OwnedProperties.Add(this);

                                        if (highestBidder.Money < 0)
                                        {
                                            aServer.FreeMove(highestBidder, false);
                                        }
                                    }
                                    else
                                        aServer.SendMessage("noAuctionWinner");

                                    return;
                                default:
                                   GameServer.GameServer.ProtocolError(aPlayer);
                                    break;
                            }
                        }
                        else
                            GameServer.GameServer.ProtocolError(aPlayer);


                        //Je¿eli s¹ jakieœ spóŸnione bid'y to trzeba teraz siê ich pozbyæ, ¿eby nie by³o
                        //b³êdów protoko³u
                        System.Threading.Thread.Sleep(500);
                        foreach (ServerPlayer pi in aServer.ServerPlayers)
                            while (pi.MyMessageQueue.Count > 0)
                            {
                                ServerPlayer src;
                                aServer.ProcessGetNextMessage(out src, out msg, false);
                                if (msg != null && msg.LocalName != "bid")
                                    GameServer.GameServer.ProtocolError(pi);
                            }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e);
                    }

                }


            }
            else if (Owner != aPlayer && Owner.TurnsToLeaveJail == 0) //ojoj p³acimy
            {
                int rent = CalculateRent(aDice1, aDice2);
                aServer.SendMessage("rent", "owner", Owner.Nickname, "player", aPlayer.Nickname,
                    "price", rent);
                aPlayer.Money -= rent;
                Owner.Money += rent;
            }
        }
    }
}
