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
    public class NavigableTerrain : MonoBehaviour, INavigableTerrain
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
            float margin = 0.1f,
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

            navTerrain._enableDebugView         = enableMeshDebugging;
            navTerrain._maxSize                 = Mathf.Max(margin, 0f);
            navTerrain.NavigationIsInLocalSpace = localSpaceNavigation;

            navTerrain.SetNavigableCollider(collider);

            return navTerrain;
        }

        // ------------------------------
        // Serialized fields

        [FormerlySerializedAs("BoundingBoxForNavServer"), SerializeField]
        private Collider NavigableCollider;

        private Bounds Bounds => NavigableCollider != null ? NavigableCollider.bounds : new Bounds();

        [SerializeField]
        private bool NavigationIsInLocalSpace;

        [SerializeField, Min(0f),
         Tooltip(
             "Maximum size (in meter) the navigable terrain should be able to grow. Note that this is a hard upper limit" +
             " and it can be arbitrarily large (should be kept in the order of magnitude of the maximum size the collider" +
             "will ever be able to grow to)"
         )]
        private float _maxSize = 1f;

        [SerializeField]
        private List<GameObject> SpawningPoints;

        [SerializeField]
        public Settings NavigableTerrainSettings = new();

        [SerializeField, Header("Used to visualize navmeshes in the editor")]
        private bool _enableDebugView;

        [SerializeField]
        private bool _displayWholeNavMesh;

        // ------------------------------
        // INavigableTerrain

        public void UpdateCollider(Collider collider)
        {
            Container.NavigableTerrainManager.UpdateNavigationCollider(this, collider);
        }

        public Collider  NavigationCollider                 => NavigableCollider;
        public Transform Transform                          => transform;
        public bool      DefineNavigableTerrainInLocalSpace => NavigationIsInLocalSpace;
        public float     Epsilon                            => NavigableTerrainSettings.Epsilon;
        public float MaxTimePerFrameForNavigationProcessingInMs =>
            NavigableTerrainSettings.TimeInMsPerFrameForNavMeshProcessing;
        public ILineSegmentVisualizer LineSegmentVisualizer
        {
            get => _lineSegmentVisualizer;
            set
            {
                _lineSegmentVisualizer = value;
                Container?.NavigableTerrainManager?.SetVisualizer(this, value);
            }
        }
        private ILineSegmentVisualizer _lineSegmentVisualizer;
        public  float                  FloorHeight => transform.position.y;

        public bool EnableDebugView
        {
            get => _enableDebugView;
            set
            {
                _enableDebugView = value;
                Container?.NavigableTerrainManager?.SetDebugView(this, value);
            }
        }

        public bool DisplayWholeNavMesh
        {
            get => _displayWholeNavMesh;
            set {
                _displayWholeNavMesh = value;
                Container?.NavigableTerrainManager?.DisplayWholeNavMesh(this, value);
            }
        }

        public Vector3 CenterPosition
        {
            get
            {
                if (NavigableCollider != null)
                    return new Vector3(
                        Bounds.center.x,
                        FloorHeight,
                        Bounds.center.z
                    ); // TODO(Raph) : recalculate only if navigableTerrain has moved
                else
                    return Vector3.zero;
            }
        }
        public float MaxSize => _maxSize;

        public Vector3 CenterOfPlayerArea    => Bounds.center;
        public float   MinRadiusOfPlayerArea => Math.Min(Bounds.extents.x, Bounds.extents.z);
        public float   MaxRadiusOfPlayerArea => Mathf.Sqrt(MMath.Sqr(Bounds.extents.x) + MMath.Sqr(Bounds.extents.z));

        public bool IsInsideBounds(Vector3 position, float yUnderOffset, float yMaxCeiling, float margin = 0)
        {
            Vector3 min = Bounds.min;
            Vector3 max = Bounds.max;

            return
                position.x > min.x + margin &&
                position.x < max.x - margin &&
                position.z > min.z + margin &&
                position.z < max.z - margin &&
                position.y >= transform.position.y - yUnderOffset &&
                position.y <= transform.position.y + yMaxCeiling;
        }

        public float DistToBounds(Vector3 position)
        {
            return Bounds.Contains(position)
                ? 0f
                : (Bounds.ClosestPoint(position) - position).magnitude;
        }

        public Vector3 ClosestPointInsideBounds(Vector3 position, float margin = 0)
        {
            if (margin > 0f)
            {
                Bounds reducedBoundsOfPlayerArea = new(Bounds.center, Bounds.size);
                reducedBoundsOfPlayerArea.Expand(-margin); // shrink bounds by margin

                return reducedBoundsOfPlayerArea.Contains(position)
                    ? position
                    : reducedBoundsOfPlayerArea.ClosestPoint(position);
            }

            return Bounds.Contains(position)
                ? position
                : Bounds.ClosestPoint(position);
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

        public IRootParentProvider GetFreeRootParentProvider(
            Being forBeing,
            RootParentType rootParentType = RootParentType.Any,
            IInteractable reference = null,
            float minDistFromReference = float.MaxValue
        )
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
                return _resultBufferForInteractableSearches[Rand.Range(0, _resultBufferForInteractableSearches.Count)]
                    as IRootParentProvider;

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

        private void OnDisable()
        {
            if (NavigableCollider != null)
            {
                _worldEvents.Raise(new Event_World_UnregisterNavigableTerrain(this));
            }
        }

        private void Init(EventManager worldEvents, InteractionDB interactionDB)
        {
            _worldEvents   = worldEvents;
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

        private          EventManager        _worldEvents;
        private          InteractionDB       _interactionDB;
        private readonly List<IInteractable> _resultBufferForInteractableSearches = new();

        [Serializable]
        public class Settings
        {
            [Header("Navigation server")]
            public float Epsilon = 0.002f;
            public float      TimeInMsPerFrameForNavMeshProcessing = 1f;
            public GameObject LineSegmentVisualizer; // unused if null
        }
    }
}
