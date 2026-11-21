using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame.Utilities
{
    public static class RNGService
    {
        private static System.Random _rng;
        private static int _seed;

        static RNGService() => Reseed(Environment.TickCount);

        public static void Reseed(int seed)
        {
            _seed = seed;
            _rng = new System.Random(seed);
        }

        public static int Seed => _seed;

        public static int Range(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return _rng.Next(max);
        }

        public static int Range(int min, int max)
        {
            if (max <= min) throw new ArgumentException("max must be greater than min");
            return _rng.Next(min, max);
        }

        public static float Value() => (float)_rng.NextDouble();

        public static T Pick<T>(IList<T> list)
        {
            if (list == null || list.Count == 0) throw new ArgumentException("list empty");
            return list[Range(list.Count)];
        }

        public static int WeightedIndex(int[] weights)
        {
            if (weights == null || weights.Length == 0) throw new ArgumentException("weights empty");

            int total = 0;
            foreach (int w in weights) total += Mathf.Max(0, w);
            if (total <= 0) return Range(weights.Length);

            int roll = Range(total);
            int cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += Mathf.Max(0, weights[i]);
                if (roll < cumulative) return i;
            }
            return weights.Length - 1;
        }
    }
}