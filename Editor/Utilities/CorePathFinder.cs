﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Editor;
using UnityEngine;

namespace RealityToolkit.Editor.Utilities
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path to com.realitytoolkit.core.
    /// </summary>
    /// <inheritdoc cref="IPathFinder" />
    public class CorePathFinder : ScriptableObject, IPathFinder
    {
        /// <inheritdoc />
        public string Location => $"/Editor/Utilities/{nameof(CorePathFinder)}.cs";
    }
}