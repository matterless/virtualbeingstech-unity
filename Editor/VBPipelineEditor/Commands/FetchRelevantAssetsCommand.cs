// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace VirtualBeings
{
    /// <summary>
    /// A struct of data used to wrap the model fetch result given by <see cref="FetchRelevantAssetsCommand"/>
    /// </summary>
    public struct ModelAssetData
    {
        public Mesh ModelMesh;
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public GameObject ModelAsset;
    }

    /// <summary>
    /// A pair of data used to wrap the animation fetch result given by <see cref="FetchRelevantAssetsCommand"/>
    /// </summary>
    public struct AnimationAssetData
    {
        public AnimationClip AnimationClip;
        public GameObject AnimAsset;
    }

    /// <summary>
    /// Input struct for the command <see cref="FetchRelevantAssetsCommand"/>
    /// </summary>
    public struct FetchRelevantAssetsInputs
    {
        public List<string> ScannablePaths;
    }

    /// <summary>
    /// Output struct for the command <see cref="FetchRelevantAssetsCommand"/>
    /// </summary>
    public struct FetchRelevantAssetsOutputs
    {
        public List<ModelAssetData> Models;
        public List<AnimationAssetData> Anims;
        public List<AvatarMask> AvatarMasks;
        public List<string> Errors;
    }

    /// <summary>
    /// Command used to take a list of asset AND folder paths and filter it to extract the releavant asset to the <see cref="ImportModelView"/> part of the pipeline process
    /// </summary>
    public class FetchRelevantAssetsCommand
    {
        private const string MODEL_SEARCH_FILTER = "t:avatarmask t:model";
        private const string INTERNAL_UNITY_ANIMATION_CLIP = "__preview__Scene";

        private bool IsValidMask(ModelImporter importer, out AnimationAssetData result)
        {
            using (ListPool<AnimationClip>.Get(out List<AnimationClip> allAnims))
            {
                result = default;
                string modelPath = importer.assetPath;
                Object[] allSubAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

                Mesh mesh = allSubAssets.OfType<Mesh>().FirstOrDefault();

                allAnims.AddRange(allSubAssets.OfType<AnimationClip>());

                for (int i = allAnims.Count - 1; i >= 0; i--)
                {
                    AnimationClip a = allAnims[i];

                    if (a.name == INTERNAL_UNITY_ANIMATION_CLIP)
                        allAnims.RemoveAt(i);
                }

                // no model data found
                if (allAnims.Count == 0)
                    return false;

                AnimationClip anim = allAnims[0];

                result.AnimationClip = anim;
                result.AnimAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

                return true;
            }
        }

        private bool IsValidAnimation(ModelImporter importer, out AnimationAssetData result)
        {
            using (ListPool<AnimationClip>.Get(out List<AnimationClip> allAnims))
            {
                result = default;
                string modelPath = importer.assetPath;
                Object[] allSubAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

                Mesh mesh = allSubAssets.OfType<Mesh>().FirstOrDefault();

                allAnims.AddRange(allSubAssets.OfType<AnimationClip>());

                for (int i = allAnims.Count - 1; i >= 0; i--)
                {
                    AnimationClip a = allAnims[i];

                    if (a.name == INTERNAL_UNITY_ANIMATION_CLIP)
                        allAnims.RemoveAt(i);
                }

                // no model data found
                if (allAnims.Count == 0)
                    return false;

                AnimationClip anim = allAnims[0];

                result.AnimationClip = anim;
                result.AnimAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

                return true;
            }
        }

        public bool IsValidModel(ModelImporter importer, out ModelAssetData result)
        {
            result = default;
            string modelPath = importer.assetPath;
            Object[] allSubAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

            Mesh mesh = allSubAssets.OfType<Mesh>().FirstOrDefault();
            SkinnedMeshRenderer skinnedMeshRenderer = allSubAssets.OfType<SkinnedMeshRenderer>().FirstOrDefault();

            // no model data found
            if (mesh == null || skinnedMeshRenderer == null)
                return false;

            result = new ModelAssetData()
            {
                ModelMesh = mesh,
                ModelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath),
                SkinnedMeshRenderer = skinnedMeshRenderer
            };

            return true;
        }

        private void GetAssetsRecursively(IList<string> inputPath, List<string> outputPath)
        {
            using (ListPool<string>.Get(out List<string> folderPaths))
            {
                // filter the folder paths from the the asset paths
                foreach (string path in inputPath)
                {
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        folderPaths.Add(path);
                    }
                    else
                    {
                        outputPath.Add(path);
                    }
                }


                if (folderPaths.Count != 0)
                {
                    string[] searchFolders = folderPaths.ToArray();
                    string[] modelGuids = AssetDatabase.FindAssets(MODEL_SEARCH_FILTER, searchFolders);

                    foreach (string modelGuid in modelGuids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(modelGuid);

                        if (outputPath.Contains(assetPath))
                            continue;

                        outputPath.Add(assetPath);
                    }
                }
            }
        }

        public bool Execute(FetchRelevantAssetsInputs input, out FetchRelevantAssetsOutputs output)
        {
            output = new FetchRelevantAssetsOutputs();
            output.Anims = new List<AnimationAssetData>();
            output.Models = new List<ModelAssetData>();
            output.AvatarMasks = new List<AvatarMask>();
            output.Errors = new List<string>();

            using (ListPool<string>.Get(out List<string> assetPaths))
            using (ListPool<ModelImporter>.Get(out List<ModelImporter> modelAssets))
            using (ListPool<AvatarMask>.Get(out List<AvatarMask> masksAssets))
            {
                // get all the assets inside the paths that reprensent asset paths
                GetAssetsRecursively(input.ScannablePaths, assetPaths);

                // filter model & avatar mask assets
                {
                    for (int i = assetPaths.Count - 1; i >= 0; i--)
                    {
                        string assetPath = assetPaths[i];

                        // check is it's a model or avatar mask asset

                        AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(assetPath);
                        ModelImporter model = AssetImporter.GetAtPath(assetPath) as ModelImporter;

                        if (model != null)
                        {
                            modelAssets.Add(model);
                            continue;
                        }

                        if (mask != null)
                        {
                            masksAssets.Add(mask);
                            continue;
                        }

                        assetPaths.RemoveAt(i);
                    }
                }

                // add the avatarMasks
                output.AvatarMasks.AddRange(masksAssets);


                // extract the mesh model
                {
                    foreach (ModelImporter m in modelAssets)
                    {
                        if (!IsValidModel(m, out ModelAssetData res))
                            continue;

                        output.Models.Add(res);
                    }
                }

                // extract the animations
                {

                    foreach (ModelImporter m in modelAssets)
                    {
                        if (!IsValidAnimation(m, out AnimationAssetData res))
                            continue;

                        output.Anims.Add(res);
                    }
                }

                return true;
            }
        }
    }
}