﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Controllers.UnityInput;
using RealityToolkit.Input.Definitions;

namespace RealityToolkit.Definitions.Controllers.UnityInput.Profiles
{
    public class MouseControllerServiceModuleProfile : BaseControllerServiceModuleProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(MouseController), Handedness.Any, true),
            };
        }
    }
}