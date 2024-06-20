// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Physics;
using UnityEngine;

namespace RealityToolkit.Input.Interactors
{
    /// <summary>
    /// A simple line pointer for drawing lines from the input source origin to the current pointer position.
    /// </summary>
    [AddComponentMenu("")]
    public class FarInteractor : BaseControllerInteractor
    {
        [Range(2, 50)]
        [SerializeField]
        [Tooltip("This setting has a high performance cost. Values above 20 are not recommended.")]
        private int lineCastResolution = 2;

        /// <summary>
        /// The amount of rays to cast.
        /// </summary>
        /// <remarks>
        /// This setting has a high performance cost. Values above 20 are not recommended.
        /// </remarks>
        public int LineCastResolution
        {
            get => lineCastResolution;
            set => lineCastResolution = value;
        }

        /// <inheritdoc />
        public override bool IsFarInteractor => true;

        public override void OnPreRaycast()
        {
            // Make sure our array will hold
            if (Rays == null || Rays.Length != lineCastResolution)
            {
                Rays = new RayStep[lineCastResolution];
            }

            base.OnPreRaycast();
        }

        /// <inheritdoc />
        public override void OnPostRaycast()
        {
            if (!IsInteractionEnabled)
            {
                if (BaseCursor != null)
                {
                    BaseCursor.IsVisible = false;
                }

                return;
            }

            if (BaseCursor != null)
            {
                BaseCursor.IsVisible = true;
            }

            base.OnPostRaycast();
        }
    }
}