// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VirtualBeings.Tech.Beings.Bird;
using VirtualBeings.Tech.Beings.SmallQuadrupeds;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
#if UNITY_EDITOR
    public partial class PostProcessAnimationEditorOld : UnityEditor.Editor
    {
        ////////////////////////////////////////////////////////////////////////////////
        /// global variables, change if needed

        ////////////////////////////////////////////////////////////////////////////////
        private void SetColliderChainRelationship(List<Being.ColliderChainLink> chainLink, Collider parent, Collider child)
        {
            int parentIndex = chainLink.FindIndex((c) => c.C == parent);
            int childIndex = chainLink.FindIndex((c) => c.C == child);

            if (child == parent || parentIndex == -1 || childIndex == -1 ||
                chainLink[parentIndex].Child != -1 || chainLink[childIndex].Parent != -1)
                throw new System.Exception("Cant find collider, or child == parent, or parent/child already set");

            chainLink[childIndex] = new Being.ColliderChainLink(
                chainLink[childIndex].C,
                chainLink[childIndex].PartUpper,
                chainLink[childIndex].PartLower,
                chainLink[childIndex].DotProdThreshold,
                parentIndex,
                chainLink[childIndex].Child);
            chainLink[parentIndex] = new Being.ColliderChainLink(
                chainLink[parentIndex].C,
                chainLink[parentIndex].PartUpper,
                chainLink[parentIndex].PartLower,
                chainLink[parentIndex].DotProdThreshold,
                chainLink[parentIndex].Parent,
                childIndex);
        }

        private void OnInspectorGUI_Colliders(PostProcessAnimation postProcessAnimation, AgentType agentType)
        {
            Dictionary<string, Transform> dictTransforms = GetTransformDictionary(postProcessAnimation.gameObject);

            string[] zeroFrictionMaterialGuid = UnityEditor.AssetDatabase.FindAssets("ZeroFriction");
            PhysicMaterial zeroFrictionMaterial = (PhysicMaterial)UnityEditor.AssetDatabase.LoadAssetAtPath(
                UnityEditor.AssetDatabase.GUIDToAssetPath(zeroFrictionMaterialGuid[0]), typeof(PhysicMaterial));

            ////////////////////////////////////////////////////////////////////////////////

            if (agentType == AgentType.SmallQuadruped)
            {
                SphereCollider sc_S0;
                if ((sc_S0 = dictTransforms["spine.001"].GetComponent<SphereCollider>()) == null) sc_S0 = dictTransforms["spine.001"].gameObject.AddComponent<SphereCollider>();
                sc_S0.radius = 0.07752535f;
                sc_S0.center = new Vector3(0f, -0.01247464f, 0.02f);
                sc_S0.material = zeroFrictionMaterial;

                SphereCollider sc_S1;
                if ((sc_S1 = dictTransforms["spine.002"].GetComponent<SphereCollider>()) == null) sc_S1 = dictTransforms["spine.002"].gameObject.AddComponent<SphereCollider>();
                sc_S1.radius = 0.07342021f;
                sc_S1.center = new Vector3(0f, -0.01225775f, 0.02f);
                sc_S1.material = zeroFrictionMaterial;

                SphereCollider sc_S2;
                if ((sc_S2 = dictTransforms["spine.003"].GetComponent<SphereCollider>()) == null) sc_S2 = dictTransforms["spine.003"].gameObject.AddComponent<SphereCollider>();
                sc_S2.radius = 0.07663699f;
                sc_S2.center = new Vector3(0f, -0.02243827f, 0.01999998f);
                sc_S2.material = zeroFrictionMaterial;

                SphereCollider sc_S3;
                if ((sc_S3 = dictTransforms["spine.004"].GetComponent<SphereCollider>()) == null) sc_S3 = dictTransforms["spine.004"].gameObject.AddComponent<SphereCollider>();
                sc_S3.radius = 0.06632616f;
                sc_S3.center = new Vector3(0f, -.009996297f, 0.02f);
                sc_S3.material = zeroFrictionMaterial;

                SphereCollider sc_neck;
                if ((sc_neck = dictTransforms["neck"].GetComponent<SphereCollider>()) == null) sc_neck = dictTransforms["neck"].gameObject.AddComponent<SphereCollider>();
                sc_neck.radius = 0.05709931f;
                sc_neck.center = new Vector3(0f, .003816187f, 0.03000001f);
                sc_neck.material = zeroFrictionMaterial;

                SphereCollider sc_head;
                if ((sc_head = dictTransforms["head"].GetComponent<SphereCollider>()) == null) sc_head = dictTransforms["head"].gameObject.AddComponent<SphereCollider>();
                sc_head.radius = 0.06182345f;
                sc_head.center = new Vector3(0f, .002683662f, 0.0223047f);
                sc_head.material = zeroFrictionMaterial;

                SphereCollider sc_jaw;
                if ((sc_jaw = dictTransforms["jaw"].GetComponent<SphereCollider>()) == null) sc_jaw = dictTransforms["jaw"].gameObject.AddComponent<SphereCollider>();
                sc_jaw.radius = 0.03441563f;
                sc_jaw.center = new Vector3(0f, -0.01f, 0.05187103f);
                sc_jaw.material = zeroFrictionMaterial;

                SphereCollider sc_earmainL;
                if ((sc_earmainL = dictTransforms["ear_main.L"].GetComponent<SphereCollider>()) == null) sc_earmainL = dictTransforms["ear_main.L"].gameObject.AddComponent<SphereCollider>();
                sc_earmainL.radius = 0.025f;
                sc_earmainL.center = new Vector3(0f, 0f, 0f);
                sc_earmainL.material = zeroFrictionMaterial;

                SphereCollider sc_eartipL;
                if ((sc_eartipL = dictTransforms["ear_tip.L"].GetComponent<SphereCollider>()) == null) sc_eartipL = dictTransforms["ear_tip.L"].gameObject.AddComponent<SphereCollider>();
                sc_eartipL.radius = 0.01325631f;
                sc_eartipL.center = new Vector3(.006439917f, -.002550672f, .004555897f);
                sc_eartipL.material = zeroFrictionMaterial;

                SphereCollider sc_earmainR;
                if ((sc_earmainR = dictTransforms["ear_main.R"].GetComponent<SphereCollider>()) == null) sc_earmainR = dictTransforms["ear_main.R"].gameObject.AddComponent<SphereCollider>();
                sc_earmainR.radius = 0.025f;
                sc_earmainR.center = new Vector3(0f, 0f, 0f);
                sc_earmainR.material = zeroFrictionMaterial;

                SphereCollider sc_eartipR;
                if ((sc_eartipR = dictTransforms["ear_tip.R"].GetComponent<SphereCollider>()) == null) sc_eartipR = dictTransforms["ear_tip.R"].gameObject.AddComponent<SphereCollider>();
                sc_eartipR.radius = 0.01325631f;
                sc_eartipR.center = new Vector3(-.006439917f, -.002550672f, .004555897f);
                sc_eartipR.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                CapsuleCollider cc_LL0;
                if ((cc_LL0 = dictTransforms["leg.L.001"].GetComponent<CapsuleCollider>()) == null) cc_LL0 = dictTransforms["leg.L.001"].gameObject.AddComponent<CapsuleCollider>();
                cc_LL0.radius = 0.03797573f;
                cc_LL0.height = 0.1346349f;
                cc_LL0.center = new Vector3(.00630226f, -0.02140509f, 0.03768252f);
                cc_LL0.direction = 2; // 2==Z axis
                cc_LL0.material = zeroFrictionMaterial;

                CapsuleCollider cc_LL1;
                if ((cc_LL1 = dictTransforms["leg.L.002"].GetComponent<CapsuleCollider>()) == null) cc_LL1 = dictTransforms["leg.L.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_LL1.radius = 0.02231434f;
                cc_LL1.height = 0.1134319f;
                cc_LL1.center = new Vector3(.003143437f, -.005184702f, 0.0332841f);
                cc_LL1.direction = 2; // 2==Z axis
                cc_LL1.material = zeroFrictionMaterial;

                CapsuleCollider cc_LL2;
                if ((cc_LL2 = dictTransforms["leg.L.003"].GetComponent<CapsuleCollider>()) == null) cc_LL2 = dictTransforms["leg.L.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_LL2.radius = 0.01821283f;
                cc_LL2.height = 0.08807943f;
                cc_LL2.center = new Vector3(0f, .009170119f, 0.02903971f);
                cc_LL2.direction = 2; // 2==Z axis
                cc_LL2.material = zeroFrictionMaterial;

                SphereCollider sc_LL3;
                if ((sc_LL3 = dictTransforms["leg.L.004"].GetComponent<SphereCollider>()) == null) sc_LL3 = dictTransforms["leg.L.004"].gameObject.AddComponent<SphereCollider>();
                sc_LL3.radius = 0.0196857f;
                sc_LL3.center = new Vector3(0f, .001257189f, 0.01848602f);
                sc_LL3.material = zeroFrictionMaterial;

                CapsuleCollider cc_LR0;
                if ((cc_LR0 = dictTransforms["leg.R.001"].GetComponent<CapsuleCollider>()) == null) cc_LR0 = dictTransforms["leg.R.001"].gameObject.AddComponent<CapsuleCollider>();
                cc_LR0.radius = 0.03797573f;
                cc_LR0.height = 0.1346349f;
                cc_LR0.center = new Vector3(-.00630226f, -0.02140509f, 0.03768252f);
                cc_LR0.direction = 2; // 2==Z axis
                cc_LR0.material = zeroFrictionMaterial;

                CapsuleCollider cc_LR1;
                if ((cc_LR1 = dictTransforms["leg.R.002"].GetComponent<CapsuleCollider>()) == null) cc_LR1 = dictTransforms["leg.R.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_LR1.radius = 0.02231434f;
                cc_LR1.height = 0.1134319f;
                cc_LR1.center = new Vector3(-.003143437f, -.005184702f, 0.0332841f);
                cc_LR1.direction = 2; // 2==Z axis
                cc_LR1.material = zeroFrictionMaterial;

                CapsuleCollider cc_LR2;
                if ((cc_LR2 = dictTransforms["leg.R.003"].GetComponent<CapsuleCollider>()) == null) cc_LR2 = dictTransforms["leg.R.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_LR2.radius = 0.01821283f;
                cc_LR2.height = 0.08807943f;
                cc_LR2.center = new Vector3(0f, .009170119f, 0.02903971f);
                cc_LR2.direction = 2; // 2==Z axis
                cc_LR2.material = zeroFrictionMaterial;

                SphereCollider sc_LR3;
                if ((sc_LR3 = dictTransforms["leg.R.004"].GetComponent<SphereCollider>()) == null) sc_LR3 = dictTransforms["leg.R.004"].gameObject.AddComponent<SphereCollider>();
                sc_LR3.radius = 0.0196857f;
                sc_LR3.center = new Vector3(0f, .001257189f, 0.01848602f);
                sc_LR3.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                CapsuleCollider cc_AL0;
                if ((cc_AL0 = dictTransforms["arm.L.001"].GetComponent<CapsuleCollider>()) == null) cc_AL0 = dictTransforms["arm.L.001"].gameObject.AddComponent<CapsuleCollider>();
                cc_AL0.radius = 0.03489739f;
                cc_AL0.height = 0.1140379f;
                cc_AL0.center = new Vector3(.001102611f, 0.01999994f, 0.03701869f);
                cc_AL0.direction = 2; // 2==Z axis
                cc_AL0.material = zeroFrictionMaterial;

                CapsuleCollider cc_AL1;
                if ((cc_AL1 = dictTransforms["arm.L.002"].GetComponent<CapsuleCollider>()) == null) cc_AL1 = dictTransforms["arm.L.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_AL1.radius = 0.01965143f;
                cc_AL1.height = 0.1f;
                cc_AL1.center = new Vector3(0f, .00304819f, 0.03999999f);
                cc_AL1.direction = 2; // 2==Z axis
                cc_AL1.material = zeroFrictionMaterial;

                CapsuleCollider cc_AL2;
                if ((cc_AL2 = dictTransforms["arm.L.003"].GetComponent<CapsuleCollider>()) == null) cc_AL2 = dictTransforms["arm.L.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_AL2.radius = 0.01885373f;
                cc_AL2.height = 0.08609057f;
                cc_AL2.center = new Vector3(.001146268f, -.005000001f, 0.02804529f);
                cc_AL2.direction = 2; // 2==Z axis
                cc_AL2.material = zeroFrictionMaterial;

                SphereCollider sc_AL3;
                if ((sc_AL3 = dictTransforms["arm.L.004"].GetComponent<SphereCollider>()) == null) sc_AL3 = dictTransforms["arm.L.004"].gameObject.AddComponent<SphereCollider>();
                sc_AL3.radius = 0.01961973f;
                sc_AL3.center = new Vector3(0f, 0f, .004943567f);
                sc_AL3.material = zeroFrictionMaterial;

                CapsuleCollider cc_AR0;
                if ((cc_AR0 = dictTransforms["arm.R.001"].GetComponent<CapsuleCollider>()) == null) cc_AR0 = dictTransforms["arm.R.001"].gameObject.AddComponent<CapsuleCollider>();
                cc_AR0.radius = 0.03489739f;
                cc_AR0.height = 0.1140379f;
                cc_AR0.center = new Vector3(-.001102611f, 0.01999994f, 0.03701869f);
                cc_AR0.direction = 2; // 2==Z axis
                cc_AR0.material = zeroFrictionMaterial;

                CapsuleCollider cc_AR1;
                if ((cc_AR1 = dictTransforms["arm.R.002"].GetComponent<CapsuleCollider>()) == null) cc_AR1 = dictTransforms["arm.R.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_AR1.radius = 0.01965143f;
                cc_AR1.height = 0.1f;
                cc_AR1.center = new Vector3(0f, .00304819f, 0.03999999f);
                cc_AR1.direction = 2; // 2==Z axis
                cc_AR1.material = zeroFrictionMaterial;

                CapsuleCollider cc_AR2;
                if ((cc_AR2 = dictTransforms["arm.R.003"].GetComponent<CapsuleCollider>()) == null) cc_AR2 = dictTransforms["arm.R.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_AR2.radius = 0.01885373f;
                cc_AR2.height = 0.08609057f;
                cc_AR2.center = new Vector3(-.001146268f, -.005000001f, 0.02804529f);
                cc_AR2.direction = 2; // 2==Z axis
                cc_AR2.material = zeroFrictionMaterial;

                SphereCollider sc_AR3;
                if ((sc_AR3 = dictTransforms["arm.R.004"].GetComponent<SphereCollider>()) == null) sc_AR3 = dictTransforms["arm.R.004"].gameObject.AddComponent<SphereCollider>();
                sc_AR3.radius = 0.01961973f;
                sc_AR3.center = new Vector3(0f, 0f, .004943567f);
                sc_AR3.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                CapsuleCollider cc_T0;
                if ((cc_T0 = dictTransforms["tail.001"].GetComponent<CapsuleCollider>()) == null) cc_T0 = dictTransforms["tail.001"].gameObject.AddComponent<CapsuleCollider>();
                cc_T0.radius = 0.01851567f;
                cc_T0.height = 0.06992123f;
                cc_T0.center = new Vector3(0f, -.002847323f, 0.0263349f);
                cc_T0.direction = 2; // 2==Z axis
                cc_T0.material = zeroFrictionMaterial;

                CapsuleCollider cc_T1;
                if ((cc_T1 = dictTransforms["tail.002"].GetComponent<CapsuleCollider>()) == null) cc_T1 = dictTransforms["tail.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_T1.radius = 0.01611972f;
                cc_T1.height = 0.07543668f;
                cc_T1.center = new Vector3(0f, -.001140824f, 0.02357711f);
                cc_T1.direction = 2; // 2==Z axis
                cc_T1.material = zeroFrictionMaterial;

                CapsuleCollider cc_T2;
                if ((cc_T2 = dictTransforms["tail.003"].GetComponent<CapsuleCollider>()) == null) cc_T2 = dictTransforms["tail.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_T2.radius = 0.01499999f;
                cc_T2.height = 0.06098622f;
                cc_T2.center = new Vector3(0f, -.00228163f, 0.02547853f);
                cc_T2.direction = 2; // 2==Z axis
                cc_T2.material = zeroFrictionMaterial;

                CapsuleCollider cc_T3;
                if ((cc_T3 = dictTransforms["tail.004"].GetComponent<CapsuleCollider>()) == null) cc_T3 = dictTransforms["tail.004"].gameObject.AddComponent<CapsuleCollider>();
                cc_T3.radius = 0.01350001f;
                cc_T3.height = 0.05870458f;
                cc_T3.center = new Vector3(0f, -.003042223f, 0.01977438f);
                cc_T3.direction = 2; // 2==Z axis
                cc_T3.material = zeroFrictionMaterial;

                CapsuleCollider cc_T4;
                if ((cc_T4 = dictTransforms["tail.005"].GetComponent<CapsuleCollider>()) == null) cc_T4 = dictTransforms["tail.005"].gameObject.AddComponent<CapsuleCollider>();
                cc_T4.radius = 0.012f;
                cc_T4.height = 0.07315508f;
                cc_T4.center = new Vector3(0f, -.003042221f, 0.02699957f);
                cc_T4.direction = 2; // 2==Z axis
                cc_T4.material = zeroFrictionMaterial;

                // set links of ColliderChain
                Being being;

                if (agentType == AgentType.SmallQuadruped)
                    being = postProcessAnimation.GetComponent<SmallQuadrupedBeing>();
                else if (agentType == AgentType.Bird)
                    being = postProcessAnimation.GetComponent<BirdBeing>();
                else
                    throw new System.Exception("Only cats and bird are supported ATM");

                Being.ColliderChainLink[] colliderChain =
                    new Being.ColliderChainLink[]
                    {
                    new Being.ColliderChainLink(sc_S0, BodyPart.Butt, BodyPart.Crotch, -.4f),
                    new Being.ColliderChainLink(sc_S1, BodyPart.Back, BodyPart.Belly, -.4f),
                    new Being.ColliderChainLink(sc_S2, BodyPart.Back, BodyPart.Belly, -.4f),
                    new Being.ColliderChainLink(sc_S3, BodyPart.Back, BodyPart.Chest, -.3f),
                    new Being.ColliderChainLink(sc_neck, BodyPart.Neck, BodyPart.Throat),
                    new Being.ColliderChainLink(sc_head, BodyPart.Head, BodyPart.Throat),
                    new Being.ColliderChainLink(sc_jaw, BodyPart.Head, BodyPart.Throat),
                    new Being.ColliderChainLink(sc_earmainL, BodyPart.EarL, BodyPart.EarL),
                    new Being.ColliderChainLink(sc_eartipL, BodyPart.EarL, BodyPart.EarL),
                    new Being.ColliderChainLink(sc_earmainR, BodyPart.EarR, BodyPart.EarR),
                    new Being.ColliderChainLink(sc_eartipR, BodyPart.EarR, BodyPart.EarR),
                    new Being.ColliderChainLink(cc_LL0, BodyPart.LL, BodyPart.LL),
                    new Being.ColliderChainLink(cc_LL1, BodyPart.LL, BodyPart.LL),
                    new Being.ColliderChainLink(cc_LL2, BodyPart.LL, BodyPart.LLPaw, -.6f),
                    new Being.ColliderChainLink(sc_LL3, BodyPart.LL, BodyPart.LLPaw, -.1f),
                    new Being.ColliderChainLink(cc_LR0, BodyPart.LR, BodyPart.LR),
                    new Being.ColliderChainLink(cc_LR1, BodyPart.LR, BodyPart.LR),
                    new Being.ColliderChainLink(cc_LR2, BodyPart.LR, BodyPart.LRPaw, -.6f),
                    new Being.ColliderChainLink(sc_LR3, BodyPart.LR, BodyPart.LRPaw, -.1f),
                    new Being.ColliderChainLink(cc_AL0, BodyPart.AL, BodyPart.AL),
                    new Being.ColliderChainLink(cc_AL1, BodyPart.AL, BodyPart.AL),
                    new Being.ColliderChainLink(cc_AL2, BodyPart.AL, BodyPart.ALPaw, -.6f),
                    new Being.ColliderChainLink(sc_AL3, BodyPart.AL, BodyPart.ALPaw, -.1f),
                    new Being.ColliderChainLink(cc_AR0, BodyPart.AR, BodyPart.AR),
                    new Being.ColliderChainLink(cc_AR1, BodyPart.AR, BodyPart.AR),
                    new Being.ColliderChainLink(cc_AR2, BodyPart.AR, BodyPart.ARPaw, -.6f),
                    new Being.ColliderChainLink(sc_AR3, BodyPart.AR, BodyPart.ARPaw, -.1f),
                    new Being.ColliderChainLink(cc_T0, BodyPart.Butt, BodyPart.Crotch, -.5f),
                    new Being.ColliderChainLink(cc_T1, BodyPart.Tail, BodyPart.Tail),
                    new Being.ColliderChainLink(cc_T2, BodyPart.Tail, BodyPart.Tail),
                    new Being.ColliderChainLink(cc_T3, BodyPart.Tail, BodyPart.Tail),
                    new Being.ColliderChainLink(cc_T4, BodyPart.TailTip, BodyPart.TailTip),
                    };

                List<Being.ColliderChainLink> colliderChainList = new List<Being.ColliderChainLink>(colliderChain);

                SetColliderChainRelationship(colliderChainList, sc_S0, sc_S1);
                SetColliderChainRelationship(colliderChainList, sc_S1, sc_S2);
                SetColliderChainRelationship(colliderChainList, sc_S2, sc_S3);
                SetColliderChainRelationship(colliderChainList, sc_S3, sc_neck);
                SetColliderChainRelationship(colliderChainList, sc_neck, sc_head);
                SetColliderChainRelationship(colliderChainList, sc_head, sc_jaw);

                SetColliderChainRelationship(colliderChainList, sc_earmainL, sc_eartipL);
                SetColliderChainRelationship(colliderChainList, sc_earmainR, sc_eartipR);

                SetColliderChainRelationship(colliderChainList, cc_LL0, cc_LL1);
                SetColliderChainRelationship(colliderChainList, cc_LL1, cc_LL2);
                SetColliderChainRelationship(colliderChainList, cc_LL2, sc_LL3);

                SetColliderChainRelationship(colliderChainList, cc_LR0, cc_LR1);
                SetColliderChainRelationship(colliderChainList, cc_LR1, cc_LR2);
                SetColliderChainRelationship(colliderChainList, cc_LR2, sc_LR3);

                SetColliderChainRelationship(colliderChainList, cc_AL0, cc_AL1);
                SetColliderChainRelationship(colliderChainList, cc_AL1, cc_AL2);
                SetColliderChainRelationship(colliderChainList, cc_AL2, sc_AL3);

                SetColliderChainRelationship(colliderChainList, cc_AR0, cc_AR1);
                SetColliderChainRelationship(colliderChainList, cc_AR1, cc_AR2);
                SetColliderChainRelationship(colliderChainList, cc_AR2, sc_AR3);

                SetColliderChainRelationship(colliderChainList, cc_T0, cc_T1);
                SetColliderChainRelationship(colliderChainList, cc_T1, cc_T2);
                SetColliderChainRelationship(colliderChainList, cc_T2, cc_T3);
                SetColliderChainRelationship(colliderChainList, cc_T3, cc_T4);

                typeof(Being).GetField("_colliderChain", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, colliderChain);

                typeof(Being).GetField("_impenetrableColliders", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new Collider[] { sc_S0, sc_S1, sc_S2, sc_S3, sc_head, });

                // add the navserver colliders
                typeof(Being).GetField("_gameObjectsForNavmesh", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new GameObject[] { sc_S0.gameObject, sc_head.gameObject, });
            }
            else if (agentType == AgentType.Bird)
            {
                SphereCollider sc_S0;
                if ((sc_S0 = dictTransforms["spine.001"].GetComponent<SphereCollider>()) == null) sc_S0 = dictTransforms["spine.001"].gameObject.AddComponent<SphereCollider>();
                sc_S0.radius = 0.028f;
                sc_S0.center = new Vector3(0f, -.005f, 0.015f);
                sc_S0.material = zeroFrictionMaterial;

                SphereCollider sc_S1;
                if ((sc_S1 = dictTransforms["spine.002"].GetComponent<SphereCollider>()) == null) sc_S1 = dictTransforms["spine.002"].gameObject.AddComponent<SphereCollider>();
                sc_S1.radius = 0.035f;
                sc_S1.center = new Vector3(0f, -.005f, 0.01f);
                sc_S1.material = zeroFrictionMaterial;

                SphereCollider sc_S2;
                if ((sc_S2 = dictTransforms["spine.003"].GetComponent<SphereCollider>()) == null) sc_S2 = dictTransforms["spine.003"].gameObject.AddComponent<SphereCollider>();
                sc_S2.radius = 0.042f;
                sc_S2.center = new Vector3(0f, -0.01f, 0.01f);
                sc_S2.material = zeroFrictionMaterial;

                SphereCollider sc_S3;
                if ((sc_S3 = dictTransforms["spine.004"].GetComponent<SphereCollider>()) == null) sc_S3 = dictTransforms["spine.004"].gameObject.AddComponent<SphereCollider>();
                sc_S3.radius = 0.042f;
                sc_S3.center = new Vector3(0f, -.007f, 0f);
                sc_S3.material = zeroFrictionMaterial;

                //SphereCollider sc_neck;
                //if ((sc_neck = dictTransforms["neck"].GetComponent<SphereCollider>()) == null) sc_neck = dictTransforms["neck"].gameObject.AddComponent<SphereCollider>();
                //sc_neck.radius = 0.05709931f;
                //sc_neck.center = new Vector3(0f, .003816187f, 0.03000001f);
                //sc_neck.material = zeroFrictionMaterial;

                SphereCollider sc_head;
                if ((sc_head = dictTransforms["head"].GetComponent<SphereCollider>()) == null) sc_head = dictTransforms["head"].gameObject.AddComponent<SphereCollider>();
                sc_head.radius = 0.035f;
                sc_head.center = new Vector3(0f, 0.01f, 0.01f);
                sc_head.material = zeroFrictionMaterial;

                CapsuleCollider cc_jaw;
                if ((cc_jaw = dictTransforms["beak"].GetComponent<CapsuleCollider>()) == null) cc_jaw = dictTransforms["beak"].gameObject.AddComponent<CapsuleCollider>();
                cc_jaw.radius = .008f;
                cc_jaw.height = 0.03f;
                cc_jaw.center = new Vector3(0f, 0f, 0.02f);
                cc_jaw.direction = 2; // 2==Z axis
                cc_jaw.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                CapsuleCollider cc_LL1;
                if ((cc_LL1 = dictTransforms["leg.L.002"].GetComponent<CapsuleCollider>()) == null) cc_LL1 = dictTransforms["leg.L.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_LL1.radius = 0.01f;
                cc_LL1.height = 0.025f;
                cc_LL1.center = new Vector3(0f, 0f, 0.025f);
                cc_LL1.direction = 2; // 2==Z axis
                cc_LL1.material = zeroFrictionMaterial;

                CapsuleCollider cc_LL2;
                if ((cc_LL2 = dictTransforms["leg.L.003"].GetComponent<CapsuleCollider>()) == null) cc_LL2 = dictTransforms["leg.L.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_LL2.radius = 0.01f;
                cc_LL2.height = 0.04f;
                cc_LL2.center = new Vector3(0f, 0f, 0.01f);
                cc_LL2.direction = 2; // 2==Z axis
                cc_LL2.material = zeroFrictionMaterial;

                BoxCollider bc_LL3;
                if ((bc_LL3 = dictTransforms["leg.L.004"].GetComponent<BoxCollider>()) == null) bc_LL3 = dictTransforms["leg.L.004"].gameObject.AddComponent<BoxCollider>();
                bc_LL3.center = new Vector3(0f, 0f, 0f);
                bc_LL3.size = new Vector3(0.025f, .006f, 0.04f);
                bc_LL3.material = zeroFrictionMaterial;

                CapsuleCollider cc_LR1;
                if ((cc_LR1 = dictTransforms["leg.R.002"].GetComponent<CapsuleCollider>()) == null) cc_LR1 = dictTransforms["leg.R.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_LR1.radius = 0.01f;
                cc_LR1.height = 0.025f;
                cc_LR1.center = new Vector3(0f, 0f, 0.025f);
                cc_LR1.direction = 2; // 2==Z axis
                cc_LR1.material = zeroFrictionMaterial;

                CapsuleCollider cc_LR2;
                if ((cc_LR2 = dictTransforms["leg.R.003"].GetComponent<CapsuleCollider>()) == null) cc_LR2 = dictTransforms["leg.R.003"].gameObject.AddComponent<CapsuleCollider>();
                cc_LR2.radius = 0.01f;
                cc_LR2.height = 0.04f;
                cc_LR2.center = new Vector3(0f, 0f, 0.01f);
                cc_LR2.direction = 2; // 2==Z axis
                cc_LR2.material = zeroFrictionMaterial;

                BoxCollider bc_LR3;
                if ((bc_LR3 = dictTransforms["leg.R.004"].GetComponent<BoxCollider>()) == null) bc_LR3 = dictTransforms["leg.R.004"].gameObject.AddComponent<BoxCollider>();
                bc_LR3.center = new Vector3(0f, 0f, 0f);
                bc_LR3.size = new Vector3(0.025f, .006f, 0.04f);
                bc_LR3.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                BoxCollider bc_AL0;
                if ((bc_AL0 = dictTransforms["arm.L.001"].GetComponent<BoxCollider>()) == null) bc_AL0 = dictTransforms["arm.L.001"].gameObject.AddComponent<BoxCollider>();
                bc_AL0.center = new Vector3(0f, -0.01f, 0.024f);
                bc_AL0.size = new Vector3(0.01f, 0.04f, 0.04f);
                bc_AL0.material = zeroFrictionMaterial;

                BoxCollider bc_AL1;
                if ((bc_AL1 = dictTransforms["arm.L.002"].GetComponent<BoxCollider>()) == null) bc_AL1 = dictTransforms["arm.L.002"].gameObject.AddComponent<BoxCollider>();
                bc_AL1.center = new Vector3(.001882075f, -.005722334f, 0.01236671f);
                bc_AL1.size = new Vector3(.006235553f, 0.03344425f, 0.04487598f);
                bc_AL1.material = zeroFrictionMaterial;

                BoxCollider bc_AL2;
                if ((bc_AL2 = dictTransforms["arm.L.003"].GetComponent<BoxCollider>()) == null) bc_AL2 = dictTransforms["arm.L.003"].gameObject.AddComponent<BoxCollider>();
                bc_AL2.center = new Vector3(.002109802f, -.005802867f, 0.01743235f);
                bc_AL2.size = new Vector3(.007409455f, 0.02839423f, 0.03686472f);
                bc_AL2.material = zeroFrictionMaterial;

                BoxCollider bc_AR0;
                if ((bc_AR0 = dictTransforms["arm.R.001"].GetComponent<BoxCollider>()) == null) bc_AR0 = dictTransforms["arm.R.001"].gameObject.AddComponent<BoxCollider>();
                bc_AR0.center = new Vector3(0f, -0.01f, 0.024f);
                bc_AR0.size = new Vector3(0.01f, 0.04f, 0.04f);
                bc_AR0.material = zeroFrictionMaterial;

                BoxCollider bc_AR1;
                if ((bc_AR1 = dictTransforms["arm.R.002"].GetComponent<BoxCollider>()) == null) bc_AR1 = dictTransforms["arm.R.002"].gameObject.AddComponent<BoxCollider>();
                bc_AR1.center = new Vector3(-.001882075f, -.005722334f, 0.01236671f);
                bc_AR1.size = new Vector3(.006235553f, 0.03344425f, 0.04487598f);
                bc_AR1.material = zeroFrictionMaterial;

                BoxCollider bc_AR2;
                if ((bc_AR2 = dictTransforms["arm.R.003"].GetComponent<BoxCollider>()) == null) bc_AR2 = dictTransforms["arm.R.003"].gameObject.AddComponent<BoxCollider>();
                bc_AR2.center = new Vector3(-.002109802f, -.005802867f, 0.01743235f);
                bc_AR2.size = new Vector3(.007409455f, 0.02839423f, 0.03686472f);
                bc_AR2.material = zeroFrictionMaterial;

                ////////////////////////////////////////////////////////////////////////////////
                SphereCollider sc_T0;
                if ((sc_T0 = dictTransforms["tail.001"].GetComponent<SphereCollider>()) == null) sc_T0 = dictTransforms["tail.001"].gameObject.AddComponent<SphereCollider>();
                sc_T0.radius = 0.018f;
                sc_T0.center = new Vector3(0f, 0f, 0f);
                sc_T0.material = zeroFrictionMaterial;

                CapsuleCollider cc_T1;
                if ((cc_T1 = dictTransforms["tail.002"].GetComponent<CapsuleCollider>()) == null) cc_T1 = dictTransforms["tail.002"].gameObject.AddComponent<CapsuleCollider>();
                cc_T1.radius = 0.01611972f;
                cc_T1.height = 0.07543668f;
                cc_T1.center = new Vector3(0f, -.001140824f, 0.02357711f);
                cc_T1.direction = 2; // 2==Z axis
                cc_T1.material = zeroFrictionMaterial;

                // set links of ColliderChain
                Being being;

                if (agentType == AgentType.SmallQuadruped)
                    being = postProcessAnimation.GetComponent<SmallQuadrupedBeing>();
                else if (agentType == AgentType.Bird)
                    being = postProcessAnimation.GetComponent<BirdBeing>();
                else
                    throw new System.Exception("Only cats and bird are supported ATM");

                Being.ColliderChainLink[] colliderChain =
                    new Being.ColliderChainLink[]
                    {
                    new Being.ColliderChainLink(sc_S0, BodyPart.Butt, BodyPart.Crotch, -.4f),
                    new Being.ColliderChainLink(sc_S1, BodyPart.Back, BodyPart.Belly, -.4f),
                    new Being.ColliderChainLink(sc_S2, BodyPart.Back, BodyPart.Belly, -.4f),
                    new Being.ColliderChainLink(sc_S3, BodyPart.Back, BodyPart.Chest, -.3f),
                    new Being.ColliderChainLink(sc_head, BodyPart.Head, BodyPart.Throat),
                    new Being.ColliderChainLink(cc_jaw, BodyPart.Mouth, BodyPart.Mouth),
                    new Being.ColliderChainLink(cc_LL1, BodyPart.LL, BodyPart.LL),
                    new Being.ColliderChainLink(cc_LL2, BodyPart.LL, BodyPart.LLPaw, -.6f),
                    new Being.ColliderChainLink(bc_LL3, BodyPart.LL, BodyPart.LLPaw, -.1f),
                    new Being.ColliderChainLink(cc_LR1, BodyPart.LR, BodyPart.LR),
                    new Being.ColliderChainLink(cc_LR2, BodyPart.LR, BodyPart.LRPaw, -.6f),
                    new Being.ColliderChainLink(bc_LR3, BodyPart.LR, BodyPart.LRPaw, -.1f),
                    new Being.ColliderChainLink(bc_AL0, BodyPart.AL, BodyPart.AL),
                    new Being.ColliderChainLink(bc_AL1, BodyPart.AL, BodyPart.AL),
                    new Being.ColliderChainLink(bc_AL2, BodyPart.AL, BodyPart.ALPaw, -.6f),
                    new Being.ColliderChainLink(bc_AR0, BodyPart.AR, BodyPart.AR),
                    new Being.ColliderChainLink(bc_AR1, BodyPart.AR, BodyPart.AR),
                    new Being.ColliderChainLink(bc_AR2, BodyPart.AR, BodyPart.ARPaw, -.6f),
                    new Being.ColliderChainLink(sc_T0, BodyPart.Butt, BodyPart.Crotch, -.5f),
                    new Being.ColliderChainLink(cc_T1, BodyPart.Tail, BodyPart.Tail),
                    };

                List<Being.ColliderChainLink> colliderChainList = new List<Being.ColliderChainLink>(colliderChain);

                SetColliderChainRelationship(colliderChainList, sc_S0, sc_S1);
                SetColliderChainRelationship(colliderChainList, sc_S1, sc_S2);
                SetColliderChainRelationship(colliderChainList, sc_S2, sc_S3);
                SetColliderChainRelationship(colliderChainList, sc_S3, sc_head);
                SetColliderChainRelationship(colliderChainList, sc_head, cc_jaw);

                SetColliderChainRelationship(colliderChainList, cc_LL1, cc_LL2);
                SetColliderChainRelationship(colliderChainList, cc_LL2, bc_LL3);

                SetColliderChainRelationship(colliderChainList, cc_LR1, cc_LR2);
                SetColliderChainRelationship(colliderChainList, cc_LR2, bc_LR3);

                SetColliderChainRelationship(colliderChainList, bc_AL0, bc_AL1);
                SetColliderChainRelationship(colliderChainList, bc_AL1, bc_AL2);

                SetColliderChainRelationship(colliderChainList, bc_AR0, bc_AR1);
                SetColliderChainRelationship(colliderChainList, bc_AR1, bc_AR2);

                SetColliderChainRelationship(colliderChainList, sc_T0, cc_T1);

                typeof(Being).GetField("_colliderChain", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, colliderChain);

                typeof(Being).GetField("_impenetrableColliders", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new Collider[] { sc_T0, sc_S0, sc_S1, sc_S2, sc_S3, sc_head, });

                // add the navserver colliders
                typeof(Being).GetField("_gameObjectsForNavmesh", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new GameObject[] { sc_S0.gameObject, sc_head.gameObject, });

                // add the excluded colliders, currently just 'ColliderAvoidance'
                var trColliderAvoidance = postProcessAnimation.transform.Find("ColliderAvoidance");
                if (trColliderAvoidance != null)
                    DestroyImmediate(trColliderAvoidance.gameObject);

                var ObstacleDetector = new GameObject("ObstacleDetector");
                ObstacleDetector.transform.SetParent(postProcessAnimation.transform, false);

                var rb = ObstacleDetector.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                var sc = ObstacleDetector.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = .5f;

                var ct = ObstacleDetector.AddComponent<ObstacleDetector>();
                ct.sphereCollider = sc;
                ct.collisionLayers =
                    (1 << LayerMask.NameToLayer("StaticObstacles")) |
                    (1 << LayerMask.NameToLayer("Interactable")) |
                    (1 << LayerMask.NameToLayer("Ground"));

                typeof(Being).GetField("_exclusionsFromInteractionDB", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being,
                    new Collider[] { sc, });
            }
        }
    }
#endif
}
