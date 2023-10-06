using System;
using UnityEngine;
using VirtualBeings.Tech.Beings.Birds;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;
using VirtualBeings.Tech.Shared;
using System.Collections.Generic;

namespace VirtualBeings.Beings.Bird
{
    public class BirdWalkableZone : WalkableZone, IBirdWalkableZone
    {
        [Space]
        public List<Transform> LandingPosition;
        public List<Transform> TakeOffPosition;

        public Vector3 GetFreeLandPosition()
        {
            if (LandingPosition == null || LandingPosition.Count <= 0)
            {
                return transform.position;
            }

            Transform landingPositon = LandingPosition[Rand.Range(0, LandingPosition.Count)];

            return landingPositon.position;
        }

        public Vector3 GetFreeTakeOffPosition()
        {
            if(TakeOffPosition == null || TakeOffPosition.Count <= 0)
            {
                return transform.position;
            }

            Transform landingPositon = TakeOffPosition[Rand.Range(0, LandingPosition.Count)];

            return landingPositon.position;
        }

        protected override void OnEnableSubclasses()
        {
            _interactionDB.RegisterInteractable(gameObject, this as IBirdWalkableZone);
        }

        protected override void OnDisableSubclasses()
        {
            _interactionDB.UnregisterInteractable(gameObject, this as IBirdWalkableZone);
        }
    }
}
