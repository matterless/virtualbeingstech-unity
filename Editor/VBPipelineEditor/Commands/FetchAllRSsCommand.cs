// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEditor.Animations;

namespace VirtualBeings
{

    /// <summary>
    /// Input struct for the command <see cref="FetchAllRSsCommand"/>
    /// </summary>
    public struct FetchAllRSsInputs
    {
        public AnimatorController AnimatorController;
        public List<RSEditorInfo> Results;
    }


    /// <summary>
    /// Output struct for the command <see cref="FetchAllRSsCommand"/>
    /// </summary>
    public struct FetchAllRSsOutputs
    {
        public List<RSEditorInfo> Results;
    }

    /// <summary>
    /// Command used to extract all the RSs from the <see cref="AnimatorController"/> <see cref="FetchAllRSsInputs.AnimatorController"/> passed in the input struct 
    /// </summary>
    public class FetchAllRSsCommand
    {
        public bool Execute(FetchAllRSsInputs input, out FetchAllRSsOutputs output)
        {
            foreach (AnimatorControllerLayer layer in input.AnimatorController.layers)
            {
                foreach (ChildAnimatorState s in layer.stateMachine.states)
                {
                    if (!VBPipelineUtils.IsRS(s.state, out string withoutPrefix))
                        continue;

                    RSEditorInfo info = new RSEditorInfo();
                    info.RSName = withoutPrefix;
                    info.StateName = s.state.name;
                    info.Path = $"{layer.name}/{s.state.name}";

                    input.Results.Add(info);
                }
            }

            output.Results = input.Results;

            return true;
        }
    }
}