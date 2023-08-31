// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor.Animations;
using UnityEngine;

namespace VirtualBeings
{
    /// <summary>
    /// Class contraining some utility methods that are usefull for all sorts of VP Pipeline processing
    /// </summary>
    public static class VBPipelineUtils
    {
        /// <summary>
        /// Tag specific to the STs AnimatorState
        /// </summary>
        public const string ST_TAG = "TransitionBehaviour";

        /// <summary>
        /// Prefix related to the RSs naming convension
        /// </summary>
        private const string RS_ANIMATION_PREFIX = "Root_";

        /// <summary>
        /// Prefix related to the STs naming convension
        /// </summary>
        private const string ST_ANIMATION_PREFIX = "ST_";

        public static bool IsRS(AnimatorState state, out string withoutPrefix)
        {
            if (!state.name.StartsWith(RS_ANIMATION_PREFIX))
            {
                withoutPrefix = null;
                return false;
            }

            withoutPrefix = state.name.Substring(RS_ANIMATION_PREFIX.Length);
            return true;
        }

        public static bool IsST(AnimatorState state, out string fromRSName , out string STName)
        {
            if (!state.name.StartsWith(ST_ANIMATION_PREFIX))
            {
                fromRSName = null;
                STName = null;
                return false;
            }

            // state name would normally take the form of : ST_[FROM]_[ST_NAME];
            string[] split = state.name.Split('_');

            if(state.tag == ST_TAG)
            {
                Debug.Log($"Error checking ST named {state} , the {nameof(AnimatorState)} doesn't have the tag '{ST_TAG}'");
                fromRSName = null;
                STName = null;
                return false;
            }

            if(split.Length < 3)
            {
                Debug.Log($"Error while splitting the name of the ST {state} , could be a naming convension error");
                fromRSName = null;
                STName = null;
                return false;
            }

            fromRSName = split[1];
            STName = string.Join( "_", split , 2 , split.Length - 2 );

            return true;
        }
    }
}
