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
        /// <summary>
        /// Internal structure keeping track of locked visualizers and their state.
        /// </summary>
        private class VisualizerLockData
        {
            public bool IsLocked { get; set; }

            public bool IsPendingUnlock { get; set; }

            public float SmoothingStartTime { get; set; }

            public float SmoothingProgress { get; set; }

            public float SmoothingDuration { get; set; }
        }

        [SerializeField, Tooltip("Optional: The pose to hold / grab onto when locking onto this interactable. Must be a child " +
        "transform of the component transform. If not set, the component transform is used.")]
        private Transform hint = null;

        [SerializeField, Tooltip("If set, the controller visualizer will smoothly lock to the interactable instead of instantly.")]
        private bool smooth = true;

        [SerializeField, Min(.01f), Tooltip("Duration in seconds to sync the visualizer pose with the interactable. The value is in seconds per unit and will " +
            "interpolate based on distance. So if the visualizer is 5 units away, the total sync duration is five times the configured duration.")]
        private float smoothingDuration = 1f;

        private readonly Dictionary<IControllerVisualizer, VisualizerLockData> visualizers = new();
        private readonly List<IControllerVisualizer> visualizersForClearing = new();

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            if (hint.IsNull())
            {
                hint = transform;
            }
            else if (!hint.IsChildOf(transform))
            {
                Debug.LogError($"{hint.name} must be a child transform of {transform.name}.", this);
            }

            if (smooth && smoothingDuration < 0f)
            {
                Debug.LogError("Sync duration must be non-negative and above 0.", this);
            }
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (visualizers.Count == 0)
            {
                return;
            }

            foreach (var item in visualizers)
            {
                var visualizer = item.Key;
                var data = item.Value;

                var t = CalculateSmoothingTransition(visualizer, data);
                if (data.IsPendingUnlock && t >= 1f)
                {
                    visualizersForClearing.Add(visualizer);
                    continue;
                }

                var lockPose = GetInteractableLockPose(visualizer);
                var targetPose = lockPose;

                if (data.IsPendingUnlock)
                {
                    // Smoothly return to actual controller pose.
                    var startPose = GetInteractableLockPose(visualizer);
                    var unlockPose = GetVisualizerUnlockPose(visualizer);

                    targetPose.position = Vector3.Slerp(startPose.position, unlockPose.position, data.SmoothingProgress);
                    targetPose.rotation = Quaternion.Slerp(startPose.rotation, unlockPose.rotation, data.SmoothingProgress);
                }
                else if (!data.IsPendingUnlock && t < 1f)
                {
                    // Smoothly lock onto the interactable.
                    var startPose = GetVisualizerUnlockPose(visualizer);
                    targetPose.position = Vector3.Slerp(startPose.position, lockPose.position, data.SmoothingProgress);
                    targetPose.rotation = Quaternion.Slerp(startPose.rotation, lockPose.rotation, data.SmoothingProgress);
                }

                // The position we store is in local space while the rotation is a world rotation.
                visualizer.PoseDriver.localPosition = targetPose.position;
                visualizer.PoseDriver.rotation = targetPose.rotation;
            }

            for (var i = 0; i < visualizersForClearing.Count; i++)
            {
                CleanUpVisualizer(visualizersForClearing[i]);
            }

            visualizersForClearing.Clear();
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
                IsLocked = !smooth,
                SmoothingStartTime = Time.time,
                SmoothingProgress = 0f,
                SmoothingDuration = smooth ? CalculateSmoothingDuration(visualizer) : 0f
            });

            visualizer.OverrideSourcePose = true;
        }

        private void UnlockVisualizer(IControllerVisualizer visualizer)
        {
            if (!smooth)
            {
                CleanUpVisualizer(visualizer);
                return;
            }

            visualizers.EnsureDictionaryItem(visualizer, new VisualizerLockData
            {
                IsPendingUnlock = true,
                IsLocked = false,
                SmoothingStartTime = Time.time,
                SmoothingProgress = 0f,
                SmoothingDuration = CalculateSmoothingDuration(visualizer)
            }, true);
        }

        private void CleanUpVisualizer(IControllerVisualizer visualizer)
        {
            visualizers.SafeRemoveDictionaryItem(visualizer);
            visualizer.OverrideSourcePose = false;
        }

        private Pose GetInteractableLockPose(IControllerVisualizer visualizer)
        {
            // If the visualizer is not parented, that means we'll be positioning it in world space.
            // So we can use the hint transform world pose directly.
            if (visualizer.PoseDriver.parent.IsNull())
            {
                return new Pose(hint.position, hint.rotation);
            }

            // If the visualzer is parented, we'll be positioning it within the local space of its parent,
            // so we must first figure out where the hint transform is located within that space.
            return new Pose(visualizer.PoseDriver.parent.InverseTransformPoint(hint.position), hint.rotation);
        }

        private Pose GetVisualizerUnlockPose(IControllerVisualizer visualizer)
        {
            visualizer.Controller.TryGetPose(Space.World, out var controllerPose);

            // If the visualizer is not parented, we can use the controller's actual world space
            // pose to determine where it currently is, since we'll be positioning in world space.
            if (visualizer.PoseDriver.parent.IsNull())
            {
                return controllerPose;
            }

            // If the visualizer is parented, we'll be positioning it within the local space of its parent,
            // so we need to know where the actual controller currently is within that space.
            return new Pose(visualizer.PoseDriver.parent.InverseTransformPoint(controllerPose.position), controllerPose.rotation);
        }

        private float CalculateSmoothingDuration(IControllerVisualizer visualizer)
        {
            var start = hint.position;
            var end = visualizer.Controller.TryGetPose(Space.World, out var controllerPose) ? controllerPose.position : visualizer.PoseDriver.position;
            var distance = Vector3.Distance(start, end);

            return distance * smoothingDuration;
        }

        private float CalculateSmoothingTransition(IControllerVisualizer visualizer, VisualizerLockData data)
        {
            if (!smooth || data.IsLocked || data.SmoothingProgress >= 1f)
            {
                return 1f;
            }

            var t = (Time.time - data.SmoothingStartTime) / data.SmoothingDuration;
            data.SmoothingProgress = t;

            return t;
        }
    }
}
