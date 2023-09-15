// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace VirtualBeings
{
    /// <summary>
    /// Default fallback decorator if not specific one is present , basically returns all the existing attributes on the class fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DefaultInspectorDecorator<T> : IInspectorDecorator
    {
        bool IInspectorDecorator.AcceptSubclasses => false;

        internal DefaultInspectorDecorator()
        {
        }

        IEnumerable<MemberDecoration> IInspectorDecorator.GetMemberDecorations(Type t)
        {
            foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                yield return new MemberDecoration()
                {
                    MemberInfo = m,
                    Attributes = new List<Attribute>(m.GetCustomAttributes<Attribute>(true))
                };
            }
        }
    }
}