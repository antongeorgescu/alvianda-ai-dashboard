﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
