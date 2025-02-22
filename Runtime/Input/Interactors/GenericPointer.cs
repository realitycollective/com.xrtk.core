﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Definitions.Physics;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.Input.Interfaces.Handlers;
using RealityToolkit.Input.Physics;
using System;
using System.Collections;
using UnityEngine;

namespace RealityToolkit.Input.Interactors
{
    /// <summary>
    /// Base Class for pointers that don't inherit from MonoBehaviour.
    /// </summary>
    public class GenericPointer : IInteractor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pointerName"></param>
        /// <param name="inputSourceParent"></param>
        /// <param name="interactionMode"></param>
        public GenericPointer(string pointerName, IInputSource inputSourceParent)
        {
            if (ServiceManager.Instance.TryGetService<IInputService>(out var inputService))
            {
                PointerId = inputService.FocusProvider.GenerateNewPointerId();
                PointerName = pointerName;
                this.inputSourceParent = inputSourceParent;
            }
            else
            {
                throw new ArgumentException($"Couldn't find a valid {nameof(IInputService)}!");
            }
        }

        /// <inheritdoc />
        public virtual IController Controller
        {
            get => controller;
            set
            {
                controller = value;
                inputSourceParent = controller.InputSource;
            }
        }

        private IController controller;

        /// <inheritdoc />
        public uint PointerId { get; }

        /// <inheritdoc />
        public bool IsFarInteractor => true;

        /// <inheritdoc />
        public string PointerName { get; set; }

        /// <inheritdoc/>
        public bool IsOverUI { get; } = false;

        /// <inheritdoc/>
        public bool IsInputDown { get; private set; }

        /// <inheritdoc />
        public virtual IInputSource InputSource
        {
            get => inputSourceParent;
            protected set => inputSourceParent = value;
        }

        private IInputSource inputSourceParent;

        /// <inheritdoc />
        public ICursor BaseCursor { get; set; }

        private ICursorModifier cursorModifier = null;

        /// <inheritdoc />
        public ICursorModifier CursorModifier
        {
            get
            {
                if (cursorModifier != null &&
                    cursorModifier.HostTransform != null &&
                    !cursorModifier.HostTransform.gameObject.activeInHierarchy)
                {
                    cursorModifier = null;
                }

                return cursorModifier;
            }
            set => cursorModifier = value;
        }

        /// <inheritdoc/>
        public bool IsTeleportRequestActive { get; set; } = false;

        /// <inheritdoc />
        public bool IsInteractionEnabled { get; protected set; }

        private bool isFocusLocked = false;

        /// <inheritdoc />
        public bool IsFocusLocked
        {
            get
            {
                if (isFocusLocked &&
                    syncedTarget == null)
                {
                    isFocusLocked = false;
                }

                if (syncedTarget != null)
                {
                    if (syncedTarget.activeInHierarchy)
                    {
                        isFocusLocked = true;
                    }
                    else
                    {
                        isFocusLocked = false;
                        syncedTarget = null;
                    }
                }

                return isFocusLocked;
            }
            set
            {
                if (value && syncedTarget == null)
                {
                    if (Result.CurrentTarget != null)
                    {
                        syncedTarget = Result.CurrentTarget;
                    }
                    else
                    {
                        Debug.LogWarning("No Sync Target to lock onto!");
                        return;
                    }
                }

                if (!value && syncedTarget != null)
                {
                    syncedTarget = null;
                }

                isFocusLocked = value;
            }
        }

        /// <inheritdoc />
        public GameObject SyncedTarget
        {
            get => syncedTarget = IsFocusLocked ? syncedTarget : null;
            set
            {
                IsFocusLocked = value != null;
                syncedTarget = value;
            }
        }

        private GameObject syncedTarget = null;

        /// <inheritdoc />
        public Vector3? OverrideGrabPoint { get; set; } = null;

        /// <inheritdoc />
        public virtual float Extent { get; set; } = 10f;

        /// <inheritdoc />
        public virtual float DefaultPointerExtent { get; } = 10f;

        /// <inheritdoc />
        public RayStep[] Rays { get; protected set; } = { new RayStep(Vector3.zero, Vector3.forward) };

        /// <inheritdoc />
        public LayerMask[] PointerRaycastLayerMasksOverride { get; set; } = null;

        /// <inheritdoc />
        public IFocusHandler FocusHandler { get; set; }

        /// <inheritdoc />
        public IInteractorResult Result { get; set; }

        /// <inheritdoc />
        public IBaseRayStabilizer RayStabilizer { get; set; }

        /// <inheritdoc />
        public RaycastMode RaycastMode { get; set; } = RaycastMode.Simple;

        /// <inheritdoc />
        public float SphereCastRadius { get; set; }

        /// <inheritdoc />
        public float PointerOrientation { get; } = 0f;

        /// <inheritdoc />
        public virtual void OnPreRaycast()
        {
            if (TryGetPointingRay(out var pointingRay))
            {
                Rays[0].CopyRay(pointingRay, Extent);
            }

            if (RayStabilizer != null)
            {
                RayStabilizer.UpdateStability(Rays[0].Origin, Rays[0].Direction);
                Rays[0].CopyRay(RayStabilizer.StableRay, Extent);
            }
        }

        /// <inheritdoc />
        public virtual void OnPostRaycast() { }

        /// <inheritdoc />
        public virtual bool TryGetPointerPosition(out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetPointingRay(out Ray pointingRay)
        {
            pointingRay = default;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetPointerRotation(out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return false;
        }

        #region IEquality Implementation

        /// <inheritdoc />
        bool IEqualityComparer.Equals(object left, object right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((IInteractor)obj);
        }

        private bool Equals(IInteractor other)
        {
            return other != null && PointerId == other.PointerId && string.Equals(PointerName, other.PointerName);
        }

        /// <inheritdoc />
        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ (int)PointerId;
                hashCode = (hashCode * 397) ^ (PointerName != null ? PointerName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion IEquality Implementation
    }
}
