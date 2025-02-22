﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Modules;
using RealityCollective.Utilities.Extensions;
using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.Input.Interfaces.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.Input.Controllers
{
    /// <summary>
    /// Base controller service module to inherit from when implementing <see cref="IControllerServiceModule"/>s
    /// </summary>
    public abstract class BaseControllerServiceModule : BaseServiceModule, IControllerServiceModule
    {
        /// <inheritdoc />
        protected BaseControllerServiceModule(string name, uint priority, BaseControllerServiceModuleProfile profile, IInputService parentService)
            : base(name, priority, profile, parentService)
        {
            if (profile.IsNull())
            {
                throw new UnassignedReferenceException($"A {nameof(profile)} is required for {name}");
            }

            controllerMappingProfiles = profile.ControllerMappingProfiles;

            if (controllerMappingProfiles == null ||
                controllerMappingProfiles.Length == 0)
            {
                throw new UnassignedReferenceException($"{nameof(controllerMappingProfiles)} has no defined controller mappings for {name}");
            }

            InputService = parentService;
        }

        protected readonly IInputService InputService;

        private readonly ControllerProfile[] controllerMappingProfiles;

        private readonly List<IController> activeControllers = new List<IController>();

        /// <inheritdoc />
        public IReadOnlyList<IController> ActiveControllers => activeControllers;

        /// <inheritdoc />
        public ControllerProfile GetControllerMappingProfile(Type controllerType, Handedness handedness)
        {
            if (TryGetControllerMappingProfile(controllerType, handedness, out var controllerMappingProfile))
            {
                return controllerMappingProfile;
            }

            Debug.LogError($"Failed to find a controller mapping for {controllerType.Name} with with handedness: {handedness}");
            return null;
        }

        /// <inheritdoc />
        public bool TryGetControllerMappingProfile(Type controllerType, Handedness handedness, out ControllerProfile controllerMappingProfile)
        {
            if (controllerType == null)
            {
                Debug.LogError($"{nameof(controllerType)} is null!");
                controllerMappingProfile = null;
                return false;
            }

            if (!typeof(IController).IsAssignableFrom(controllerType))
            {
                Debug.LogError($"{controllerType.Name} does not implement {nameof(IController)}");
                controllerMappingProfile = null;
                return false;
            }

            // TODO provide a way to choose profiles with additional args instead of returning the first one found.
            for (int i = 0; i < controllerMappingProfiles.Length; i++)
            {
                if (handedness == controllerMappingProfiles[i].Handedness &&
                    controllerMappingProfiles[i].ControllerType?.Type == controllerType)
                {
                    controllerMappingProfile = controllerMappingProfiles[i];
                    return true;
                }
            }

            controllerMappingProfile = null;
            return false;
        }

        protected void AddController(IController controller)
        {
            activeControllers.Add(controller);
        }

        protected void RemoveController(IController controller)
        {
            if (controller != null)
            {
                activeControllers.Remove(controller);
            }
        }
    }
}