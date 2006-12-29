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

namespace Monopoly
{


    abstract class Field
    {
        public string Name;
        public int Id;

        //public delegate void ActionDelegate(Field aThis, Dictionary<string, object> aParams);
        //public ActionDelegate Action;

        //public abstract void ServerAction(ServerPlayer aPlayer, GameServer aServer, int aDice1, int aDice2);
    }

    abstract class Property : Field
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
        
    }

    class PropertyGroup
    {
        public PropertyGroup(string aName)
        {
            Name = aName;
        }

        public string Name;
        public Player Monopolist
        {
            get
            {
                Player o = null;
                foreach (Property p in Properties)
                {
                    if (o == null)
                        o = p.Owner;
                    else if (o != p.Owner || p.Mortgaged)
                        return null;
                }

                return o;
            }
        }
        
        public List<Property> Properties = new List<Property>();
    }


    class City : Property
    {
        public int NumHouses = 0; //5 = 1 hotel itd.
        public int[] Rents;
        public int PricePerHouse;
        public override int CalculateRent(int aDice1, int aDice2)
        {
            if (Owner == null || Mortgaged)
                return 0;

            return (NumHouses / Rents.Length) * Rents[Rents.Length - 1] //hotele
                + (NumHouses % Rents.Length) * Rents[NumHouses % Rents.Length]; //domy
        }

        public override int TotalValue
        {
            get
            {
                return Price + NumHouses * PricePerHouse;
            }
        }

        public City(int aId, string aName, PropertyGroup aCountry, int aPrice, int aPricePerHouse, int[] aRents)
        {
            Id = aId;
            Name = aName;
            Price = aPrice;
            Group = aCountry;
            aCountry.Properties.Add(this);
            PricePerHouse = aPricePerHouse;
            Rents = aRents;
        }
    }

    class RailRoad : Property
    {
        public override int CalculateRent(int aDice1, int aDice2)
        {
            if (Owner == null || Mortgaged)
                return 0;

            int ownedRailroads = 0;
            foreach (Property p in Group.Properties)
                if (p.Owner == Owner)
                    ownedRailroads++;

            switch (ownedRailroads)
            {
                case 1: return 25;
                case 2: return 50;
                case 3: return 100;
                case 4: return 200;
            }

            throw new Exception("Internal error");
            //return 0;
        }

        public RailRoad(int aId, string aName, PropertyGroup aGroup)
        {
            Id = aId;
            Name = aName;
            Group = aGroup;
            Price = 200;
        }
    }

    class Utility : Property
    {
        public Utility(int aId, string aName, PropertyGroup aGroup)
        {
            Id = aId;
            Name = aName;
            Price = 150;
            Group = aGroup;
        }

        public override int CalculateRent(int aDice1, int aDice2)
        {
            if (Owner == null || Mortgaged)
                return 0;

            if (Group.Monopolist == Owner)
                return (aDice1 + aDice2) * 10;
            else
                return (aDice1 + aDice2) * 4;
        }


    }

    class DoNothingField : Field
    {
        public DoNothingField(int aId)
        {
            Id = aId;
        }
    }


    /*
    class Card
    {
        public abstract void ServerAction(Player aPlayer, int aDice1, int aDice2);
    }

    class GetOutOfJailFreeCard
    {
        public virtual void ServerAction(Player aPlayer, int aDice1, int aDice2)
        {
        }
    }
      */

    class Board
    {
        //Stan planszy, klient powinie go zmieniaŠ tylko poťrednio, wysy│aj╣c polecenia przez sieŠ
        public Field[] Fields;
        public List<Card> CommunityChest;
        public List<Card> CommunityChestUsed;
        public List<Card> Chances;
        public List<Card> ChancesUsed;
        public Dictionary<string, PropertyGroup> PropertyGroups = new Dictionary<string, PropertyGroup>();

        public Board()
        {
            PropertyGroups.Add("1", new PropertyGroup("1"));
            PropertyGroups.Add("2", new PropertyGroup("2"));
            PropertyGroups.Add("3", new PropertyGroup("3"));
            PropertyGroups.Add("4", new PropertyGroup("4"));
            PropertyGroups.Add("5", new PropertyGroup("5"));
            PropertyGroups.Add("6", new PropertyGroup("6"));
            PropertyGroups.Add("7", new PropertyGroup("7"));
            PropertyGroups.Add("8", new PropertyGroup("8"));
            PropertyGroups.Add("Railroads", new PropertyGroup("Railroads"));
            PropertyGroups.Add("Utilities", new PropertyGroup("Utilities"));

            Fields = new Field[]
            {
                new DoNothingField(0), 
                new City(1, "1-1", PropertyGroups["1"], 60, 50, new int[]{2, 10, 30, 90, 160, 250}),
                new ChanceField(2, CommunityChest, CommunityChestUsed),
                new City(3, "1-2", PropertyGroups["1"], 60, 50, new int[]{4, 20, 60, 180, 320, 450}),
                new IncomeTaxField(4),
                new RailRoad(5, "Railroads-1", PropertyGroups["Railroads"]),
                new City(6, "2-1", PropertyGroups["2"], 100, 50, new int[]{6, 30, 90, 270, 400, 550}),
                new ChanceField(7, Chances, ChancesUsed),
                new City(8, "2-2", PropertyGroups["2"], 100, 50, new int[]{6, 30, 90, 270, 400, 550}),
                new City(9, "2-3", PropertyGroups["2"], 120, 50, new int[]{8, 40, 100, 300, 450, 600}),
                new DoNothingField(10), 
                new City(11, "3-1", PropertyGroups["3"], 140, 100, new int[]{10, 50, 150, 450, 625, 750}),
                new Utility(12, "Electric Company", PropertyGroups["Utilities"]),
                new City(13, "3-2", PropertyGroups["3"], 140, 100, new int[]{10, 50, 150, 450, 625, 750}),
                new City(14, "3-3", PropertyGroups["3"], 160, 100, new int[]{12, 60, 180, 500, 700, 900}),
                new RailRoad(15, "Railroads-2", PropertyGroups["Railroads"]),
                new City(16, "4-1", PropertyGroups["4"], 180, 100, new int[]{14, 70, 200, 550, 750, 950}),
                new ChanceField(17, CommunityChest, CommunityChestUsed),
                new City(18, "4-2", PropertyGroups["4"], 180, 100, new int[]{14, 70, 200, 550, 750, 950}),
                new City(19, "4-3", PropertyGroups["4"], 200, 100, new int[]{16, 80, 220, 600, 800, 1000}),
                new DoNothingField(20),
                new City(21, "5-1", PropertyGroups["5"], 220, 150, new int[]{18, 90, 250, 700, 875, 1050}),
                new ChanceField(22, Chances, ChancesUsed),
                new City(23, "5-2", PropertyGroups["5"], 220, 150, new int[]{18, 90, 250, 700, 875, 1050}),
                new City(24, "5-3", PropertyGroups["5"], 240, 150, new int[]{20, 100, 300, 750, 925, 1100}),
                new RailRoad(25, "Railroads-3", PropertyGroups["Railroads"]),
                new City(26, "6-1", PropertyGroups["6"], 260, 150, new int[]{22, 110, 330, 800, 975, 1150}),
                new City(27, "6-2", PropertyGroups["6"], 260, 150, new int[]{22, 110, 330, 800, 975, 1150}),
                new Utility(28, "Waterworks", PropertyGroups["Utilities"]),
                new City(29, "6-3", PropertyGroups["6"], 280, 150, new int[]{24, 120, 360, 850, 1025, 1200}),
                new GoToJailField(30),
                new City(31, "7-1", PropertyGroups["7"], 300, 200, new int[]{26, 130, 390, 900, 1100, 1275}),
                new City(32, "7-2", PropertyGroups["7"], 300, 200, new int[]{26, 130, 390, 900, 1100, 1275}),
                new ChanceField(33, CommunityChest, CommunityChestUsed),
                new City(34, "7-3", PropertyGroups["7"], 320, 200, new int[]{28, 150, 450, 1000, 1200, 1400}),
                new RailRoad(35, "Railroads-4", PropertyGroups["Railroads"]),
                new ChanceField(36, Chances, ChancesUsed),
                new City(37, "8-1", PropertyGroups["8"], 350, 200, new int[]{35, 175, 500, 1100, 1300, 1500}),
                new LuxuryTaxField(38),
                new City(39, "8-2", PropertyGroups["8"], 400, 200, new int[]{50, 200, 600, 1400, 1700, 2000})
            };
        }

    }

    class ChanceField : Field
    {
        public List<Card> Cards;
        public List<Card> UsedCards;

        public ChanceField(int aId, List<Card> aCards, List<Card> aUsedCards)
        {
            Id = aId;
            Cards = aCards;
            UsedCards = aUsedCards;
        }
    }

    class IncomeTaxField : Field
    {
        public IncomeTaxField(int aId)
        {
            Id = aId;
        }
    }

    class LuxuryTaxField : Field
    {
        public LuxuryTaxField(int aId)
        {
            Id = aId;
        }
    }

    class GoToJailField : Field
    {
        public GoToJailField(int aId)
        {
            Id = aId;
        }
    }



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
        public int TurnsToLeaveJail = 0; //0->na wolnoťci
        public List<Property> OwnedProperties = new List<Property>();
        List<GetOutOfJailFreeCard> GetOutOfJailFreeCards = new List<GetOutOfJailFreeCard>();
    }

    class Card
    {
        void DoAction()
        {
        }
    }

    class GetOutOfJailFreeCard : Card
    {
    }

}
