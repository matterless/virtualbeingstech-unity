// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// A collections of utility methods used for editor logic and helper/redundant operations
    /// </summary>
    public static class EditorUtils
    {
        private static string _pathToProject;

        /// <summary>
        ///<para>Path to project (without the Asset folder)</para>
        ///<para>Example : C:/UnityProjects/[ProjectName]</para>
        /// </summary>
        public static string PathToProject
        {
            get
            {
                if (string.IsNullOrEmpty(_pathToProject))
                {
                    _pathToProject = Application.dataPath;
                    _pathToProject = _pathToProject.Substring(0, _pathToProject.Length - "/Assets".Length);
                }

                return _pathToProject;
            }
        }

        /// <summary>
        /// <para>Create a folder based on path</para>
        /// <para>Works if the path ends with filename + extension also</para>
        /// <para>example : "Assets/Resources/Foo/Bar"</para>
        /// <para>OR : "Assets/Resources/Foo/Bar/SomeAsset.asset"</para>
        /// </summary>
        /// <param name="path"></param>
        public static void CreateFoldersFromPath(string path)
        {
            using (ListPool<string>.Get(out var folders))
            {
                folders.AddRange(path.Split('/'));

                if (folders.Last().Contains('.'))
                {
                    folders.RemoveAt(folders.Count - 1);
                }

                // the path must start from assets
                Assert.AreEqual(folders[0], "Assets");

                string prevFolder = folders[0];

                for (int i = 1; i < folders.Count; i++)
                {
                    string folderToCheck = prevFolder + "/" + folders[i];

                    if (!AssetDatabase.IsValidFolder(folderToCheck))
                    {
                        AssetDatabase.CreateFolder(prevFolder, folders[i]);
                    }

                    prevFolder = folderToCheck;
                }
            }
        }

        /// <summary>
        /// Check if the VisualElement's layout is fully built or not
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsLayoutBuilt(this VisualElement root)
        {
            return !root.Query<VisualElement>()
                 .Build()
                 .Any(v => float.IsNaN(v.layout.width) || float.IsNaN(v.layout.height));
        }

        /// <summary>
        /// Toggle the display of a VisualElement
        /// </summary>
        /// <param name="visualElement"></param>
        /// <param name="show"></param>
        public static void Display(this VisualElement visualElement, bool show)
        {
            DisplayStyle style = show ? DisplayStyle.Flex : DisplayStyle.None;
            visualElement.style.display = style;
        }


        /// <summary>
        /// <para>Takes a relative paht and returns it as absolute path</para>
        /// <para>Relative paths are usually returned from <see cref="AssetDatabase.GetAssetPath(int)"/> or other <see cref="AssetDatabase"/> methods </para>
        /// </summary>
        /// <param name="relative">The relative path , usually the path you get from <see cref="AssetDatabase.GetAssetPath(int)"/> </param>
        /// <returns>The absolute path</returns>
        public static string RelativeToAbsolutePath(string relative)
        {
            return PathToProject + "/" + relative;
        }


        /// <summary>
        /// <para>Takes an anbsolute path and returns it as relative path</para>
        /// <para>Relative paths are usually used in methods provided in <see cref="AssetDatabase"/> </para>
        /// </summary>
        /// <param name="relative">The relative path , usually the path you get from <see cref="AssetDatabase.GetAssetPath(int)"/> </param>
        /// <returns>The relative path OR <see cref="null"/> if the path isn't in the project</returns>
        public static string AbsoluteToRelativePath(string absolute)
        {
            string relative = null;

            if (absolute.StartsWith(Application.dataPath))
            {
                relative = "Assets" + absolute.Substring(Application.dataPath.Length);
            }

            return relative;
        }

    }
}