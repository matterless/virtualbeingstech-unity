// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VirtualBeings.UIElements;

namespace VirtualBeings
{
    /// <summary>
    /// Input struct for the command <see cref="MoveAssetsCommand"/>
    /// </summary>
    public struct MoveAssetsInputs
    {
        public GameObject ModelAsset;
        public string ExportPath;
        public Dictionary<string, List<GameObject>> AnimationAssets;
        public bool MoveAssets;
    }

    /// <summary>
    /// Output struct for the command <see cref="MoveAssetsCommand"/>
    /// </summary>
    public struct MoveAssetsOutputs
    {
        public string ExportPath;
        public GameObject ModelAsset;
        public Dictionary<string, List<GameObject>> ModelAnimations;
    }

    /// <summary>
    /// Command used to move or copy the model and animation assets to an organized folder structure
    /// </summary>
    public class MoveAssetsCommand
    {
        public bool Execute(MoveAssetsInputs input, out MoveAssetsOutputs output)
        {
            output = new MoveAssetsOutputs();
            output.ModelAnimations = new Dictionary<string, List<GameObject>>(input.AnimationAssets.Count);

            output.ExportPath = input.ExportPath;

            CreateFolders(input);

            AssetDatabase.Refresh();

            MoveOrCopyAssets(input);

            AssetDatabase.Refresh();

            AssignToOutput(input, ref output);

            return true;
        }

        private void AssignToOutput(MoveAssetsInputs input, ref MoveAssetsOutputs output)
        {
            // model mesh
            {
                string dstPath = $"{input.ExportPath}/Model/{input.ModelAsset.name}.fbx";
                output.ModelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(dstPath);
            }

            // animations
            foreach (var (domain, animAssets) in input.AnimationAssets)
            {
                string rootFolder = $"{input.ExportPath}/{domain}";
                List<GameObject> outputList = new List<GameObject>();
                output.ModelAnimations.Add(domain, outputList);

                foreach (GameObject anim in animAssets)
                {
                    string dstPath = $"{rootFolder}/{anim.name}.fbx";

                    GameObject newAnim = AssetDatabase.LoadAssetAtPath<GameObject>(dstPath);
                    outputList.Add(newAnim);
                }
            }
        }

        private void MoveOrCopyAssets(MoveAssetsInputs input)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                // model mesh
                {
                    string dstPath = $"{input.ExportPath}/Model/{input.ModelAsset.name}.fbx";
                    string srcPath = AssetDatabase.GetAssetPath(input.ModelAsset);

                    if (input.MoveAssets)
                    {
                        AssetDatabase.MoveAsset(srcPath, dstPath);
                    }
                    else
                    {
                        AssetDatabase.CopyAsset(srcPath, dstPath);
                    }
                }

                // animations

                foreach (var (domain, animAssets) in input.AnimationAssets)
                {
                    string rootFolder = $"{input.ExportPath}/{domain}";

                    foreach (GameObject anim in animAssets)
                    {
                        string srcPath = AssetDatabase.GetAssetPath(anim);
                        string dstPath = $"{rootFolder}/{anim.name}.fbx";

                        if (input.MoveAssets)
                        {
                            AssetDatabase.MoveAsset(srcPath, dstPath);
                        }
                        else
                        {
                            AssetDatabase.CopyAsset(srcPath, dstPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void CreateFolders(MoveAssetsInputs input)
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                // create parent folder
                {
                    EditorUtils.CreateFoldersFromPath(input.ExportPath);
                }

                // create model folder
                {
                    string dstPath = $"{input.ExportPath}/Model/{input.ModelAsset.name}.fbx";

                    EditorUtils.CreateFoldersFromPath(dstPath);
                }

                // create domains folders
                {
                    foreach (var (domain, animAssets) in input.AnimationAssets)
                    {
                        string rootFolder = $"{input.ExportPath}/{domain}";
                        EditorUtils.CreateFoldersFromPath(rootFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}