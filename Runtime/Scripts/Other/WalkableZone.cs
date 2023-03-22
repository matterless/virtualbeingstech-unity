// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using UnityEngine;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;

namespace VirtualBeings.Tech.Shared
{
    /// <summary>
    /// A *circular* part of a navigable terrain that's sufficiently flat and unencumbered by static obstacles
    /// to be walkable by a being. Can however include dynamic obstacles.
    /// </summary>
    [DisallowMultipleComponent]
    public class WalkableZone : MonoBehaviour, IWalkableZone
    {
        ////////////////////////////////////////////////////////////////////////////////////
        /// Fields serialized by Unity

        [SerializeField]
        private float _maxWalkableRadius;

        [SerializeField]
        private Collider _groundCollider;

        [SerializeField]
        private InteractableProperty[] _interactableProperties;

        [SerializeField, HideInInspector]
        private int _interactableID;
        
        ////////////////////////////////////////////////////////////////////////////////////
        /// Initialization & Unity messages

        Container Container => Container.Instance;
        private void Awake()
        {
            Init(Container.InteractionDB);
        }

        private void Init(InteractionDB interactionDB)
        {
            _interactionDB = interactionDB;

            if (_interactableProperties != null)
                foreach (InteractableProperty v in _interactableProperties)
                    Properties.Add((int)v);
        }

        /// <summary>
        /// </summary>
        private void OnEnable()
        {
            _interactionDB.RegisterInteractable(gameObject, this as IInteractable);
            _interactionDB.RegisterInteractable(gameObject, this as IWalkableZone);
        }

        private void OnDisable()
        {
            _interactionDB.UnregisterInteractable(gameObject, this as IInteractable);
            _interactionDB.UnregisterInteractable(gameObject, this as IWalkableZone);
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /// IWalkableZone implementation

        /// <summary>
        /// Use spherecasting to find a randomized, free (unobstructed) position where being can be (e.g., by landing).
        /// </summary>
        /// <param name="being">The being that wants to use this position</param>
        /// <param name="position">The free position</param>
        /// <param name="maxDistFromCenter">How far from the zone's center the position is allowed to be</param>
        /// <param name="nMaxAttempts">How many attempts until we give up?</param>
        /// <returns>False if no free position could be found</returns>
        public bool GetRandomFreePosition(Being being, float freeRadius, out Vector3 position,
            float maxDistFromCenter = float.MaxValue, int nMaxAttempts = 5)
        {
            float sphereCastDistance = freeRadius * 4f;
            LayerMask sphereCastLayerMask = being.SharedSettings.NavObstacleMask | being.OtherCharactersLayerMask;
            maxDistFromCenter = Math.Min(maxDistFromCenter, _maxWalkableRadius);

            // first, search once in every direction (radially around root position)
            for (int i = 0; i < nMaxAttempts; i++)
            {
                // set 'position' to a random candidate position thats not further than 'maxDistFromCenter - freeRadius'
                position = transform.position + Vector3.right.RotateAround(Vector3.up, Rand.Range(-180f, 180f)) *
                    Rand.Range(0f, Math.Max(0f, maxDistFromCenter - freeRadius));

                // spherecast reminder: a cast of maxDistance 1 with radius .5 towards an object with radius .5
                // will return false from a distance of 2.01, and true from 1.99
                Vector3 sphereCastOrigin = position + Vector3.up * sphereCastDistance;

                // succeed ...
                // - if spherecast didnt find anything
                // - if it found the ground collider associated with this walkable zone
                if (!Physics.SphereCast(sphereCastOrigin, freeRadius, Vector3.down, out RaycastHit hit,
                    sphereCastDistance, sphereCastLayerMask) || hit.collider == _groundCollider)
                {
                    position.y = hit.point.y;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /// IInteractable implementation

        public GameObject TopLevelParent => gameObject;
        public Set<int> Properties { get; } = new Set<int>();
        public Vector3 RootPosition => transform.position;
        public Quaternion RootRotation => transform.rotation;
        public Vector3 CenterPosition => RootPosition;
        public Quaternion CenterRotation => RootRotation;
        public Vector3 SalientPosition => RootPosition;

        public Vector3 Velocity { get; set; } // administered by SpatialDB, dont change manually
        public Vector3 ___PrevPosition { get; set; } // administered by SpatialDB, dont use
        public Vector3 CorrectiveDelta { get; set; } // normally zero; can be set by users *per frame* to make SpatialDB correct Velocity

        public int InteractableID { get => _interactableID; set => _interactableID = value; }

        public bool IsLocalizable => false;
        public bool IsLocal => true; // TODO multiplayer
        public bool IsDynamic => false;
        public bool IsHandled => false;
        public bool IsDestroyed => this == null;
        public float MaxRadius => _maxWalkableRadius;
        public float Height => 0f;
        public float Width => _maxWalkableRadius * 2f;
        public float Length => _maxWalkableRadius * 2f;
        public IInteractable Handler => null;
        public bool IsInPlacementMode => false;
        public IInteractionMode InteractionMode => null;

        public bool SetHandler(IInteractable handler) => false;
        public void ReleaseHandler(IInteractable handlerToReleaseFrom) { }
        public event Action OnHandlerChanged;

        public bool HasProperty(InteractableProperty property) => Properties.Contains((int)property);
        public bool IsInInteractionDB() => this != null && _interactionDB.IsInDatabase(this, typeof(IInteractable));
        public IAgent DestroyedBy { get; private set; }

        /// <summary>
        /// Kill owning gameObject
        /// </summary>
        public void Destroy(IAgent destroyedBy = null)
        {
            if (this != null)
            {
                DestroyedBy = destroyedBy;
                GameObject.Destroy(gameObject);
            }
        }

        public Vector3 GetNearestPointOnSurface(Vector3 referencePoint)
        {
            return CenterPosition;
        }

        protected InteractionDB _interactionDB;
    }
}
