using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Property
{
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
}
