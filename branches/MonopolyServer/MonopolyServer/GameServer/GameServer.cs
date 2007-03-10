using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using MonopolyServer.Model;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using MonopolyServer.Model.Field;
using MonopolyServer.Model.Property;
using MonopolyServer.Model.Card;

namespace MonopolyServer.GameServer
{
    class GameServer
    {
        public List<ServerPlayer> ServerPlayers = new List<ServerPlayer>();
        public MonopolyServer.Model.Board GameBoard = new Board();
        uint mMaxPlayers;
        ushort mPort;
        Socket mSocket;
        string mWelcomeMessage;
        public bool GameOver;


        public GameServer(ushort aPort, uint aMaxPlayers, string aWelcomeMessage)
        {
            mPort = aPort;
            mMaxPlayers = aMaxPlayers;
            mWelcomeMessage = aWelcomeMessage;



        }



        //NETWORK

        public static byte[] UnicodeToIso(string aStr)
        {
            Encoding iso = Encoding.GetEncoding("iso8859-2");
            byte[] buf = new byte[aStr.Length * 2];
            Buffer.BlockCopy(aStr.ToCharArray(), 0, buf, 0, aStr.Length * 2);
            return Encoding.Convert(Encoding.Unicode, iso, buf, 0, buf.Length);
        }

        public static string IsoToUnicode(byte[] aBuf)
        {
            Encoding iso = Encoding.GetEncoding("iso8859-2");
            byte[] buf = Encoding.Convert(iso, Encoding.Unicode, aBuf);
            char[] charBuf = new char[buf.Length / 2];
            Buffer.BlockCopy(buf, 0, charBuf, 0, buf.Length);
            return new string(charBuf);
        }

        public static void SendMessage(Socket aSocket, string aMsg)
        {
            byte[] buf = new byte[aMsg.Length + 2];
            byte[] encoded = UnicodeToIso(aMsg);
            Buffer.BlockCopy(encoded, 0, buf, 0, aMsg.Length);
            buf[buf.Length - 2] = 13;
            buf[buf.Length - 1] = 10;

            Console.WriteLine("Message to " + aSocket.RemoteEndPoint + ":" + aMsg);

            aSocket.Send(buf);
        }

        public void SendMessage(ServerPlayer aPlayer, string aMsg)
        {
            if (!aPlayer.Disconnected)
            {
                try
                {
                    SendMessage(aPlayer.Connection, aMsg);
                }
                catch (SocketException)
                {
                    aPlayer.Disconnected = true;
                    SendMessage("playerDisconnected", "player", aPlayer.SafeName);
                }
            }
        }

        public void SendMessageToEveryone(string aMsg)
        {
            foreach (ServerPlayer pi in ServerPlayers)
            {
                if (!pi.Disconnected)
                    SendMessage(pi, aMsg);
            }
        }

        public void SendMessage(string aType, params object[] aAttributes)
        {
            if (aAttributes.Length % 2 != 0)
                throw new ArgumentException("Number of parameters should be odd");
            string s = "<" + aType + " ";

            for (int i = 0; i < aAttributes.Length - 1; i += 2)
                s += aAttributes[i] + "=\"" + Uri.EscapeDataString(aAttributes[i + 1].ToString()) + "\" ";



            s += "/>";
            SendMessageToEveryone(s);
        }



        //Przetwarza wiadomoœæ chat (niejako w tle). Inny rodzaj wiadomoœci parsuje i zwraca. Funkcja blokuj¹ca!
        public bool ProcessGetNextMessage(out ServerPlayer aFrom, out XmlElement aMsg, bool wait)
        {
            for (; ; )
            {
                //TODO: mo¿e connection test?


                foreach (ServerPlayer pi in ServerPlayers)
                {
                    if (!pi.Disconnected)
                    {
                        try
                        {
                            while (pi.MyMessageQueue.Count != 0)
                            {
                                XmlDocument doc = new XmlDocument();

                                doc.LoadXml(pi.MyMessageQueue.Pop());


                                XmlElement e = (XmlElement)doc.FirstChild;
                                if (e.LocalName == "chat")
                                {
                                    SendMessageToEveryone("<chat from=\"" + pi.Nickname + "\" message=\"" +
                                        e.GetAttribute("message") + "\"/>");
                                }
                                else
                                {
                                    aMsg = (XmlElement)doc.FirstChild;
                                    aFrom = pi;
                                    return true;
                                }

                            }
                        }
                        catch (SocketException)
                        {
                            pi.Disconnected = true;
                            SendMessage("playerDisconnected", "player", pi.Nickname);

                        }
                        catch (Exception e)
                        {
                            //ProtocolError(pi);
                            Console.WriteLine(e);
                        }
                    }
                }

                if (wait)
                    System.Threading.Thread.Sleep(100);
                else
                {
                    aFrom = null;
                    aMsg = null;
                    return false;
                }
            }
        }

