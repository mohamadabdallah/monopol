using System;
using System.Collections.Generic;
using System.Text;
using MonopolyServer.Model.Card;
using MonopolyServer.Model.Field;
using MonopolyServer.Model.Property;

namespace MonopolyServer.Model
{
    class Board
    {
        //Stan planszy, klient powinie go zmieniaŠ tylko poťrednio, wysy│aj╣c polecenia przez sieŠ
        public Field.Field[] Fields;
        public List<Card.Card> CommunityChest;
        public List<Card.Card> CommunityChestUsed = new List<Card.Card>();
        public List<Card.Card> Chances;
        public List<Card.Card> ChancesUsed = new List<Card.Card>();
        public Dictionary<string, PropertyGroup> PropertyGroups = new Dictionary<string, PropertyGroup>();

        public Board()
        {
            /*
       Community Chest: 
          * Advance to Go (Collect $200)
          * Bank error in your favor – collect $200 [£200]
          * Doctor's fee – Pay $50 [£50]
          * Get out of jail free – this card may be kept until needed, or sold
          * Go to jail – go directly to jail – Do not pass Go, do not collect $200 [£200]
          * Grand opera Night – collect $50 from every player for opening night seats [omitted]
          * Tax refund – collect $20 [£20]
          * Life Insurance Matures – collect $100 [omitted]
          * Pay hospital $100 [£100]
          * Pay School tax of $150 [omitted]
          * Receive for services $25 [Receive interest on 7% preference shares: £25]
          * You are assessed for street repairs – $40 per house, $115 per hotel [omitted]
          * You have won second prize in a beauty contest– collect $10 [£10]
          * You inherit $100 [£100]
          * From sale of stock you get $45 [£50]
          * Xmas fund matures - collect $100 [Annuity matures Collect £100]
          * [Pay a £10 fine or take a "Chance"]
          * [Go back to Old Kent Road]
          * [Pay your insurance premium £50]
          * [It is your birthday Collect £10 from each player]
      */

            CommunityChest = new List<Card.Card>(new Card.Card[]
            {
                new AdvanceGoBackToCard(0, 0, true),
                new CollectPayCard(1, 200),
                new CollectPayCard(2, -50),
                new GetOutOfJailFreeCard(3, Card.Card.CardType.CommunityChest),
                new AdvanceGoBackToCard(4, 30, false),
                new CollectPayEachPlayerCard(5, 50),
                new CollectPayCard(6, 20),
                new CollectPayCard(7, 100),
                new CollectPayCard(8, -100),
                new CollectPayCard(9, -150),
                new CollectPayCard(10, 25),
                new RepairsCard(11, 40, 115),
                new CollectPayCard(12, 10),
                new CollectPayCard(13, 100),
                new CollectPayCard(14, 45),
                new CollectPayCard(15, 100)
            });

            RandomizeCards(CommunityChest);

            /* Chances:
            * ADVANCE TO GO (COLLECT $200) [UK card says simply "Advance to 'GO'"]
            * ADVANCE TO ILLINOIS AVE. [ADVANCE TO TRAFALGAR SQUARE]
            * ADVANCE TOKEN TO NEAREST UTILITY. IF UNOWNED you may buy it from bank. IF OWNED, throw dice and pay owner a total ten times the amount thrown. [Does not exist]
            * Advance token to the nearest Railroad and pay owner Twice the Rental to which he is otherwise entitled. If Railroad is unowned, you may buy it from the Bank. [Two such cards in the U.S. version; does not exist in the UK version]
            * ADVANCE TO ST. CHARLES PLACE [Pall Mall] IF YOU PASS GO, COLLECT $200
            * BANK PAYS YOU DIVIDEND OF $50 [£50]
            * GO BACK 3 SPACES
            * GO DIRECTLY TO JAIL DO NOT PASS GO, DO NOT COLLECT $200 [GO TO JAIL MOVE DIRECTLY TO JAIL DO NOT PASS "GO" DO NOT COLLECT £200]
            * Make General Repairs On All Your Property [HOUSES] FOR EACH HOUSE PAY $25 [£25] FOR EACH HOTEL $100 [£100]
            * PAY POOR TAX OF $15 [Does not exist]
            * TAKE A RIDE ON THE READING [Marylebone Station] IF YOU PASS GO COLLECT $200
            * TAKE A WALK ON THE BOARD WALK ADVANCE TOKEN TO BOARD WALK [ADVANCE TO MAYFAIR]
            * THIS CARD MAY BE KEPT UNTIL NEEDED OR SOLD GET OUT OF JAIL FREE [GET OUT OF JAIL FREE This card may be kept until needed or sold]
            * You Have Been ELECTED CHAIRMAN OF THE BOARD PAY EACH PLAYER $50 [Does not exist]
            * YOUR BUILDING AND LOAN MATURES COLLECT $150 [£150]
            * [PAY SCHOOL FEES OF £150]
            * [YOU ARE ASSESSED FOR STREET REPAIRS: £40 PER HOUSE £115 PER HOTEL]
            * ["DRUNK IN CHARGE" FINE £20]
            * [SPEEDING FINE £15]
            * [YOU HAVE WON A CROSSWORD COMPETITION COLLECT £100]
            */

            Chances = new List<Card.Card>(new Card.Card[]
            {
                new AdvanceGoBackToCard(0, 0, true),
                new AdvanceGoBackToCard(1, 24, true),
                new AdvanceToNearestUtilityCard(2),
                new AdvanceToNearestRailroadCard(3),
                new AdvanceGoBackToCard(4, 11, true),
                new CollectPayCard(5, 50),
                new GoBackThreeSpacesCard(6),
                new AdvanceGoBackToCard(7, 30, false),
                new RepairsCard(8, 25, 100),
                new CollectPayCard(9, -15),
                new AdvanceGoBackToCard(10, 5, true),
                new AdvanceGoBackToCard(11, 39, false),
                new GetOutOfJailFreeCard(12, Card.Card.CardType.Chance),
                new CollectPayEachPlayerCard(13, -50),
                new CollectPayCard(14, 150),
                new AdvanceToNearestRailroadCard(15)
            });

            RandomizeCards(Chances);




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

            Fields = new Field.Field[]
            {
                new DoNothingField(0), 
                new City(1, "1-1", PropertyGroups["1"], 60, 50, new int[]{2, 10, 30, 90, 160, 250}),
                new ChanceCommunityChestField(2, CommunityChest, CommunityChestUsed),
                new City(3, "1-2", PropertyGroups["1"], 60, 50, new int[]{4, 20, 60, 180, 320, 450}),
                new IncomeTaxField(4),
                new RailRoad(5, "Railroads-1", PropertyGroups["Railroads"]),
                new City(6, "2-1", PropertyGroups["2"], 100, 50, new int[]{6, 30, 90, 270, 400, 550}),
                new ChanceCommunityChestField(7, Chances, ChancesUsed),
                new City(8, "2-2", PropertyGroups["2"], 100, 50, new int[]{6, 30, 90, 270, 400, 550}),
                new City(9, "2-3", PropertyGroups["2"], 120, 50, new int[]{8, 40, 100, 300, 450, 600}),
                new DoNothingField(10), 
                new City(11, "3-1", PropertyGroups["3"], 140, 100, new int[]{10, 50, 150, 450, 625, 750}),
                new Utility(12, "Electric Company", PropertyGroups["Utilities"]),
                new City(13, "3-2", PropertyGroups["3"], 140, 100, new int[]{10, 50, 150, 450, 625, 750}),
                new City(14, "3-3", PropertyGroups["3"], 160, 100, new int[]{12, 60, 180, 500, 700, 900}),
                new RailRoad(15, "Railroads-2", PropertyGroups["Railroads"]),
                new City(16, "4-1", PropertyGroups["4"], 180, 100, new int[]{14, 70, 200, 550, 750, 950}),
                new ChanceCommunityChestField(17, CommunityChest, CommunityChestUsed),
                new City(18, "4-2", PropertyGroups["4"], 180, 100, new int[]{14, 70, 200, 550, 750, 950}),
                new City(19, "4-3", PropertyGroups["4"], 200, 100, new int[]{16, 80, 220, 600, 800, 1000}),
                new DoNothingField(20),
                new City(21, "5-1", PropertyGroups["5"], 220, 150, new int[]{18, 90, 250, 700, 875, 1050}),
                new ChanceCommunityChestField(22, Chances, ChancesUsed),
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
                new ChanceCommunityChestField(33, CommunityChest, CommunityChestUsed),
                new City(34, "7-3", PropertyGroups["7"], 320, 200, new int[]{28, 150, 450, 1000, 1200, 1400}),
                new RailRoad(35, "Railroads-4", PropertyGroups["Railroads"]),
                new ChanceCommunityChestField(36, Chances, ChancesUsed),
                new City(37, "8-1", PropertyGroups["8"], 350, 200, new int[]{35, 175, 500, 1100, 1300, 1500}),
                new LuxuryTaxField(38),
                new City(39, "8-2", PropertyGroups["8"], 400, 200, new int[]{50, 200, 600, 1400, 1700, 2000})



            };


        }

        public static void RandomizeCards(List<Card.Card> aCards)
        {
            Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                int p1 = r.Next(0, aCards.Count - 1);
                int p2 = r.Next(0, aCards.Count - 1);

                Card.Card c = aCards[p1];
                aCards[p1] = aCards[p2];
                aCards[p2] = c;
            }
        }
    }
}
