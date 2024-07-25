// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using RealityToolkit.Input.Hands.Poses;
using RealityToolkit.Input.Hands.Visualizers;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// The <see cref="FocusHandPoseBehaviour"/> will animate the <see cref="RiggedHandControllerVisualizer"/>
    /// into the assigned <see cref="focusPose"/>, when the <see cref="Interactables.IInteractable"/> is focused.
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/focus-hand-pose-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(FocusHandPoseBehaviour))]
    public class FocusHandPoseBehaviour : BaseInteractionBehaviour, IProvideHandPose
    {
        [SerializeField, Tooltip("Hand pose applied when focusing the interactable.")]
        private HandPose focusPose = null;

        /// <inheritdoc/>
        public HandPose FocusPose => focusPose;

        /// <inheritdoc/>
        public HandPose SelectPose { get; } = null;

        /// <inheritdoc/>
        public HandPose GrabPose { get; } = null;

        /// <inheritdoc/>
        protected override void OnFocusEntered(InteractionEventArgs eventArgs)
        {
            if (Interactable.IsSelected || Interactable.IsGrabbed)
            {
                return;
            }

            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
               directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = focusPose;
            }
        }

        /// <inheritdoc/>
        protected override void OnFocusExited(InteractionExitEventArgs eventArgs)
        {
            if (Interactable.IsSelected || Interactable.IsGrabbed)
            {
                return;
            }

            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = null;
            }
        }
    }
}
