// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// Use this <see cref="IInteractionBehaviour"/> to make an <see cref="Interactables.IInteractable"/> behave like
    /// a steering wheel.
    /// </summary>
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/steering-wheel-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(SteeringWheelBehaviour))]
    public class SteeringWheelBehaviour : BaseInteractionBehaviour
    {
        [SerializeField]
        [Tooltip("Controls the threshold angle for steering when at maximum steering in either direction.")]
        [Range(1f, 45)]
        private float steeringAngleLimit = 45f;

        [SerializeField, Tooltip("If set, the steering resets to neutral on release.")]
        private bool resetsToNeutral = true;

        [SerializeField, Tooltip("If set, the steering will smoothly reset instead of instantly.")]
        private bool smoothReset = true;

        [SerializeField, Tooltip("Time in seconds to return to neutral position."), Min(.01f)]
        private float smoothResetDuration = .5f;

        [SerializeField, Tooltip("Consistent up transform for the steering wheel.")]
        private Transform upTransform = null;

        private float minAngle;
        private float maxAngle;
        private bool resetting;
        private float elapsedResetTime;
        private Quaternion resetStartRotation;
        private Quaternion neutralSteeringRotation = Quaternion.Euler(0f, 0f, 0f);
        private readonly List<IControllerInteractor> interactors = new();
        private float currentAngle;

        /// <summary>
        /// Gets or sets current steering as an absolute angle relative to the neutral
        /// steering position. E.g. a value of 90 degrees means the steering wheel is rotated
        /// by 90 degrees.
        /// </summary>
        public float CurrentSteeringAngle
        {
            get => transform.localEulerAngles.z;
            set
            {
                var clamped = Mathf.Clamp(WrapAngle(value), minAngle, maxAngle);
                if (Mathf.Approximately(clamped, transform.localEulerAngles.z))
                {
                    return;
                }

                var rotation = transform.localEulerAngles;
                rotation.z = clamped;
                transform.localEulerAngles = rotation;
                SteeringChanged?.Invoke(CurrentSteeringNormalized);
            }
        }

        /// <summary>
        /// Gets or sets current steering as a normalized value.
        /// Value is in range [-1, 1] inclusive, where -1 is full steering to the left and 1 to the right.
        /// </summary>
        /// <remarks>Set value will be clamped to [-1, 1] inclusive.</remarks>
        public float CurrentSteeringNormalized
        {
            get => -(Mathf.Clamp(WrapAngle(transform.localEulerAngles.z), minAngle, maxAngle) / steeringAngleLimit);
            set
            {
                // We have to invert the value when being set because we are internally
                // working with an inverted range, where -1 is full left.
                value = -value;

                var previousAngle = WrapAngle(transform.localEulerAngles.z);
                var newAngle = Mathf.Clamp(value, -1f, 1f) * steeringAngleLimit;
                var delta = Mathf.Clamp(newAngle - previousAngle, -steeringAngleLimit, steeringAngleLimit);

                CurrentSteeringAngle += delta;
            }
        }

        /// <summary>
        /// Event raised whenever <see cref="CurrentSteeringNormalized"/> has changed.
        /// </summary>
        public UnityEvent<float> SteeringChanged;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            minAngle = -steeringAngleLimit;
            maxAngle = steeringAngleLimit;
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (interactors.Count == 0 && !resetting)
            {
                return;
            }

            if (resetting)
            {
                elapsedResetTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedResetTime / smoothResetDuration);
                CurrentSteeringAngle = Quaternion.Slerp(resetStartRotation, neutralSteeringRotation, t).eulerAngles.z;

                if (t >= 1f)
                {
                    resetting = false;
                    CurrentSteeringAngle = neutralSteeringRotation.eulerAngles.z;
                }

                return;
            }

            var angle = FindWheelAngle();
            var angleDifference = currentAngle - angle;
            Rotate(-angleDifference);
            currentAngle = angle;
        }

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor)
            {
                return;
            }

            interactors.EnsureListItem(controllerInteractor);
            currentAngle = FindWheelAngle();
        }

        /// <inheritdoc/>
        protected override void OnGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor)
            {
                return;
            }

            interactors.SafeRemoveListItem(controllerInteractor);
            currentAngle = FindWheelAngle();

            if (resetsToNeutral)
            {
                ReturnToNeutral();
            }
        }

        private float FindWheelAngle()
        {
            var totalAngle = 0f;

            foreach (var interactor in interactors)
            {
                var direction = FindLocalPoint(interactor.GameObject.transform.position);
                totalAngle += FindRotationSensitivity() * ConvertToAngle(direction);
            }

            return totalAngle;
        }

        private Vector2 FindLocalPoint(Vector3 position) => upTransform.InverseTransformPoint(position).normalized;

        private float ConvertToAngle(Vector2 direction) => Vector2.SignedAngle(upTransform.up, direction);

        private float FindRotationSensitivity() => 1f / interactors.Count;

        private void Rotate(float angle)
        {
            resetting = false;

            var rotation = transform.localEulerAngles;
            var updated = rotation.z + angle;
            CurrentSteeringAngle = updated;
        }

        private void ReturnToNeutral()
        {
            if (resetting)
            {
                return;
            }

            if (smoothReset)
            {
                resetting = true;
                elapsedResetTime = 0f;
                resetStartRotation = Quaternion.Euler(0f, 0f, CurrentSteeringAngle);
                return;
            }

            CurrentSteeringAngle = neutralSteeringRotation.eulerAngles.z;
        }

        private static float WrapAngle(float angle)
        {
            angle %= 360;

            if (angle > 180)
            {
                return angle - 360;
            }

            return angle;
        }
    }
}
