// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;

using UnityEditor;

using UnityEngine.Pool;

using VirtualBeings.UIElements;

namespace VirtualBeings
{
    /// <summary>
    /// Input struct for the command <see cref="HasChildOrParentImportCommand"/>
    /// </summary>
    public struct HasChildOrParentImportInputs
    {
        public Dictionary<string, BeingAssetData> AllBeingAssetsInProject;
        public string AssetPath;
    }

    /// <summary>
    /// Output struct for the command <see cref="HasChildOrParentImportCommand"/>
    /// </summary>
    public struct HasChildOrParentImportOutputs
    {
        public BeingAssetData BeingAsset;
        public ImportPathError AssetImportError;
        public string BeingAssetPath;
    }

    /// <summary>
    /// Enumerate the types of error that can occure when importing an asset
    /// </summary>
    public enum ImportPathError
    {
        AssetAlreadyInsideImport,
        AssetIsImportFolder,
        ImportIsInsideAsset
    }

    /// <summary>
    /// Command used to check if the asset at the provided path already belongs to an existing import either as child or parent
    /// </summary>
    public class HasChildOrParentImportCommand
    {
        public bool Execute(HasChildOrParentImportInputs input, out HasChildOrParentImportOutputs output)
        {
            output = new HasChildOrParentImportOutputs();

            using (ListPool<string>.Get(out List<string> dragParentPaths))
            using (ListPool<string>.Get(out List<string> assetParentPaths))
            using (ListPool<BeingAssetData>.Get(out List<BeingAssetData> assets))
            {
                EditorUtils.GetParentFoldersFromPath(input.AssetPath, dragParentPaths);

                foreach (var (beingAssetPath, beingAsset) in input.AllBeingAssetsInProject)
                {
                    assetParentPaths.Clear();
                    EditorUtils.GetParentFoldersFromPath(beingAssetPath, assetParentPaths);

                    // check if the dragged asset/folder is inside an import asset's folder

                    if (CheckIfFolderAContainsB(assetParentPaths, dragParentPaths))
                    {
                        output.BeingAsset = beingAsset;
                        output.BeingAssetPath = beingAssetPath;
                        output.AssetImportError = ImportPathError.AssetAlreadyInsideImport;
                        return true;
                    }

                    // if the dragged asset is NOT a folder and it's outside the import folder, then exit
                    if (!AssetDatabase.IsValidFolder(input.AssetPath))
                    {
                        output = default;
                        return false;
                    }

                    if(CheckIfFolderAEqualsB(assetParentPaths , dragParentPaths))
                    {
                        output.BeingAsset = beingAsset;
                        output.BeingAssetPath = beingAssetPath;
                        output.AssetImportError = ImportPathError.AssetIsImportFolder;
                        return true;
                    }

                    // if the dragged asset is a folder , check if it contains an import asset
                    if (CheckIfFolderAContainsB(dragParentPaths, assetParentPaths))
                    {
                        output.BeingAsset = beingAsset;
                        output.BeingAssetPath = beingAssetPath;
                        output.AssetImportError = ImportPathError.ImportIsInsideAsset;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckIfFolderAEqualsB(List<string> a, List<string> b)
        {
            if (b.Count != a.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        private static bool CheckIfFolderAContainsB(List<string> a, List<string> b)
        {
            if (b.Count <= a.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}