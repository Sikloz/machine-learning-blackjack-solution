﻿using System;

namespace BlackjackStrategy.Models
{
    // encapsulates one complete strategy to play Blackjack
    class Strategy : StrategyBase
    {
        public Strategy Clone()
        {
            var result = new Strategy();
            result.DeepCopy(this);

            return result;
        }

        public void Randomize()
        {
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // randomize pairs
                foreach (var pairRank in Card.ListOfRanks)
                    SetActionForPair(upcardRank, pairRank, GetRandomAction(true));

                // and soft hands
                for (int softRemainder = 2; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                    SetActionForSoftHand(upcardRank, softRemainder, GetRandomAction(false));

                // and hard hands
                for (int hardValue = 5; hardValue <= HighestHardHandValue; hardValue++)
                    SetActionForHardHand(upcardRank, hardValue, GetRandomAction(false));
            }
        }

        public void Mutate(double impact)
        {
            // impact is the % of cells that should be mutated

            // randomly set cells in each of the arrays
            int NumPairMutations = (int) (100F * impact);     // 10 possible holdings x 10 upcards
            int NumSoftMutations = (int) (80F * impact);     // 8 possible holdings
            int NumHardMutations = (int) (160F * impact);     // 16 possible holdings

            // pairs
            for (int i = 0; i < NumPairMutations; i++)
            {
                var upcardRank = GetRandomRankForMutation();
                var randomPairRank = GetRandomRankForMutation();
                SetActionForPair(upcardRank, randomPairRank, GetRandomAction(true));
            }

            // soft hands
            for (int i = 0; i < NumSoftMutations; i++)
            {
                var upcardRank = GetRandomRankForMutation();
                var randomRemainder = Randomizer.IntBetween(LowestSoftHandRemainder, HighestSoftHandRemainder);
                SetActionForSoftHand(upcardRank, randomRemainder, GetRandomAction(false));
            }

            // hard hands
            for (int i = 0; i < NumHardMutations; i++)
            {
                var upcardRank = GetRandomRankForMutation();
                var hardTotal = Randomizer.IntBetween(LowestHardHandValue, HighestHardHandValue);
                SetActionForHardHand(upcardRank, hardTotal, GetRandomAction(false));
            }
        }

        private Card.Ranks GetRandomRankForMutation()
        {
            Card.Ranks rank;
            do
                rank = (Card.Ranks) Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            while (rank == Card.Ranks.King || 
                   rank == Card.Ranks.Queen || 
                   rank == Card.Ranks.Jack);

            return rank;
        }

        private ActionToTake GetRandomAction(bool includeSplit)
        {
            return includeSplit ? 
                (ActionToTake)Randomizer.IntLessThan(NumActionsWithSplit) :
                (ActionToTake)Randomizer.IntLessThan(NumActionsNoSplit);
        }

        public void CrossOverWith(Strategy otherParent, Strategy child)
        {
            // here we create one child, with genetic information from each parent 
            // in proportion to their relative fitness scores
            float myScore = this.Fitness;
            float theirScore = otherParent.Fitness;
            float percentageChanceOfMine = 0;

            // it depends on whether the numbers are positive or negative
            if (myScore >= 0 && theirScore >= 0)
            {
                float totalScore = (myScore + theirScore);
                // safety check (avoiding / 0)
                if (totalScore < 0.001) totalScore = 1;
                percentageChanceOfMine = (myScore / totalScore);
            }
            else if (myScore >= 0 && theirScore < 0)
            {
                // hard to compare a positive and a negative, so let's tip the hat to Mr. Pareto
                percentageChanceOfMine = 0.8F;
            }
            else if (myScore < 0 && theirScore >= 0)
            {
                // hard to compare a positive and a negative, so let's tip the hat to Mr. Pareto
                percentageChanceOfMine = 0.2F;
            }
            else
            {
                // both negative, so use abs value and 1-(x)
                myScore = Math.Abs(myScore);
                theirScore = Math.Abs(theirScore);
                percentageChanceOfMine = 1 - (myScore / (myScore + theirScore));
            }

            foreach (var upcardRank in Card.ListOfRanks)
            {
                // populate the pairs
                foreach (var pairRank in Card.ListOfRanks)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForPair(upcardRank, pairRank, 
                        useMyAction ?
                            this.GetActionForPair(upcardRank, pairRank) :
                            otherParent.GetActionForPair(upcardRank, pairRank));
                }

                // populate the soft hands
                for (int softRemainder = LowestSoftHandRemainder; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForSoftHand(upcardRank, softRemainder, 
                        useMyAction ?
                        this.GetActionForSoftHand(upcardRank, softRemainder) :
                        otherParent.GetActionForSoftHand(upcardRank, softRemainder));
                }

                // populate the hard hands
                for (int hardValue = LowestHardHandValue; hardValue <= HighestHardHandValue; hardValue++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForHardHand(upcardRank, hardValue, 
                        useMyAction ?
                        this.GetActionForHardHand(upcardRank, hardValue) :
                        otherParent.GetActionForHardHand(upcardRank, hardValue));
                }
            }
        }
    }
}