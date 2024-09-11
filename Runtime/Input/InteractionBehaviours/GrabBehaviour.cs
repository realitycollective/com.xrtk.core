// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// The <see cref="GrabBehaviour"/> is an <see cref="IInteractionBehaviour"/> for use with
    /// <see cref="IDirectInteractor"/>s. It allows to "pick up" the <see cref="Interactables.IInteractable"/>
    /// and carry it around.
    /// </summary>
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/grab-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(GrabBehaviour))]
    public class GrabBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("Optional local offset from the object's pivot.")]
        private Vector3 grabPoseLocalOffset = Vector3.zero;

        [SerializeField, Tooltip("Optional local offset from the object's pivot.")]
        private Vector3 grabPoseLocalRotationOffset = Vector3.zero;

        private static GrabBehaviour primary;
        private static IDirectInteractor primaryInteractor;

        /// <inheritdoc/>
        protected override void Update()
        {
            if (primary == this && primaryInteractor != null)
            {
                transform.SetPositionAndRotation(GetGrabPosition(transform, grabPoseLocalOffset), GetGrabRotation(grabPoseLocalRotationOffset));
            }
        }

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor)
            {
                primary = this;
                primaryInteractor = directInteractor;
            }
        }

        /// <inheritdoc/>
        protected override void OnGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                primaryInteractor == directInteractor)
            {
                primary = null;
                primaryInteractor = null;
            }
        }

        private static Vector3 GetGrabPosition(Transform interactable, Vector3 grabPoseLocalOffset) => primaryInteractor.Controller.Visualizer.GripPose.position + interactable.TransformDirection(grabPoseLocalOffset);

        private static Quaternion GetGrabRotation(Vector3 grabPoseLocalRotationOffset) => primaryInteractor.Controller.Visualizer.GripPose.rotation * Quaternion.Euler(grabPoseLocalRotationOffset);
    }
}
