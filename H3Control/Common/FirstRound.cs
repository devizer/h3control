using System;
using System.Collections.Generic;

namespace H3Control.Common
{
    public class FirstRound
    {
        static Dictionary<object, short> _Cache = new Dictionary<object, short>(EqualityComparer<object>.Default);
        static readonly object Sync = new object();

        public static void Only(string key, RoundCounter when, Action action)
        {
            Try(key, when, action);
        }

        public static void Only(string key, Action action)
        {
            Try(key, RoundCounter.First, action);
        }

        public static void Only(Action action)
        {
            Try(action, RoundCounter.First, action);
        }

        public static void OnlyTwice(Action action)
        {
            Try(action, RoundCounter.Twice, action);
        }

        public static void Try(object key, RoundCounter when, Action action)
        {
            short number;
            lock (Sync)
                if (!_Cache.TryGetValue(key, out number))
                    number = 0;

            if (number > 10) return;
            number++;
            if (number == 1 && (when == RoundCounter.First || when == RoundCounter.Twice))
                action();

            if (number == 2 && when == RoundCounter.Twice)
                action();

            lock (Sync)
            {
                if (!_Cache.TryGetValue(key, out number))
                    number = 0;
                
                _Cache[key] = ++number;
            }
        }

    }

    public enum RoundCounter
    {
        Never,
        First,
        Twice
    }
}
