// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEditor.Animations;

namespace VirtualBeings
{
    /// <summary>
    /// Input struct for the command <see cref="FetchSTsCommand"/>
    /// </summary>
    public struct FetchSTsInputs
    {
        public AnimatorController AnimatorController;
        public RSEditorInfo FromRS;
        public List<STEditorInfo> Results;
    }
    /// <summary>
    /// Output struct for the command <see cref="FetchSTsCommand"/>
    /// </summary>
    public struct FetchSTsOutputs
    {
        public RSEditorInfo FromRS;
        public List<STEditorInfo> Results;
    }

    /// <summary>
    /// Command used to extract the STs linked to the RS <see cref="FetchSTsInputs.FromRS"/> passed in input struct 
    /// </summary>
    public class FetchSTsCommand
    {
        public bool Execute(FetchSTsInputs input, out FetchSTsOutputs output)
        {
            foreach (AnimatorControllerLayer layer in input.AnimatorController.layers)
            {
                foreach (ChildAnimatorState s in layer.stateMachine.states)
                {
                    if (!VBPipelineUtils.IsST(s.state, out string fromRSName, out string STName))
                        continue;

                    if (fromRSName != input.FromRS.RSName)
                        continue;

                    STEditorInfo info = new STEditorInfo();
                    info.From = input.FromRS;
                    info.STName = STName;
                    info.StateName = s.state.name;
                    info.Path = $"{layer.name}/{s.state.name}";

                    input.Results.Add(info);
                }
            }
            output = new FetchSTsOutputs();
            output.FromRS = input.FromRS;
            output.Results = input.Results;

            return true;
        }
    }
}