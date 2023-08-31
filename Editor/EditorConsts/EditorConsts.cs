// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// A static class contating global resource/assets and values that might be useful and shared by all the editors in the project
    /// </summary>
    public static class EditorConsts
    {
        public  const   string          GLOBAL_EDTOR_BASE_PATH = "Packages/com.virtualbeings.tech.virtualbeingstech/Editor";
        private const   string          GLOBAL_USS_PATH        = GLOBAL_EDTOR_BASE_PATH + "/GlobalStylesheet/GlobalStylesheet.uss";
        private static  StyleSheet      _globalStylesheet;

        public static StyleSheet GlobalStylesheet
        {
            get
            {
                if (_globalStylesheet == null)
                {
                    _globalStylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GLOBAL_USS_PATH);
                }

                Assert.IsNotNull(_globalStylesheet, "Woops ! Something went wrong with the GlobalStylesheet!");

                return _globalStylesheet;
            }
        }
    }
}