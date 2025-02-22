﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;

namespace RealityToolkit.Input.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="InteractionMapping"/> to refactor the generic methods used for raising events.
    /// </summary>
    public static class InteractionMappingsExtensions
    {
        private static IInputService inputService = null;

        private static IInputService InputService
            => inputService ?? (inputService = ServiceManager.Instance.GetService<IInputService>());

        /// <summary>
        /// Raise the actions to the input system.
        /// </summary>
        /// <param name="interactionMapping"></param>
        /// <param name="inputSource"></param>
        /// <param name="controllerHandedness"></param>
        public static void RaiseInputAction(this InteractionMapping interactionMapping, IInputSource inputSource, Handedness controllerHandedness)
        {
            var changed = interactionMapping.ControlActivated;
            var updated = interactionMapping.Updated;

            if (changed &&
                (interactionMapping.AxisType == AxisType.Digital ||
                 interactionMapping.AxisType == AxisType.SingleAxis))
            {
                if (interactionMapping.BoolData)
                {
                    InputService?.RaiseOnInputDown(inputSource, controllerHandedness, interactionMapping.InputAction);
                }
                else
                {
                    InputService?.RaiseOnInputUp(inputSource, controllerHandedness, interactionMapping.InputAction);
                }
            }

            if (updated)
            {
                switch (interactionMapping.AxisType)
                {
                    case AxisType.Digital:
                        InputService?.RaiseOnInputPressed(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.BoolData ? 1 : 0);
                        break;
                    case AxisType.SingleAxis:
                        InputService?.RaiseOnInputPressed(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.FloatData);
                        break;
                    case AxisType.DualAxis:
                        InputService?.RaisePositionInputChanged(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.Vector2Data);
                        break;
                    case AxisType.ThreeDofPosition:
                        InputService?.RaisePositionInputChanged(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.PositionData);
                        break;
                    case AxisType.ThreeDofRotation:
                        InputService?.RaiseRotationInputChanged(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.RotationData);
                        break;
                    case AxisType.SixDof:
                        InputService?.RaisePoseInputChanged(
                            inputSource,
                            controllerHandedness,
                            interactionMapping.InputAction,
                            interactionMapping.PoseData);
                        break;
                }
            }
        }

        /// <summary>
        /// Overload extension to enable getting of an InteractionDefinition of a specific type
        /// </summary>
        /// <param name="input">The InteractionDefinition array reference</param>
        /// <param name="key">The specific DeviceInputType value to query</param>
        public static InteractionMapping GetInteractionByType(this InteractionMapping[] input, DeviceInputType key)
        {
            for (int i = 0; i < input?.Length; i++)
            {
                if (input[i].InputType == key)
                {
                    return input[i];
                }
            }

            return default;
        }

        /// <summary>
        /// Overload extension to enable getting of an InteractionDefinition of a specific type
        /// </summary>
        /// <param name="input">The InteractionDefinition array reference</param>
        /// <param name="key">The specific DeviceInputType value to query</param>
        public static bool SupportsInputType(this InteractionMapping[] input, DeviceInputType key)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].InputType == key)
                {
                    return true;
                }
            }

            return false;
        }
    }
}