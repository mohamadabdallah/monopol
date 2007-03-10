using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.GameServer;

namespace MonopolyServer.Model.Field
{
    abstract class Field
    {
        //public enum FieldType {Go, City, CommunityChest, IncomeTax, Railroad, Chance, Jail, Utility,
        //  FreeParking, GoToJail, LuxuryTax}
        //public FieldType Type;
        public string Name;
        public int Id;

        //public delegate Action(

        public abstract void ServerAction(ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2);
    }
}
