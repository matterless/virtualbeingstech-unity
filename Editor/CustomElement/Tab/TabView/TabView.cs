// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// A UI Element used to contain the content of a single tab that is parented by <see cref="TabRoot"/>
    /// </summary>
    public class TabView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }

        private const string BASE_PATH  = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/CustomElement/Tab/TabView";
        private const string UXML_PATH  = BASE_PATH + "/" + nameof(TabView) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/" + nameof(TabView) + ".uss";

        public TabView()
        {
        }
    }
}