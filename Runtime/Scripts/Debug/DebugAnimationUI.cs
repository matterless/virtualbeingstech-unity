// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEngine;
using VirtualBeings.BehaviorComposition.Activities;
using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings.Tech.Shared
{ 
    public class DebugAnimationUI : MonoBehaviour
    {
        [HideInInspector]
        public Being being;

        [HideInInspector]
        public ActuatorViewer ActuatorViewer;

        private GUIStyle currentStyle = null;

        public Dictionary<IRS, List<IST>> RStoSTs = new();
        private Vector2 scrollPositionRS = new Vector2(0, 20);
        private Vector2 scrollPositionST = new Vector2(0, 20);
        private Vector2 scrollPositionField = new Vector2(0, 20);

        private float urgency01 = 0f;
        private float transitionSpeed01 = 0.5f;
        private float transitionIntensity01 = 1f;
        private float transitionLeftRight = 0f;
        private float transitionParameterA = 0f;
        private float transitionParameterB = 0f;
        private float crossFadeDurationOverride = 0f;

        private void Awake()
        {
            being = GetComponentInParent<Being>();
        }

        void OnGUI()
        {
            if(being == null || ActuatorViewer == null || ActuatorViewer.RStoSTs == null || ActuatorViewer.RStoSTs.Count <= 0)
            {
                return;
            }
            InitStyles();

            // Root States
            GUILayout.BeginArea(new Rect(0, 0, 150, 150), currentStyle);
            GUILayout.Label("Root States");
            scrollPositionRS = GUILayout.BeginScrollView(scrollPositionRS, GUILayout.Width(140), GUILayout.ExpandHeight(true));
            foreach(var key in ActuatorViewer.RStoSTs.Keys)
            {
                if(GUILayout.Button(key.Name))
                {
                    ActuatorViewer.RequestRS(key, 
                        urgency01, 
                        transitionSpeed01, 
                        transitionIntensity01, 
                        transitionLeftRight, 
                        transitionParameterA, 
                        transitionParameterB, 
                        crossFadeDurationOverride);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Root States
            GUILayout.BeginArea(new Rect(0, 200, 150, 150), currentStyle);
            GUILayout.Label("Self Transitions");
            scrollPositionST = GUILayout.BeginScrollView(scrollPositionST, GUILayout.Width(140), GUILayout.ExpandHeight(true));

            foreach (IST ist in ActuatorViewer.GetCurrentST())
            {
                if (GUILayout.Button(ist.Name))
                {
                    ActuatorViewer.RequestST(ist, 
                        transitionSpeed01, 
                        transitionIntensity01, 
                        transitionLeftRight, 
                        transitionParameterA, 
                        transitionParameterB, 
                        crossFadeDurationOverride);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();


            GUILayout.BeginArea(new Rect(Screen.width - 250, 0, 250, 350), currentStyle);
            //scrollPositionField = GUILayout.BeginScrollView(scrollPositionField, GUILayout.Width(140), GUILayout.ExpandHeight(true));
            //GUILayout.BeginHorizontal();
            GUILayout.Label("Urgency (only RS) " + urgency01.ToString("f2"));
            urgency01                   = GUILayout.HorizontalScrollbar(urgency01, 0f, 0f, 1f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Transition Speed " + transitionSpeed01.ToString("f2"));
            transitionSpeed01           = GUILayout.HorizontalScrollbar(transitionSpeed01, 0f, 0f, 1f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Intensity " + transitionIntensity01.ToString("f2"));
            transitionIntensity01       = GUILayout.HorizontalScrollbar(transitionIntensity01, 0f, 0f, 1f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Left Right " + transitionLeftRight.ToString("f2"));
            transitionLeftRight         = GUILayout.HorizontalScrollbar(transitionLeftRight, 0f, -2f, 2f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Param A " + transitionParameterA.ToString("f2"));
            transitionParameterA        = GUILayout.HorizontalScrollbar(transitionParameterA, 0f, -1f, 1f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Param B " + transitionParameterB.ToString("f2"));
            transitionParameterB        = GUILayout.HorizontalScrollbar(transitionParameterB, 0f, -1f, 1f);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Cross Fade " + crossFadeDurationOverride.ToString("f2"));
            crossFadeDurationOverride   = GUILayout.HorizontalScrollbar(crossFadeDurationOverride, 0f, 0f, 1f);
            //GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void InitStyles()
        {
            if (currentStyle == null)
            {
                currentStyle = new GUIStyle(GUI.skin.box);
                currentStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f));
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }

}
