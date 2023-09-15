// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;

namespace VirtualBeings
{
    /// <summary>
    /// Mark a field as a member of the specific tab
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TabAttribute : Attribute
    {
        public readonly string TabName;

        public TabAttribute(string tabName)
        {
            this.TabName = tabName;
        }
    }
}