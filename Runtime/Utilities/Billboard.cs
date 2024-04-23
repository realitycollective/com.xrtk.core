// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityToolkit.Utilities
{
    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [SerializeField, Tooltip("Specifies the axis about which the object will rotate.")]
        private SnapAxis axes = SnapAxis.Y;

        [SerializeField, Tooltip("Inverts the rotation, if set.")]
        private bool invert = false;

        [SerializeField, Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
        private Transform targetTransform;

        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        public SnapAxis Axes
        {
            get => axes;
            set => axes = value;
        }

        /// <summary>
        /// The target we will orient to. If no target is specified, the main camera will be used.
        /// </summary>
        public Transform TargetTransform
        {
            get => targetTransform = targetTransform == null ? Camera.main.transform : targetTransform;
            set => targetTransform = value == null ? Camera.main.transform : value;
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            var direction = invert ?
                transform.position - TargetTransform.position :
                TargetTransform.position - transform.position;

            // If we are right next to the camera the rotation is undefined.
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            var rotation = Quaternion.LookRotation(-direction).eulerAngles;

            if ((axes & SnapAxis.X) <= 0)
            {
                rotation.x = 0f;
            }

            if ((axes & SnapAxis.Y) <= 0)
            {
                rotation.y = 0f;
            }

            if ((axes & SnapAxis.Z) <= 0)
            {
                rotation.z = 0f;
            }

            transform.eulerAngles = rotation;
        }
    }
}