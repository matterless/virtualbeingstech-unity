// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;

namespace VirtualBeings
{
    /// <summary>
    /// Attribute used to tell inspector decorators which type they should inspect"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DecoratorForAttribute : Attribute
    {
        public readonly Type type;

        public DecoratorForAttribute(Type type)
        {
            this.type = type;
        }
    }
}