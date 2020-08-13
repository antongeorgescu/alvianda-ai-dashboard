using System;
using System.Collections.Generic;

namespace Alvianda.AI.Service.CoreNet.Extensions
{
    public static class ObjectExtensions
    {
        public static void Each<T>(IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }
    }
}
