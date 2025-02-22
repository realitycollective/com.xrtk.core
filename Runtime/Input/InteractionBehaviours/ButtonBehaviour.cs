﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// A <see cref="IInteractionBehaviour"/> for creating <see cref="Interactables.IInteractable"/>s that mimick button behaviour.
    /// </summary>
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/button-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(ButtonBehaviour))]
    public class ButtonBehaviour : BaseInteractionBehaviour
    {
        /// <summary>
        /// <see cref="UnityEvent"/> for when a button is clicked.
        /// </summary>
        [Serializable]
        public class ButtonClickEvent : UnityEvent { }

        [SerializeField, Tooltip("If set, the button will raise click on input down instead of when input is released.")]
        private bool raiseOnInputDown = false;

        [Space]
        [SerializeField, Tooltip("List of click delegates triggered on click.")]
        private ButtonClickEvent click = null;

        /// <summary>
        /// The button was clicked.
        /// </summary>
        public ButtonClickEvent Click => click;

        /// <inheritdoc/>
        protected override void OnFirstSelectEntered(InteractionEventArgs eventArgs)
        {
            if (raiseOnInputDown && !Interactable.IsGrabbed)
            {
                Click?.Invoke();
            }
        }

        /// <inheritdoc/>
        protected override void OnLastSelectExited(InteractionExitEventArgs eventArgs)
        {
            if (!raiseOnInputDown && !Interactable.IsGrabbed)
            {
                Click?.Invoke();
            }
        }

        /// <inheritdoc/>
        protected override void OnFirstGrabEntered(InteractionEventArgs eventArgs)
        {
            if (raiseOnInputDown && !Interactable.IsSelected)
            {
                Click?.Invoke();
            }
        }

        /// <inheritdoc/>
        protected override void OnLastGrabExited(InteractionExitEventArgs eventArgs)
        {
            if (!raiseOnInputDown && !Interactable.IsSelected)
            {
                Click?.Invoke();
            }
        }
    }
}
