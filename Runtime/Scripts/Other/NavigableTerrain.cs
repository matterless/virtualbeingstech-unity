// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
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
        ////////////////////////////////////////////////////////////////////////////////////////
        // Serialized fields

        [SerializeField]
        private BoxCollider BoundingBoxForNavServer;

        [SerializeField]
        private Bounds BoundsOfPlayerArea;
        public Bounds BoundsOfThePlayerArea => BoundsOfPlayerArea;

        [SerializeField]
        private List<GameObject> SpawningPoints;

        [SerializeField, Header("Use to visualize navmeshes in the editor")]
        private int IndexOfVisualizedDebugMesh = -1;

        [SerializeField]
        private Settings NavigableTerrainSettings;

        [Header("Flight Structure")]
        [SerializeField]
        private IFlightMap _flightMap; // TODO : better way for integration

        [SerializeField, HideInInspector]
        private int _interactableID;

        /******************************************/

        ////////////////////////////////////////////////////////////////////////////////////////
        // IInteractable

        public GameObject TopLevelParent { get { return gameObject; } }
        public bool IsDestroyed => gameObject == null;
        public IAgent DestroyedBy { get; private set; }
        public Vector3 CenterPosition => transform.position;
        public Quaternion CenterRotation => transform.rotation;
        public Vector3 SalientPosition => transform.position;
        public bool IsInInteractionDB() => false; // terrains aren't saved to interactionDB
        public bool HasProperty(InteractableProperty property) => false; // no properties
        public void Destroy(IAgent _ = null) => Destroy(gameObject);
        public Set<int> Properties { get; } = new Set<int>();
        public Vector3 RootPosition => transform.position;
        public Quaternion RootRotation => transform.rotation;
        public bool IsLocalizable => false;
        public bool IsLocal => true; // TODO multiplayer
        public float MaxRadius => BoundsOfPlayerArea.extents.magnitude;
        public float Height => 0f;
        public float Length => 0f;
        public float Width => 0f;
        public float Desirability => 0f;
        public float Repulsiveness => 0f;
        public bool IsDynamic => false;
        public bool IsHandled => false;
        public IInteractable Handler => null;
        public bool IsInPlacementMode => false;
        public IInteractionMode InteractionMode => null;

        // the following three are never used as terrains aren't saved to interactionDB
        public Vector3 Velocity { get; set; }
        public Vector3 ___PrevPosition { get; set; }
        public Vector3 CorrectiveDelta { get; set; }
        
        public int InteractableID { get => _interactableID; set => _interactableID = value; }

        public bool SetHandler(IInteractable handler) => false;
        public void ReleaseHandler(IInteractable handlerToReleaseFrom) { }

        public event Action OnHandlerChanged;

        ////////////////////////////////////////////////////////////////////////////////////////
        // INavigableTerrain

        public Vector3 CenterOfPlayerArea => BoundsOfPlayerArea.center;
        public float MinRadiusOfPlayerArea => Math.Min(BoundsOfPlayerArea.extents.x, BoundsOfPlayerArea.extents.z);
        public float MaxRadiusOfPlayerArea => Mathf.Sqrt(MMath.Sqr(BoundsOfPlayerArea.extents.x) + MMath.Sqr(BoundsOfPlayerArea.extents.z));

        /// <summary>
        /// Determine if position is inside *planar* player area and
        /// - if margin is positive: at least 'margin' inside of it (this now works just like 'ClosestPointInsidePlayerArea()');
        /// - if margin is negative: at most '-margin' away from it
        /// </summary>
        public bool IsInPlayerArea(Vector3 position, float margin = 0f)
        {
            Vector3 min = BoundsOfPlayerArea.min;
            Vector3 max = BoundsOfPlayerArea.max;

            return
                position.x > min.x + margin &&
                position.x < max.x - margin &&
                position.z > min.z + margin &&
                position.z < max.z - margin;
        }

        /// <summary>
        /// PS: why not give users of INavigableTerrain direct access to BoundsOfPlayerArea? Because they shouldn't assume
        /// that the player area is an AABB. In the future, this area may have different shapes.
        /// </summary>
        public float DistToPlayerArea(Vector3 position)
        {
            return BoundsOfPlayerArea.Contains(position)
                ? 0f
                : (BoundsOfPlayerArea.ClosestPoint(position) - position).magnitude;
        }

        /// <summary>
        /// Get closest point inside player area for 'position', and ensure that this point is
        /// at least at distance 'margin' *inside* of the border.
        /// Result undefined if margin is larger than any vector from border to center.
        /// </summary>
        public Vector3 ClosestPointInsidePlayerArea(Vector3 position, float margin = 0f)
        {
            if (margin > 0f)
            {
                Bounds reducedBoundsOfPlayerArea = new(BoundsOfPlayerArea.center, BoundsOfPlayerArea.size);
                reducedBoundsOfPlayerArea.Expand(-margin); // shrink bounds by margin

                return reducedBoundsOfPlayerArea.Contains(position)
                    ? position
                    : reducedBoundsOfPlayerArea.ClosestPoint(position);
            }
            else
                return BoundsOfPlayerArea.Contains(position)
                    ? position
                    : BoundsOfPlayerArea.ClosestPoint(position);
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
            for (int i = 0; i < _perBeingResources.Length; i++)
                _perBeingResources[i].NavServer.RegisterGameObjectAsObstacle(go, updateFrequencyInSeconds, borderOffset);
        }

        public void UnregisterGameObjectAsObstacle(GameObject go)
        {
            for (int i = 0; i < _perBeingResources.Length; i++)
                _perBeingResources[i].NavServer.UnregisterGameObjectAsObstacle(go);
        }

        public NavigationServerSynchronous GetNewNavServerAndRegisterBeing(int beingID, GameObject[] gosWithCollidersOnBeing, float borderOffset,
            out int ownCharacterLayer, out LayerMask otherCharactersLayerMask)
        {
            // TODO(Raph) make it so we use one navserver for multiple being
            //if(_dictBeingIDsToNavServers.Count >= _perBeingResources.Length)
            //{
            //    throw new Exception("Erroneous call to GetNewNavServerAndRegisterBeing; not enough space ( " +
            //        _dictBeingIDsToNavServers.Count + " >= " + _perBeingResources.Length + " ?");
            //}
            if ( _dictBeingIDsToNavServers.ContainsKey(beingID))
            {
                throw new Exception("Erroneous call to GetNewNavServerAndRegisterBeing; nav server already contains being: " +
                    _dictBeingIDsToNavServers.ContainsKey(beingID));
            }
                
            // start by finding the first unused navserver
            NavigationServerSynchronous navServer = null;
            ownCharacterLayer = 0;
            otherCharactersLayerMask = 0;

            // Use first resource
            if(_perBeingResources == null || _perBeingResources.Length == 0)
            {
                throw new Exception("NavigableTerrain not initialised.");
            }
            navServer = _perBeingResources[0].NavServer;
            ownCharacterLayer = _perBeingResources[0].Layer;
            otherCharactersLayerMask = NavigableTerrainSettings.AssignableCharacterLayers & ~(1 << ownCharacterLayer);

            //for (int i = 0; i < _perBeingResources.Length; i++)
            //{
            //    if (!_dictBeingIDsToNavServers.ContainsValue(_perBeingResources[i].NavServer))
            //    {
            //        navServer = _perBeingResources[i].NavServer;
            //        ownCharacterLayer = _perBeingResources[i].Layer;
            //        otherCharactersLayerMask = NavigableTerrainSettings.AssignableCharacterLayers & ~(1 << ownCharacterLayer);
            //        break;
            //    }
            //}

            // mark it as 'used'
            _dictBeingIDsToNavServers[beingID] = navServer;

            // then optionally insert being into the other navservers, whether or not they are used
            if (gosWithCollidersOnBeing != null)
                for (int i = 0; i < _perBeingResources.Length; i++)
                    if (navServer != _perBeingResources[i].NavServer)
                        for (int j = 0; j < gosWithCollidersOnBeing.Length; j++)
                            _perBeingResources[i].NavServer.RegisterGameObjectAsObstacle(gosWithCollidersOnBeing[j], NavigableTerrainSettings.UpdateFrequencyForBeingsInSeconds, borderOffset);

            // done
            return navServer;
        }

        public void ReleaseNavServerAndUnregisterBeing(int beingID, GameObject[] gosWithCollidersOnBeing = null)
        {
            // find the navserver registered to this being
            if (_dictBeingIDsToNavServers.TryGetValue(beingID, out NavigationServerSynchronous navServer))
            {
                // optionally unregister being from all other navservers
                if (gosWithCollidersOnBeing != null)
                    for (int i = 0; i < _perBeingResources.Length; i++)
                        if (navServer != _perBeingResources[i].NavServer)
                            for (int j = 0; j < gosWithCollidersOnBeing.Length; j++)
                                _perBeingResources[i].NavServer.UnregisterGameObjectAsObstacle(gosWithCollidersOnBeing[j]);

                // mark the navserver as unused
                _dictBeingIDsToNavServers.Remove(beingID);
            }
            else
                throw new Exception("Erroneous call to ReleaseNavServerAndUnregisterBeing: beingID not found");
        }

        /// <summary>
        /// NB: removes all previous navservers
        /// </summary>
        public void Debug_ResetNNavServers(int n)
        {
            CreateAndInitNavServers(n);
        }

        public IFlightMap GetFlightMap()
        {
            return _flightMap;
        }

        public bool GetFreeSpotInWalkableZones(Being forBeing, out Vector3 position, IInteractable reference = null, float minDistFromReference = 0f)
        {
            if (reference == null || minDistFromReference == 0f)
            {
                _interactionDB.FindAll(typeof(IWalkableZone), (i) => true, _resultBufferForInteractableSearches);
            }
            else
            {
                _interactionDB.FindAll(typeof(IWalkableZone),
                    (i) => (i.SalientPosition - reference.SalientPosition).sqrMagnitude > MMath.Sqr(minDistFromReference),
                    _resultBufferForInteractableSearches);
            }

            if (_resultBufferForInteractableSearches.Count > 0)
            {
                // get a randome item from the list of walkable zones
                var walkableZone = _resultBufferForInteractableSearches[Rand.Range(0, _resultBufferForInteractableSearches.Count)] as IWalkableZone;

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

        ////////////////////////////////////////////////////////////////////////////////////////
        // internal stuff

        private void Update()
        {
#if UNITY_EDITOR
            for (int i = 0; i < _perBeingResources.Length; i++)
                _perBeingResources[i].NavServer.ShowDebugMesh = i == IndexOfVisualizedDebugMesh;
#endif

            for (int i = 0; i < _perBeingResources.Length; i++)
                _perBeingResources[i].NavServer.UpdateOncePerFrame();
        }

        Container Container => Container.Instance;

        protected virtual void Awake()
        {
            Debug.Log(Container);
            Debug.Log(Container.WorldEvents);
            Init(Container.WorldEvents, Container.InteractionDB, Container.NavigableTerrainManager);
        }

        private void OnEnable()
        {
            _worldEvents.Raise(new Event_World_RegisterNavigableTerrain(this));
        }

        private void Init(EventManager worldEvents, InteractionDB interactionDB, NavigableTerrainManager navigableTerrainManager)
        {
            _worldEvents = worldEvents;
            _navigableTerrainManager = navigableTerrainManager;
            _interactionDB = interactionDB;

            CreateAndInitNavServers(1);

            // Register navServer with the gameManager to make it globally available; unregistering happens in OnDestroy().
            //
            // PS: important that his happens here rather than in OnEnable(), so that obstacles (TableWorldObject etc) can register in OnEnable even
            // when they are created and injected together with this navigable terrain
            //_gameManager.WorldEvents.Raise(new Event_World_RegisterNavigableTerrain(this));
        }

        private void CreateAndInitNavServers(int nNavServers)
        {
            List<int> assignableLayers = new(32);

            for (int i = 0; i < 32; i++)
            {
                if ((NavigableTerrainSettings.AssignableCharacterLayers & (1 << i)) != 0)
                    assignableLayers.Add(i);
            }

            if (assignableLayers.Count < nNavServers)
                throw new Exception("Not enough character layers defined: " + assignableLayers.Count + " (vs " + nNavServers + " needed). " +
                    "You need to assign a layer to your being, then specify it in the NavigableTerrain.AssignableCharacterLayers");

            //_perBeingResources = new PerBeingResources[nNavServers];
            _perBeingResources = new PerBeingResources[1]; // TODO(RAPH)

            for (int i = 0; i < nNavServers; i++)
            {
                _perBeingResources[i] = new PerBeingResources
                {
                    NavServer = new NavigationServerSynchronous(transform, BoundingBoxForNavServer, NavigableTerrainSettings.Epsilon,
                        NavigableTerrainSettings.TimeInMsPerFrameForNavMeshProcessing, false, NavigableTerrainSettings.YValueForDebugMesh),
                    Layer = assignableLayers[i],
                };
            }

            foreach (Collider c in FindObjectsOfType<Collider>(false))
            {
                IObstacle obstacle = c.gameObject.GetComponent<IObstacle>();

                if (c.gameObject.activeInHierarchy && obstacle != null && obstacle.ObstacleType == NavigableTerrainManager.ObstacleType.Static)
                {
                    for (int i = 0; i < nNavServers; i++)
                    {
                        _perBeingResources[i].NavServer.RegisterGameObjectAsObstacle(c.gameObject);
                    }
                }
            }

            foreach (MeshFilter meshFilter in FindObjectsOfType<MeshFilter>(true))
            {
                IObstacle obstacle = meshFilter.gameObject.GetComponent<IObstacle>();

                if (meshFilter.gameObject.activeInHierarchy && obstacle != null && obstacle.ObstacleType == NavigableTerrainManager.ObstacleType.Static)
                {
                    for (int i = 0; i < nNavServers; i++)
                    {
                        _perBeingResources[i].NavServer.RegisterGameObjectAsObstacle(meshFilter.gameObject);
                    }
                }
            }
        }

        private EventManager _worldEvents;
        private NavigableTerrainManager _navigableTerrainManager;
        private InteractionDB _interactionDB;
        private PerBeingResources[] _perBeingResources;
        private readonly Dictionary<int, NavigationServerSynchronous> _dictBeingIDsToNavServers = new();
        private readonly List<IInteractable> _resultBufferForInteractableSearches = new();

        private struct PerBeingResources
        {
            public NavigationServerSynchronous NavServer;
            public int Layer;
        }

        [Serializable]
        public class Settings
        {
            [Header("Navigation server")]
            public double Epsilon = .002;
            public float TimeInMsPerFrameForNavMeshProcessing = 1f;
            public float YValueForDebugMesh = .01f;
            public float UpdateFrequencyForBeingsInSeconds = .33f;
            public LayerMask AssignableCharacterLayers;
        }
    }
}
