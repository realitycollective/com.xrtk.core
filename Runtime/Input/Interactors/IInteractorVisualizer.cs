// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityToolkit.Input.Interactors
{
    public interface IInteractorVisualizer
    {
        /// <summary>
        /// The <see cref="IInteractor"/> visualized.
        /// </summary>
        IInteractor Interactor { get; }

        /// <summary>
        /// Called before all rays have casted on the <see cref="IInteractor"/>.
        /// </summary>
        void OnPreRaycast();

        /// <summary>
        /// Called after all rays have casted on the <see cref="IInteractor"/>.
        /// </summary>
        void OnPostRaycast();
    }
}
