// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/auto-reset-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(AutoResetBehaviour))]
    public class AutoResetBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("The delay in seconds after interaction has stopped before the reset will fire.")]
        private float resetDelay = 5f;

        [SerializeField, Tooltip("If set, the interactable will be reset to its initial pose upon reset.")]
        private bool resetPose = true;

        private bool didReset;
        private float interactionStoppedTime;
        private Pose initialPose;

        /// <inheritdoc/>
        protected override void Awake()
        {
            initialPose = new Pose(transform.position, transform.rotation);
            base.Awake();
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            didReset = true;
            base.OnEnable();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            if (didReset)
            {
                return;
            }

            var timePassed = Time.time - interactionStoppedTime;
            if (timePassed > resetDelay)
            {
                didReset = true;
                Interactable.ResetInteractable();
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            didReset = true;
            base.OnDisable();
        }

        /// <inheritdoc/>
        protected override void OnFocusEntered(InteractionEventArgs eventArgs) => OnInteractionDetected();

        /// <inheritdoc/>
        protected override void OnLastFocusExited(InteractionExitEventArgs eventArgs)
        {
            if (Interactable.IsSelected || Interactable.IsGrabbed)
            {
                return;
            }

            OnInteractionEnded();
        }

        /// <inheritdoc/>
        protected override void OnSelectEntered(InteractionEventArgs eventArgs) => OnInteractionDetected();

        /// <inheritdoc/>
        protected override void OnLastSelectExited(InteractionExitEventArgs eventArgs)
        {
            if (Interactable.IsFocused || Interactable.IsGrabbed)
            {
                return;
            }

            OnInteractionEnded();
        }

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs) => OnInteractionDetected();

        /// <inheritdoc/>
        protected override void OnLastGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (Interactable.IsSelected || Interactable.IsFocused)
            {
                return;
            }

            OnInteractionEnded();
        }

        /// <inheritdoc/>
        protected override void OnResetBehaviour()
        {
            didReset = true;

            if (resetPose)
            {
                ResetPose();
            }
        }

        private void OnInteractionDetected()
        {
            didReset = true;
        }

        private void OnInteractionEnded()
        {
            interactionStoppedTime = Time.time;
            didReset = false;
        }

        private void ResetPose()
        {
            transform.SetPositionAndRotation(initialPose.position, initialPose.rotation);
        }
    }
}
