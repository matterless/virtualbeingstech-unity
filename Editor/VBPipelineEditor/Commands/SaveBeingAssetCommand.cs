// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.Assertions;

using VirtualBeings.Tech.BehaviorComposition;

namespace VirtualBeings
{
    /// <summary>
    /// Input struct for the command <see cref="SaveBeingAssetCommand"/>
    /// </summary>
    public struct SaveBeingAssetInputs
    {
        public ImportModelContext ImportContext;
        public CreateStatesContext CreateStatesContext;
    }

    /// <summary>
    /// Output struct for the command <see cref="SaveBeingAssetCommand"/>
    /// </summary>
    public struct SaveBeingAssetOutputs
    {
    }

    /// <summary>
    /// Command used to Apply the correct import settings to assets passed in the <see cref="ApplyImportSettingsInputs"/> input struct
    /// </summary>
    public class SaveBeingAssetCommand
    {
        public bool Execute(SaveBeingAssetInputs input, out SaveBeingAssetOutputs output)
        {
            return true;
        }
    }
}