﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using UnityEngine;

namespace RealityToolkit.Input.Hands.Poses
{
    /// <summary>
    /// A hand controller pose definition with recorded hand joint data.
    /// Defined hand poses can be recognized and trigger input actions.
    /// </summary>
    public class HandControllerPoseProfile : BaseProfile
    {
        [SerializeField]
        [Tooltip("A unique ID to the pose. Can be a descriptive name, but must be unique!")]
        private string id = string.Empty;

        /// <summary>
        /// A unique ID to the pose.
        /// </summary>
        public string Id => id;

        [SerializeField]
        [Tooltip("Describes the hand pose.")]
        private string description = string.Empty;

        /// <summary>
        /// Describes the hand pose.
        /// </summary>
        public string Description => description;

        [SerializeField]
        [Tooltip("Is this the default (idle) hand pose?")]
        private bool isDefault = false;

        /// <summary>
        /// Is this the default (idle) hand pose?
        /// </summary>
        public bool IsDefault => isDefault;

        [SerializeField]
        [Tooltip("Assign JSON definition file containing simulated gesture information.")]
        private TextAsset data = null;

        /// <summary>
        /// Gets the gesture definition's joint information used to simulate the gesture.
        /// </summary>
        public TextAsset Data => data;

        #region Baked Hand Data

        [SerializeField]
        [Tooltip("Was the data for this pose definition baked?")]
        private bool didBake = false;

        /// <summary>
        /// Was the data for this pose definition baked?
        /// </summary>
        public bool DidBake => didBake;

        [SerializeField]
        [Tooltip("Is the hand currently in a gripping pose?")]
        private bool isGripping = false;

        /// <summary>
        /// Is the hand currently in a gripping pose?
        /// </summary>
        public bool IsGripping => isGripping;

        [SerializeField]
        [Tooltip("Finger curling values per hand finger.")]
        private float[] fingerCurlStrengths = new float[] { 0, 0, 0, 0, 0 };

        /// <summary>
        /// Finger curling values per hand finger.
        /// </summary>
        public float[] FingerCurlStrengths => fingerCurlStrengths;

        [SerializeField]
        [Tooltip("What's the grip strength of the hand?")]
        private float gripStrength = 0f;

        /// <summary>
        /// What's the grip strength of the hand?
        /// </summary>
        public float GripStrength => gripStrength;

        #endregion Baked Hand Data
    }
}