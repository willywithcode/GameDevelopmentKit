namespace GameFoundation.Scripts.Extenstions
{
    using System;
    using System.Collections.Generic;

    public static class ListExtension
    {
        private static Random rng;

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (rng == null) rng = new();
            var count            = list.Count;
            while (count > 1)
            {
                --count;
                var index = rng.Next(count + 1);
                (list[index], list[count]) = (list[count], list[index]);
            }

            return list;
        }
    }
}