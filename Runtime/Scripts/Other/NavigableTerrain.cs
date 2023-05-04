// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.UnityIntegration.NavigableTerrain;
using VirtualBeings.Tech.Utils;

namespace VirtualBeings.Tech.Shared
{
    // Contrary to INavigableTerrain, NavigableTerrain assumes to be AABB.
    /// <summary>
    /// Navigable terrain must execute before other MonoBehaviour to register as a navigable terrain :
    /// since Being and Interactable need these terrain when they awake (so they are executed after the terrain)
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class NavigableTerrain : MonoBehaviour, IInteractable, INavigableTerrain
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="collider">
        /// The navigable collider to use to construct the navigable terrain.
        /// Only MeshCollider, BoxCollider and SphereCollider supported
        /// **Mandatory, Cannot be null**
        /// </param>
        /// <param name="floorHeight">
        /// The height of the floor of the navigable terrain. If not set, the height of the parent transform
        /// will be used as the floor height.
        /// </param>
        /// <param name="margin">The wiggle room area around the collider. Will be clamped to 0 if negative.</param>
        /// <param name="refreshRate">
        /// The main collider refresh rate. Useful in the case of a movable collider, or if the input can vary a bit
        /// (e.g. AR Planes).
        /// Can be -1 to disable it and save resources in case of a static collider.
        /// </param>
        /// <param name="localSpaceNavigation">
        /// Whether the navigation mesh should be defined relative to it's container game object or in world space.
        /// Should be set to true for movable platforms for example.
        /// </param>
        /// <param name="parent">The parent of the navigable terrain. Can be null.</param>
        /// <param name="name">The name of the navigable terrain. Can be empty.</param>
        /// <param name="enableMeshDebugging"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static NavigableTerrain CreateNavigableTerrain(
            Collider collider,
            float? floorHeight = null,
            float margin = 1f,
            float refreshRate = -1f,
            bool localSpaceNavigation = false,
            Transform parent = null,
            string name = "",
            bool enableMeshDebugging = false
        )
        {
            if (collider == null)
            {
                throw new Exception("A collider must be provided to create a navigable terrain");
            }

            GameObject navGameObject = new(name);

            if (parent != null)
            {
                navGameObject.transform.parent = parent;
            }

            navGameObject.transform.localPosition = Vector3.zero;

            if (floorHeight.HasValue)
            {
                Vector3 p = navGameObject.transform.position;
                p.y = floorHeight.Value;

                navGameObject.transform.position = p;
            }

            navGameObject.transform.localRotation = Quaternion.identity;

            navGameObject.transform.localScale = Vector3.one;

            NavigableTerrain navTerrain = navGameObject.AddComponent<NavigableTerrain>();

            navTerrain.NavigationAreaMargin         = Mathf.Max(margin, 0f);
            navTerrain.NavigationAreaRefreshRate    = refreshRate;
            navTerrain.EnableNavigableMeshDebugging = enableMeshDebugging;
            navTerrain.NavigationIsInLocalSpace     = localSpaceNavigation;

            navTerrain.SetNavigableCollider(collider);

            return navTerrain;
        }

        // ------------------------------
        // Serialized fields

        [FormerlySerializedAs("BoundingBoxForNavServer"), SerializeField]
        private Collider NavigableCollider;

        private Bounds _bounds
        {
            get
            {
                if (NavigableCollider != null)
                    return NavigableCollider.bounds;
                else
                    return new Bounds();
            }
        }
       

        [SerializeField]
        private bool NavigationIsInLocalSpace;

        [SerializeField, Min(0f)]
        private float NavigationAreaMargin = 1f;

        [SerializeField, Tooltip("The rate at which the main navigation collider should be refreshed. A negative value means static.")]
        private float NavigationAreaRefreshRate = -1f;

        [SerializeField]
        private List<GameObject> SpawningPoints;

        [SerializeField, Header("Use to visualize navmeshes in the editor")]
        public bool EnableNavigableMeshDebugging;

        [SerializeField]
        public Settings NavigableTerrainSettings = new();

        // TODO(Raph): Flight map got removed from Navigable Terrain
        // [Header("Flight Structure")]
        // [SerializeField]
        // private IFlightMap _flightMap; // TODO : better way for integration

        [SerializeField, HideInInspector]
        private int _interactableID;

        // ------------------------------
        // IInteractable

        public GameObject TopLevelParent  => gameObject;
        public bool       IsDestroyed     => gameObject == null;
        public IAgent     DestroyedBy     { get; private set; }
        public Quaternion CenterRotation  => transform.rotation;
        public Vector3    SalientPosition => transform.position;
        public bool IsInInteractionDB() => false; // terrains aren't saved to interactionDB
        public bool HasProperty(InteractableProperty property) => false; // no properties
        public void Destroy(IAgent _ = null) => Destroy(gameObject);
        public Set<int>         Properties        { get; } = new Set<int>();
        public Vector3          RootPosition      => transform.position;
        public Quaternion       RootRotation      => transform.rotation;
        public bool             IsLocalizable     => false;
        public bool             IsLocal           => true; // TODO multiplayer
        public float            MaxRadius         => _bounds.extents.magnitude;
        public float            Length            => 0f;
        public float            Width             => 0f;
        public float            Height            => 0f;
        public float            Desirability      => 0f;
        public float            Repulsiveness     => 0f;
        public bool             IsDynamic         => false;
        public bool             IsHandled         => false;
        public IInteractable    Handler           => null;
        public bool             IsInPlacementMode => false;
        public IInteractionMode InteractionMode   => null;

        // the following three are never used as terrains aren't saved to interactionDB
        public Vector3 Velocity { get; set; }
        public Vector3 ___PrevPosition { get; set; }
        public Vector3 CorrectiveDelta { get; set; }

        public int InteractableID { get => _interactableID; set => _interactableID = value; }

        public bool SetHandler(IInteractable handler) => false;
        public void ReleaseHandler(IInteractable handlerToReleaseFrom) { }

        public event Action OnHandlerChanged;

        // ------------------------------
        // INavigableTerrain

        public Collider  NavigationCollider                 => NavigableCollider;
        public Transform Transform                          => transform;
        public bool      DefineNavigableTerrainInLocalSpace => NavigationIsInLocalSpace;
        public float     Epsilon                            => NavigableTerrainSettings.Epsilon;
        public float MaxTimePerFrameForNavigationProcessingInMs =>
            NavigableTerrainSettings.TimeInMsPerFrameForNavMeshProcessing;
        public ILineSegmentVisualizer LineSegmentVisualizer { get; private set; }
        public float                  FloorHeight           => transform.position.y;
        public bool                   EnableDebugView       => EnableNavigableMeshDebugging;
        public Vector3 CenterPosition
        {
            get
            {
                if (NavigableCollider != null)
                    return new Vector3(_bounds.center.x, FloorHeight, _bounds.center.z); // TODO(Raph) : recalculate only if navigableTerrain has moved
                else
                    return Vector3.zero;
            }
        }
       

        public float Margin => NavigationAreaMargin;

        public float RefreshRate => NavigationAreaRefreshRate;

        public Vector3 CenterOfPlayerArea => _bounds.center;
        public float MinRadiusOfPlayerArea => Math.Min(_bounds.extents.x, _bounds.extents.z);
        public float MaxRadiusOfPlayerArea => Mathf.Sqrt(MMath.Sqr(_bounds.extents.x) + MMath.Sqr(_bounds.extents.z));

        /// <summary>
        /// The height of the navigable plane, with the HeightForGroundingRaycasts taken into account
        ///
        /// "true" navigable plane:                       ---------------------------------------
        /// yOffset,                                       ↕ Settings.HeightForGroundingRaycasts
        /// final height considered as the plane ceiling: =======================================
        ///
        /// </summary>
        public bool IsInsideBounds(Vector3 position, float yOffset, float margin = 0)
        {
            Vector3 min = _bounds.min;
            Vector3 max = _bounds.max;

            return
                position.x > min.x + margin &&
                position.x < max.x - margin &&
                position.z > min.z + margin &&
                position.z < max.z - margin &&
                position.y >= transform.position.y - yOffset;
        }

        public float DistToBounds(Vector3 position)
        {
            return _bounds.Contains(position)
                ? 0f
                : (_bounds.ClosestPoint(position) - position).magnitude;
        }

        public Vector3 ClosestPointInsideBounds(Vector3 position, float margin = 0)
        {
            if (margin > 0f)
            {
                Bounds reducedBoundsOfPlayerArea = new(_bounds.center, _bounds.size);
                reducedBoundsOfPlayerArea.Expand(-margin); // shrink bounds by margin

                return reducedBoundsOfPlayerArea.Contains(position)
                    ? position
                    : reducedBoundsOfPlayerArea.ClosestPoint(position);
            }

            return _bounds.Contains(position)
                ? position
                : _bounds.ClosestPoint(position);
        }

        private void OnDestroy()
        {
            _worldEvents.Raise(new Event_World_UnregisterNavigableTerrain(this));
        }

        public Vector3 GetNearestPointOnSurface(Vector3 referencePoint) => Vector3.zero;

        public IReadOnlyList<GameObject> GetSpawningPositions()
        {
            return SpawningPoints;
        }

        public void RegisterGameObjectAsObstacle(GameObject go, float updateFrequencyInSeconds = 0f, float borderOffset = 0f)
        {
            Container.NavigableTerrainManager.RegisterGameObjectAsObstacle(
                this,
                go,
                updateFrequencyInSeconds,
                borderOffset
            );
        }

        public void UnregisterGameObjectAsObstacle(GameObject go)
        {
            Container.NavigableTerrainManager.UnregisterGameObjectAsObstacle(this, go);
        }

        [Obsolete("This method is only used for the (early) CaaS demo and will be removed soon")]
        public void GetDebugPath(Vector3 start, Vector3 goal, float clearance, List<Vector3> resultBuffer)
        {
            Container.NavigableTerrainManager.GetDebugPath(this, start, goal, clearance, resultBuffer);
        }

        public bool GetFreeSpotInWalkableZones(
            Being forBeing,
            out Vector3 position,
            IInteractable reference = null,
            float minDistFromReference = 0f
        )
        {
            if (reference == null || minDistFromReference == 0f)
            {
                _interactionDB.FindAll(typeof(IWalkableZone), (i) => true, _resultBufferForInteractableSearches);
            }
            else
            {
                _interactionDB.FindAll(
                    typeof(IWalkableZone),
                    (i) => (i.SalientPosition - reference.SalientPosition).sqrMagnitude >
                        MMath.Sqr(minDistFromReference),
                    _resultBufferForInteractableSearches
                );
            }

            if (_resultBufferForInteractableSearches.Count > 0)
            {
                // get a randome item from the list of walkable zones
                var walkableZone =
                    _resultBufferForInteractableSearches[Rand.Range(0, _resultBufferForInteractableSearches.Count)] as
                        IWalkableZone;

                // use it to get a free position
                if (walkableZone.GetRandomFreePosition(forBeing, forBeing.MaxRadius, out position))
                    return true;
            }

            position = Vector3.zero;
            return false;
        }

        public IRootParentProvider GetFreeRootParentProvider(Being forBeing, RootParentType rootParentType = RootParentType.Any,
                                                             IInteractable reference = null, float minDistFromReference = float.MaxValue)
        {
            bool predicate(IInteractable i)
            {
                return
                    i is IRootParentProvider r &&
                    (rootParentType == RootParentType.Any || rootParentType == r.RootParentType) &&
                    r.ReadyForRootParenting(forBeing) &&
                    (reference == null || minDistFromReference == 0f ||
                        (i.SalientPosition - reference.SalientPosition).sqrMagnitude > MMath.Sqr(minDistFromReference));
            }

            _interactionDB.FindAll(typeof(IRootParentProvider), predicate, _resultBufferForInteractableSearches);

            if (_resultBufferForInteractableSearches.Count > 0)
                return _resultBufferForInteractableSearches[Rand.Range(0, _resultBufferForInteractableSearches.Count)] as IRootParentProvider;

            return null;
        }

        public void SetNavigableCollider(Collider collider)
        {
            if (NavigableCollider != null)
            {
                throw new Exception("This function should only be called once, and if no collider has been set before");
            }

            NavigableCollider = collider;

            _worldEvents.Raise(new Event_World_RegisterNavigableTerrain(this));
        }

        // ------------------------------
        // internal stuff

        private void Update()
        {
            if (NavigableCollider == null)
            {
                _worldEvents.Raise(new Event_World_UnregisterNavigableTerrain(this));
                Destroy(gameObject);
                return;
            }

            Container.NavigableTerrainManager.UpdateNavigation(this);
        }

        Container Container => Container.Instance;

        protected virtual void Awake()
        {
            Init(Container.WorldEvents, Container.InteractionDB);
        }

        private void OnEnable()
        {
            if (NavigableCollider != null)
            {
                _worldEvents.Raise(new Event_World_RegisterNavigableTerrain(this));
            }
        }

        private void Init(EventManager worldEvents, InteractionDB interactionDB)
        {
            _worldEvents = worldEvents;
            _interactionDB = interactionDB;

            LineSegmentVisualizer = NavigableTerrainSettings.LineSegmentVisualizer == null
                ? null
                : Instantiate(NavigableTerrainSettings.LineSegmentVisualizer, transform)
                   .GetComponent<LineSegmentVisualizer>();

            // Register navServer with the gameManager to make it globally available; unregistering happens in OnDestroy().
            //
            // PS: important that his happens here rather than in OnEnable(), so that obstacles (TableWorldObject etc) can register in OnEnable even
            // when they are created and injected together with this navigable terrain
            //_gameManager.WorldEvents.Raise(new Event_World_RegisterNavigableTerrain(this));
        }

        private EventManager _worldEvents;
        private InteractionDB _interactionDB;
        private readonly List<IInteractable> _resultBufferForInteractableSearches = new();

        [Serializable]
        public class Settings
        {
            [Header("Navigation server")]
            public float Epsilon = 0.002f;
            public float TimeInMsPerFrameForNavMeshProcessing = 1f;
            public GameObject LineSegmentVisualizer; // unused if null
        }
    }
}
