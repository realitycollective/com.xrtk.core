﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace RealityToolkit.Input.Definitions
{
    /// <summary>
    /// The Handedness defines which hand a controller is currently operating in.
    /// It is up to the developer to determine whether this affects the use of a controller or not.
    /// "Other" defines potential controllers that will offer a "third" hand, e.g. a full body tracking suit.
    /// </summary>
    [Flags]
    public enum Handedness
    {
        /// <summary>
        /// No hand specified by the SDK for the controller
        /// </summary>
        None = 0,
        /// <summary>
        /// The controller is identified as being provided in a Left hand
        /// </summary>
        Left = 1 << 0,
        /// <summary>
        /// The controller is identified as being provided in a Right hand
        /// </summary>
        Right = 1 << 1,
        /// <summary>
        /// The controller is identified as being either left and/or right handed.
        /// </summary>
        Both = Left | Right,
        /// <summary>
        /// Reserved, for systems that provide alternate hand state.
        /// </summary>
        Other = 1 << 2,
        /// <summary>
        /// Global catchall, used to map actions to any controller (provided the controller supports it)
        /// </summary>
        /// <remarks>Note, by default the specific hand actions will override settings mapped as both</remarks>
        Any = Other | Left | Right,
    }
}