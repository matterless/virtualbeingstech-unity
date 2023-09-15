// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;

namespace VirtualBeings
{
    /// <summary>
    /// Utility class containing a set useful reflection related methods
    /// </summary>
    public static class TypeUtils
    {
        /// <summary>
        /// <para>Returns all the base types of <paramref name="type"/></para>
        /// <para>The returned <see cref="IEnumerable{T}"/> starts from the type <paramref name="type"/> (inclusive) going all the way down until it reaches <see cref="object"/> (inclusive)</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllBaseTypes(Type type)
        {
            Type curr = type;

            do
            {
                yield return curr;
                curr = curr.BaseType;
            }
            while (curr != null);
        }
    }
}
