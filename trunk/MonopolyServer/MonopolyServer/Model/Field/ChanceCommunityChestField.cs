using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyServer.Model.Field
{
    class ChanceCommunityChestField : Field
    {
        public override void ServerAction(GameServer.ServerPlayer aPlayer, GameServer.GameServer aServer, int aDice1, int aDice2)
        {
            if (Cards.Count == 0)
            {
                Board.RandomizeCards(UsedCards);
                Cards.InsertRange(0, UsedCards);
                UsedCards.Clear();
            }

            Card.Card c = Cards[Cards.Count - 1];
            Cards.Remove(c);

            if (c.ServerAction(aPlayer, aServer, aDice1, aDice2))
                UsedCards.Add(c);
        }

        public List<Card.Card> Cards;
        public List<Card.Card> UsedCards;

        public ChanceCommunityChestField(int aId, List<Card.Card> aCards, List<Card.Card> aUsedCards)
        {
            Id = aId;
            Cards = aCards;
            UsedCards = aUsedCards;


        }
    }
}
