﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using RealityToolkit.Input.Definitions;
using System;
using UnityEngine;

namespace RealityToolkit.Editor.Data
{
    /// <summary>
    /// Used to aid in layout of Controller Input Actions.
    /// </summary>
    [Serializable]
    public struct ControllerInputActionOption
    {
        public string Controller;
        public Handedness Handedness;
        public Vector2[] InputLabelPositions;
        public bool[] IsLabelFlipped;
    }
}