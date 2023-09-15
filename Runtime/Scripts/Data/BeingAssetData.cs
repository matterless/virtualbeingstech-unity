using System;
using System.Collections.Generic;
using UnityEngine;
using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings
{
    [Serializable]
    public class AnimationData
    {
        public GameObject AnimtionAsset;
        public AnimationClip AnimationClip;
    }

    [Serializable]
    public class DomainAnimationData
    {
        public string DomainName;
        public List<AnimationData> Animations;
    }

    /// <summary>
    /// Data representing the state/info of the <see cref="ImportModelView"/>
    /// </summary>
    [Serializable]
    public class ImportModelContext
    {
        [SerializeField]
        public BeingArchetype BeingArchetype;

        [SerializeField]
        public GameObject ModelAsset;

        [SerializeField]
        public float ScaleFactor;

        [SerializeField]
        public List<AvatarMask> AvatarMasks = new List<AvatarMask>();
    
        [SerializeField]
        public List<DomainAnimationData> DomainAnimations = new List<DomainAnimationData>();
    }

    /// <summary>
    /// Data representing the state/info of the <see cref="CreateStatesView"/>
    /// </summary>
    [Serializable]
    public class CreateStatesContext
    {
        [SerializeField]
        public RuntimeAnimatorController animatorController;

        [SerializeField]
        public List<string> states = new List<string>();
    }

    public class BeingAssetData : ScriptableObject
    {
        [SerializeField]
        public ImportModelContext ImportContext;

        [SerializeField]
        public CreateStatesContext CreateStatesContext;


    }
}