        public bool ProcessGetNextMessageFrom(ServerPlayer aFrom, bool wait, out XmlElement aMsg)
        {
            for (; ; )
            {
                foreach (ServerPlayer pi in ServerPlayers)
                {
                    if (!pi.Disconnected)
                    {
                        try
                        {
                            if (pi.MyMessageQueue.Count != 0 && pi != aFrom)
                            {
                                ProtocolError(pi);
                                continue;
                            }
                            else
                            {
                                while (pi.MyMessageQueue.Count != 0)
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(pi.MyMessageQueue.Pop());



                                    XmlElement e = (XmlElement)doc.FirstChild;
                                    if (e.LocalName == "chat")
                                    {
                                        SendMessageToEveryone("<chat from=\"" + pi.Nickname + "\" message=\"" +
                                            e.GetAttribute("message") + "\"/>");
                                    }
                                    else
                                    {
                                        aMsg = (XmlElement)doc.FirstChild;
                                        return true;
                                    }

                                }
                            }
                        }
                        catch (SocketException)
                        {
                            pi.Disconnected = true;
                            SendMessage("playerDisconnected", "player", pi.Nickname);
                            aMsg = null;
                            return false;
                        }
                        catch (Exception e)
                        {
                            //ProtocolError(pi);
                            Console.WriteLine(e);
                            break;
                        }
                    }
                }

                if (wait)
                    System.Threading.Thread.Sleep(100);
                else
                {
                    aFrom = null;
                    aMsg = null;
                    return false;
                }
            }
        }

        public static void ProtocolError(ServerPlayer aPi)
        {
            StackTrace st = new StackTrace(1, true);
            Console.WriteLine("Protocol error! (" + aPi.SafeName + ") at ");
            for (int i = 0; i < st.FrameCount - 6; i++)
            {
                //Console.WriteLine(st.GetFrame(i));                    
                StackFrame f = st.GetFrame(i);

                Console.WriteLine(f.GetMethod() + " at " + System.IO.Path.GetFileName(f.GetFileName())
                    + ":" + f.GetFileLineNumber() + ":" + f.GetFileColumnNumber());
            }
        }

