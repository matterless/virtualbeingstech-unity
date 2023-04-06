// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using VirtualBeings.Tech.Utils;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
    [DisallowMultipleComponent]
    public class LineSegmentVisualizer : MonoBehaviour, ILineSegmentVisualizer
    {
        [SerializeField]
        private GameObject[] _lineRendererPrefabs;

        private ObjectPoolWithFactory<GameObject>[] _lineSegmentFactories;
        private List<GameObject>[] _activeLineSegments;

        public void AddSegment(Vector3 p0, Vector3 p1, int materialIndex)
        {
            if (materialIndex < 0 || materialIndex >= _lineSegmentFactories.Length)
                throw new ArgumentOutOfRangeException();

            GameObject lineSegment = _lineSegmentFactories[materialIndex].New();
            LineRenderer lineRenderer = lineSegment.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, p0);
            lineRenderer.SetPosition(1, p1);

            _activeLineSegments[materialIndex].Add(lineSegment);
        }

        public void Clear()
        {
            for (int i = 0; i < _activeLineSegments.Length; i++)
            {
                foreach (GameObject go in _activeLineSegments[i])
                    _lineSegmentFactories[i].Store(go);

                _activeLineSegments[i].Clear();
            }
        }

        private void Start()
        {
            _lineSegmentFactories = new ObjectPoolWithFactory<GameObject>[_lineRendererPrefabs.Length];
            _activeLineSegments = new List<GameObject>[_lineRendererPrefabs.Length];

            for (int i = 0; i < _lineSegmentFactories.Length; i++)
            {
                _lineSegmentFactories[i] = new ObjectPoolWithFactory<GameObject>(4,
                    i switch // factory method
                    {
                        0 => () => Instantiate(_lineRendererPrefabs[0], transform),
                        1 => () => Instantiate(_lineRendererPrefabs[1], transform),
                        2 => () => Instantiate(_lineRendererPrefabs[2], transform),
                        3 => () => Instantiate(_lineRendererPrefabs[3], transform),
                        4 => () => Instantiate(_lineRendererPrefabs[4], transform),
                        5 => () => Instantiate(_lineRendererPrefabs[5], transform),
                        6 => () => Instantiate(_lineRendererPrefabs[6], transform),
                        7 => () => Instantiate(_lineRendererPrefabs[7], transform),
                        8 => () => Instantiate(_lineRendererPrefabs[8], transform),
                        _ => () => Instantiate(_lineRendererPrefabs[9], transform),
                    },
                    (go) => // store action
                    {
                        go.SetActive(false);
                    },
                    (go) => // restore action
                    {
                        go.SetActive(true);
                    });

                _activeLineSegments[i] = new List<GameObject>(4);
            }
        }
    }
}
