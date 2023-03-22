// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;
using System.Collections.Generic;
using System;

namespace VirtualBeings.Tech.Shared
{
    /// <summary>
    /// ABC for game objects that can touch beings via a trigger collider
    /// </summary>
    public abstract class ATouchDataProvider : MonoBehaviour
    {
        public abstract void Init(EventManager worldEvents, InteractionDB interactionDB);
        public abstract float MaxRadiusOfTouchingCollider { get; }
        public abstract SphereCollider TriggerCollider { get; }
        public abstract Dictionary<ITouchResponder, Set<Collider>> CurrentTouchRespondersInsideTrigger { get; }
    }
}