        //GAME
        public void Run()
        {
            AssemblyName an = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            Console.WriteLine("Starting server ver. " + an.Version + "...");

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, mPort);
            mSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream, ProtocolType.Tcp);
            mSocket.Blocking = false;
            mSocket.Bind(localEndPoint);
            mSocket.Listen(10);


            ////////////////////////
            TestGameClient tgc = new TestGameClient(IPAddress.Loopback, 8000, "AI1");
            ////////////////////////



            //Czekamy na zg³oszenie gotowoœci wszystkich graczy
            while (true)
            {
                int numReady = 0;
                foreach (ServerPlayer pi in ServerPlayers)
                    if (pi.Disconnected || pi.Ready)
                        numReady++;
                if (numReady != 0 && numReady == ServerPlayers.Count)
                    break;

                if (ServerPlayers.Count < mMaxPlayers)
                {
                    Socket client = null;
                    try
                    {
                        client = mSocket.Accept();
                    }
                    catch (Exception)
                    {
                    }

                    if (client != null)
                    {
                        ServerPlayer pi = new ServerPlayer();
                        //pi.MyPlayer = new Player();
                        pi.MyMessageQueue = new MessageQueue(client);
                        pi.Connection = client;
                        ServerPlayers.Add(pi);

                        //AssemblyName an = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                        string txt = "<welcome serverVersion=\"" + an.Version +
                            "\" message=\"" + mWelcomeMessage + "\"> ";
                        foreach (ServerPlayer sp in ServerPlayers)
                            if (sp.Nickname != null)
                                txt += "<player nick=\"" + sp.Nickname + "\"/> ";
                        txt += "</welcome>";

                        SendMessage(client, txt);
                    }
                }

                ServerPlayer p; XmlElement e;

                if (ProcessGetNextMessage(out p, out e, false))
                {
                    switch (e.LocalName)
                    {
                        case "setNick":
                            if (e.GetAttribute("nick") == "")
                            {
                                ProtocolError(p);
                                break;
                            }

                            bool taken = false;
                            foreach (ServerPlayer p2 in ServerPlayers)
                                if (p2.Nickname == e.GetAttribute("nick"))
                                {
                                    taken = true;
                                    break;
                                }
                            if (taken)
                                SendMessage(p, "<nickTaken/>");
                            else
                            {
                                SendMessage(p, "<nickOK/>");
                                p.Nickname = e.GetAttribute("nick");
                                SendMessageToEveryone("<newPlayer nick=\"" + e.GetAttribute("nick") + "\"/>");
                            }

                            /////////////////////////////////
                            tgc.GameBoard = GameBoard;
                            foreach (ServerPlayer p3 in ServerPlayers)
                                if (p3.Nickname == "AI1")
                                    tgc.MyPlayer = p3;
                            Thread t = new Thread(tgc.Run);
                            t.Start();
                            //////////////////////////////////


                            break;
                        case "ready":
                            if (p.Nickname == null) //nie ustawi³ nicka
                            {
                                ProtocolError(p);

                                // p.Disconnected = true;
                                //p.Connection.Close();
                            }
                            else if (!p.Ready)
                            {
                                p.Ready = true;
                                //numReady++;
                                SendMessageToEveryone("<playerReady player=\"" + p.Nickname + "\"/>");
                            }

                            break;
                        case "chat":
                            SendMessageToEveryone("<chat from=\"" + p.Nickname + "\" message=\""
                                + e.GetAttribute("message") + "\"/>");
                            break;

                    }
                }

            }


            //Rozpoczynamy grê
            SendMessageToEveryone("<allReady/>");
            Console.WriteLine("Starting game!");

            ////////////////////////////////////////
            tgc.Players = new Player[ServerPlayers.Count];
            for (int i = 0; i < ServerPlayers.Count; i++)
                tgc.Players[i] = ServerPlayers[i];
            ///////////////////////////////////////

            Random rand = new Random();
            int curPlayer = rand.Next(0, ServerPlayers.Count - 1);
            while (true)
            {
                int numActive = 0;
                foreach (ServerPlayer p in ServerPlayers)
                    if (!p.Disconnected)
                        numActive++;
                if (numActive == 0)
                    return;

                ServerPlayer pi = ServerPlayers[curPlayer];

                if (pi.TurnsToLeaveJail > 0)
                    pi.TurnsToLeaveJail--;

                if (!pi.Bankrupt && !pi.Disconnected)
                {
                    int dice1 = rand.Next(1, 6);
                    int dice2 = rand.Next(1, 6);
                    int dstPos = (pi.Position + dice1 + dice2) % 40;

                    if (pi.TurnsToLeaveJail != 0)
                    {
                        bool freed = dice1 == dice2;
                        SendMessage("jail", "player", pi.Nickname, "dice1", dice1, "dice2", dice2,
                                "freed", freed);

                        if (freed)
                            pi.TurnsToLeaveJail = 0;
                    }

                    if (pi.TurnsToLeaveJail == 0)
                    {

                        bool passedStart = dstPos < pi.Position;
                        SendMessage("move", "player", pi.Nickname, "dice1", dice1, "dice2", dice2,
                            "srcPos", pi.Position, "dstPos", dstPos, "passedStart", passedStart);

                        pi.Position = dstPos;
                        if (passedStart)
                            pi.Money += 200;

                        Field f = GameBoard.Fields[dstPos];
                        GameBoard.Fields[dstPos].ServerAction(pi, this, dice1, dice2);

                        //czas na kupno/sprzeda¿ itp.
                        SendMessage("freeMove", "player", pi.Nickname,
                            "debtToPay", pi.Money > 0 ? 0 : -pi.Money);
                        FreeMove(pi, true);


                        //czekamy na gotowoœæ
                        foreach (ServerPlayer sp in ServerPlayers)
                            sp.Ready = false;
                        SendMessage("areYouReady");
                        for (; ; )
                        {
                            ServerPlayer from;
                            XmlElement msg;
                            if (ProcessGetNextMessage(out from, out msg, true) && msg.LocalName == "ready")
                            {
                                from.Ready = true;
                                foreach (ServerPlayer sp in ServerPlayers)
                                    if (!sp.Ready)
                                        continue;

                                break;
                            }
                            else
                                ProtocolError(from);
                        }


                        if (GameOver)
                            return;
                    }



                }

                curPlayer = (curPlayer + 1) % ServerPlayers.Count;
            }


        }

        public void FreeMove(ServerPlayer aPi, bool aCanBuy)
        {
            for (; ; )
            {
                bool done = false;
                XmlElement msg;
                if (ProcessGetNextMessageFrom(aPi, true, out msg))
                {
                    try
                    {
                        if (!aCanBuy && (msg.LocalName == "buyHouses" || msg.LocalName == "unmortgage"))
                        {
                            ProtocolError(aPi);
                            continue;
                        }

                        switch (msg.LocalName)
                        {
                            case "done":
                                SendMessage("done", "player", aPi.Nickname);
                                done = true;
                                break;
                            case "buyHouses":
                                int num = (int)uint.Parse(msg.GetAttribute("number"));
                                int fieldId = (int)uint.Parse(msg.GetAttribute("fieldId"));
                                if (GameBoard.Fields[fieldId].GetType() == typeof(City) &&
                                    !((City)GameBoard.Fields[fieldId]).Mortgaged &&
                                    ((City)GameBoard.Fields[fieldId]).Group.Monopolist == aPi &&
                                    ((City)GameBoard.Fields[fieldId]).NumHouses + num <= 5)
                                {
                                    City c = (City)GameBoard.Fields[fieldId];
                                    c.NumHouses += num;
                                    aPi.Money -= num * c.PricePerHouse;
                                    SendMessage("housesBought", "player", aPi.Nickname,
                                        "fieldId", fieldId, "number", num, "price", num * c.PricePerHouse);
                                }
                                else
                                    Console.WriteLine("Protocol error! (" + aPi.Nickname + ") "
                                        + new StackTrace(true));
                                break;
                            case "sellHouses":
                                num = (int)uint.Parse(msg.GetAttribute("number"));
                                fieldId = (int)uint.Parse(msg.GetAttribute("fieldId"));
                                if (GameBoard.Fields[fieldId].GetType() == typeof(City))
                                {
                                    City c = (City)GameBoard.Fields[fieldId];
                                    if (!c.Mortgaged && c.Group.Monopolist == aPi && c.NumHouses >= num)
                                    {
                                        c.NumHouses -= num;
                                        aPi.Money += (int)(0.5 * num * c.PricePerHouse);
                                        SendMessage("housesSold", "player", aPi.Nickname,
                                            "fieldId", fieldId, "number", num, "price",
                                                (int)(0.5 * num * c.PricePerHouse));
                                    }
                                    else
                                        ProtocolError(aPi);
                                }
                                else
                                    ProtocolError(aPi);
                                break;
                            case "mortgage":
                                int propertyId = (int)uint.Parse(msg.GetAttribute("propertyId"));
                                if (GameBoard.Fields[propertyId].GetType().IsSubclassOf(typeof(Property)))
                                {
                                    Property p = ((Property)GameBoard.Fields[propertyId]);
                                    bool hasHouses = p.GetType() == typeof(City) && ((City)p).NumHouses > 0;
                                    if (p.Owner == aPi && !p.Mortgaged && !hasHouses)
                                    {
                                        SendMessage("propertyMortgaged", "propertyId", propertyId, "player",
                                            aPi.Nickname, "mortgage", p.MortgageValue);
                                        p.Mortgaged = true;
                                        aPi.Money += p.MortgageValue;
                                    }
                                    else
                                        ProtocolError(aPi);

                                }
                                else
                                    ProtocolError(aPi);
                                break;
                            case "unmortgage":
                                propertyId = (int)uint.Parse(msg.GetAttribute("propertyId"));
                                if (GameBoard.Fields[propertyId].GetType().IsSubclassOf(typeof(Property)))
                                {
                                    Property p = ((Property)GameBoard.Fields[propertyId]);
                                    if (p.Owner == aPi && p.Mortgaged)
                                    {

                                        SendMessage("propertyUnmortgaged", "propertyId", propertyId, "player",
                                            aPi.Nickname, "mortgage", p.UnmortgageValue);
                                        p.Mortgaged = true;
                                        aPi.Money -= p.UnmortgageValue;
                                    }
                                    else
                                        ProtocolError(aPi);

                                }
                                else
                                    ProtocolError(aPi);
                                break;
                            case "offerProperty":
                                ServerPlayer player = FindPlayer(msg.GetAttribute("player"));
                                if (player == null)
                                {
                                    ProtocolError(aPi);
                                    break;
                                }
                                int offer = (int)uint.Parse(msg.GetAttribute("offer"));
                                propertyId = (int)uint.Parse(msg.GetAttribute("propertyId"));

                                Property property = null;
                                foreach (Property p in aPi.OwnedProperties)
                                    if (p.Id == propertyId)
                                        property = p;

                                if (property == null)
                                {
                                    ProtocolError(aPi);
                                    break;
                                }

                                SendMessage("propertyOffer", "offerer", aPi.Nickname,
                                    "player", player.Nickname, "offer", offer,
                                    "propertyId", propertyId);
                                for (; ; )
                                {
                                    XmlElement msg2;
                                    if (ProcessGetNextMessageFrom(aPi, true, out msg2))
                                    {
                                        if (msg2.LocalName == "propertyOffer")
                                        {
                                            if (msg2.GetAttribute("accepted") == "true")
                                            {
                                                aPi.Money += offer;
                                                aPi.OwnedProperties.Remove(property);

                                                property.Owner = player;
                                                player.Money -= offer;
                                                player.OwnedProperties.Add(property);

                                                SendMessage("offerAccepted", "offerer", aPi.Nickname,
                                                    "player", player.Nickname, "offer", offer,
                                                    "propertyId", propertyId, "accepted", true);

                                                if (player.Money < 0)
                                                    FreeMove(player, false);
                                            }
                                            else
                                                SendMessage("offerAccepted", "offerer", aPi.Nickname,
                                                    "player", player.Nickname, "offer", offer,
                                                    "propertyId", propertyId, "accepted", false);

                                        }
                                        else
                                            ProtocolError(aPi);
                                    }
                                    else
                                        break;
                                }
                                break;

                            case "offerGetOutOfJailCard":
                                player = FindPlayer(msg.GetAttribute("player"));
                                if (player == null)
                                {
                                    ProtocolError(aPi);
                                    break;
                                }
                                offer = (int)uint.Parse(msg.GetAttribute("offer"));
                                int cardId = (int)uint.Parse(msg.GetAttribute("cardId"));

                                GetOutOfJailFreeCard card = null;
                                foreach (GetOutOfJailFreeCard c in aPi.GetOutOfJailFreeCards)
                                    if (c.Id == cardId)
                                        card = c;

                                if (card == null)
                                {
                                    ProtocolError(aPi);
                                    break;
                                }

                                SendMessage("getOutOfJailFreeCardOffer", "offerer", aPi.Nickname,
                                    "player", player.Nickname, "offer", offer,
                                    "cardId", cardId);

                                for (; ; )
                                {
                                    XmlElement msg2;
                                    if (ProcessGetNextMessageFrom(aPi, true, out msg2))
                                    {
                                        if (msg2.LocalName == "getOutOfJailFreeCardOffer")
                                        {
                                            if (msg2.GetAttribute("accepted") == "true")
                                            {
                                                aPi.Money += offer;
                                                aPi.GetOutOfJailFreeCards.Remove(card);


                                                player.Money -= offer;
                                                player.GetOutOfJailFreeCards.Add(card);

                                                SendMessage("cardOfferAccepted", "offerer", aPi.Nickname,
                                                    "player", player.Nickname, "offer", offer,
                                                    "cardId", cardId, "accepted", true);

                                                if (player.Money < 0)
                                                    FreeMove(player, false);
                                            }
                                            else
                                                SendMessage("cardOfferAccepted", "offerer", aPi.Nickname,
                                                    "player", player.Nickname, "offer", offer,
                                                    "cardId", cardId, "accepted", false);

                                        }
                                        else
                                            ProtocolError(aPi);
                                    }
                                    else
                                        break;
                                }
                                break;

                            default:
                                ProtocolError(aPi);
                                break;
                        }
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine(e);
                    }

                    if (done)
                    {
                        if (aPi.Money < 0)
                        {
                            SendMessage("bankruptcy", "player", aPi.Nickname);
                            aPi.Bankrupt = true;
                            foreach (Property p in aPi.OwnedProperties)
                            {
                                p.Owner = null;
                                p.Mortgaged = false;
                                if (p.GetType() == typeof(City))
                                    ((City)p).NumHouses = 0;
                            }

                            ///TODO: zwrócenie karty wyjœcia z wiêzienia
                        }

                        int numBaunkrupts = 0;
                        ServerPlayer notBankrupt = null;
                        foreach (ServerPlayer p in ServerPlayers)
                            if (p.Disconnected || p.Bankrupt)
                                numBaunkrupts++;
                            else
                                notBankrupt = p;
                        if (numBaunkrupts == ServerPlayers.Count - 1)
                        {
                            SendMessage("gameOver", "winner", notBankrupt.Nickname);
                            GameOver = true;
                            return;
                        }

                        break;
                    }
                }
                else if (aPi.Disconnected)
                    done = true;
                else
                    ProtocolError(aPi);
            }

        }

        ServerPlayer FindPlayer(string aName)
        {
            foreach (ServerPlayer pi in ServerPlayers)
                if (pi.Nickname == aName)
                    return pi;

            return null;
        }


        static void Main(string[] args)
        {
            Console.SetBufferSize(120, 200);
            Console.SetWindowSize(120, 40);
            GameServer gs = new GameServer(8000, 4, "Witamy na serwerze");
            gs.Run();
        }
    }
}
