﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// This <see cref="IInteractionBehaviour"/> will change the main material used on a <see cref="MeshRenderer"/> on the <see cref="Interactables.IInteractable"/>.
    /// </summary>
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/change-material-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(ChangeMaterialBehaviour))]
    public class ChangeMaterialBehaviour : BaseInteractionBehaviour
    {
        [SerializeField]
        private Material normalMaterial = null;

        [SerializeField]
        private Material focusedMaterial = null;

        [SerializeField]
        private Material selectedMaterial = null;

        [SerializeField]
        private Material grabbedMaterial = null;

        [SerializeField]
        private MeshRenderer meshRenderer = null;

        /// <inheritdoc/>
        protected override void Update()
        {
            if (Interactable.IsSelected)
            {
                meshRenderer.material = selectedMaterial;
            }
            else if (Interactable.IsGrabbed)
            {
                meshRenderer.material = grabbedMaterial;
            }
            else if (Interactable.IsFocused)
            {
                meshRenderer.material = focusedMaterial;
            }
            else
            {
                meshRenderer.material = normalMaterial;
            }
        }
    }
}