// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.InteractionBehaviours;
using RealityToolkit.Input.Interactors;

namespace RealityToolkit.Input.Interactables
{
    /// <summary>
    /// An <see cref="IInteractable"/> marks an object that can be interacted with by <see cref="IInteractor"/>s.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Gets or sets the <see cref="IInteractable"/>s label that may be used to
        /// identify the interactable or categorize it
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// A display name for the <see cref="IInteractable"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is the <see cref="IInteractable"/> valid for interaciton?
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Is the <see cref="IInteractable"/> currently focused by an <see cref="IInteractor"/>?
        /// </summary>
        bool IsFocused { get; }

        /// <summary>
        /// Is the <see cref="IInteractable"/> currently selected by an <see cref="IInteractor"/>?
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Is the <see cref="IInteractable"/> currently grabbed by an <see cref="IInteractor"/>?
        /// </summary>
        bool IsGrabbed { get; }

        /// <summary>
        /// The <see cref="IInteractable"/>'s focus mode.
        /// </summary>
        InteractableFocusMode FocusMode { get; }

        /// <summary>
        /// Does the <see cref="IInteractable"/> allow direct interaction?
        /// </summary>
        bool DirectInteractionEnabled { get; }

        /// <summary>
        /// Does the <see cref="IInteractable"/> allow interaction from a distance?
        /// </summary>
        bool FarInteractionEnabled { get; }

        /// <summary>
        /// Adds the <paramref name="behaviour"/> to the <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="behaviour">The <see cref="IInteractionBehaviour"/>.</param>
        void Add(IInteractionBehaviour behaviour);

        /// <summary>
        /// Removes the <paramref name="behaviour"/> from the <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="behaviour">The <see cref="IInteractionBehaviour"/>.</param>
        void Remove(IInteractionBehaviour behaviour);

        /// <summary>
        /// Resets the <see cref="IInteractable"/> and notifies any attached <see cref="IInteractionBehaviour"/>s
        /// about it using the <see cref="IInteractionBehaviour.OnInteractableReset"/> hook.
        /// </summary>
        void ResetInteractable();
    }
}
