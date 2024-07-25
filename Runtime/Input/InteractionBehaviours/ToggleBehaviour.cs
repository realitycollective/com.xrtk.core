// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// A <see cref="IInteractionBehaviour"/> for creating <see cref="Interactables.IInteractable"/>s that mimick toggle button behaviour.
    /// </summary>
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/toggle-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(ToggleBehaviour))]
    public class ToggleBehaviour : BaseInteractionBehaviour
    {
        /// <summary>
        /// <see cref="UnityEvent"/> for when a toggle is toggled.
        /// </summary>
        [Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        [SerializeField, Tooltip("Is the toggle currently on or off?")]
        private bool isOn = false;

        [SerializeField, Tooltip("If set, the button will raise click on input down instead of when input is released.")]
        private bool raiseOnInputDown = false;

        [Space]
        [SerializeField, Tooltip("List of delegates triggered on value change.")]
        private ToggleEvent valueChanged = null;

        /// <summary>
        /// The toggle <see cref="IsOn"/> value has changed.
        /// </summary>
        public ToggleEvent ValueChanged => valueChanged;

        /// <summary>
        /// Is the toggle currently on or off?
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {
                if (value == isOn)
                {
                    return;
                }

                isOn = value;
                ValueChanged?.Invoke(isOn);
            }
        }

        /// <summary>
        /// Updates <see cref="IsOn"/> without raising <see cref="ValueChanged"/>.
        /// </summary>
        /// <param name="isOn">The new <see cref="IsOn"/> value.</param>
        public void SetIsOnWithoutNotify(bool isOn) => this.isOn = isOn;

        /// <inheritdoc/>
        protected override void OnFirstSelectEntered(InteractionEventArgs eventArgs)
        {
            if (raiseOnInputDown && !Interactable.IsGrabbed)
            {
                IsOn = !IsOn;
            }
        }

        /// <inheritdoc/>
        protected override void OnLastSelectExited(InteractionExitEventArgs eventArgs)
        {
            if (!raiseOnInputDown && !Interactable.IsGrabbed)
            {
                IsOn = !IsOn;
            }
        }

        /// <inheritdoc/>
        protected override void OnFirstGrabEntered(InteractionEventArgs eventArgs)
        {
            if (raiseOnInputDown && !Interactable.IsSelected)
            {
                IsOn = !IsOn;
            }
        }

        /// <inheritdoc/>
        protected override void OnLastGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (!raiseOnInputDown && !Interactable.IsSelected)
            {
                IsOn = !IsOn;
            }
        }
    }
}
