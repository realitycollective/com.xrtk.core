﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using RealityToolkit.CameraSystem.Definitions;
using System.Collections.Generic;
using UnityEngine.XR;

namespace RealityToolkit.CameraSystem.Interfaces
{
    /// <summary>
    /// The base interface for implementing a mixed reality camera system.
    /// </summary>
    public interface IMixedRealityCameraSystem : IService
    {
        /// <summary>
        /// The list of <see cref="IMixedRealityCameraServiceModule"/>s registered and running with the system.
        /// </summary>
        IReadOnlyCollection<IMixedRealityCameraServiceModule> CameraDataProviders { get; }

        /// <summary>
        /// The reference to the <see cref="IMixedRealityCameraRig"/> attached to the Main Camera (typically this is the player's camera).
        /// </summary>
        IMixedRealityCameraRig MainCameraRig { get; }

        /// <summary>
        /// Gets the configured <see cref="TrackingType"/> for the active <see cref="IMixedRealityCameraRig"/>.
        /// </summary>
        TrackingType TrackingType { get; }

        /// <summary>
        /// Gets the active <see cref="XRDisplaySubsystem"/> for the currently loaded
        /// XR plugin / platform.
        /// </summary>
        /// <remarks>The reference is lazy loaded once on first access and then cached for future use.</remarks>
        XRDisplaySubsystem DisplaySubsystem { get; }

        /// <summary>
        /// Registers the <see cref="IMixedRealityCameraServiceModule"/> with the <see cref="IMixedRealityCameraSystem"/>.
        /// </summary>
        /// <param name="dataProvider"></param>
        void RegisterCameraDataProvider(IMixedRealityCameraServiceModule dataProvider);

        /// <summary>
        /// UnRegisters the <see cref="IMixedRealityCameraServiceModule"/> with the <see cref="IMixedRealityCameraSystem"/>.
        /// </summary>
        /// <param name="dataProvider"></param>
        void UnRegisterCameraDataProvider(IMixedRealityCameraServiceModule dataProvider);
    }
}