using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace Monopoly
{
    

    class MessageQueue
    {
        public MessageQueue(Socket aSocket)
        {
            mSocket = aSocket;
        }

        Socket mSocket;
        Queue<string> mMessages = new Queue<string>();
        List<byte> mReadBytes = new List<byte>();


        void Read()
        {            
            if (mSocket.Available > 0)
            {
                byte[] buf = new byte[mSocket.Available];
                mSocket.Receive(buf);
                mReadBytes.AddRange(buf);
            }


        Again:
            int i = 0;

            while (i < mReadBytes.Count - 1)
            {
                byte b1, b2;
                b1 = mReadBytes[i];
                b2 = mReadBytes[i + 1];
                //if (b1 == 0 && b2 == 0)
                if (b1 == 13 && b2 == 10)
                {
                    byte[] buf = new byte[i];
                    mReadBytes.CopyTo(0, buf, 0, i);
                    mReadBytes.RemoveRange(0, i + 2);


                    mMessages.Enqueue(GameServer.IsoToUnicode(buf));
                    Console.WriteLine("Message from " + mSocket.RemoteEndPoint + ":" +
                        GameServer.IsoToUnicode(buf));

                    goto Again;
                }
                i += 1;
            }
        }

        public string Pop()
        {
            Read();
            return mMessages.Dequeue();
        }

        public int Count
        {
            get
            {
                Read();
                return mMessages.Count;
            }
        }


    }


    class ServerPlayer : Player
    {
        public MessageQueue MyMessageQueue;
        public Socket Connection;
        public string SafeName
        {
            get
            {
                return Nickname != null ? Nickname : Connection.RemoteEndPoint.ToString();
            }
        }
        
    }

            


    class GameServer
    {
        //List<Player> mPlayers = new List<Player>();
        //List<List<string>> mMessages = new List<List<string>>();
        //List<MessageQueue> mMessageQueues = new List<MessageQueue>();
        public List<ServerPlayer> ServerPlayers = new List<ServerPlayer>();
        public Board GameBoard = new Board();
        uint mMaxPlayers;
        ushort mPort;
        Socket mSocket;
        string mWelcomeMessage;
        public bool GameOver;
        delegate void ActionDelegate(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2);
        Dictionary<Type, ActionDelegate> mFieldActions = new Dictionary<Type, ActionDelegate>();


        

        public GameServer(ushort aPort, uint aMaxPlayers, string aWelcomeMessage)
        {
            mPort = aPort;
            mMaxPlayers = aMaxPlayers;
            mWelcomeMessage = aWelcomeMessage;

            mFieldActions.Add(typeof(DoNothingField), DoNothingServerAction);
            mFieldActions.Add(typeof(City), PropertyServerAction);
            mFieldActions.Add(typeof(ChanceField), ChanceFieldServerAction);
            mFieldActions.Add(typeof(IncomeTaxField), IncomeTaxServerAction);
            mFieldActions.Add(typeof(Utility), PropertyServerAction);
            mFieldActions.Add(typeof(RailRoad), PropertyServerAction);
            mFieldActions.Add(typeof(GoToJailField), GoToJailServerAction);
            mFieldActions.Add(typeof(LuxuryTaxField), LuxuryTaxServerAction);
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
            foreach(ServerPlayer pi in ServerPlayers)
            {
                if(!pi.Disconnected)
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



        //Przetwarza wiadomość chat (niejako w tle). Inny rodzaj wiadomości parsuje i zwraca. Funkcja blokująca!
        public bool ProcessGetNextMessage(out ServerPlayer aFrom, out XmlElement aMsg, bool wait)
        {
            for (; ; )
            {
                //TODO: może connection test?


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
            


            //Czekamy na zgłoszenie gotowości wszystkich graczy
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

                        SendMessage(client, "<welcome serverVersion=\"" + an.Version +
                            "\" message=\"" + mWelcomeMessage + "\"/>");
                    }
                }

                ServerPlayer p; XmlElement e;
              
                if(ProcessGetNextMessage(out p, out e, false))

              
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
                            foreach(ServerPlayer p3 in ServerPlayers)
                                if(p3.Nickname == "AI1")
                                    tgc.MyPlayer = p3;
                            Thread t = new Thread(tgc.Run);
                            t.Start();
                            //////////////////////////////////


                            break;
                        case "ready":
                            if (p.Nickname == null) //nie ustawił nicka
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


            //Rozpoczynamy grę
            SendMessageToEveryone("<allReady/>");
            Console.WriteLine("Starting game!");

            ////////////////////////////////////////
            tgc.Players = new Player[ServerPlayers.Count];
            for (int i = 0; i < ServerPlayers.Count; i++)
                tgc.Players[i] = ServerPlayers[i];
            ///////////////////////////////////////

            Random rand = new Random();
            int curPlayer = rand.Next(0, ServerPlayers.Count - 1);
            while(true)
            {
                int numActive = 0;
                foreach (ServerPlayer p in ServerPlayers)
                    if (!p.Disconnected)
                        numActive++;
                if (numActive == 0)
                    return;

                ServerPlayer pi = ServerPlayers[curPlayer];

                if(pi.TurnsToLeaveJail > 0)
                    pi.TurnsToLeaveJail--;
                
                if (pi.TurnsToLeaveJail == 0 && !pi.Bankrupt && !pi.Disconnected)
                {
                    int dice1 = rand.Next(1, 6);
                    int dice2 = rand.Next(1, 6);
                    int dstPos = (pi.Position + dice1 + dice2) % 40;
                    bool passedStart = dstPos < pi.Position;
                    SendMessage("move", "player", pi.Nickname, "dice1", dice1, "dice2", dice2,
                        "srcPos", pi.Position, "dstPos", dstPos, "passedStart", passedStart);

                    pi.Position = dstPos;
                    if (passedStart)
                        pi.Money += 200;

                    Field f = GameBoard.Fields[dstPos];
                    mFieldActions[f.GetType()](f, pi, this, dice1, dice2);
                    //GameBoard.Fields[dstPos].ServerAction(pi, this, dice1, dice2);

                    //czas na kupno/sprzedaż itp.
                    SendMessage("freeMove", "player", pi.Nickname,
                        "debtToPay", pi.Money > 0 ? 0 : -pi.Money);
                    FreeMove(pi, true);
                    if (GameOver)
                        return;
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
                                    ((City)GameBoard.Fields[fieldId]).Group.Monopolist == aPi)
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

                            ///TODO: zwrócenie karty wyjścia z więzienia
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

        void ChanceFieldServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
        }
        
        void PropertyServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
            Property me = (Property)aField;
            XmlElement msg;
            if (me.Owner == null)
            {
                aServer.SendMessage("buyVisitedProperty", "player", aPlayer.Nickname, "fieldId",
                    me.Id);

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
                                    me.Owner = aPlayer;
                                    aPlayer.Money -= me.Price;
                                    aPlayer.OwnedProperties.Add(me);
                                    aServer.SendMessageToEveryone("<propertyBought player=\"" + aPlayer.Nickname +
                                        "\" fieldId=\"" + aPlayer.Position + "\" price=\"" + me.Price + "\"/>");

                                    
                                    return;
                                case "false":
                                    aServer.SendMessage("auction", "fieldId", me.Id);
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
                                                GameServer.ProtocolError(src);
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
                                        me.Owner = highestBidder;
                                        highestBidder.Money -= highestBid;
                                        highestBidder.OwnedProperties.Add(me);

                                        if (highestBidder.Money < 0)
                                        {
                                            aServer.FreeMove(highestBidder, false);
                                        }
                                    }
                                    else
                                        aServer.SendMessage("noAuctionWinner");

                                    return;
                                default:
                                    GameServer.ProtocolError(aPlayer);
                                    break;
                            }
                        }
                        else
                            GameServer.ProtocolError(aPlayer);


                        //Jeżeli są jakieś spóźnione bid'y to trzeba teraz się ich pozbyć, żeby nie było
                        //błędów protokołu
                        System.Threading.Thread.Sleep(500);
                        foreach (ServerPlayer pi in aServer.ServerPlayers)
                            while (pi.MyMessageQueue.Count > 0)
                            {
                                ServerPlayer src;
                                aServer.ProcessGetNextMessage(out src, out msg, false);
                                if (msg != null && msg.LocalName != "bid")
                                    GameServer.ProtocolError(pi);
                            }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e);
                    }

                }
            }
            else if (me.Owner != aPlayer && me.Owner.TurnsToLeaveJail == 0) //ojoj płacimy
            {
                int rent = me.CalculateRent(aDice1, aDice2);
                aServer.SendMessage("rent", "owner", me.Owner.Nickname, "player", aPlayer.Nickname,
                    "price", rent);
                aPlayer.Money -= rent;
                me.Owner.Money += rent;
            }
        }

        void IncomeTaxServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
            IncomeTaxField me = (IncomeTaxField)aField;

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
                                foreach (Property p in aPlayer.OwnedProperties)
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
                                GameServer.ProtocolError(aPlayer);
                                break;
                        }
                    }
                    else
                        GameServer.ProtocolError(aPlayer);
                }
                catch (XmlException e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        void LuxuryTaxServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
            aPlayer.Money -= 75;
            aServer.SendMessage("luxuryTax", "player", aPlayer.Nickname, "tax", 75);
        }

        void GoToJailServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
            aPlayer.TurnsToLeaveJail = 3;
            aPlayer.Position = 10;
            aServer.SendMessage("goToJail", "player", aPlayer.Nickname);
        }

        void DoNothingServerAction(Field aField, ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2)
        {
        }











        static void Main(string[] args)
        {
            Console.SetBufferSize(120, 200);
            Console.SetWindowSize(120, 40);
            GameServer gs = new GameServer(8000, 4, "Witamy na serwerze");
            gs.Run();
        }
    }

    /*
    class Field1
    {
        public delegate void ActionDelegate(Field aThis);
        public ActionDelegate Action;
        public Field1()
        {
            d1 = new Delegate(
        }
    }
    */
}


