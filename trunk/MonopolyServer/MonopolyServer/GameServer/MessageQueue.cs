using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MonopolyServer.GameServer
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
}
