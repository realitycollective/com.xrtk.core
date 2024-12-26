// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using System.Collections.Generic;
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
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/hold-onto-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(HoldOntoBehaviour))]
    public class HoldOntoBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("Optional: The pose to hold / grab onto when locking onto this interactable. Must be a child " +
        "transform of the component transform. If not set, the component transform is used.")]
        private Transform hint = null;

        [SerializeField, Tooltip("If set, the controller visualizer will smoothly attach to the interactable instead of instantly.")]
        private bool smoothSyncPose = true;

        [SerializeField, Tooltip("Duration in seconds to sync the visualizer pose with the interactable."), Min(.01f)]
        private float syncDuration = 1f;

        private struct VisualizerLockData
        {
            public bool IsLocked { get; set; }

            public bool IsPendingUnlock { get; set; }

            public Pose SmoothingStartPose { get; set; }

            public float SmoothingStartTime { get; set; }

            public float SmoothingProgress { get; set; }
        }

        private readonly Dictionary<IControllerVisualizer, VisualizerLockData> visualizers = new();

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            if (hint.IsNull())
            {
                hint = transform;
            }
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (visualizers.Count == 0)
            {
                return;
            }

            var lockPose = GetInteractableLockPose();

            foreach (var visualizer in visualizers)
            {
                if (!visualizers.TryGetValue(visualizer.Key, out var data))
                {
                    continue;
                }

                if (data.IsPendingUnlock)
                {
                    var t = CalculateSmoothingTransition(visualizer.Key);
                    if (t >= 1f)
                    {
                        CleanUpVisualizer(visualizer.Key);
                        continue;
                    }

                    data.SmoothingProgress = t;
                    visualizers[visualizer.Key] = data;

                    if (visualizer.Key.Controller.TryGetPose(Space.World, out var unlockPose))
                    {
                        unlockPose.position = Vector3.Slerp(data.SmoothingStartPose.position, unlockPose.position, data.SmoothingProgress);
                        unlockPose.rotation = Quaternion.Slerp(data.SmoothingStartPose.rotation, unlockPose.rotation, data.SmoothingProgress);

                        visualizer.Key.PoseDriver.SetPositionAndRotation(unlockPose.position, unlockPose.rotation);
                    }
                }
                else
                {
                    var t = CalculateSmoothingTransition(visualizer.Key);

                    data.SmoothingProgress = t;
                    visualizers[visualizer.Key] = data;

                    if (t < 1f)
                    {
                        lockPose.position = Vector3.Slerp(data.SmoothingStartPose.position, lockPose.position, data.SmoothingProgress);
                        lockPose.rotation = Quaternion.Slerp(data.SmoothingStartPose.rotation, lockPose.rotation, data.SmoothingProgress);
                    }

                    visualizer.Key.PoseDriver.SetPositionAndRotation(lockPose.position, lockPose.rotation);
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
            visualizers.EnsureDictionaryItem(visualizer, new VisualizerLockData
            {
                IsPendingUnlock = false,
                IsLocked = !smoothSyncPose,
                SmoothingStartPose = new Pose(visualizer.PoseDriver.position, visualizer.PoseDriver.rotation),
                SmoothingStartTime = Time.time,
                SmoothingProgress = 0f
            });

            visualizer.OverrideSourcePose = true;
        }

        private void UnlockVisualizer(IControllerVisualizer visualizer)
        {
            if (!smoothSyncPose)
            {
                CleanUpVisualizer(visualizer);
                return;
            }

            visualizers.EnsureDictionaryItem(visualizer, new VisualizerLockData
            {
                IsPendingUnlock = true,
                IsLocked = false,
                SmoothingStartPose = GetInteractableLockPose(),
                SmoothingStartTime = Time.time,
                SmoothingProgress = 0f
            }, true);
        }

        private void CleanUpVisualizer(IControllerVisualizer visualizer)
        {
            visualizers.SafeRemoveDictionaryItem(visualizer);
            visualizer.OverrideSourcePose = false;
        }

        private Pose GetInteractableLockPose() => new Pose(hint.position, hint.rotation);

        private float CalculateSmoothingTransition(IControllerVisualizer visualizer)
        {
            if (!visualizers.TryGetValue(visualizer, out var data) ||
            data.IsLocked || data.SmoothingProgress >= 1f)
            {
                return 1f;
            }

            var t = (Time.time - data.SmoothingStartTime) / syncDuration;
            return t;
        }
    }
}
