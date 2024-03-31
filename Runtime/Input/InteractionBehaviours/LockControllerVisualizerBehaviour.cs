// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// Syncs the <see cref="IControllerInteractor"/>'s <see cref="Controllers.IControllerVisualizer"/>
    /// to the <see cref="Interactables.IInteractable"/> using <see cref="Controllers.IControllerVisualizer.OverrideSourcePose"/>.
    /// </summary>
    /// <remarks>
    /// Only supports <see cref="IControllerInteractor"/>s.
    /// Does not support <see cref="IPokeInteractor"/>s and will ignore them.
    /// </remarks>
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/lock-controller-visualizer-behaviour")]
    public class LockControllerVisualizerBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("Optional local offset from the object's pivot.")]
        private Vector3 poseLocalPositionOffset = Vector3.zero;

        [SerializeField, Tooltip("Optional local offset from the object's pivot.")]
        private Vector3 poseLocalRotationOffset = Vector3.zero;

        [SerializeField, Tooltip("If set, the controller visualizer will snap to the interactable instead of a smooth transition.")]
        private bool snapToInteractable = false;

        [SerializeField, Tooltip("Speed applied to smoothly move to the interactable position."), Min(1f)]
        private float syncPositionSpeed = 2f;

        [SerializeField, Tooltip("Speed applied to smoothly rotate to the interactable rotation."), Min(1f)]
        private float syncRotationSpeed = 360f;

        private readonly List<IControllerVisualizer> lockedVisualizers = new();
        private const float snapPoseEpsilon = .0001f;

        /// <inheritdoc/>
        protected override void Update()
        {
            var syncPose = GetSyncPose();

            for (int i = 0; i < lockedVisualizers.Count; i++)
            {
                var visualizer = lockedVisualizers[i];
                var shouldSnap = snapToInteractable || HasReachedSnapPose(visualizer, syncPose);

                if (!shouldSnap)
                {
                    syncPose.position = Vector3.MoveTowards(visualizer.PoseDriver.position, syncPose.position, syncPositionSpeed * Time.deltaTime);
                    syncPose.rotation = Quaternion.RotateTowards(visualizer.PoseDriver.rotation, syncPose.rotation, syncRotationSpeed * Time.deltaTime);
                }

                visualizer.Controller.Visualizer.PoseDriver.SetPositionAndRotation(syncPose.position, syncPose.rotation);
            }
        }

        /// <inheritdoc/>
        protected override void OnSelectEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor ||
                eventArgs.Interactor is IPokeInteractor)
            {
                return;
            }

            LockVisualizer(controllerInteractor.Controller.Visualizer);
        }

        /// <inheritdoc/>
        protected override void OnSelectExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor ||
                eventArgs.Interactor is IPokeInteractor)
            {
                return;
            }

            UnlockVisualizer(controllerInteractor.Controller.Visualizer);
        }

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor ||
                eventArgs.Interactor is IPokeInteractor)
            {
                return;
            }

            LockVisualizer(controllerInteractor.Controller.Visualizer);
        }

        /// <inheritdoc/>
        protected override void OnGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor ||
                eventArgs.Interactor is IPokeInteractor)
            {
                return;
            }

            UnlockVisualizer(controllerInteractor.Controller.Visualizer);
        }

        private void LockVisualizer(IControllerVisualizer visualizer)
        {
            lockedVisualizers.EnsureListItem(visualizer);
            visualizer.OverrideSourcePose = true;
        }

        private void UnlockVisualizer(IControllerVisualizer visualizer)
        {
            lockedVisualizers.SafeRemoveListItem(visualizer);
            visualizer.OverrideSourcePose = false;
        }

        private Pose GetSyncPose() => new Pose(transform.TransformPoint(poseLocalPositionOffset), transform.rotation * Quaternion.Euler(poseLocalRotationOffset));

        private bool HasReachedSnapPose(IControllerVisualizer visualizer, Pose snapPose)
        {
            var currentPose = new Pose(
                visualizer.PoseDriver.position,
                visualizer.PoseDriver.rotation);

            return Vector3.SqrMagnitude(snapPose.position - currentPose.position) <= snapPoseEpsilon &&
                Quaternion.Angle(snapPose.rotation, currentPose.rotation) <= snapPoseEpsilon;
        }
    }
}
