﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Extensions;
using RealityToolkit.Input.Interfaces.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.Input.Hands
{
    /// <summary>
    /// Platform agnostic hand controller type.
    /// </summary>
    [System.Runtime.InteropServices.Guid("B18A9A6C-E5FD-40AE-89E9-9822415EC62B")]
    public class HandController : BaseController, IHandController
    {
        /// <inheritdoc />
        public HandController() : base() { }

        /// <inheritdoc />
        public HandController(IControllerServiceModule controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, ControllerProfile controllerMappingProfile)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile)
        {
        }

        private const int POSE_FRAME_BUFFER_SIZE = 5;
        private const float NEW_VELOCITY_WEIGHT = .2f;
        private const float CURRENT_VELOCITY_WEIGHT = .8f;

        private readonly int velocityUpdateFrameInterval = 9;
        private readonly Bounds[] cachedPalmBounds = new Bounds[4];
        private readonly Bounds[] cachedThumbBounds = new Bounds[2];
        private readonly Bounds[] cachedIndexFingerBounds = new Bounds[2];
        private readonly Bounds[] cachedMiddleFingerBounds = new Bounds[2];
        private readonly Bounds[] cachedRingFingerBounds = new Bounds[2];
        private readonly Bounds[] cachedLittleFingerBounds = new Bounds[2];
        private readonly Dictionary<TrackedHandBounds, Bounds[]> bounds = new Dictionary<TrackedHandBounds, Bounds[]>();
        private readonly Dictionary<HandJoint, Pose> jointPoses = new Dictionary<HandJoint, Pose>();
        private readonly Queue<bool> isPinchingBuffer = new Queue<bool>(POSE_FRAME_BUFFER_SIZE);
        private readonly Queue<bool> isGrippingBuffer = new Queue<bool>(POSE_FRAME_BUFFER_SIZE);
        private readonly Queue<bool> isPointingBuffer = new Queue<bool>(POSE_FRAME_BUFFER_SIZE);

        private int velocityUpdateFrame = 0;
        private float deltaTimeStart = 0;

        private Pose lastHandRootPose;
        private Vector3 lastPalmNormal = Vector3.zero;
        private Vector3 lastPalmPosition = Vector3.zero;

        /// <inheritdoc />
        public override InteractionMapping[] DefaultInteractions { get; } =
        {
            // 6 DoF pose of the spatial pointer ("far interaction pointer").
            new InteractionMapping("Spatial Pointer Pose", AxisType.SixDof, DeviceInputType.SpatialPointer),
            // Select / pinch button press / release.
            new InteractionMapping("Select", AxisType.Digital, DeviceInputType.Select),
            // Hand in pointing pose yes/no?
            new InteractionMapping("Point", AxisType.Digital, DeviceInputType.ButtonPress),
            // Grip / grab button press / release.
            new InteractionMapping("Grip", AxisType.Digital, DeviceInputType.TriggerPress),
            // 6 DoF grip pose ("Where to put things when grabbing something?")
            new InteractionMapping("Grip Pose", AxisType.SixDof, DeviceInputType.SpatialGrip),
            // 6 DoF index finger tip pose (mainly for "near interaction pointer").
            new InteractionMapping("Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger)
        };

        /// <inheritdoc />
        public override InteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        /// <inheritdoc />
        public override InteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        /// <summary>
        /// Gets the current palm normal of the hand controller.
        /// </summary>
        private Vector3 PalmNormal => TryGetJointPose(HandJoint.Palm, out var pose) ? -pose.up : Vector3.zero;

        /// <summary>
        /// Is pinching state from the previous update frame.
        /// </summary>
        private bool LastIsPinching { get; set; }

        /// <inheritdoc />
        public bool IsPinching { get; private set; }

        /// <inheritdoc />
        public float PinchStrength { get; private set; }

        /// <summary>
        /// Is pointing state from the previous update frame.
        /// </summary>
        private bool LastIsPointing { get; set; }

        /// <inheritdoc />
        public bool IsPointing { get; private set; }

        /// <inheritdoc />
        public bool IsGripping { get; private set; }

        /// <inheritdoc />
        public float GripStrength { get; private set; }

        /// <inheritdoc />
        public float[] FingerCurlStrengths { get; set; } = new float[] { };

        /// <summary>
        /// Is gripping state from the previous update frame.
        /// </summary>
        private bool LastIsGripping { get; set; }

        /// <inheritdoc />
        public string TrackedPoseId { get; private set; }

        /// <summary>
        /// The hand's pointer pose in the player's rig local coordinate space.
        /// </summary>
        private Pose SpatialPointerPose { get; set; }

        /// <summary>
        /// The hand's index finger tip pose in the player's rig local coordinate space.
        /// </summary>
        private Pose IndexFingerTipPose { get; set; }

        /// <summary>
        /// The hand's grip pose in the player's rig local coordinate space.
        /// </summary>
        private Pose GripPose { get; set; }

        /// <summary>
        /// Updates the hand controller with new hand data UnityEngine.Input.
        /// </summary>
        /// <param name="handData">Updated hand data.</param>
        public void UpdateController(HandData handData)
        {
            if (!Enabled) { return; }

            var lastTrackingState = TrackingState;
            TrackingState = handData.TrackingState;

            if (lastTrackingState != TrackingState)
            {
                InputService?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }

            if (TrackingState == TrackingState.Tracked)
            {
                IsPositionAvailable = true;
                IsPositionApproximate = false;
                IsRotationAvailable = true;

                lastHandRootPose = handData.RootPose;
                LastIsPinching = IsPinching;
                LastIsGripping = IsGripping;
                LastIsPointing = IsPointing;

                UpdateJoints(handData);
                UpdateIsPinching(handData);
                UpdateIsIsPointing(handData);
                UpdateIsIsGripping(handData);
                UpdateIndexFingerTipPose();
                UpdateGripPose();
                UpdateFingerCurlStrength(handData);
                UpdateBounds();
                UpdateVelocity();

                TrackedPoseId = handData.TrackedPoseId;
                PinchStrength = handData.PinchStrength;
                SpatialPointerPose = handData.PointerPose;
                Pose = handData.RootPose;
                InputService?.RaiseSourcePoseChanged(InputSource, this, handData.RootPose);
            }

            UpdateInteractionMappings();
        }

        #region Hand Bounds Implementation

        /// <inheritdoc />
        public bool TryGetBounds(TrackedHandBounds handBounds, out Bounds[] newBounds)
        {
            if (bounds.ContainsKey(handBounds))
            {
                newBounds = bounds[handBounds];
                return true;
            }

            newBounds = null;
            return false;
        }

        private void UpdateBounds()
        {
            var handControllerDataProvider = (IHandControllerServiceModule)ServiceModule;

            if (handControllerDataProvider.HandPhysicsEnabled && handControllerDataProvider.BoundsMode == HandBoundsLOD.Low)
            {
                UpdateHandBounds();
            }
            else if (handControllerDataProvider.HandPhysicsEnabled && handControllerDataProvider.BoundsMode == HandBoundsLOD.High)
            {
                UpdatePalmBounds();
                UpdateThumbBounds();
                UpdateIndexFingerBounds();
                UpdateMiddleFingerBounds();
                UpdateRingFingerBounds();
                UpdateLittleFingerBounds();
            }
        }

        private void UpdatePalmBounds()
        {
            if (TryGetJointPose(HandJoint.LittleMetacarpal, out var pinkyMetacarpalPose, Space.World) &&
                TryGetJointPose(HandJoint.LittleProximal, out var pinkyKnucklePose, Space.World) &&
                TryGetJointPose(HandJoint.RingMetacarpal, out var ringMetacarpalPose, Space.World) &&
                TryGetJointPose(HandJoint.RingProximal, out var ringKnucklePose, Space.World) &&
                TryGetJointPose(HandJoint.MiddleMetacarpal, out var middleMetacarpalPose, Space.World) &&
                TryGetJointPose(HandJoint.MiddleProximal, out var middleKnucklePose, Space.World) &&
                TryGetJointPose(HandJoint.IndexMetacarpal, out var indexMetacarpalPose, Space.World) &&
                TryGetJointPose(HandJoint.IndexProximal, out var indexKnucklePose, Space.World))
            {
                // Palm bounds are a composite of each finger's metacarpal -> knuckle joint bounds.
                // Excluding the thumb here.

                // Index
                var indexPalmBounds = new Bounds(indexMetacarpalPose.position, Vector3.zero);
                indexPalmBounds.Encapsulate(indexKnucklePose.position);
                cachedPalmBounds[0] = indexPalmBounds;

                // Middle
                var middlePalmBounds = new Bounds(middleMetacarpalPose.position, Vector3.zero);
                middlePalmBounds.Encapsulate(middleKnucklePose.position);
                cachedPalmBounds[1] = middlePalmBounds;

                // Ring
                var ringPalmBounds = new Bounds(ringMetacarpalPose.position, Vector3.zero);
                ringPalmBounds.Encapsulate(ringKnucklePose.position);
                cachedPalmBounds[2] = ringPalmBounds;

                // Pinky
                var pinkyPalmBounds = new Bounds(pinkyMetacarpalPose.position, Vector3.zero);
                pinkyPalmBounds.Encapsulate(pinkyKnucklePose.position);
                cachedPalmBounds[3] = pinkyPalmBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.Palm))
                {
                    bounds[TrackedHandBounds.Palm] = cachedPalmBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.Palm, cachedPalmBounds);
                }
            }
        }

        private void UpdateHandBounds()
        {
            if (TryGetJointPose(HandJoint.Palm, out var palmPose))
            {
                var newHandBounds = new Bounds(palmPose.position, Vector3.zero);

                foreach (var kvp in jointPoses)
                {
                    if (kvp.Key == HandJoint.Palm)
                    {
                        continue;
                    }

                    newHandBounds.Encapsulate(kvp.Value.position);
                }

                if (bounds.ContainsKey(TrackedHandBounds.Hand))
                {
                    bounds[TrackedHandBounds.Hand] = new[] { newHandBounds };
                }
                else
                {
                    bounds.Add(TrackedHandBounds.Hand, new[] { newHandBounds });
                }
            }
        }

        private void UpdateThumbBounds()
        {
            if (TryGetJointPose(HandJoint.ThumbMetacarpal, out var knucklePose, Space.World) &&
                TryGetJointPose(HandJoint.ThumbProximal, out var middlePose, Space.World) &&
                TryGetJointPose(HandJoint.ThumbTip, out var tipPose, Space.World))
            {
                // Thumb bounds include metacarpal -> proximal and proximal -> tip bounds.

                // Knuckle to middle joint bounds.
                var knuckleToMiddleBounds = new Bounds(knucklePose.position, Vector3.zero);
                knuckleToMiddleBounds.Encapsulate(middlePose.position);
                cachedThumbBounds[0] = knuckleToMiddleBounds;

                // Middle to tip joint bounds.
                var middleToTipBounds = new Bounds(middlePose.position, Vector3.zero);
                middleToTipBounds.Encapsulate(tipPose.position);
                cachedThumbBounds[1] = middleToTipBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.Thumb))
                {
                    bounds[TrackedHandBounds.Thumb] = cachedThumbBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.Thumb, cachedThumbBounds);
                }
            }
        }

        private void UpdateIndexFingerBounds()
        {
            if (TryGetJointPose(HandJoint.IndexProximal, out var knucklePose, Space.World) &&
                TryGetJointPose(HandJoint.IndexIntermediate, out var middlePose, Space.World) &&
                TryGetJointPose(HandJoint.IndexTip, out var tipPose, Space.World))
            {
                // Index finger bounds include knuckle -> middle and middle -> tip bounds.

                // Knuckle to middle joint bounds.
                var knuckleToMiddleBounds = new Bounds(knucklePose.position, Vector3.zero);
                knuckleToMiddleBounds.Encapsulate(middlePose.position);
                cachedIndexFingerBounds[0] = knuckleToMiddleBounds;

                // Middle to tip joint bounds.
                var middleToTipBounds = new Bounds(middlePose.position, Vector3.zero);
                middleToTipBounds.Encapsulate(tipPose.position);
                cachedIndexFingerBounds[1] = middleToTipBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.IndexFinger))
                {
                    bounds[TrackedHandBounds.IndexFinger] = cachedIndexFingerBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.IndexFinger, cachedIndexFingerBounds);
                }
            }
        }

        private void UpdateMiddleFingerBounds()
        {
            if (TryGetJointPose(HandJoint.MiddleProximal, out var knucklePose, Space.World) &&
                TryGetJointPose(HandJoint.MiddleIntermediate, out var middlePose, Space.World) &&
                TryGetJointPose(HandJoint.MiddleTip, out var tipPose, Space.World))
            {
                // Middle finger bounds include knuckle -> middle and middle -> tip bounds.

                // Knuckle to middle joint bounds.
                var knuckleToMiddleBounds = new Bounds(knucklePose.position, Vector3.zero);
                knuckleToMiddleBounds.Encapsulate(middlePose.position);
                cachedMiddleFingerBounds[0] = knuckleToMiddleBounds;

                // Middle to tip joint bounds.
                var middleToTipBounds = new Bounds(middlePose.position, Vector3.zero);
                middleToTipBounds.Encapsulate(tipPose.position);
                cachedMiddleFingerBounds[1] = middleToTipBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.MiddleFinger))
                {
                    bounds[TrackedHandBounds.MiddleFinger] = cachedMiddleFingerBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.MiddleFinger, cachedMiddleFingerBounds);
                }
            }
        }

        private void UpdateRingFingerBounds()
        {
            if (TryGetJointPose(HandJoint.RingProximal, out var knucklePose, Space.World) &&
                TryGetJointPose(HandJoint.RingIntermediate, out var middlePose, Space.World) &&
                TryGetJointPose(HandJoint.RingTip, out var tipPose, Space.World))
            {
                // Ring finger bounds include knuckle -> middle and middle -> tip bounds.

                // Knuckle to middle joint bounds.
                var knuckleToMiddleBounds = new Bounds(knucklePose.position, Vector3.zero);
                knuckleToMiddleBounds.Encapsulate(middlePose.position);
                cachedRingFingerBounds[0] = knuckleToMiddleBounds;

                // Middle to tip joint bounds.
                var middleToTipBounds = new Bounds(middlePose.position, Vector3.zero);
                middleToTipBounds.Encapsulate(tipPose.position);
                cachedRingFingerBounds[1] = middleToTipBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.RingFinger))
                {
                    bounds[TrackedHandBounds.RingFinger] = cachedRingFingerBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.RingFinger, cachedRingFingerBounds);
                }
            }
        }

        private void UpdateLittleFingerBounds()
        {
            if (TryGetJointPose(HandJoint.LittleProximal, out var knucklePose, Space.World) &&
                TryGetJointPose(HandJoint.LittleIntermediate, out var middlePose, Space.World) &&
                TryGetJointPose(HandJoint.LittleTip, out var tipPose, Space.World))
            {
                // Pinky finger bounds include knuckle -> middle and middle -> tip bounds.

                // Knuckle to middle joint bounds.
                var knuckleToMiddleBounds = new Bounds(knucklePose.position, Vector3.zero);
                knuckleToMiddleBounds.Encapsulate(middlePose.position);
                cachedLittleFingerBounds[0] = knuckleToMiddleBounds;

                // Middle to tip joint bounds.
                var middleToTipBounds = new Bounds(middlePose.position, Vector3.zero);
                middleToTipBounds.Encapsulate(tipPose.position);
                cachedLittleFingerBounds[1] = middleToTipBounds;

                // Update cached bounds entry.
                if (bounds.ContainsKey(TrackedHandBounds.Pinky))
                {
                    bounds[TrackedHandBounds.Pinky] = cachedLittleFingerBounds;
                }
                else
                {
                    bounds.Add(TrackedHandBounds.Pinky, cachedLittleFingerBounds);
                }
            }
        }

        #endregion Hand Bounds Implementation

        #region Interaction Mappings

        protected virtual void UpdateInteractionMappings()
        {
            for (int i = 0; i < Interactions.Length; i++)
            {
                var interactionMapping = Interactions[i];
                switch (interactionMapping.InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        UpdateSpatialPointerMapping(interactionMapping);
                        break;
                    case DeviceInputType.Select:
                        UpdateSelectMapping(interactionMapping);
                        break;
                    case DeviceInputType.ButtonPress:
                        UpdatePointingMapping(interactionMapping);
                        break;
                    case DeviceInputType.TriggerPress:
                        UpdateGripMapping(interactionMapping);
                        break;
                    case DeviceInputType.SpatialGrip:
                        UpdateGripPoseMapping(interactionMapping);
                        break;
                    case DeviceInputType.IndexFinger:
                        UpdateIndexFingerMapping(interactionMapping);
                        break;
                }

                interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
            }
        }

        private void UpdateGripPoseMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);
            interactionMapping.PoseData = GripPose;
        }

        private void UpdateSpatialPointerMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);
            interactionMapping.PoseData = SpatialPointerPose;
        }

        private void UpdateSelectMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            if (!LastIsPinching && IsPinching)
            {
                interactionMapping.BoolData = true;
            }
            else if (LastIsPinching && !IsPinching)
            {
                interactionMapping.BoolData = false;
            }
            else if (IsPinching)
            {
                interactionMapping.BoolData = LastIsPinching;
            }
        }

        private void UpdateGripMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            if (!LastIsGripping && IsGripping)
            {
                interactionMapping.BoolData = true;
            }
            else if (LastIsGripping && !IsGripping)
            {
                interactionMapping.BoolData = false;
            }
            else if (IsGripping)
            {
                interactionMapping.BoolData = LastIsGripping;
            }
        }

        private void UpdatePointingMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            if (!LastIsPointing && IsPointing)
            {
                interactionMapping.BoolData = true;
            }
            else if (LastIsPointing && !IsPointing)
            {
                interactionMapping.BoolData = false;
            }
            else if (IsPointing)
            {
                interactionMapping.BoolData = LastIsPointing;
            }
        }

        private void UpdateIndexFingerMapping(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);
            interactionMapping.PoseData = IndexFingerTipPose;
        }

        #endregion Interaction Mappings

        /// <summary>
        /// Updates the controller's velocity / angular velocity.
        /// </summary>
        private void UpdateVelocity()
        {
            Vector3 palmPosition = Vector3.zero;
            if (TryGetJointPose(HandJoint.Palm, out var palmPose, Space.World))
            {
                palmPosition = palmPose.position;
            }

            if (velocityUpdateFrame == 0)
            {
                deltaTimeStart = Time.unscaledTime;
                lastPalmPosition = palmPosition;
                lastPalmNormal = PalmNormal;
            }
            else if (velocityUpdateFrame == velocityUpdateFrameInterval)
            {
                // Update linear velocity.
                var deltaTime = Time.unscaledTime - deltaTimeStart;
                var newVelocity = (palmPosition - lastPalmPosition) / deltaTime;
                Velocity = (Velocity * CURRENT_VELOCITY_WEIGHT) + (newVelocity * NEW_VELOCITY_WEIGHT);

                // Update angular velocity.
                var currentPalmNormal = PalmNormal;
                var rotation = Quaternion.FromToRotation(lastPalmNormal, currentPalmNormal);
                var rotationRate = rotation.eulerAngles * Mathf.Deg2Rad;
                AngularVelocity = rotationRate / deltaTime;
            }

            velocityUpdateFrame++;
            velocityUpdateFrame = velocityUpdateFrame > velocityUpdateFrameInterval ? 0 : velocityUpdateFrame;
        }

        /// <summary>
        /// Updates the controller's joint poses using provided hand data.
        /// </summary>
        /// <param name="handData">The updated hand data for this controller.</param>
        private void UpdateJoints(HandData handData)
        {
            for (int i = 0; i < HandData.JointCount; i++)
            {
                var handJoint = (HandJoint)i;

                if (TryGetJointPose(handJoint, out _))
                {
                    jointPoses[handJoint] = handData.Joints[i];
                }
                else
                {
                    jointPoses.Add(handJoint, handData.Joints[i]);
                }
            }
        }

        /// <summary>
        /// Updates the index finger tip pose value for the hand controller.
        /// </summary>
        private void UpdateIndexFingerTipPose()
        {
            if (TryGetJointPose(HandJoint.IndexTip, out var indexTipPose, Space.World))
            {
                IndexFingerTipPose = indexTipPose;
            }
        }

        /// <summary>
        /// Updates the grip pose value for the hand controller.
        /// </summary>
        private void UpdateGripPose()
        {
            if (TryGetJointPose(HandJoint.Palm, out var palmPose, Space.World))
            {
                GripPose = palmPose;
            }
        }

        /// <summary>
        /// Updates the finger curl values for the controller.
        /// </summary>
        /// <param name="handData">Updated hand data.</param>
        private void UpdateFingerCurlStrength(HandData handData)
        {
            if (FingerCurlStrengths == null)
            {
                FingerCurlStrengths = new float[handData.FingerCurlStrengths.Length];
            }

            Array.Copy(handData.FingerCurlStrengths, FingerCurlStrengths, FingerCurlStrengths.Length);
        }

        /// <summary>
        /// Updates the hand controller's internal is pinching state.
        /// Instead of updating the value for each frame, is pinching state
        /// is buffered for a few frames to stabilize and avoid false positives.
        /// </summary>
        /// <param name="handData">The hand data received for the current hand update frame.</param>
        private void UpdateIsPinching(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked)
            {
                var isPinchingThisFrame = handData.IsPinching;
                if (isPinchingBuffer.Count < POSE_FRAME_BUFFER_SIZE)
                {
                    isPinchingBuffer.Enqueue(isPinchingThisFrame);
                    IsPinching = false;
                }
                else
                {
                    isPinchingBuffer.Dequeue();
                    isPinchingBuffer.Enqueue(isPinchingThisFrame);

                    isPinchingThisFrame = true;
                    for (int i = 0; i < isPinchingBuffer.Count; i++)
                    {
                        var value = isPinchingBuffer.Dequeue();

                        if (!value)
                        {
                            isPinchingThisFrame = false;
                        }

                        isPinchingBuffer.Enqueue(value);
                    }

                    IsPinching = isPinchingThisFrame;
                }
            }
            else
            {
                isPinchingBuffer.Clear();
                IsPinching = false;
            }
        }

        /// <summary>
        /// Updates the hand controller's internal is pointing state.
        /// Instead of updating the value for each frame, is pointing state
        /// is buffered for a few frames to stabilize and avoid false positives.
        /// </summary>
        /// <param name="handData">The hand data received for the current hand update frame.</param>
        private void UpdateIsIsPointing(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked)
            {
                var isPointingThisFrame = handData.IsPointing;
                if (isPointingBuffer.Count < POSE_FRAME_BUFFER_SIZE)
                {
                    isPointingBuffer.Enqueue(isPointingThisFrame);
                    IsPointing = false;
                }
                else
                {
                    isPointingBuffer.Dequeue();
                    isPointingBuffer.Enqueue(isPointingThisFrame);

                    isPointingThisFrame = true;
                    for (int i = 0; i < isPointingBuffer.Count; i++)
                    {
                        var value = isPointingBuffer.Dequeue();

                        if (!value)
                        {
                            isPointingThisFrame = false;
                        }

                        isPointingBuffer.Enqueue(value);
                    }

                    IsPointing = isPointingThisFrame;
                }
            }
            else
            {
                isPointingBuffer.Clear();
                IsPointing = false;
            }
        }

        /// <summary>
        /// Updates the hand controller's internal is gripping state.
        /// Instead of updating the value for each frame, is gripping state
        /// is buffered for a few frames to stabilize and avoid false positives.
        /// </summary>
        /// <param name="handData">The hand data received for the current hand update frame.</param>
        private void UpdateIsIsGripping(HandData handData)
        {
            if (handData.TrackingState == TrackingState.Tracked)
            {
                var isGrippingThisFrame = handData.IsGripping;
                if (isGrippingBuffer.Count < POSE_FRAME_BUFFER_SIZE)
                {
                    isGrippingBuffer.Enqueue(isGrippingThisFrame);
                    IsGripping = false;
                }
                else
                {
                    isGrippingBuffer.Dequeue();
                    isGrippingBuffer.Enqueue(isGrippingThisFrame);

                    isGrippingThisFrame = true;
                    for (int i = 0; i < isGrippingBuffer.Count; i++)
                    {
                        var value = isGrippingBuffer.Dequeue();

                        if (!value)
                        {
                            isGrippingThisFrame = false;
                        }

                        isGrippingBuffer.Enqueue(value);
                    }

                    IsGripping = isGrippingThisFrame;
                }
            }
            else
            {
                isGrippingBuffer.Clear();
                IsGripping = false;
            }
        }

        /// <inheritdoc />
        public bool TryGetJointPose(HandJoint joint, out Pose pose, Space relativeTo = Space.Self)
        {
            if (relativeTo == Space.Self)
            {
                // Return joint pose relative to hand root.
                return jointPoses.TryGetValue(joint, out pose);
            }

            if (jointPoses.TryGetValue(joint, out var localPose))
            {
                pose = new Pose
                {
                    // Combine root pose with local joint pose.
                    position = lastHandRootPose.position + lastHandRootPose.rotation * localPose.position,
                    rotation = lastHandRootPose.rotation * localPose.rotation
                };

                // Translate to world space.
                if (InputService.InputRig.RigTransform.IsNotNull())
                {
                    pose.position = InputService.InputRig.RigTransform.TransformPoint(pose.position);
                    pose.rotation = InputService.InputRig.RigTransform.rotation * pose.rotation;
                }

                return lastHandRootPose != Pose.identity;
            }

            pose = Pose.identity;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetFingerCurlStrength(HandFinger handFinger, out float curlStrength)
        {
            var index = (int)handFinger;
            if (FingerCurlStrengths != null && FingerCurlStrengths.Length >= index)
            {
                curlStrength = FingerCurlStrengths[index];
                return true;
            }

            curlStrength = 0f;
            return false;
        }
    }
}
