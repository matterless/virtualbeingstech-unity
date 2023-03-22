// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using VirtualBeings.Tech.Beings.Bird;
using VirtualBeings.Tech.Beings.SmallQuadrupeds;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
#if UNITY_EDITOR
    public partial class PostProcessAnimationEditorOld : Editor
    {
        ////////////////////////////////////////////////////////////////////////////////
        /// global variables, change if needed
        public const int BeingExecutionOrderValue = 100;

        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// From https://forum.unity.com/threads/script-execution-order-manipulation.130805/#post-1323087
        /// </summary>
        private void SetExecutionOrder(MonoBehaviour mb, int order)
        {
            // First you get the MonoScript of your MonoBehaviour
            MonoScript monoScript = MonoScript.FromMonoBehaviour(mb);

            // Getting the current execution order of that MonoScript
            // int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

            // Changing the MonoScript's execution order
            if (order != MonoImporter.GetExecutionOrder(monoScript))
                MonoImporter.SetExecutionOrder(monoScript, order);
        }

        private void OnInspectorGUI_CreateBeing(PostProcessAnimation postProcessAnimation, AgentType agentType)
        {
            {
                Animator animator = postProcessAnimation.GetComponent<Animator>();
                SkinnedMeshRenderer smr = postProcessAnimation.GetComponentInChildren<SkinnedMeshRenderer>();

                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;

                smr.quality = SkinQuality.Auto;
                smr.updateWhenOffscreen = true;
            }

            Being being;

            if (agentType == AgentType.SmallQuadruped)
                being = postProcessAnimation.GetComponent<SmallQuadrupedBeing>() ?? postProcessAnimation.gameObject.AddComponent<SmallQuadrupedBeing>();
            else if (agentType == AgentType.Bird)
                being = postProcessAnimation.GetComponent<BirdBeing>() ?? postProcessAnimation.gameObject.AddComponent<BirdBeing>();
            else
                throw new System.Exception("Only cats and bird are supported ATM");

            SetExecutionOrder(being, BeingExecutionOrderValue);

            typeof(Being).GetField("_postProcessAnimation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, postProcessAnimation);

            object sharedSettingsObj = typeof(Being).GetField("_sharedSettings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(being);

            if (sharedSettingsObj == null)
            {
                if (agentType == AgentType.SmallQuadruped)
                {
                    //string[] settingsGuid = AssetDatabase.FindAssets("CatSharedClientSettingsAsset");
                    //BirdSharedClientSettings settings = (BirdSharedClientSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsGuid[0]), typeof(BirdSharedClientSettings));
                    //typeof(Being).GetField("_sharedSettings", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, settings);
                }
                else // TODO Birds
                {
                    string[] settingsGuid = AssetDatabase.FindAssets("BirdSharedClientSettingsAsset");
                    //BirdSharedClientSettings settings = (BirdSharedClientSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsGuid[0]), typeof(BirdSharedClientSettings));
                    //typeof(Being).GetField("_sharedSettings", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, settings);
                }
            }

            Dictionary<string, Transform> dictTransforms = GetTransformDictionary(postProcessAnimation.gameObject);

            // set voice and audio clips
            {
                AudioSource voice = null, voicePurr = null;

                try // strangely necessary; GetComponent<AudioSource>() throws exception...
                {
                    voice = dictTransforms["tongue.001"].GetComponent<AudioSource>();
                }
                catch (System.Exception _) { }

                if (voice == null)
                    voice = dictTransforms["tongue.001"].gameObject.AddComponent<AudioSource>();

                voice.loop = false;
                voice.playOnAwake = false;
                voice.mute = false;

                typeof(Being).GetField("_voice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, voice);

                if (agentType == AgentType.SmallQuadruped)
                {
                    try
                    {
                        voicePurr = dictTransforms["jaw"].GetComponent<AudioSource>();
                    }
                    catch (System.Exception _) { }

                    if (voicePurr == null)
                        voicePurr = dictTransforms["jaw"].gameObject.AddComponent<AudioSource>();

                    voicePurr.loop = false;
                    voicePurr.playOnAwake = false;
                    voicePurr.mute = false;

                    typeof(Being).GetField("_voiceSecondary", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, voicePurr);

                    typeof(Being).GetField("_clips_secondaryVocalization", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                        new AudioClip[]
                        {
                                (AudioClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(
                                    AssetDatabase.FindAssets("LUCY Purr louder")[0]), typeof(AudioClip)),
                        });
                    Debug.LogError("Reminder: Don't forget to add different purr audio clips");
                }
            }

            // add various transforms
            typeof(Being).GetField("_nose", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["nose"]);
            typeof(Being).GetField("_head", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["head"]);
            typeof(Being).GetField("_head_up_axis_marker", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["head_up_axis_marker"]);
            typeof(Being).GetField("_mindsEye", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["minds_eye"]);
            typeof(Being).GetField("_eyeLeft", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["eye.L"]);
            typeof(Being).GetField("_eyeRight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["eye.R"]);
            typeof(Being).GetField("_mouthTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["mouth_target"]);
            typeof(Being).GetField("_mouthPickupPoint", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["tongue.003"]);
            typeof(Being).GetField("_midBodyMarker", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.004_parent"]);

            typeof(Being).GetField("_LL0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.L.001"]);
            typeof(Being).GetField("_LL1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.L.002"]);
            typeof(Being).GetField("_LL2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.L.003"]);
            typeof(Being).GetField("_LL3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.L.004"]);
            typeof(Being).GetField("_LLTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg_magnet.L"]);

            typeof(Being).GetField("_LR0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.R.001"]);
            typeof(Being).GetField("_LR1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.R.002"]);
            typeof(Being).GetField("_LR2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.R.003"]);
            typeof(Being).GetField("_LR3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg.R.004"]);
            typeof(Being).GetField("_LRTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["leg_magnet.R"]);

            typeof(Being).GetField("_AL0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.L.001"]);
            typeof(Being).GetField("_AL1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.L.002"]);
            typeof(Being).GetField("_AL2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.L.003"]);

            typeof(Being).GetField("_AR0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.R.001"]);
            typeof(Being).GetField("_AR1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.R.002"]);
            typeof(Being).GetField("_AR2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.R.003"]);

            if (agentType == AgentType.SmallQuadruped)
            {
                typeof(Being).GetField("_AL3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.L.004"]);
                typeof(Being).GetField("_ALTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm_magnet.L"]);

                typeof(Being).GetField("_AR3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm.R.004"]);
                typeof(Being).GetField("_ARTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["arm_magnet.R"]);
            }

            typeof(Being).GetField("_S0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.001_parent"]);
            typeof(Being).GetField("_S1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.002_parent"]);
            typeof(Being).GetField("_S2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.003_parent"]);
            typeof(Being).GetField("_S3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.004_parent"]);
            typeof(Being).GetField("_neck", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["neck_parent"]);
            typeof(Being).GetField("_innerS0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.001"]);
            typeof(Being).GetField("_innerS1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.002"]);
            typeof(Being).GetField("_innerS2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.003"]);
            typeof(Being).GetField("_innerS3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["spine.004"]);
            typeof(Being).GetField("_innerNeck", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, dictTransforms["neck"]);

            if (agentType == AgentType.SmallQuadruped)
            {
                typeof(Being).GetField("_tail", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new Transform[]
                    {
                            dictTransforms["tail.001"],
                            dictTransforms["tail.002"],
                            dictTransforms["tail.003"],
                            dictTransforms["tail.004"],
                            dictTransforms["tail.005"],
                    });
            }
            else if (agentType == AgentType.Bird)
            {
                typeof(Being).GetField("_tail", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new Transform[]
                    {
                            dictTransforms["tail.001"],
                            dictTransforms["tail.002"],
                    });
            }
        }
    }
#endif
}
