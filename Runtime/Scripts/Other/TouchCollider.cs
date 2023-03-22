// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;
using System.Collections.Generic;

namespace VirtualBeings.Tech.Shared
{
    [RequireComponent(typeof(SphereCollider)), RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class TouchCollider : ATouchDataProvider
    {
        [SerializeField, Tooltip("*Visual* radius of the touching object. (Must be smaller than radius of SphereCollider.)")]
        private float _maxRadiusOfTouchingCollider = .008f;
        public SphereCollider _triggerCollider;
        public Dictionary<ITouchResponder, Set<Collider>> _currentTouchRespondersInsideTrigger;

        private bool _isInit = false;
        private EventManager _worldEvents;

        public override void Init(EventManager worldEvents, InteractionDB interactionDB)
        {
            _interactionDB = interactionDB;
            _worldEvents = worldEvents;

            _triggerCollider = GetComponent<SphereCollider>();
            _triggerCollider.isTrigger = true;
            _currentTouchRespondersInsideTrigger = new Dictionary<ITouchResponder, Set<Collider>>();

            _isInit = true;

            // If touchResponder was destroyed when inside trigger, it wont activate OnTriggerExit.
            // So we need to remove it from the dictionary when it is destroyed
            _worldEvents.AddListener<WorldEvent_InteractableUnregistration<IInteractable>>(OnIInteractableUnregistered);
        }

        public override float MaxRadiusOfTouchingCollider => _maxRadiusOfTouchingCollider;
        public override SphereCollider TriggerCollider => _triggerCollider;
        public override Dictionary<ITouchResponder, Set<Collider>> CurrentTouchRespondersInsideTrigger => _currentTouchRespondersInsideTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (!_isInit)
            {
                return;
            }

            if (_interactionDB.TryGetInteractable(other.gameObject, typeof(ITouchResponder), out IInteractable interactable) &&
                interactable is ITouchResponder touchResponder)
            {
                //touchResponder can be a collider on the being body, that respond to this "TouchCollider"(for example, a finger)
                if (CurrentTouchRespondersInsideTrigger.TryGetValue(touchResponder, out Set<Collider> setColliders))
                    setColliders.Add(other);
                else
                    CurrentTouchRespondersInsideTrigger[touchResponder] = new Set<Collider>(new Collider[] { other });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isInit)
            {
                return;
            }

            if (_interactionDB.TryGetInteractable(other.gameObject, typeof(ITouchResponder), out IInteractable interactable) &&
                interactable is ITouchResponder touchResponder &&
                CurrentTouchRespondersInsideTrigger.TryGetValue(touchResponder, out Set<Collider> setColliders))
                    setColliders.Remove(other);
        }

        private void OnIInteractableUnregistered(WorldEvent_InteractableUnregistration<IInteractable> evt)
        {
            IInteractable interactable = evt.Interactable;

            if (interactable is ITouchResponder touchResponder)
            {
                CurrentTouchRespondersInsideTrigger.Remove(touchResponder);
            }
        }

        private InteractionDB _interactionDB;
    }
}
