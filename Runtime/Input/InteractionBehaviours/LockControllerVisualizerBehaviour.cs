// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// Syncs the <see cref="IControllerInteractor"/>'s <see cref="IControllerVisualizer"/>
    /// to the <see cref="Interactables.IInteractable"/> using <see cref="IControllerVisualizer.OverrideSourcePose"/>.
    /// </summary>
    /// <remarks>
    /// Only supports <see cref="IControllerInteractor"/>s.
    /// Does not support <see cref="IPokeInteractor"/>s and will ignore them.
    /// </remarks>
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/lock-controller-visualizer-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(LockControllerVisualizerBehaviour))]
    public class LockControllerVisualizerBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("Optional offset pose applied to the visualizer.")]
        private Pose localOffsetPose = Pose.identity;

        [SerializeField, Tooltip("If set, the controller visualizer will snap to the interactable instead of a smooth transition.")]
        private bool snapToLockPose = false;

        [SerializeField, Tooltip("Speed applied to smoothly move to the interactable position."), Min(1f)]
        private float syncPositionSpeed = 2f;

        [SerializeField, Tooltip("Speed applied to smoothly rotate to the interactable rotation."), Min(1f)]
        private float syncRotationSpeed = 360f;

        private readonly Dictionary<IControllerVisualizer, bool> lockedVisualizers = new();
        private const float lockPositionTolerance = .001f;
        private const float lockRotationTolerance = 0.1f;

        /// <inheritdoc/>
        protected override void Update()
        {
            if (lockedVisualizers.Count == 0)
            {
                return;
            }

            var lockPose = GetLockPose();
            var visualizers = lockedVisualizers.Keys.ToList();

            foreach (var visualizer in visualizers)
            {
                var shouldLock = HasFinishedSmoothTransition(visualizer, lockPose);

                if (!shouldLock)
                {
                    lockPose.position = Vector3.MoveTowards(visualizer.PoseDriver.position, lockPose.position, syncPositionSpeed * Time.deltaTime);
                    lockPose.rotation = Quaternion.RotateTowards(visualizer.PoseDriver.rotation, lockPose.rotation, syncRotationSpeed * Time.deltaTime);
                }

                visualizer.PoseDriver.SetPositionAndRotation(lockPose.position, lockPose.rotation);
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
            lockedVisualizers.EnsureDictionaryItem(visualizer, snapToLockPose, true);
            visualizer.OverrideSourcePose = true;
        }

        private void UnlockVisualizer(IControllerVisualizer visualizer)
        {
            lockedVisualizers.SafeRemoveDictionaryItem(visualizer);
            visualizer.OverrideSourcePose = false;
        }

        private Pose GetLockPose() => new Pose(transform.TransformPoint(localOffsetPose.position), transform.rotation * Quaternion.Euler(localOffsetPose.rotation.eulerAngles));

        private bool HasFinishedSmoothTransition(IControllerVisualizer visualizer, Pose snapPose)
        {
            if (lockedVisualizers[visualizer])
            {
                return true;
            }

            if (Vector3.Distance(snapPose.position, visualizer.PoseDriver.position) > lockPositionTolerance)
            {
                return false;
            }

            var rotationDifference = Quaternion.FromToRotation(visualizer.PoseDriver.forward, snapPose.forward);
            var angleDifference = Quaternion.Angle(visualizer.PoseDriver.rotation, snapPose.rotation * rotationDifference);
            if (angleDifference > lockRotationTolerance)
            {
                return false;
            }

            lockedVisualizers[visualizer] = true;
            return true;
        }
    }
}
