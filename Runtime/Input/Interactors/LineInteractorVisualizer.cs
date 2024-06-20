// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Utilities.Lines.DataProviders;
using RealityToolkit.Utilities.Lines.Renderers;
using UnityEngine;

namespace RealityToolkit.Input.Interactors
{
    public class LineInteractorVisualizer : BaseInteractorVisualizer
    {
        [SerializeField]
        private BaseLineDataProvider lineBase = null;

        [SerializeField]
        [Tooltip("If no line renderers are specified, this array will be auto-populated on startup.")]
        private BaseLineRenderer[] lineRenderers = null;

        [SerializeField]
        private Gradient defaultLineColor = new Gradient();

        public Gradient DefaultLineColor
        {
            get => defaultLineColor;
            set => defaultLineColor = value;
        }

        [SerializeField]
        private Gradient lineColorInputDown = new Gradient();

        public Gradient LineColorInputDown
        {
            get => lineColorInputDown;
            set => lineColorInputDown = value;
        }

        private FarInteractor farInteractor;
        /// <inheritdoc />
        public override IInteractor Interactor
        {
            get => farInteractor;
            set
            {
                if (value is FarInteractor farInteractor)
                {
                    this.farInteractor = farInteractor;
                    return;
                }

                Debug.LogError($"{nameof(LineInteractorVisualizer)} is only meant to be used with {nameof(FarInteractor)}s.");
            }
        }

        protected virtual void OnValidate()
        {
            CheckInitialization();
        }

        protected virtual void OnEnable()
        {
            CheckInitialization();
        }

        private void CheckInitialization()
        {
            if (lineBase.IsNull())
            {
                lineBase = GetComponent<BaseLineDataProvider>();
            }

            if (lineBase.IsNull())
            {
                Debug.LogError($"No {nameof(BaseLineDataProvider)} found on {gameObject.name}.");
            }

            if (lineBase.IsNotNull() && (lineRenderers == null || lineRenderers.Length == 0))
            {
                lineRenderers = lineBase.GetComponentsInChildren<BaseLineRenderer>();
            }

            if (lineRenderers == null || lineRenderers.Length == 0)
            {
                Debug.LogError($"No {nameof(BaseLineRenderer)}s found on {gameObject.name}.");
            }
        }

        /// <inheritdoc />
        public override void OnPreRaycast()
        {
            Debug.Assert(lineBase.IsNotNull());

            lineBase.UpdateMatrix();

            if (Interactor == null || !Interactor.IsInteractionEnabled)
            {
                lineBase.enabled = false;
                return;
            }

            if (Interactor.RayStabilizer != null)
            {
                Interactor.RayStabilizer.UpdateStability(Interactor.Rays[0].Origin, Interactor.Rays[0].Direction);
                Interactor.Rays[0].CopyRay(Interactor.RayStabilizer.StableRay, Interactor.Extent);
            }

            Interactor.TryGetPointerPosition(out var pointerPosition);
            Interactor.TryGetPointerRotation(out var pointerRotation);

            // Set our first and last points
            lineBase.FirstPoint = pointerPosition;

            if (Interactor.IsFocusLocked && Interactor.Result.CurrentTarget != null)
            {
                if (Interactor.SyncedTarget != null)
                {
                    // Now raycast out like nothing happened so we can get an updated pointer position.
                    lineBase.LastPoint = pointerPosition + pointerRotation * (Vector3.forward * Interactor.Extent);
                }
                else
                {
                    // Set the line to the locked position.
                    lineBase.LastPoint = Interactor.Result.EndPoint;
                }
            }
            else
            {
                lineBase.LastPoint = pointerPosition + pointerRotation * (Vector3.forward * Interactor.Extent);
            }

            var stepSize = 1f / Interactor.Rays.Length;
            var lastPoint = lineBase.GetUnClampedPoint(0f);

            for (int i = 0; i < Interactor.Rays.Length; i++)
            {
                var currentPoint = lineBase.GetUnClampedPoint(stepSize * (i + 1));
                Interactor.Rays[i].UpdateRayStep(ref lastPoint, ref currentPoint);
                lastPoint = currentPoint;
            }
        }

        /// <inheritdoc />
        public override void OnPostRaycast()
        {
            if (!Interactor.IsInteractionEnabled)
            {
                lineBase.enabled = false;
                return;
            }

            lineBase.enabled = true;

            Gradient lineColor;

            lineColor = Interactor.IsInputDown ? LineColorInputDown : DefaultLineColor;
            var maxClampLineSteps = farInteractor.LineCastResolution;

            // Used to ensure the line doesn't extend beyond the cursor
            float cursorOffsetWorldLength = Interactor.BaseCursor?.SurfaceCursorDistance ?? 0f;

            // The distance the ray travels through the world before it hits something.
            // Measured in world-units (as opposed to normalized distance).
            var clearWorldLength = (Interactor.Result?.CurrentTarget != null) ? Interactor.Result.RayDistance : Interactor.Extent;

            for (var i = 0; i < lineRenderers.Length; i++)
            {
                var lineRenderer = lineRenderers[i];
                // Renderers are enabled by default if line is enabled
                lineRenderer.enabled = true;
                maxClampLineSteps = Mathf.Max(maxClampLineSteps, lineRenderer.LineStepCount);
                lineRenderer.LineColor = lineColor;
            }

            // If focus is locked, we're sticking to the target
            // So don't clamp the world length
            if (Interactor.IsFocusLocked && Interactor.Result.CurrentTarget != null)
            {
                if (Interactor.SyncedTarget != null)
                {
                    if (Interactor.Result.GrabPoint == Vector3.zero)
                    {
                        lineBase.LastPoint = Interactor.Result.EndPoint;
                    }
                    else
                    {
                        lineBase.LastPoint = Interactor.Result.GrabPoint;
                    }
                }

                var cursorOffsetLocalLength = lineBase.GetNormalizedLengthFromWorldLength(cursorOffsetWorldLength);
                lineBase.LineEndClamp = 1 - cursorOffsetLocalLength;
            }
            else
            {
                // Otherwise clamp the line end by the clear distance
                float clearLocalLength = lineBase.GetNormalizedLengthFromWorldLength(clearWorldLength - cursorOffsetWorldLength, maxClampLineSteps);
                lineBase.LineEndClamp = clearLocalLength;
            }
        }
    }
}
