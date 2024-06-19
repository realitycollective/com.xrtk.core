// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using UnityEngine;
using UnityEngine.Serialization;

namespace RealityToolkit.Input.Interactors
{
    /// <summary>
    /// This configuration profile is for global <see cref="IInteractor"/>s configuration
    /// within the <see cref="Interfaces.IInputService"/>.
    /// </summary>
    public class InteractorsProfile : BaseProfile
    {
        [SerializeField]
        [Tooltip("The Physics Layers, in prioritized order, that are used to determine the pointers target when raycasting.")]
        [FormerlySerializedAs("pointingRaycastLayerMasks")]
        private LayerMask[] pointerRaycastLayerMasks = { UnityEngine.Physics.DefaultRaycastLayers };

        /// <summary>
        /// The Physics Layers, in prioritized order, that are used to determine the <see cref="IPointerResult.CurrentPointerTarget"/> when raycasting.
        /// </summary>
        public LayerMask[] PointerRaycastLayerMasks => pointerRaycastLayerMasks;

        [SerializeField]
        private bool drawDebugPointingRays = false;

        /// <summary>
        /// Toggle to enable or disable debug pointing rays.
        /// </summary>
        public bool DrawDebugPointingRays => drawDebugPointingRays;

        [SerializeField]
        private Color[] debugPointingRayColors = { Color.green };

        /// <summary>
        /// The colors to use when debugging pointer rays.
        /// </summary>
        public Color[] DebugPointingRayColors => debugPointingRayColors;

        /// <summary>
        /// Default <see cref="IControllerInteractor"/>s attached to any <see cref="Controllers.IController"/> detected.
        /// </summary>
        [field: SerializeField, Tooltip("Default controller interactors attached to any controller detected.")]
        public BaseControllerInteractor[] DefaultControllerInteractors { get; private set; }
    }
}