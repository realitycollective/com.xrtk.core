﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using RealityToolkit.Input.Hands.Poses;
using RealityToolkit.Input.Hands.Visualizers;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// The <see cref="GrabHandPoseBehaviour"/> will animate the <see cref="RiggedHandControllerVisualizer"/>
    /// into the assigned <see cref="grabPose"/>, when the <see cref="Interactables.IInteractable"/> is grabbed.
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/grab-hand-pose-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(GrabHandPoseBehaviour))]
    public class GrabHandPoseBehaviour : BaseInteractionBehaviour, IProvideHandPose
    {
        [SerializeField, Tooltip("Hand pose applied when grabbing the interactable.")]
        private HandPose grabPose = null;

        /// <inheritdoc/>
        public HandPose FocusPose { get; } = null;

        /// <inheritdoc/>
        public HandPose SelectPose { get; } = null;

        /// <inheritdoc/>
        public HandPose GrabPose => grabPose;

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = grabPose;
            }
        }

        /// <inheritdoc/>
        protected override void OnGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = null;
            }
        }
    }
}
