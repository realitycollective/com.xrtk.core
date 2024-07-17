// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityToolkit.Input.Hands.Poses
{
    /// <summary>
    /// Identifies and implementation that provides <see cref="HandPose"/>s to <see cref="Visualizers.BaseHandControllerVisualizer"/>s.
    /// </summary>
    public interface IProvideHandPose
    {
        /// <summary>
        /// Optional focus <see cref="HandPose"/> override.
        /// </summary>
        HandPose FocusPose { get; }

        /// <summary>
        /// Optional select <see cref="HandPose"/> override.
        /// </summary>
        HandPose SelectPose { get; }

        /// <summary>
        /// Optional grab <see cref="HandPose"/> override.
        /// </summary>
        HandPose GrabPose { get; }
    }
}
