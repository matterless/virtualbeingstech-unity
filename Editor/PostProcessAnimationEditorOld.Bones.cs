// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VirtualBeings.Tech.Beings.Bird;
using VirtualBeings.Tech.Beings.SmallQuadrupeds;
using VirtualBeings.Tech.Utils;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.ActiveCognition;

namespace VirtualBeings.Tech.Shared
{
#if UNITY_EDITOR
    public partial class PostProcessAnimationEditorOld : Editor
    {
        ////////////////////////////////////////////////////////////////////////////////
        /// global variables, change if needed
        public const float FactorForPositionDrivenBlendshapes = 50.02f / .75f;
        public const float FactorForRotationDrivenBlendshapes = 100f / 5.73f;
        public const float FactorForPositionDrivenLayerWeights = .5002f / .75f;
        public const float FactorForRotationDrivenLayerWeights = 1f / 5.73f;

        ////////////////////////////////////////////////////////////////////////////////
        private void OnInspectorGUI_Bones(PostProcessAnimation postProcessAnimation, AgentType agentType)
        {
            //SharedSettings sharedSettings;
            //Transform rig;

            //if (agentType == AgentType.SmallQuadruped)
            //{
            //    string[] settingsGuid = AssetDatabase.FindAssets("CatSharedClientSettingsAsset");
            //    sharedSettings = null;// (SharedSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsGuid[0]), typeof(CatSharedClientSettings));

            //    postProcessAnimation._being = postProcessAnimation.GetComponent<SmallQuadrupedBeing>();

            //    rig = postProcessAnimation.transform.Find("CatRig");
            //}
            //else if (agentType == AgentType.Bird) // TODO Birds
            //{
            //    string[] settingsGuid = AssetDatabase.FindAssets("BirdSharedClientSettingsAsset");
            //    sharedSettings = null;// (SharedSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsGuid[0]), typeof(BirdSharedClientSettings));

            //    postProcessAnimation._being = postProcessAnimation.GetComponent<BirdBeing>();

            //    rig = postProcessAnimation.transform.Find("BirdRig");
            //}
            //else
            //    throw new System.Exception("Only cats and bird are supported ATM");

            //int nDriversPerBone = System.Enum.GetNames(typeof(PostProcessAnimation.DriverOrigin)).Length;

            //postProcessAnimation._factorForPositionDrivenBlendshapes = FactorForPositionDrivenBlendshapes;
            //postProcessAnimation._factorForRotationDrivenBlendshapes = FactorForRotationDrivenBlendshapes;
            //postProcessAnimation._blendShapedrivers.Clear();

            //postProcessAnimation._factorForPositionDrivenLayerWeights = FactorForPositionDrivenLayerWeights;
            //postProcessAnimation._factorForRotationDrivenLayerWeights = FactorForRotationDrivenLayerWeights;
            //postProcessAnimation._layerDrivers.Clear();

            //postProcessAnimation._variableDrivers.Clear();

            //postProcessAnimation._smr = postProcessAnimation.GetComponentInChildren<SkinnedMeshRenderer>();

            //// first bones that drive blend shapes
            //{
            //    SkinnedMeshRenderer smr = postProcessAnimation._smr;
            //    Mesh mesh = smr.sharedMesh;
            //    int nBlendShapes = mesh.blendShapeCount;

            //    Dictionary<string, Pair<int, string>> domains = new Dictionary<string, Pair<int, string>>()
            //    {
            //        ["Body"] = new Pair<int, string>(0, "body"),
            //        ["Eyes"] = new Pair<int, string>(0, "eyes"),
            //        ["Face"] = new Pair<int, string>(0, "face"),
            //    };

            //    for (int i = 0; i < nBlendShapes; i++)
            //    {
            //        Pair<int, string> domain = null;

            //        foreach (KeyValuePair<string, Pair<int, string>> kvp in domains)
            //        {
            //            if (mesh.GetBlendShapeName(i).StartsWith(kvp.Key))
            //            {
            //                domain = kvp.Value;
            //                break;
            //            }
            //        }

            //        if (domain == null)
            //        {
            //            Debug.Log("Found non-domain blendshape: " + mesh.GetBlendShapeName(i) + "; skipping it.");
            //            continue;
            //        }

            //        int boneIndex = domain.First / nDriversPerBone + 1;
            //        int driverIndex = domain.First % nDriversPerBone;
            //        string boneName = "BSDriver_" + domain.Second + "_0" + boneIndex;
            //        Transform bone = rig.Find("root").Find(boneName);

            //        if (bone == null)
            //            throw new System.Exception("No driver bone found: " + boneName);

            //        PostProcessAnimation.DriverOrigin origin = (PostProcessAnimation.DriverOrigin)driverIndex;
            //        PostProcessAnimation.BlendShapeDriverInfo driverInfo = new PostProcessAnimation.BlendShapeDriverInfo()
            //        {
            //            Bone = bone,
            //            BlendShapeIndex = i,
            //            Origin = origin,
            //        };

            //        postProcessAnimation._blendShapedrivers.Add(driverInfo);
            //        domain.First++;
            //    }
            //}

            //if (agentType == AgentType.SmallQuadruped)
            //{
            //    // now bones that drive layers & variables
            //    Transform driver_prop_01 = rig.Find("root").Find("BSDriver_prop_01");
            //    Transform driver_prop_02 = rig.Find("root").Find("BSDriver_prop_02");

            //    if (driver_prop_01 == null)
            //    {
            //        Debug.LogError("Didnt find driver bone: " + "BSDriver_prop_01");
            //        return;
            //    }

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "FBIK_TongueTip",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_X
            //    });

            //    // NB: layer drivers always drive layer weight reversely: driver value 0f means layer weight of 1f, 1f means 0f
            //    // NB: cant reuse this driver for Blink, although it's synchronized with Face, because its layer weight is done manually
            //    postProcessAnimation._layerDrivers.Add(new PostProcessAnimation.LayerWeightDriverInfo()
            //    {
            //        Bone = driver_prop_01,
            //        LayerIndex = sharedSettings.LayerIndexEyes,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Y // OverrideFace
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "OverrideLookAt",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Z
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "PreserveOffsetForNeckAndHead",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_X
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Tail_IK",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_Y
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_AL",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_Z
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_AR",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_X
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_LL",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_Y
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_LR",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_Z
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_Root",
            //        Bone = driver_prop_02,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_X
            //    });

            //    postProcessAnimation._layerDrivers.Add(new PostProcessAnimation.LayerWeightDriverInfo()
            //    {
            //        Bone = driver_prop_02,
            //        LayerIndex = sharedSettings.LayerIndexMouth,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Y // OverrideMouth
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "OverrideBlink",
            //        Bone = driver_prop_02,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Z
            //    });
            //}
            //else if (agentType == AgentType.Bird)
            //{
            //    // now bones that drive layers & variables
            //    Transform driver_prop_01 = rig.Find("root").Find("BSDriver_prop_01");
            //    Transform driver_prop_02 = rig.Find("root").Find("BSDriver_prop_02");

            //    if (driver_prop_01 == null)
            //    {
            //        Debug.LogError("Didnt find driver bone: " + "BSDriver_prop_01");
            //        return;
            //    }

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "OverrideBlink",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_X
            //    });

            //    // NB: layer drivers always drive layer weight reversely: driver value 0f means layer weight of 1f, 1f means 0f
            //    // NB: cant reuse this driver for Blink, although it's synchronized with Face, because its layer weight is done manually
            //    postProcessAnimation._layerDrivers.Add(new PostProcessAnimation.LayerWeightDriverInfo()
            //    {
            //        Bone = driver_prop_01,
            //        LayerIndex = sharedSettings.LayerIndexEyes,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Y // OverrideFace
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "OverrideLookAt",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Location_Z
            //    });

            //    postProcessAnimation._layerDrivers.Add(new PostProcessAnimation.LayerWeightDriverInfo()
            //    {
            //        Bone = driver_prop_01,
            //        LayerIndex = sharedSettings.LayerIndexMouth,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_X // OverrideMouth
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "FBIK_TongueTip",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_Y
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "PreserveOffsetForNeckAndHead",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Rotation_Z
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_Root",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_X
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_LL",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_Y
            //    });

            //    postProcessAnimation._variableDrivers.Add(new PostProcessAnimation.VariableDriverInfo()
            //    {
            //        Name = "Detach_LR",
            //        Bone = driver_prop_01,
            //        Origin = PostProcessAnimation.DriverOrigin.Scale_Z
            //    });
            //}
        }
    }
#endif
}
