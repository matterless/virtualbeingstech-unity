// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;

namespace VirtualBeings.Tech.Shared
{
    public class NavigationTerrainDebug : MonoBehaviour
    {
        public GameObject LineSegmentVisualizerPrefab;

        private NavigableTerrainManager NavigableTerrainManager => Container.Instance.NavigableTerrainManager;
        private EventManager WorldEvents => Container.Instance.WorldEvents;

        private void Start()
        {
            WorldEvents.AddListener<Event_World_RegisterNavigableTerrain>(OnRegisterNavigableTerrain);

            foreach(NavigableTerrain navigableTerrain in NavigableTerrainManager.NavigableTerrains)
            {
                AddVisualization(navigableTerrain);
            }
        }

        private void OnRegisterNavigableTerrain(Event_World_RegisterNavigableTerrain evt)
        {
            AddVisualization(evt.NavigableTerrain as NavigableTerrain);
        }

        private void AddVisualization(NavigableTerrain navigableTerrain)
        {
            if (navigableTerrain.EnableDebugView && navigableTerrain.LineSegmentVisualizer != null)
            {
                return;
            }

            GameObject lineSegmentVisualizerInstance = Instantiate(LineSegmentVisualizerPrefab, navigableTerrain.gameObject.transform);
            ILineSegmentVisualizer lineSegmentVisualizer = lineSegmentVisualizerInstance.GetComponent<ILineSegmentVisualizer>();

            navigableTerrain.EnableDebugView = true;
            navigableTerrain.LineSegmentVisualizer = lineSegmentVisualizer;
        }
    }
}

