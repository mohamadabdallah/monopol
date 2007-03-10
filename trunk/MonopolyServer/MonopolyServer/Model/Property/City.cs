using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Property
{
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
}
