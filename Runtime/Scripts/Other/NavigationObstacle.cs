// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.UnityIntegration.NavigableTerrain;
using static VirtualBeings.Tech.UnityIntegration.NavigableTerrainManager;

namespace VirtualBeings.Tech.Shared
{
    /// <summary>
    /// For objects that need to self-register as navigation obstacles *without* participating in the interaction system
    /// </summary>
    [DisallowMultipleComponent]
    public class NavigationObstacle : MonoBehaviour, IObstacle
    {
        public ObstacleType ObstacleType { get => _obstacleType; set => _obstacleType = value; }
        private Container Container => Container.Instance;

        [SerializeField]
        private ObstacleType _obstacleType;

        [SerializeField]
        private float _overrideRefreshPeriod = -1f;

        private void Awake()
        {
            Init(Container.NavigableTerrainManager);
        }

        private void Init(NavigableTerrainManager navigableTerrainManager)
        {
            _navigableTerrainManager = navigableTerrainManager;
        }

        private void OnEnable()
        {
            _terrain = _navigableTerrainManager.FindOwningNavigableTerrain(transform.position, 0f);
            _colliders = GetComponentsInChildren<Collider>(false);

            if(_terrain == null)
            {
                Misc.LogWarning($"{this} could not find a NavigableTerrain to register to. This obstacle need to be inside one !");
                return;
            }

            foreach (Collider c in _colliders)
            {
                switch(ObstacleType)
                {
                    case ObstacleType.Dynamic:
                        _terrain.RegisterGameObjectAsObstacle(c.gameObject,
                            _overrideRefreshPeriod > 0f ? _overrideRefreshPeriod : _navigableTerrainManager.DynamicAIObstacleRefreshPeriod);
                        break;

                    case ObstacleType.Static:
                        _terrain.RegisterGameObjectAsObstacle(c.gameObject);
                        break;

                    default:
                    case ObstacleType.None:
                        Debug.LogWarning(this + " as NavigableObstacle has its obstacle type set to None. It will not be registered as an obstacle.");
                        break;
                }
            }
        }

        private void OnDisable()
        {
            if(_terrain == null)
            {
                return;
            }

            foreach (Collider c in _colliders)
                _terrain.UnregisterGameObjectAsObstacle(c.gameObject);
        }

        private NavigableTerrainManager _navigableTerrainManager;
        private INavigableTerrain _terrain;
        private Collider[] _colliders;
    }
}
