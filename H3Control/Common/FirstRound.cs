using System;
using System.Collections.Generic;

namespace H3Control.Common
{
    public class FirstRound
    {
        static readonly Dictionary<string, int> Cache = new Dictionary<string, int>(EqualityComparer<string>.Default);
        static readonly object Sync = new object();

        public static void Only(string key, RoundCounter when, Action action)
        {
            Try(key, (int)when, action);
        }

        public static void Only(string key, int countNumber, Action action)
        {
            Try(key, countNumber, action);
        }

        public static void Only(string key, Action action)
        {
            Try(key, (int)RoundCounter.First, action);
        }

        public static void OnlyTwice(string key, Action action)
        {
            Try(key, (int)RoundCounter.Twice, action);
        }

        private static void Try(string key, int countNumber, Action action)
        {
            int number;
            lock (Sync)
                if (!Cache.TryGetValue(key, out number))
                    number = 0;

            if (number > 10) return;
            number++;
            if (number <= countNumber)
                action();

            lock (Sync)
            {
                if (!Cache.TryGetValue(key, out number))
                    number = 0;

                Cache[key] = ++number;
            }
        }

    }

    public enum RoundCounter
    {
        Never = 0,
        First = 1,
        Twice = 2
    }
}
