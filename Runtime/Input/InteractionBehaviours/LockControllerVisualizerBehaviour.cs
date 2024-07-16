// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
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

        [SerializeField, Tooltip("If set, the controller visualizer will smoothly attach to the interactable instead of instantly.")]
        private bool smoothSyncPose = true;

        [SerializeField, Tooltip("Duration in seconds to sync the visualizer pose with the interactable."), Min(.01f)]
        private float syncDuration = 1f;

        private readonly Dictionary<IControllerVisualizer, bool> lockedVisualizers = new();
        private readonly Dictionary<IControllerVisualizer, bool> pendingUnlockVisualizers = new();
        private readonly Dictionary<IControllerVisualizer, Pose> smoothingStartPose = new();
        private readonly Dictionary<IControllerVisualizer, float> smoothingStartTime = new();
        private readonly Dictionary<IControllerVisualizer, float> smoothingProgress = new();

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
                if (pendingUnlockVisualizers.TryGetValue(visualizer, out _))
                {
                    var finishedUnlock = HasFinishedSmoothTransition(pendingUnlockVisualizers, visualizer);
                    if (finishedUnlock)
                    {
                        CleanUpVisualizer(visualizer);
                        continue;
                    }

                    var unlockPose = visualizer.SourcePose;
                    unlockPose.position = Vector3.Slerp(smoothingStartPose[visualizer].position, unlockPose.position, smoothingProgress[visualizer]);
                    unlockPose.rotation = Quaternion.Slerp(smoothingStartPose[visualizer].rotation, unlockPose.rotation, smoothingProgress[visualizer]);
                    visualizer.PoseDriver.SetPositionAndRotation(unlockPose.position, unlockPose.rotation);
                }
                else
                {
                    var shouldLock = HasFinishedSmoothTransition(lockedVisualizers, visualizer);

                    if (!shouldLock)
                    {
                        lockPose.position = Vector3.Slerp(smoothingStartPose[visualizer].position, lockPose.position, smoothingProgress[visualizer]);
                        lockPose.rotation = Quaternion.Slerp(smoothingStartPose[visualizer].rotation, lockPose.rotation, smoothingProgress[visualizer]);
                    }

                    visualizer.PoseDriver.SetPositionAndRotation(lockPose.position, lockPose.rotation);
                }
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
            lockedVisualizers.EnsureDictionaryItem(visualizer, !smoothSyncPose, true);
            smoothingStartPose.EnsureDictionaryItem(visualizer, new Pose(visualizer.PoseDriver.position, visualizer.PoseDriver.rotation), true);
            smoothingStartTime.EnsureDictionaryItem(visualizer, Time.time, true);
            smoothingProgress.EnsureDictionaryItem(visualizer, 0f, true);
            visualizer.OverrideSourcePose = true;
        }

        private void UnlockVisualizer(IControllerVisualizer visualizer)
        {
            if (!smoothSyncPose)
            {
                CleanUpVisualizer(visualizer);
                return;
            }

            pendingUnlockVisualizers.EnsureDictionaryItem(visualizer, false, true);
            smoothingStartPose.EnsureDictionaryItem(visualizer, GetLockPose(), true);
            smoothingStartTime.EnsureDictionaryItem(visualizer, Time.time, true);
            smoothingProgress.EnsureDictionaryItem(visualizer, 0f, true);
        }

        private void CleanUpVisualizer(IControllerVisualizer visualizer)
        {
            lockedVisualizers.SafeRemoveDictionaryItem(visualizer);
            smoothingStartPose.SafeRemoveDictionaryItem(visualizer);
            smoothingStartTime.SafeRemoveDictionaryItem(visualizer);
            smoothingProgress.SafeRemoveDictionaryItem(visualizer);
            pendingUnlockVisualizers.SafeRemoveDictionaryItem(visualizer);
            visualizer.OverrideSourcePose = false;
        }

        private Pose GetLockPose() => new Pose(transform.TransformPoint(localOffsetPose.position), transform.rotation * Quaternion.Euler(localOffsetPose.rotation.eulerAngles));

        private bool HasFinishedSmoothTransition(Dictionary<IControllerVisualizer, bool> smoothingStateDictionary, IControllerVisualizer visualizer)
        {
            if (smoothingStateDictionary[visualizer])
            {
                return true;
            }

            var t = (Time.time - smoothingStartTime[visualizer]) / syncDuration;
            smoothingProgress[visualizer] = t;

            if (t < 1f)
            {
                return false;
            }

            smoothingStateDictionary[visualizer] = true;
            return true;
        }
    }
}
