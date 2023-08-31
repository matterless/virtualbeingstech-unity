// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings
{
    /// <summary>
    /// Input struct for the command <see cref="ApplyImportSettingsCommand"/>
    /// </summary>
    public struct ApplyImportSettingsInputs
    {
        public BeingArchetype BeingArchetype;
        public string ExportPath;
        public GameObject ModelAsset;
        public Dictionary<string, List<GameObject>> AnimationAssets;
    }

    /// <summary>
    /// Output struct for the command <see cref="ApplyImportSettingsCommand"/>
    /// </summary>
    public struct ApplyImportSettingsOutput
    {
    }

    /// <summary>
    /// Command used to Apply the correct import settings to assets passed in the <see cref="ApplyImportSettingsInputs"/> input struct
    /// </summary>
    public class ApplyImportSettingsCommand
    {
        public bool Execute(ApplyImportSettingsInputs input, out ApplyImportSettingsOutput output)
        {
            Avatar modelAvater = null;

            BeingAssetData dataAsset = new BeingAssetData();
            dataAsset.beingArchetype = input.BeingArchetype;

            // model
            {
                string path = AssetDatabase.GetAssetPath(input.ModelAsset);

                UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                modelAvater = allAssets.OfType<Avatar>().FirstOrDefault();
                Mesh modelMesh = allAssets.OfType<Mesh>().FirstOrDefault();

                dataAsset.modelAsset = input.ModelAsset;
                dataAsset.modelMesh = modelMesh;

                Assert.IsNotNull(modelAvater);
            }


            try
            {
                //AssetDatabase.StartAssetEditing();

                // animations
                {
                    dataAsset.domainAnimations = new BeingAssetData.DomainAnimationData[input.AnimationAssets.Count];

                    int index = 0;
                    foreach (KeyValuePair<string, List<GameObject>> kv in input.AnimationAssets)
                    {
                        string domainName = kv.Key;
                        List<GameObject> domainAnims = kv.Value;

                        BeingAssetData.DomainAnimationData currDomain = new BeingAssetData.DomainAnimationData();

                        currDomain.domainName = kv.Key;
                        currDomain.animations = new BeingAssetData.AnimationData[kv.Value.Count];

                        dataAsset.domainAnimations[index++] = currDomain;

                        for (int i = 0; i < kv.Value.Count; i++)
                        {
                            GameObject animAsset = kv.Value[i];
                            string path = AssetDatabase.GetAssetPath(animAsset);


                            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                            AnimationClip animClip = allAssets.OfType<AnimationClip>().FirstOrDefault();
                            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(path);

                            // model tab
                            {
                                importer.globalScale = 1;
                                importer.useFileUnits = true;
                                importer.bakeAxisConversion = false;
                                importer.importBlendShapes = true;
                                importer.importBlendShapeDeformPercent = false;
                                importer.importVisibility = false;
                                importer.importCameras = false;
                                importer.importLights = true;
                                importer.preserveHierarchy = false;
                                importer.sortHierarchyByName = true;

                                importer.meshCompression = ModelImporterMeshCompression.Off;
                                importer.isReadable = false;
                                importer.optimizeMeshPolygons = true;
                                importer.optimizeMeshVertices = true;
                                importer.addCollider = false;

                                importer.keepQuads = false;
                                importer.weldVertices = true;

                                importer.indexFormat = ModelImporterIndexFormat.Auto;
                                importer.importNormals = ModelImporterNormals.Import;
                                importer.importBlendShapeNormals = ModelImporterNormals.None;
                                importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
                                importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
                                importer.normalSmoothingAngle = 60;

                                importer.importTangents = ModelImporterTangents.CalculateMikk;
                            }

                            // rig tab
                            {
                                importer.animationType = ModelImporterAnimationType.Human;
                                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                                importer.sourceAvatar = modelAvater;
                                importer.skinWeights = ModelImporterSkinWeights.Standard;
                                importer.optimizeBones = false;
                            }

                            // animation tab
                            {
                                importer.importConstraints = false;
                                importer.importAnimation = true;

                                importer.resampleCurves = true;
                                importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
                                importer.animationRotationError = 0.1f;
                                importer.animationPositionError = 0.1f;
                                importer.animationScaleError = 0.5f;

                                importer.importAnimatedCustomProperties = false;
                                importer.removeConstantScaleCurves = false;
                            }

                            // animation clips
                            foreach (ModelImporterClipAnimation animClipImporter in importer.clipAnimations)
                            {
                                animClipImporter.lockRootRotation = false;
                                animClipImporter.keepOriginalOrientation = true;
                                animClipImporter.rotationOffset = 0;

                                animClipImporter.lockRootHeightY = true;
                                animClipImporter.heightFromFeet = true;
                                animClipImporter.keepOriginalPositionY = true;
                                animClipImporter.heightOffset = 0;

                                animClipImporter.lockRootPositionXZ = false;
                                animClipImporter.keepOriginalPositionXZ = true;
                            }

                            BeingAssetData.AnimationData animData = new BeingAssetData.AnimationData();
                            animData.animationClip = animClip;
                            animData.animtionAsset = animAsset;
                            currDomain.animations[i] = animData;
                            importer.SaveAndReimport();
                        }


                        string dataAssetPath = $"{input.ExportPath}/Data.asset";

                        BeingAssetData existingData = AssetDatabase.LoadAssetAtPath<BeingAssetData>(dataAssetPath);


                        if (existingData != null)
                        {
                            EditorUtility.CopySerialized(dataAsset, existingData);
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(dataAsset, dataAssetPath);
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
                // AssetDatabase.StopAssetEditing();
            }

            return true;
        }

    }
}