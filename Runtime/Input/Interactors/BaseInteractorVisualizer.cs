// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityToolkit.Input.Interactors
{
    public abstract class BaseInteractorVisualizer : MonoBehaviour, IInteractorVisualizer
    {
        /// <inheritdoc />
        public virtual IInteractor Interactor { get; set; }

        /// <inheritdoc />
        public virtual void OnPreRaycast() { }

        /// <inheritdoc />
        public virtual void OnPostRaycast() { }
    }
}
