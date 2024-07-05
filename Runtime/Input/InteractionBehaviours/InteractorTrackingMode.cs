// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// Supported interactor tracking modes.
    /// </summary>
    public enum InteractorTrackingMode
    {
        /// <summary>
        /// We are tracking the interactors position to determine lever movement.
        /// </summary>
        Position = 0,
        /// <summary>
        /// We are tracking the interactors rotation to determine lever movement.
        /// </summary>
        Rotation
    }
}
