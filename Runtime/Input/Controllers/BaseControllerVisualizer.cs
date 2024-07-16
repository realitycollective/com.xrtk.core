// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.EventDatum.Input;
using UnityEngine;

namespace RealityToolkit.Input.Controllers
{
    /// <summary>
    /// Abstract base implementation for <see cref="IControllerVisualizer"/>s.
    /// </summary>
    public abstract class BaseControllerVisualizer : ControllerPoseSynchronizer, IControllerVisualizer
    {
        [SerializeField, Tooltip("Defines the pose to attach poke interactors to. Defaults to the visualizers root transform, if not set.")]
        private Transform pokePose = null;

        [SerializeField, Tooltip("Defines the pose to attach to when held. Defaults to the visualizers root transform, if not set.")]
        private Transform gripPose = null;

        /// <inheritdoc />
        public GameObject GameObject => gameObject;

        /// <inheritdoc />
        public Pose SourcePose { get; private set; }

        /// <inheritdoc />
        public bool OverrideSourcePose { get; set; }

        /// <inheritdoc />
        public Transform PokePose
        {
            get => pokePose.IsNotNull() ? pokePose : transform;
            protected set => pokePose = value;
        }

        /// <inheritdoc />
        public Transform GripPose
        {
            get => gripPose.IsNotNull() ? gripPose : transform;
            protected set => gripPose = value;
        }

        /// <inheritdoc />
        public override void OnSourcePoseChanged(SourcePoseEventData<Pose> eventData)
        {
            SourcePose = eventData.SourceData;

            if (OverrideSourcePose)
            {
                return;
            }

            base.OnSourcePoseChanged(eventData);
        }

        /// <inheritdoc />
        public override void OnSourcePoseChanged(SourcePoseEventData<Quaternion> eventData)
        {
            SourcePose = new Pose(SourcePose.position, eventData.SourceData);

            if (OverrideSourcePose)
            {
                return;
            }

            base.OnSourcePoseChanged(eventData);
        }

        /// <inheritdoc />
        public override void OnSourcePoseChanged(SourcePoseEventData<Vector2> eventData)
        {
            SourcePose = new Pose(eventData.SourceData, SourcePose.rotation);

            if (OverrideSourcePose)
            {
                return;
            }

            base.OnSourcePoseChanged(eventData);
        }

        /// <inheritdoc />
        public override void OnSourcePoseChanged(SourcePoseEventData<Vector3> eventData)
        {
            SourcePose = new Pose(eventData.SourceData, SourcePose.rotation);

            if (OverrideSourcePose)
            {
                return;
            }

            base.OnSourcePoseChanged(eventData);
        }
    }
}
