// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityToolkit.Input.Interfaces
{
    /// <summary>
    /// The <see cref="IInputRig"/> defines the coordinate space in which <see cref="IInputService"/> objects, such as
    /// <see cref="IInputSource"/>s live. It is used to perform pose transformations for controllers, visualizers and interactors.
    /// </summary>
    public interface IInputRig
    {
        /// <summary>
        /// The root rig <see cref="Transform"/>.
        /// </summary>
        Transform RigTransform { get; }

        /// <summary>
        /// The <see cref="Transform"/> where the <see cref="Camera"/> component is located.
        /// </summary>
        Transform CameraTransform { get; }

        /// <summary>
        /// The rig's <see cref="Camera"/> reference.
        /// </summary>
        Camera RigCamera { get; }
    }
}
