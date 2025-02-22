﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactables;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// A <see cref="IInteractionBehaviour"/> is any behaviour performed on a <see cref="IInteractable"/> when being interacted with.
    /// </summary>
    public interface IInteractionBehaviour
    {
        /// <summary>
        /// When comparing <see cref="IInteractionBehaviour"/>s on the same <see cref="IInteractable"/>,
        /// the one with a higher <see cref="SortingOrder"/> will always be executed after the one with a lower <see cref="SortingOrder"/>.
        /// </summary>
        /// <remarks>Internally the value is stored as a signed 16 bit integer (short) and so is limited to the range -32,768 to 32,767.</remarks>
        short SortingOrder { get; }

        /// <summary>
        /// The <see cref="IInteractable"/> the <see cref="IInteractionBehaviour"/> is attached to.
        /// </summary>
        IInteractable Interactable { get; }

        /// <summary>
        /// The <see cref="IInteractable"/> was reset. Use this hook to implement any custom reset
        /// behaviour your <see cref="IInteractionBehaviour"/> might need to perform when <see cref="IInteractable"/> resets.
        /// </summary>
        void OnInteractableReset();

        #region Focus

        /// <summary>
        /// The first <see cref="Interactors.IInteractor"/> has gained focus on this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnFirstFocusEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has gained focus on this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnFocusEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has lost focus on this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnFocusExited(InteractionExitEventArgs eventArgs);

        /// <summary>
        /// The last <see cref="Interactors.IInteractor"/> has lost focus on this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnLastFocusExited(InteractionExitEventArgs eventArgs);

        #endregion Focus

        #region Select

        /// <summary>
        /// The first <see cref="Interactors.IInteractor"/> has selected this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnFirstSelectEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has selected this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnSelectEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has stopped selecting this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnSelectExited(InteractionExitEventArgs eventArgs);

        /// <summary>
        /// The last <see cref="Interactors.IInteractor"/> has stopped selecting this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnLastSelectExited(InteractionExitEventArgs eventArgs);

        #endregion Select

        #region Grab

        /// <summary>
        /// The first <see cref="Interactors.IInteractor"/> has grabbed this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnFirstGrabEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has grabbed this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionEventArgs"/>.</param>
        void OnGrabEntered(InteractionEventArgs eventArgs);

        /// <summary>
        /// A <see cref="Interactors.IInteractor"/> has stopped grabbing this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnGrabExited(InteractionExitEventArgs eventArgs);

        /// <summary>
        /// The last <see cref="Interactors.IInteractor"/> has stopped grabbing this <see cref="IInteractable"/>.
        /// </summary>
        /// <param name="eventArgs"><see cref="InteractionExitEventArgs"/>.</param>
        void OnLastGrabExited(InteractionExitEventArgs eventArgs);

        #endregion Grab
    }
}
