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
    /// Data containing the class members along with their associated attributes
    /// </summary>
    public struct MemberDecoration
    {
        public MemberInfo MemberInfo;
        public IReadOnlyList<Attribute> Attributes;
    }

    /// <summary>
    /// <para>Interface describing the inspector decorator</para> 
    /// <para>Its purpouse is give us the capability to override or place attributes for cetains class members without the need to edit the class directly</para>
    /// </summary>
    public interface IInspectorDecorator
    {
        bool AcceptSubclasses { get; }
        IEnumerable<MemberDecoration> GetMemberDecorations(Type t);
    }
}
