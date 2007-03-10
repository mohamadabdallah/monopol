using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model
{
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
                foreach (Property.Property p in Properties)
                {
                    if (o == null)
                        o = p.Owner;
                    else if (o != p.Owner || p.Mortgaged)
                        return null;
                }

                return o;
            }
        }
        /*
                public Player GetOwner()
                {
                    Player o = null;
                    foreach (Property p in Properties)
                    {
                        if (o == null)
                            o = p.Owner;
                        else if (o != p.Owner)
                            return null;
                    }

                    return o;
                }
                */
        public List<Property.Property> Properties = new List<Property.Property>();
    }
}
