using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.Model.Card;

namespace MonopolyServer.Model
{
    class Player
    {
        public bool Ready = false;
        public string Nickname;
        public int Position = 0;
        public bool Bankrupt = false;
        public bool Disconnected = false;

        // public Bitmap Token;
        //public Socket Connection;
        // public PlayerController Controller;
        public int Money = 1500;
        public int TurnsToLeaveJail = 0; //0->na wolnoùci
        public List<Property.Property> OwnedProperties = new List<Property.Property>();
        public List<Model.Card.GetOutOfJailFreeCard> GetOutOfJailFreeCards = new List<GetOutOfJailFreeCard>();
    }
}
