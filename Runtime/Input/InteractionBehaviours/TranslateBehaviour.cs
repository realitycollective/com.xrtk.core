﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/translate-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(TranslateBehaviour))]
    public class TranslateBehaviour : BaseInteractionBehaviour
    {
        [SerializeField]
        [Tooltip("How should the transform be rotated while being dragged?")]
        private RotationModeEnum rotationMode = RotationModeEnum.Default;

        [SerializeField]
        [Tooltip("Scale by which hand movement in Z is multiplied to move the dragged object.")]
        private float distanceScale = 1f;

        [SerializeField]
        [Range(0.01f, 1.0f)]
        [Tooltip("Controls the speed at which the object will interpolate toward the desired position")]
        private float positionLerpSpeed = 0.2f;

        [SerializeField]
        [Range(0.01f, 1.0f)]
        [Tooltip("Controls the speed at which the object will interpolate toward the desired rotation")]
        private float rotationLerpSpeed = 0.2f;

        [SerializeField, Tooltip("Optional: If set, the translation is appled using the physics system on the rigidbody. Otherwise regular transform operations are performed.")]
        private new Rigidbody rigidbody = null;

        private enum RotationModeEnum
        {
            Default,
            LockObjectRotation,
            OrientTowardUser,
            OrientTowardUserAndKeepUpright
        }

        private IControllerInteractor currentInteractor;
        private bool isDragging;
        private float handRefDistance;
        private float objectReferenceDistance;
        private Vector3 draggingPosition;
        private Vector3 objectReferenceUp;
        private Vector3 objectReferenceForward;
        private Vector3 objectReferenceGrabPoint;
        private Quaternion draggingRotation;
        private Quaternion gazeAngularOffset;

        /// <inheritdoc/>
        protected override void Update()
        {
            if (isDragging)
            {
                UpdateDragging();
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <inheritdoc/>
        protected override void OnGrabEntered(InteractionEventArgs eventArgs)
        {
            // This action is not for use with direct interactors.
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor ||
                eventArgs.Interactor is IDirectInteractor)
            {
                StopDragging();
                return;
            }

            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

            currentInteractor = controllerInteractor;

            var initialDraggingPosition = controllerInteractor.Result.EndPoint;
            StartDragging(initialDraggingPosition);
        }

        /// <inheritdoc/>
        protected override void OnGrabExited(InteractionExitEventArgs eventArgs)
        {
            StopDragging();
        }

        private void StartDragging(Vector3 initialDraggingPosition)
        {
            if (isDragging)
            {
                return;
            }

            isDragging = true;

            var cameraTransform = Camera.main.transform;

            currentInteractor.TryGetPointerPosition(out Vector3 inputPosition);

            var pivotPosition = GetHandPivotPosition(cameraTransform);
            handRefDistance = Vector3.Magnitude(inputPosition - pivotPosition);
            objectReferenceDistance = Vector3.Magnitude(initialDraggingPosition - pivotPosition);

            var objForward = transform.forward;
            var objUp = transform.up;

            // Store where the object was grabbed from
            objectReferenceGrabPoint = cameraTransform.transform.InverseTransformDirection(transform.position - initialDraggingPosition);

            var objDirection = Vector3.Normalize(initialDraggingPosition - pivotPosition);
            var handDirection = Vector3.Normalize(inputPosition - pivotPosition);

            // in camera space
            objForward = cameraTransform.InverseTransformDirection(objForward);
            objUp = cameraTransform.InverseTransformDirection(objUp);
            objDirection = cameraTransform.InverseTransformDirection(objDirection);
            handDirection = cameraTransform.InverseTransformDirection(handDirection);

            objectReferenceForward = objForward;
            objectReferenceUp = objUp;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = initialDraggingPosition;
        }

        private void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            var cameraTransform = Camera.main.transform;

            currentInteractor.TryGetPointerPosition(out Vector3 inputPosition);

            var pivotPosition = GetHandPivotPosition(cameraTransform);
            var newHandDirection = Vector3.Normalize(inputPosition - pivotPosition);

            // in camera space
            newHandDirection = cameraTransform.InverseTransformDirection(newHandDirection);
            var targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);

            // back to world space
            targetDirection = cameraTransform.TransformDirection(targetDirection);

            var currentHandDistance = Vector3.Magnitude(inputPosition - pivotPosition);
            var distanceRatio = currentHandDistance / handRefDistance;
            var distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * distanceScale : 0;
            var targetDistance = objectReferenceDistance + distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);

            switch (rotationMode)
            {
                case RotationModeEnum.OrientTowardUser:
                case RotationModeEnum.OrientTowardUserAndKeepUpright:
                    draggingRotation = Quaternion.LookRotation(transform.position - pivotPosition);
                    break;
                case RotationModeEnum.LockObjectRotation:
                    draggingRotation = transform.rotation;
                    break;
                default:
                    // in world space
                    Vector3 objForward = cameraTransform.TransformDirection(objectReferenceForward);
                    // in world space
                    Vector3 objUp = cameraTransform.TransformDirection(objectReferenceUp);
                    draggingRotation = Quaternion.LookRotation(objForward, objUp);
                    break;
            }

            // Apply Final Position
            var newPosition = Vector3.Lerp(transform.position, draggingPosition + cameraTransform.TransformDirection(objectReferenceGrabPoint), positionLerpSpeed);
            if (rigidbody.IsNull())
            {
                transform.position = newPosition;
            }
            else
            {
                rigidbody.MovePosition(newPosition);
            }

            // Apply Final Rotation
            var newRotation = Quaternion.Lerp(transform.rotation, draggingRotation, rotationLerpSpeed);
            if (rigidbody.IsNull())
            {
                transform.rotation = newRotation;
            }
            else
            {
                rigidbody.MoveRotation(newRotation);
            }

            if (rotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                var upRotation = Quaternion.FromToRotation(transform.up, Vector3.up);
                transform.rotation = upRotation * transform.rotation;
            }
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private static Vector3 GetHandPivotPosition(Transform cameraTransform)
        {
            return cameraTransform.position + new Vector3(0, -0.2f, 0) - cameraTransform.forward * 0.2f; // a bit lower and behind
        }
    }
}
