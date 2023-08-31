// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// <para>A UI Element used to put content of a single tab ONLY in the UXML file</para>
    /// <para>During runtime, all <see cref="TabEntry"/> elements get converted into a pair of <see cref="TabButton"/> and <see cref="TabView"/> that gets parented to <see cref="TabRoot"/></para>
    /// </summary>
    public class TabEntry : VisualElement
    {
        public string Title { get; set; }
        public new class UxmlFactory : UxmlFactory<TabEntry, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription Title = new UxmlStringAttributeDescription { name = "title" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((TabEntry)ve).Title = Title.GetValueFromBag(bag, cc);
            }
        }
    }

    /// <summary>
    /// <para> A Custom UI element used to add tab functionality to the Editor</para>
    /// <para>This UI Element also supports creation in the UXML asset as well , just add the <see cref="TabRoot"/> element to the UXML and add your <see cref="TabEntry"/> as child elements to resprent each tab</para>
    /// </summary>
    public class TabRoot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabRoot, UxmlTraits> { }

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

        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/CustomElement/Tab/TabRoot";
        private const string UXML_PATH = BASE_PATH + "/" + nameof(TabRoot) + ".uxml";
        private const string USS_PATH = BASE_PATH + "/" + nameof(TabRoot) + ".uss";

        private VisualElement HeaderContainer   => this.Q<VisualElement>(nameof(HeaderContainer));
        private VisualElement ContentView       => this.Q<VisualElement>(nameof(ContentView));

        public int CurrentIndex { get; private set; }
        public int Count        => _tabButtons.Count;

        private List<TabButton> _tabButtons = new List<TabButton>();
        private List<TabView>   _tabViews   = new List<TabView>();



        public TabRoot()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
            styleSheets.Add(styling);

            RefreshTabs();

            this.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
        }

        public TabView ViewAt(int index)
        {
            return _tabViews[index];
        }

        public void SelectAt(int index)
        {
            foreach (TabButton t in _tabButtons)
            {
                t.Unselect();
            }

            foreach (TabView v in _tabViews)
            {
                v.Display(false);
            }

            _tabButtons[index].Select();
            _tabViews[index].Display(true);
        }

        private void HandleGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshTabs();
        }

        private void RefreshTabs()
        {
            UQueryState<TabEntry> query = contentContainer.Query<TabEntry>().Build();

            using (ListPool<TabEntry>.Get(out List<TabEntry> entries))
            using (ListPool<VisualElement>.Get(out List<VisualElement> views))
            {
                entries.AddRange(query);

                for (int i = 0; i < entries.Count; i++)
                {
                    TabEntry entry = entries[i];
                    TabButton btn = new TabButton(entry.Title);
                    TabView view = new TabView();

                    views.Clear();
                    views.AddRange(entry.contentContainer.Children());

                    foreach (VisualElement v in views)
                    {
                        view.Add(v);
                    }

                    btn.OnClicked += HandleTabClicked;

                    _tabButtons.Add(btn);
                    _tabViews.Add(view);

                    HeaderContainer.Add(btn);
                    contentContainer.Remove(entry);
                    contentContainer.Add(view);
                    view.Display(false);
                }
            }

        }

        public int AddTab(string title)
        {
            TabButton btn = new TabButton(title);
            TabView view = new TabView();

            btn.OnClicked += HandleTabClicked;

            _tabButtons.Add(btn);
            _tabViews.Add(view);

            HeaderContainer.Add(btn);
            contentContainer.Add(view);
            view.Display(false);

            return _tabButtons.Count - 1;
        }

        private void HandleTabClicked(TabButton tab)
        {
            int newIndex = HeaderContainer.IndexOf(tab);

            SelectAt(newIndex);

        }
    }
}