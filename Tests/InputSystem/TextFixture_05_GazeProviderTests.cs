// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.InputSystem.Definitions;
using RealityToolkit.InputSystem.Interfaces;
using RealityToolkit.Tests.InputSystem;

namespace RealityToolkit.Tests
{
    public class TextFixture_05_GazeProviderTests
    {
        [SetUp]
        public void SetUpGazeProviderTests()
        {
            TestUtilities.InitializeMixedRealityToolkitScene(false);

            ServiceManager.Instance.ActiveProfile.AddConfiguration(InputSystemTestUtilities.TestInputSystemConfiguration);
            ServiceManager.Instance.TryCreateAndRegisterService(InputSystemTestUtilities.TestInputSystemConfiguration, out var service);
        }

        [Test]
        public void Test01_GazeProviderSetAuto()
        {
            var inputSystem = ServiceManager.Instance.GetService<IMixedRealityInputSystem>();
            inputSystem.SetGazeProviderBehaviour(GazeProviderBehaviour.Auto);

            if (AnyControllerWithPointersAttached(inputSystem))
            {
                Assert.IsNull(inputSystem.GazeProvider);
            }
            else
            {
                Assert.IsNotNull(inputSystem.GazeProvider);
            }
        }

        [Test]
        public void Test02_GazeProviderSetInactive()
        {
            var inputSystem = ServiceManager.Instance.GetService<IMixedRealityInputSystem>();
            inputSystem.SetGazeProviderBehaviour(GazeProviderBehaviour.Inactive);

            Assert.IsNull(inputSystem.GazeProvider);
        }

        [Test]
        public void Test03_GazeProviderSetActive()
        {
            var inputSystem = ServiceManager.Instance.GetService<IMixedRealityInputSystem>();
            inputSystem.SetGazeProviderBehaviour(GazeProviderBehaviour.Active);

            Assert.IsNotNull(inputSystem.GazeProvider);
        }

        [TearDown]
        public void CleanupMixedRealityToolkitTests()
        {
            TestUtilities.CleanupScene();
        }

        private bool AnyControllerWithPointersAttached(IMixedRealityInputSystem inputSystem)
        {
            if (inputSystem.DetectedControllers != null && inputSystem.DetectedControllers.Count > 0)
            {
                foreach (var detectedController in inputSystem.DetectedControllers)
                {
                    if (detectedController.InputSource.Pointers != null && detectedController.InputSource.Pointers.Length > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
