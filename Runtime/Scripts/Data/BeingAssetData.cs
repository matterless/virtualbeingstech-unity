using System;
using UnityEngine;
using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings
{
    public class BeingAssetData : ScriptableObject
    {
        [Serializable]
        public class AnimationData
        {
            public GameObject animtionAsset;
            public AnimationClip animationClip;
        }

        [Serializable]
        public class DomainAnimationData
        {
            public string domainName;
            public AnimationData[] animations;
        }

        public GameObject modelAsset;
        public BeingArchetype beingArchetype;
        public Mesh modelMesh;
        public DomainAnimationData[] domainAnimations;
    }
}