﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.Input.Interfaces.Modules;
using UnityEngine;

namespace RealityToolkit.Input.Controllers
{
    /// <summary>
    /// A <see cref="IController"/> is an input device used to raise <see cref="Definitions.InputAction"/>s.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// The name of the <see cref="IController"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is the <see cref="IController"/> enabled?
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// The <see cref="IControllerServiceModule"/> this <see cref="IController"/> belongs to.
        /// </summary>
        IControllerServiceModule ServiceModule { get; }

        /// <summary>
        /// The designated hand that the <see cref="IController"/> is managing.
        /// </summary>
        Handedness ControllerHandedness { get; }

        /// <summary>
        /// The registered <see cref="IInputSource"/> for this <see cref="IController"/>.
        /// </summary>
        IInputSource InputSource { get; }

        /// <summary>
        /// The <see cref="IController"/>'s <see cref="IControllerVisualizer"/> in the scene.
        /// </summary>
        IControllerVisualizer Visualizer { get; }

        /// <summary>
        /// <see cref="InteractionMapping"/>s for this <see cref="IController"/>, linking the physical inputs to logical <see cref="Definitions.InputAction"/>s.
        /// </summary>
        InteractionMapping[] Interactions { get; }

        /// <summary>
        /// Gets the current position and rotation for the <see cref="IController"/>, if available.
        /// </summary>
        Pose Pose { get; }

        /// <summary>
        /// Outputs the current <see cref="RealityToolkit.Definitions.Devices.TrackingState"/> of the <see cref="IController"/>.
        /// </summary>
        TrackingState TrackingState { get; }

        /// <summary>
        /// Indicates that this <see cref="IController"/> is currently providing position data.
        /// </summary>
        /// <remarks>
        /// This value may change during usage for some controllers. As a best practice,
        /// be sure to check this value before using position data.
        /// </remarks>
        bool IsPositionAvailable { get; }

        /// <summary>
        /// Indicates the accuracy of the position data being reported.
        /// </summary>
        bool IsPositionApproximate { get; }

        /// <summary>
        /// Indicates that this <see cref="IController"/> is currently providing rotation data.
        /// </summary>
        /// <remarks>
        /// This value may change during usage for some controllers. As a best practice,
        /// be sure to check this value before using rotation data.
        /// </remarks>
        bool IsRotationAvailable { get; }

        /// <summary>
        /// Gets how fast the <see cref="IController"/> rotates or revolves relative to its pivot point on each axis.
        /// </summary>
        Vector3 AngularVelocity { get; }

        /// <summary>
        /// Gets the <see cref="IController"/>'s current movement speed as a normalized <see cref="Vector3"/>.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// The <see cref="IController"/>'s motion direction is a normalized <see cref="Vector3"/>
        /// describing in which direction is moving compared to a previous frame.
        /// </summary>
        Vector3 MotionDirection { get; }

        /// Attempts to load the <see cref="IController"/> model specified in the <see cref="RealityToolkit.Definitions.Controllers.ControllerProfile"/>
        /// to render the controllers in the scene.
        /// </summary>
        /// <returns>True, if controller model is being properly rendered.</returns>
        void TryRenderControllerModel();

        /// <summary>
        /// Updates the <see cref="IController"/>'s state.
        /// </summary>
        void UpdateController();

        /// <summary>
        /// Attempts to retrieve the <see cref="IController"/>'s pose in the scene in either the local space,
        /// that is in the <see cref="IInputRig.RigTransform"/> space or in world space.
        /// </summary>
        /// <param name="space">The space to get the <see cref="UnityEngine.Pose"/> in.</param>
        /// <param name="pose">The pose.</param>
        /// <returns><c>true</c>, if found Will return <c>false</c>, if not <see cref="IsPositionAvailable"/> or <see cref="IsRotationAvailable"/>.</returns>
        bool TryGetPose(Space space, out Pose pose);
    }
}