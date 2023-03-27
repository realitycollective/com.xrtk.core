// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.CameraService.Interfaces;
using RealityToolkit.Definitions.Physics;
using RealityToolkit.EventDatum.Input;
using RealityToolkit.InputSystem.Definitions;
using RealityToolkit.InputSystem.Interfaces;
using RealityToolkit.InputSystem.Interfaces.Controllers;
using RealityToolkit.InputSystem.Interfaces.Handlers;
using RealityToolkit.Interfaces.Physics;
using RealityToolkit.Services.InputSystem.Utilities;
using RealityToolkit.Utilities.Physics;
using System;
using System.Collections;
using UnityEngine;

namespace RealityToolkit.Utilities.UX.Pointers
{
    /// <summary>
    /// Base Pointer class for pointers that exist in the scene as GameObjects.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class BaseControllerPointer : ControllerPoseSynchronizer, IMixedRealityPointer
    {
        [SerializeField]
        private GameObject cursorPrefab = null;

        [SerializeField]
        private bool disableCursorOnStart = false;

        [SerializeField]
        private LayerMask uiLayerMask = -1;

        protected bool DisableCursorOnStart => disableCursorOnStart;

        [SerializeField]
        private bool setCursorVisibilityOnSourceDetected = false;

        private GameObject cursorInstance = null;

        [SerializeField]
        [Tooltip("Source transform for raycast origin - leave null to use default transform")]
        private Transform raycastOrigin = null;

        /// <summary>
        /// Source <see cref="Transform"/> for the raycast origin.
        /// </summary>
        public Transform RaycastOrigin
        {
            get => raycastOrigin == null ? transform : raycastOrigin;
            protected set => raycastOrigin = value;
        }

        [SerializeField]
        [Tooltip("The hold action that will enable the raise the input event for this pointer.")]
        private MixedRealityInputAction activeHoldAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("The action that will enable the raise the input event for this pointer.")]
        private MixedRealityInputAction pointerAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("The action that will enable the raise the input grab event for this pointer.")]
        protected MixedRealityInputAction grabAction = MixedRealityInputAction.None;

        /// <summary>
        /// True if grab is pressed right now
        /// </summary>
        protected bool IsGrabPressed = false;

        /// <summary>
        /// True if grab is pressed right now
        /// </summary>
        protected bool IsDragging = false;

        [SerializeField]
        [Tooltip("Does the interaction require hold?")]
        private bool requiresHoldAction = false;

        [SerializeField]
        [Tooltip("Enables the pointer ray when the pointer is started.")]
        private bool enablePointerOnStart = false;

        /// <summary>
        /// True if select is pressed right now
        /// </summary>
        protected bool IsSelectPressed { get; set; } = false;

        /// <summary>
        /// True if select has been pressed once since this component was enabled
        /// </summary>
        protected bool HasSelectPressedOnce { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this pointer is currently pressed and hold.
        /// </summary>
        protected bool IsHoldPressed { get; set; } = false;

        /// <inheritdoc/>
        public bool IsOverUI => Result != null &&
                    Result.CurrentPointerTarget.IsNotNull() &&
                    uiLayerMask == (uiLayerMask | (1 << Result.CurrentPointerTarget.layer));

        /// <inheritdoc/>
        public bool IsTeleportRequestActive { get; set; } = false;

        /// <summary>
        /// Gets the currently captured near interaction object. Only applicable
        /// if <see cref="InteractionMode.Both"/> or <see cref="InteractionMode.Near"/>.
        /// </summary>
        protected GameObject CapturedNearInteractionObject { get; private set; } = null;

        /// <summary>
        /// The forward direction of the targeting ray
        /// </summary>
        public virtual Vector3 PointerDirection => raycastOrigin != null ? raycastOrigin.forward : transform.forward;

        /// <summary>
        /// Set a new cursor for this <see cref="IMixedRealityPointer"/>
        /// </summary>
        /// <remarks>This <see cref="GameObject"/> must have a <see cref="IMixedRealityCursor"/> attached to it.</remarks>
        /// <param name="newCursor">The new cursor</param>
        public virtual void SetCursor(GameObject newCursor = null)
        {
            if (cursorInstance != null)
            {
                cursorInstance.Destroy();
                cursorInstance = newCursor;
            }

            if (cursorInstance == null && cursorPrefab != null)
            {
                cursorInstance = Instantiate(cursorPrefab, transform);
            }

            if (cursorInstance != null)
            {
                cursorInstance.name = $"{Handedness}_{name}_Cursor";
                BaseCursor = cursorInstance.GetComponent<IMixedRealityCursor>();

                if (BaseCursor != null)
                {
                    BaseCursor.Pointer = this;
                    BaseCursor.SetVisibilityOnSourceDetected = setCursorVisibilityOnSourceDetected;
                    BaseCursor.SetVisibility(!disableCursorOnStart);
                }
                else
                {
                    Debug.LogError($"No IMixedRealityCursor component found on {cursorInstance.name}", this);
                }
            }
        }

        private ICameraService cameraSystem = null;

        protected ICameraService CameraSystem
            => cameraSystem ?? (cameraSystem = ServiceManager.Instance.GetService<ICameraService>());

        private Vector3 lastPointerPosition = Vector3.zero;

        #region MonoBehaviour Implementation

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            SetCursor();
        }

        /// <inheritdoc/>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (InteractionMode.HasFlags(InteractionMode.Near) &&
                nearInteractionCollider != null &&
                other.IsValidCollision(PointerRaycastLayerMasksOverride ?? InputSystem.FocusProvider.GlobalPointerRaycastLayerMasks))
            {
                CapturedNearInteractionObject = other.gameObject;

                // Force update the focus provider so the focused target
                // gets updated before raising the event. If we don't update
                // the focus provider here, the event will not be raised on the
                // capture near interaction object.
                InputSystem.FocusProvider.Update();
                InputSystem.RaiseOnInputDown(InputSourceParent, Handedness, pointerAction);
            }
        }

        /// <inheritdoc/>
        protected virtual void OnTriggerStay(Collider other)
        {
            if (InteractionMode.HasFlags(InteractionMode.Near) &&
                nearInteractionCollider != null &&
                CapturedNearInteractionObject == other.gameObject)
            {
                // Force update the focus provider so the focused target
                // gets updated before raising the event. If we don't update
                // the focus provider here, the event will not be raised on the
                // capture near interaction object.
                InputSystem.FocusProvider.Update();
                InputSystem.RaiseOnInputPressed(InputSourceParent, Handedness, pointerAction);
            }
        }

        /// <inheritdoc/>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (InteractionMode.HasFlags(InteractionMode.Near) &&
                nearInteractionCollider != null &&
                CapturedNearInteractionObject == other.gameObject)
            {
                // Force update the focus provider so the focused target
                // gets updated before raising the event. If we don't update
                // the focus provider here, the event will not be raised on the
                // capture near interaction object.
                InputSystem.FocusProvider.Update();

                InputSystem.RaiseOnInputUp(InputSourceParent, Handedness, pointerAction);
                CapturedNearInteractionObject = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            if (IsSelectPressed || IsGrabPressed)
            {
                InputSystem.RaisePointerUp(this, pointerAction);
            }

            base.OnDisable();

            IsHoldPressed = false;
            IsSelectPressed = false;
            IsGrabPressed = false;
            HasSelectPressedOnce = false;
            BaseCursor?.SetVisibility(false);
        }

        #endregion MonoBehaviour Implementation

        #region IMixedRealityPointer Implementation

        /// <inheritdoc cref="IMixedRealityController" />
        public override IMixedRealityController Controller
        {
            get => base.Controller;
            set
            {
                base.Controller = value;
                InputSourceParent = base.Controller.InputSource;
            }
        }

        private uint pointerId;

        /// <inheritdoc />
        public uint PointerId
        {
            get
            {
                if (pointerId == 0)
                {
                    pointerId = InputSystem.FocusProvider.GenerateNewPointerId();
                }

                return pointerId;
            }
        }

        /// <inheritdoc />
        public string PointerName
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        /// <inheritdoc />
        public IMixedRealityInputSource InputSourceParent { get; protected set; }

        /// <inheritdoc />
        public IMixedRealityCursor BaseCursor { get; set; }

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

        [SerializeField]
        private InteractionMode interactionMode = InteractionMode.Both;

        /// <inheritdoc />
        public virtual InteractionMode InteractionMode => interactionMode;

        [SerializeField]
        private Collider nearInteractionCollider = null;

        /// <inheritdoc />
        public Collider NearInteractionCollider
        {
            get => nearInteractionCollider;
            protected set => nearInteractionCollider = value;
        }

        /// <inheritdoc />
        public virtual bool IsInteractionEnabled
        {
            get
            {
                if (IsTeleportRequestActive)
                {
                    return false;
                }

                if (requiresHoldAction && IsHoldPressed)
                {
                    return true;
                }

                if (IsSelectPressed || IsGrabPressed)
                {
                    return true;
                }

                return HasSelectPressedOnce || enablePointerOnStart;
            }
        }

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
                    if (Result.CurrentPointerTarget != null)
                    {
                        syncedTarget = Result.CurrentPointerTarget;
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
                isFocusLocked = value != null;
                syncedTarget = value;
            }
        }

        private GameObject syncedTarget = null;

        /// <inheritdoc />
        public Vector3? OverrideGrabPoint { get; set; } = null;

        [SerializeField]
        private bool overrideGlobalPointerExtent = false;

        [NonSerialized]
        private float pointerExtent;

        /// <inheritdoc />
        public float PointerExtent
        {
            get
            {
                if (overrideGlobalPointerExtent)
                {
                    if (InputSystem?.FocusProvider != null)
                    {
                        return InputSystem.FocusProvider.GlobalPointingExtent;
                    }
                }

                if (pointerExtent.Equals(0f))
                {
                    pointerExtent = defaultPointerExtent;
                }

                Debug.Assert(pointerExtent > 0f);
                return pointerExtent;
            }
            set
            {
                pointerExtent = value;
                Debug.Assert(pointerExtent > 0f, "Cannot set the pointer extent to 0. Resetting to the default pointer extent");
                overrideGlobalPointerExtent = false;
            }
        }

        [Min(0.01f)]
        [SerializeField]
        private float defaultPointerExtent = 10f;

        /// <inheritdoc />
        public float DefaultPointerExtent => defaultPointerExtent;

        /// <inheritdoc />
        public RayStep[] Rays { get; protected set; } = { new RayStep(Vector3.zero, Vector3.forward) };

        /// <inheritdoc />
        public LayerMask[] PointerRaycastLayerMasksOverride { get; set; } = null;

        /// <inheritdoc />
        public IMixedRealityFocusHandler FocusHandler { get; set; }

        /// <inheritdoc />
        public IMixedRealityInputHandler InputHandler { get; set; }

        /// <inheritdoc />
        public IPointerResult Result { get; set; }

        /// <inheritdoc />
        public IBaseRayStabilizer RayStabilizer { get; set; } = new GenericStabilizer();

        /// <inheritdoc />
        public RaycastMode RaycastMode { get; set; } = RaycastMode.Simple;

        /// <inheritdoc />
        public float SphereCastRadius { get; set; } = 0.1f;

        [SerializeField]
        [Range(0f, 360f)]
        [Tooltip("The Y orientation of the pointer - used for rotation and navigation")]
        private float pointerOrientation = 0f;

        /// <inheritdoc />
        public virtual float PointerOrientation
        {
            get => pointerOrientation + (raycastOrigin != null ? raycastOrigin.eulerAngles.y : transform.eulerAngles.y);
            set => pointerOrientation = value < 0
                        ? Mathf.Clamp(value, -360f, 0f)
                        : Mathf.Clamp(value, 0f, 360f);
        }

        /// <inheritdoc />
        public virtual void OnPreRaycast() { }

        /// <inheritdoc />
        public virtual void OnPostRaycast()
        {
            if (grabAction != MixedRealityInputAction.None)
            {
                if (IsGrabPressed)
                {
                    DragHandler(grabAction);
                }
            }
            else
            {
                if (IsSelectPressed)
                {
                    DragHandler(pointerAction);

                }
            }
        }

        private void DragHandler(MixedRealityInputAction action)
        {
            if (IsDragging)
            {
                var currentPointerPosition = PointerPosition;
                var delta = currentPointerPosition - lastPointerPosition;
                InputSystem.RaisePointerDrag(this, action, delta);
                lastPointerPosition = currentPointerPosition;
            }
            else
            {
                IsDragging = true;
                var currentPointerPosition = PointerPosition;
                InputSystem.RaisePointerDragBegin(this, action, currentPointerPosition);
                lastPointerPosition = currentPointerPosition;
            }
        }

        /// <inheritdoc />
        public virtual bool TryGetPointerPosition(out Vector3 position)
        {
            position = raycastOrigin != null ? raycastOrigin.position : transform.position;
            return true;
        }

        private Vector3 PointerPosition
        {
            get
            {
                if (TryGetPointerPosition(out var pos))
                {
                    return pos;
                }
                return Vector3.zero;
            }
        }

        /// <inheritdoc />
        public virtual bool TryGetPointingRay(out Ray pointingRay)
        {
            TryGetPointerPosition(out var pointerPosition);
            pointingRay = pointerRay;
            pointingRay.origin = pointerPosition;
            pointingRay.direction = PointerDirection;
            return true;
        }

        private readonly Ray pointerRay = new Ray();

        /// <inheritdoc />
        public virtual bool TryGetPointerRotation(out Quaternion rotation)
        {
            var pointerRotation = raycastOrigin != null ? raycastOrigin.eulerAngles : transform.eulerAngles;
            rotation = Quaternion.Euler(pointerRotation.x, PointerOrientation, pointerRotation.z);
            return true;
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
            if (this == null) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((IMixedRealityPointer)obj);
        }

        private bool Equals(IMixedRealityPointer other)
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

        #endregion IMixedRealityPointer Implementation

        #region IMixedRealitySourcePoseHandler Implementation

        /// <inheritdoc />
        public override void OnSourceLost(SourceStateEventData eventData)
        {
            base.OnSourceLost(eventData);

            if (eventData.SourceId == InputSourceParent.SourceId &&
                interactionMode.HasFlags(InteractionMode.Far))
            {
                if (requiresHoldAction)
                {
                    IsHoldPressed = false;
                }

                if (IsSelectPressed)
                {
                    InputSystem.RaisePointerUp(this, pointerAction);
                }

                if (IsGrabPressed)
                {
                    InputSystem.RaisePointerUp(this, grabAction);
                }

                IsSelectPressed = false;
                IsGrabPressed = false;
            }
        }

        #endregion IMixedRealitySourcePoseHandler Implementation

        #region IMixedRealityInputHandler Implementation

        /// <inheritdoc />
        public override void OnInputUp(InputEventData eventData)
        {
            base.OnInputUp(eventData);

            if (eventData.SourceId == InputSourceParent.SourceId &&
                interactionMode.HasFlags(InteractionMode.Far))
            {
                if (requiresHoldAction && eventData.MixedRealityInputAction == activeHoldAction)
                {
                    IsHoldPressed = false;
                }

                if (grabAction != MixedRealityInputAction.None &&
                    eventData.MixedRealityInputAction == grabAction)
                {
                    IsGrabPressed = false;

                    InputSystem.RaisePointerClicked(this, grabAction);
                    InputSystem.RaisePointerUp(this, grabAction);
                }

                if (eventData.MixedRealityInputAction == pointerAction)
                {
                    IsSelectPressed = false;
                    InputSystem.RaisePointerClicked(this, pointerAction);
                    InputSystem.RaisePointerUp(this, pointerAction);
                }
            }
        }

        /// <inheritdoc />
        public override void OnInputDown(InputEventData eventData)
        {
            base.OnInputDown(eventData);

            if (eventData.SourceId == InputSourceParent.SourceId &&
                interactionMode.HasFlags(InteractionMode.Far))
            {
                if (requiresHoldAction && eventData.MixedRealityInputAction == activeHoldAction)
                {
                    IsHoldPressed = true;
                }

                if (grabAction != MixedRealityInputAction.None &&
                    eventData.MixedRealityInputAction == grabAction)
                {
                    IsGrabPressed = true;

                    InputSystem.RaisePointerDown(this, grabAction);
                }

                if (eventData.MixedRealityInputAction == pointerAction)
                {
                    IsSelectPressed = true;
                    HasSelectPressedOnce = true;
                    InputSystem.RaisePointerDown(this, pointerAction);
                }
            }
        }

        #endregion  IMixedRealityInputHandler Implementation
    }
}