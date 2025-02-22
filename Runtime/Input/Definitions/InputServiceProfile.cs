﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework;
using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Utilities;
using RealityCollective.Utilities.Attributes;
using RealityToolkit.Input.Hands;
using RealityToolkit.Input.Interactors;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.Input.Interfaces.Modules;
using UnityEngine;

namespace RealityToolkit.Input.Definitions
{
    /// <summary>
    /// Configuration profile settings for setting up controller pointers.
    /// </summary>
    public class InputServiceProfile : BaseServiceProfile<IInputServiceModule>
    {
        #region Global Input System Options

        [SerializeField]
        [Tooltip("How should the gaze provider behave by default?")]
        private GazeProviderBehaviour gazeProviderBehaviour = GazeProviderBehaviour.Auto;

        /// <summary>
        /// How should the gaze provider behave by default?
        /// </summary>
        public GazeProviderBehaviour GazeProviderBehaviour
        {
            get => gazeProviderBehaviour;
            set => gazeProviderBehaviour = value;
        }

        [SerializeField]
        [Tooltip("The concrete type of IGazeProvider to use.")]
        [Implements(typeof(IGazeProvider), TypeGrouping.ByNamespaceFlat)]
        private SystemType gazeProviderType;

        /// <summary>
        /// The concrete type of <see cref="IGazeProvider"/> to use.
        /// </summary>
        public SystemType GazeProviderType
        {
            get => gazeProviderType;
            set => gazeProviderType = value;
        }

        [Prefab]
        [SerializeField]
        [Tooltip("The gaze cursor prefab to use on the Gaze pointer.")]
        private GameObject gazeCursorPrefab = null;

        /// <summary>
        /// The gaze cursor prefab to use on the Gaze pointer.
        /// </summary>
        public GameObject GazeCursorPrefab => gazeCursorPrefab;

        #endregion Global Input System Options

        #region Interactions

        /// <summary>
        /// Should direct interaction be enabled at startup?
        /// </summary>
        [field: SerializeField, Header("Interactions"), Tooltip("Should near interaction be enabled at startup?")]
        public bool DirectInteraction { get; private set; } = true;

        /// <summary>
        /// Should far interaction be enabled at startup?
        /// </summary>
        [field: SerializeField, Tooltip("Should far interaction be enabled at startup?")]
        public bool FarInteraction { get; private set; } = true;

        /// <summary>
        /// Global configuration for <see cref="IInteractor"/>s.
        /// </summary>
        [field: SerializeField, Tooltip("Global configuration for interactors.")]
        public InteractorsProfile InteractorsProfile { get; private set; }

        #endregion Interactions

        #region Profile Options

        [Space]
        [SerializeField]
        [Tooltip("Gloabl settings for hand controllers.")]
        private HandControllerSettings handControllerSettings = null;

        /// <summary>
        /// Gloabl settings for hand controllers.
        /// </summary>
        public HandControllerSettings HandControllerSettings
        {
            get => handControllerSettings;
            internal set => handControllerSettings = value;
        }

        [SerializeField]
        [Tooltip("Input System Action Mapping profile for setting up avery action a user can make in your application.")]
        private InputActionsProfile inputActionsProfile;

        /// <summary>
        /// Input System Action Mapping profile for setting up avery action a user can make in your application.
        /// </summary>
        public InputActionsProfile InputActionsProfile
        {
            get => inputActionsProfile;
            set => inputActionsProfile = value;
        }

        #endregion Profile Options
    }
}
