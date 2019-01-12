﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class Card
    {
        // the card attribute enums
        public enum Suits { Hearts, Spades, Clubs, Diamonds };
        public enum Ranks { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King };

        // this card
        public Ranks Rank { get; set; }
        public Suits Suit { get; set; }

        public Card(Ranks rankValue, Suits suit)
        {
            Rank = rankValue;
            Suit = suit;
            var c = Enum.GetValues(typeof(Ranks)).Cast<int>();
        }

        public static List<Ranks> ListOfRanks
        {
            get
            {
                var ranks = Enum.GetValues(typeof(Ranks));
                var result = new List<Ranks>();
                foreach (var rank in ranks)
                    result.Add((Ranks)rank);
                return result;
            }
        }

        public static List<Suits> ListOfSuits
        {
            get
            {
                var suits = Enum.GetValues(typeof(Suits));
                var result = new List<Suits>();
                foreach (var suit in suits)
                    result.Add((Suits)suit);
                return result;
            }
        }

        public int RankValueHigh
        {
            get
            {
                switch (Rank)
                {
                    case Ranks.Ace:
                        return 11;

                    case Ranks.King:
                    case Ranks.Queen:
                    case Ranks.Jack:
                    case Ranks.Ten:
                        return 10;

                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public int RankValueLow
        {
            get
            {
                switch (Rank)
                {
                    case Ranks.Ace:
                        return 1;

                    case Ranks.King:
                    case Ranks.Queen:
                    case Ranks.Jack:
                    case Ranks.Ten:
                        return 10;

                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public static string RankText(Ranks rank)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            return rankChars[Convert.ToInt32(rank)].ToString();
        }

        public override string ToString()
        {
            return RankText(Rank) + Suit;
        }
    }

    //=======================================================================

    class Hand
    {
        public List<Card> Cards { get; set; }

        public Hand()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public override string ToString()
        {
            List<string> cardNames = new List<string>();
            foreach (var card in Cards)
                cardNames.Add(card.ToString());

            string hand = String.Join(",", cardNames);
            return hand + " = " + HandValue().ToString();
        }

        public bool IsPair()
        {
            if (Cards.Count > 2) return false;
            return (Cards[0].Rank == Cards[1].Rank);
        }

        public bool HasSoftAce()
        {
            // first, we need to have an ace
            if (!Cards.Any(c => c.Rank == Card.Ranks.Ace)) return false;

            // and if it counts as 11 and we have a valid hand, then we have a soft ace
            int highTotal = Cards.Sum(c => c.RankValueHigh);
            return (highTotal <= 21);
        }

        public int HandValue()
        {
            // the best score possible
            int highValue = 0, lowValue = 0;
            bool aceWasUsedAsHigh = false;
            foreach (var card in Cards)
            {
                if (card.Rank == Card.Ranks.Ace)
                {
                    if (!aceWasUsedAsHigh)
                    {
                        highValue += card.RankValueHigh;
                        lowValue += card.RankValueLow;
                        aceWasUsedAsHigh = true;
                    }
                    else
                    {
                        // only one Ace can be used as high, so all others are low
                        highValue += card.RankValueLow;
                        lowValue += card.RankValueLow;
                    }

                }
                else
                {
                    highValue += card.RankValueHigh;
                    lowValue += card.RankValueLow;
                }
            }

            // if the low value > 21, then so is the high, so simply pass back the low
            if (lowValue > 21) return lowValue;

            // if the high value > 21, return the low
            if (highValue > 21) return lowValue;
            // else the high, which will be the same value as the low except when there's an Ace in the hand
            return highValue;
        }
    }

    //=======================================================================

    class MultiDeck
    {
        public List<Card> Cards { get; set; }
        private int currentCard = 0;

        public MultiDeck(int numDecks)
        {
            Cards = new List<Card>();
            for (int deckNum = 0; deckNum < numDecks; deckNum++)
            {
                Cards.AddRange(CardUtils.GetRandomDeck());
            }
        }

        public Card DealCard()
        {
            //Debug.WriteLine("Dealing card from " + this.ToString());
            Debug.Assert(currentCard < Cards.Count, "Ran out of cards to deal");

            // bad code - it doesn't deal with running out of cards
            return Cards[currentCard++];
        }

        internal Card DealNextOfRank(Card.Ranks rank)
        {
            int index = currentCard;
            while (Cards[index].Rank != rank) index++;
            var card = Cards[index];
            Cards.Remove(card);
            return card;
        }

        internal Card DealNextNotOfRank(Card.Ranks rank)
        {
            int index = currentCard;
            while (Cards[index].Rank == rank) index++;
            var card = Cards[index];
            Cards.Remove(card);
            return card;
        }

        public int CardsRemaining {
            get
            {
                return Cards.Count - currentCard;
            }
        }

        public override string ToString()
        {
            return CardsRemaining + " remaining, first cards are " +
                Cards[0].ToString() + " " + Cards[1].ToString() + " " + Cards[2].ToString();
        }

    }

    //=======================================================================

    class CardUtils
    {
        static public List<Card> GetRandomDeck()
        {
            // initially populate
            List<Card> deck = new List<Card>(52);
            foreach (var rank in Card.ListOfRanks)
                foreach (var suit in Card.ListOfSuits)
                {
                    var card = new Card(rank, suit);
                    deck.Add(card);
                }

            // then shuffle using Fisher-Yates: one pass through, swapping the current card with a random one below it
            for (int i = 51; i > 1; i--)
            {
                int swapWith = Randomizer.IntLessThan(i);

                Card hold = deck[i];
                deck[i] = deck[swapWith];
                deck[swapWith] = hold;
            }

            return deck;
        }

        static public List<Card> GetRandomCards(int numCards)
        {
            // optimized way of getting a full deck
            if (numCards == 52)
                return GetRandomDeck();

            var suits = Card.ListOfSuits;
            var ranks = Card.ListOfRanks;

            List<Card> cards = new List<Card>(numCards);
            Card.Suits suit;
            Card.Ranks rank;
            while (cards.Count < numCards)
            {
                // Generate a card and make sure we don't already have it 
                do
                {
                    suit = suits[Randomizer.IntLessThan(4)];
                    rank = ranks[Randomizer.IntLessThan(13)];
                } while (cards.Any(c => c.Rank == rank && c.Suit == suit));

                cards.Add(new Card(rank, suit));
            }
            return cards;
        }
    }
}
