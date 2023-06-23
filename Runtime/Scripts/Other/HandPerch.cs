// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;
using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings.Tech.Shared
{
    public abstract class HandPerch : MonoBehaviour, IVisibleExtremity, IPerch
    {
        public bool IsTouching { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////
        // Fields serialized by Unity

        [SerializeField]
        private GameObject[] _childrenForInteractionDB = new GameObject[0];

        [SerializeField]
        private float _maxRadius;

        [SerializeField]
        private float _length;

        [SerializeField]
        private float _minPerchRadius, _maxPerchRadius;

        [SerializeField]
        private InteractableProperty[] _interactableProperties;

        [Space, Header("IPerch interface"), SerializeField, Tooltip("Must not be null")]
        private Transform _connectionTransform;

        [SerializeField, Tooltip("Must be either null/empty OR contain exactly two transforms")]
        private Transform[] _transformsForBendAnalysis;

        [SerializeField]
        private RootParentType _rootParentType = RootParentType.Perch;

        [SerializeField, Tooltip("Max roll angle between Vector3.up and this perch up")]
        private float _maxPermissibleRoll = 110f;

        [SerializeField, Tooltip("Max tilt angle between perch forward and forward-on-XZ")]
        private float _maxPermissibleTilt = 30f;

        [SerializeField, Tooltip("Max angle permissible between first and last part of index finger")]
        private float _maxPermissibleBendAmount = 15f;

        [SerializeField, Tooltip("At what distance between ConnectionPosition and Being.RootPosition do we force termination of *inactive* root parenting?")]
        private float _maxPermissibleDistFromConnectionPosition = .25f;

        [SerializeField, Tooltip("Which parts of the hand have touch colliders? (Currently only 1 ATouchDataProvider is supported)")]
        protected ATouchDataProvider[] _touchDataProviders = new ATouchDataProvider[0];

        [SerializeField, HideInInspector]
        private int _interactableID;


        ////////////////////////////////////////////////////////////////////////////////////
        /// Initialization & Unity messages

        protected Container Container => Container.Instance;

        private void Awake()
        {
            PlayerAgent player = GetComponentInParent<PlayerAgent>();
            if (player == null)
            {
                throw new Exception("Could not find PlayerAgent script in parent. Is this PlayerHandWrapper child of a player agent ?");
            }
            Init(Container.WorldEvents, player, Container.InteractionDB);
        }

        protected virtual void Init(EventManager worldEvents, PlayerAgent playerAgent, InteractionDB interactionDB)
        {
            _visibleAgent = playerAgent;
            _interactionDB = interactionDB;

            playerAgent.RegisterExtremity(this);

            if (_interactableProperties != null)
                foreach (InteractableProperty v in _interactableProperties)
                    Properties.Add((int)v);

            _colliders = GetComponentsInChildren<Collider>(true);

            foreach (ATouchDataProvider touchDataProvider in _touchDataProviders)
                touchDataProvider.Init(worldEvents, interactionDB);
        }

        /// <summary>
        /// </summary>
        private void OnEnable()
        {
            _interactionDB.RegisterInteractable(gameObject, this as IInteractable);
            _interactionDB.RegisterInteractable(gameObject, this as IRootParentProvider);
            _interactionDB.RegisterInteractable(gameObject, this as IPerch);
            _interactionDB.RegisterInteractable(gameObject, this as IVisibleExtremity);

            if (_childrenForInteractionDB != null)
                foreach (GameObject child in _childrenForInteractionDB)
                    _interactionDB.RegisterChild(child, gameObject);

            OnEnableSubclass();
        }

        private void OnDisable()
        {
            _interactionDB?.UnregisterInteractable(gameObject, this as IInteractable);
            _interactionDB?.UnregisterInteractable(gameObject, this as IRootParentProvider);
            _interactionDB?.UnregisterInteractable(gameObject, this as IPerch);
            _interactionDB?.UnregisterInteractable(gameObject, this as IVisibleExtremity);

            _cachedTransform_Position = transform.position;
            _cachedTransform_Rotation = transform.rotation;

            _cachedConnectionTransform_Position = _connectionTransform.position + (_maxPerchRadius + _minPerchRadius) / 2f * Vector3.up;
            _cachedConnectionTransform_Rotation = _connectionTransform.rotation;

            OnDisableSubclass();
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /// IPerch implementation

        public Action OnBeingPerched = () => { };
        public Action OnBeingUnperched = () => { };

        public bool HasBeingPerched => _currentPercher != null;
        public RootParentType RootParentType => RootParentType.Finger;

        public Vector3 ConnectionPosition(Being being = null)
        {
            Vector3 offset = being == null
                ? Vector3.zero
                : being.GetOffsetForRootParenting(this);

            if (this == null)
                return _cachedConnectionTransform_Position + offset;

            return _connectionTransform.position + (_maxPerchRadius + _minPerchRadius) / 2f * Vector3.up + offset;
        }

        public Quaternion ConnectionRotation => this != null ? _connectionTransform.rotation : _cachedConnectionTransform_Rotation;
        public Vector3 ConnectionForward => this != null ? _connectionTransform.forward : _cachedConnectionTransform_Rotation * Vector3.forward;
        public Vector3 ConnectionUp => this != null ? _connectionTransform.up : _cachedConnectionTransform_Rotation * Vector3.up;
        public Vector3 ConnectionRight => this != null ? _connectionTransform.right : _cachedConnectionTransform_Rotation * Vector3.right;
        public float MaxPermissibleDistFromConnectionPosition => _maxPermissibleDistFromConnectionPosition;

        public float BendAmount
        {
            get
            {
                Vector3 p0 = _transformsForBendAnalysis[0].position;
                Vector3 p1 = _transformsForBendAnalysis[1].position;
                Vector3 p2 = _transformsForBendAnalysis[2].position;
                return Vector3.Angle(p2 - p1, p1 - p0);
            }
        }

        public virtual bool ReadyForRootParenting(Being being)
        {
            return
                (_currentPercher == null || _currentPercher == being) &&
                (SalientPosition - OwningAgent.SalientPosition).sqrMagnitude > MMath.Sqr(.15f) && // hand cannot be too close to face
                SalientPosition.y > 0.5f && // hand cannot be too low
                                            //BendAmount < _maxPermissibleBendAmount && // TODO reinstate if we ever use hand-tracking
                Vector3.Angle(ConnectionUp, Vector3.up) < _maxPermissibleRoll &&
                Vector3.Angle(ConnectionRight, ConnectionRight.OnXZ()) < _maxPermissibleTilt;
        }

        public void NotifyUseOfPerchPoint(Being being)
        {
            if (_currentPercher == null || _currentPercher != being)
            {
                _currentPercher = being;
                OnBeingPerched?.Invoke(); // Invoke after so we have the right reference of perched being
            }
        }

        public void ReleaseUseOfPerchPoint(Being being)
        {
            if (_currentPercher == being)
            {
                OnBeingUnperched?.Invoke(); // Invoke before so we have the right reference of perched being
                _currentPercher = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /// IVisibleExtremity implementation

        public abstract ExtremityType ExtremityType { get; }
        public IAgent OwningAgent { get; set; }
        public IInteractable HandledObject { get; private set; }

        public bool SetHandledObject(IInteractable interactable)
        {
            HandledObject = interactable;
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /// IInteractable implementation

        public GameObject TopLevelParent => gameObject;
        public Set<int> Properties { get; } = new Set<int>();
        public Vector3 RootPosition => this != null ? transform.position : _cachedTransform_Position;
        public Quaternion RootRotation => this != null ? transform.rotation : _cachedTransform_Rotation;
        public Vector3 CenterPosition => ConnectionPosition();
        public Quaternion CenterRotation => ConnectionRotation;
        public Vector3 SalientPosition => ConnectionPosition();
        public Quaternion SalientRotation => ConnectionRotation;

        public Vector3 Velocity { get; set; } // administered by SpatialDB, dont change manually
        public Vector3 ___PrevPosition { get; set; } // administered by SpatialDB, dont use
        public Vector3 CorrectiveDelta { get; set; } // normally zero; can be set by users *per frame* to make SpatialDB correct Velocity

        public int InteractableID { get => _interactableID; set => _interactableID = value; }

        public bool IsLocalizable => IsInInteractionDB();
        public bool IsLocal => true; // TODO multiplayer
        public bool IsDynamic => true;
        public bool IsHandled => Handler != null;
        public bool IsDestroyed => this == null;
        public float MaxRadius => _maxRadius;
        public float Height => _maxRadius * 2f;
        public float Width => _maxRadius * 2f;
        public float Length => _length;
        public IInteractable Handler => null;
        public bool IsInPlacementMode => false;
        public IInteractionMode InteractionMode => null;

        protected virtual void OnEnableSubclass() { }
        protected virtual void OnDisableSubclass() { }

        public bool SetHandler(IInteractable handler) => false;
        public void ReleaseHandler(IInteractable handlerToReleaseFrom) { }
        public event Action OnHandlerChanged;

        public bool HasProperty(InteractableProperty property) => Properties.Contains((int)property);
        public bool IsInInteractionDB() => this != null && _interactionDB.IsInDatabase(this, typeof(IInteractable));
        public IAgent DestroyedBy { get; private set; }

        public Vector3 ApproachTarget => ConnectionPosition() + Vector3.up * 0.15f;

        public float MaxApproachAngle => 45f;

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
            Vector3 point = CenterPosition;

            if (_colliders == null)
                return point;
            else
            {
                foreach (Collider collider in _colliders)
                {
                    Vector3 nearestPoint = collider.ClosestPoint(referencePoint);

                    if ((nearestPoint - referencePoint).sqrMagnitude < (point - referencePoint).sqrMagnitude)
                        point = nearestPoint;
                }

                return point;
            }
        }

        protected IAgent _visibleAgent;
        protected InteractionDB _interactionDB;
        protected Being _currentPercher;

        private Collider[] _colliders;

        private Vector3 _cachedTransform_Position;
        private Quaternion _cachedTransform_Rotation;

        private Vector3 _cachedConnectionTransform_Position;
        private Quaternion _cachedConnectionTransform_Rotation;

    }
}