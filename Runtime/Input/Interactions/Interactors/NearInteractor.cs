// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Interactions.Interactables;
using UnityEngine;

namespace RealityToolkit.Input.Interactions.Interactors
{
    /// <summary>
    /// <see cref="Interfaces.IPointer"/> used for directly interacting with interactables that are touching.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class NearInteractor : BaseControllerInteractor
    {
        private SphereCollider sphereCollider;

        /// <inheritdoc />
        public override bool IsFarInteractor => false;

        private void Awake()
        {
            ConfigureTriggerCollider();
        }

        private void ConfigureTriggerCollider()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = PointerExtent;
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable) &&
                interactable.IsValid)
            {
                interactable.OnFocused(this);
            }
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected virtual void OnTriggerStay(Collider other)
        {

        }

        /// <summary>
        /// <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnUnfocused(this);
            }
        }
    }
}
