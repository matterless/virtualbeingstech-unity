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
            _colliders = GetComponentsInChildren<Collider>(false);

            foreach (Collider c in _colliders)
            {
                switch(ObstacleType)
                {
                    case ObstacleType.Dynamic:
                        _navigableTerrainManager.RegisterGameObjectAsObstacle(c.gameObject,
                            _overrideRefreshPeriod > 0f ? _overrideRefreshPeriod : _navigableTerrainManager.DynamicAIObstacleRefreshPeriod);
                        break;

                    case ObstacleType.Static:
                        _navigableTerrainManager.RegisterGameObjectAsObstacle(c.gameObject);
                        break;
                }
            }
        }

        private void OnDisable()
        {
            foreach (Collider c in _colliders)
                _navigableTerrainManager.UnregisterGameObjectAsObstacle(c.gameObject);
        }

        private NavigableTerrainManager _navigableTerrainManager;
        private Collider[] _colliders;
    }
}
