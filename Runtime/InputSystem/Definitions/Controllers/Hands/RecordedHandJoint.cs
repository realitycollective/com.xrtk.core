﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace RealityToolkit.Definitions.Controllers.Hands
{
    /// <summary>
    /// A single recorded hand joint's information that may be used to restore the joint pose.
    /// </summary>
    [Serializable]
    public struct RecordedHandJoint
    {
        /// <summary>
        /// Constructs a new joint record.
        /// </summary>
        /// <param name="joint">The joint that was recorded.</param>
        /// <param name="pose">The joint pose that was recorded.</param>
        public RecordedHandJoint(TrackedHandJoint joint, Pose pose)
        {
            this.joint = joint;
            this.pose = pose;
        }

        [SerializeField]
        private TrackedHandJoint joint;

        /// <summary>
        /// Joint this pose represents.
        /// </summary>
        public TrackedHandJoint Joint => joint;

        [SerializeField]
        private Pose pose;

        /// <summary>
        /// The recorded pose.
        /// </summary>
        public Pose Pose => pose;
    }
}