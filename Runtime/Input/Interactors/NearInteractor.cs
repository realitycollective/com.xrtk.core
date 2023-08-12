// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Interactables;
using UnityEngine;

namespace RealityToolkit.Input.Interactors
{
    /// <summary>
    /// <see cref="Interfaces.IPointer"/> used for directly interacting with interactables that are touching.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class NearInteractor : BaseControllerInteractor, IDirectInteractor
    {
        private SphereCollider sphereCollider;
        private readonly DirectInteractorResult directResult = new DirectInteractorResult();
        private GameObject stayingColliderHit;

        /// <inheritdoc />
        public override bool IsFarInteractor => false;

        /// <inheritdoc />
        public IDirectInteractorResult DirectResult => directResult;

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
        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable) &&
                interactable.IsValid)
            {
                stayingColliderHit = other.gameObject;
                directResult.UpdateHit(this, other.gameObject);
            }
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject == stayingColliderHit)
            {
                stayingColliderHit = null;
                directResult.Clear();
            }
        }
    }
}
