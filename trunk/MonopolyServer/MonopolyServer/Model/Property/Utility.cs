using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Property
{
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
}
