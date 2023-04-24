﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using System;
using System.Collections.Generic;

namespace RealityToolkit.Editor.Data
{
    /// <summary>
    /// Used to aid in layout of Controller Input Actions.
    /// </summary>
    [Serializable]
    public struct ControllerInputActionOptions
    {
        public List<ControllerInputActionOption> Controllers;
    }
}