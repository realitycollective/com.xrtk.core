using RealityCollective.Utilities.Extensions;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.Interactors;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// The <see cref="LeverBehaviour"/> is as versatile <see cref="IInteractionBehaviour"/> component used to
    /// simulate all kinds of levers the user can interacth with.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/lever-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(LeverBehaviour))]
    public class LeverBehaviour : BaseInteractionBehaviour
    {
        /// <summary>
        /// Supported lever types.
        /// </summary>
        private enum LeverType
        {
            /// <summary>
            /// A lever that is moved.
            /// </summary>
            Translate = 0,
            /// <summary>
            /// A lever that is rotated.
            /// </summary>
            Rotate
        }

        /// <summary>
        /// Supported value mappings.
        /// </summary>
        private enum ValueMapping
        {
            /// <summary>
            /// Value will range from 0 to 1, inclusive respectively.
            /// </summary>
            Value0To1 = 0,
            /// <summary>
            /// Value will range from -1 to 1, inclusive respectively.
            /// </summary>
            ValueNegative1To1
        }

        [Header("Setup")]
        [SerializeField, Tooltip("The axes to translate upon interaction.")]
        private SnapAxis axes = SnapAxis.Z;

        [SerializeField, Tooltip("Should the interactor position or rotation be tracked to determine lever movement?")]
        private InteractorTrackingMode trackingMode = InteractorTrackingMode.Position;

        [SerializeField, Tooltip("A translating is moved upon interaction. A rotating lever stays in place but rotates.")]
        private LeverType leverType = LeverType.Translate;

        [Header("Min / Max Configuration")]
        [SerializeField, Tooltip("The lever's minimum local pose on each axis.")]
        private Vector3 minimumValues = new Vector3(0f, 0f, -.1f);

        [SerializeField, Tooltip("The lever's maximum local pose on each axis.")]
        private Vector3 maximumValues = new Vector3(0f, 0f, .1f);

        [Header("Value")]
        [SerializeField, Tooltip("The value mapping determines how the lever's output value is calculated.")]
        private ValueMapping valueMapping = ValueMapping.Value0To1;

        [SerializeField, Tooltip("If set, the lever will snap into minimum / maximum value once released.")]
        private bool snapsIntoPlace = true;

        [SerializeField, Tooltip("If set, the lever will snap smoothly snap into place instead of instantly.")]
        private bool smoothSnap = true;

        [SerializeField, Tooltip("Determines how fast the lever will snap into place upon release, if smooth snap is enabled.")]
        private float smoothSnapSpeed = 5f;

        [SerializeField, Range(-1f, 1f), Tooltip("The lever's value on the x-axis.")]
        public float valueX = 0f;

        [SerializeField, Range(-1f, 1f), Tooltip("The lever's value on the y-axis.")]
        public float valueY = 0f;

        [SerializeField, Range(-1f, 1f), Tooltip("The lever's value on the z-axis.")]
        public float valueZ = 0f;

        [SerializeField, Space, Tooltip("A normalized value per axis indicating the levers pose.")]
        private UnityEvent<Vector3> valueChanged = null;

        private Transform pivot = null;
        private IControllerInteractor currentInteractor;
        private Vector3 previousInteractorPose;
        private Vector3 ranges;
        private bool isSnapping;
        private Vector3 snapTargetValue;

        /// <summary>
        /// A normalized value per axis indicating the levers pose.
        /// </summary>
        public Vector3 Value
        {
            get => new(valueX, valueY, valueZ);
            set
            {
                valueX = MapValue(value.x);
                valueY = MapValue(value.y);
                valueZ = MapValue(value.z);

                OnValueChanged();

                if (!snapsIntoPlace ||
                    (snapsIntoPlace && (IsAtMinimum() || IsInBetween() || IsAtMaximum())))
                {
                    isSnapping = false;
                    ValueChanged?.Invoke(Value);
                }
            }
        }

        /// <summary>
        /// Should we transform the x-axis on interaction?
        /// </summary>
        protected bool TransformX => (axes & SnapAxis.X) > 0 && ranges.x > 0f;

        /// <summary>
        /// Should we transform the y-axis on interaction?
        /// </summary>
        protected bool TransformY => (axes & SnapAxis.Y) > 0 && ranges.y > 0f;

        /// <summary>
        /// Should we transform the z-axis on interaction?
        /// </summary>
        protected bool TransformZ => (axes & SnapAxis.Z) > 0 && ranges.z > 0f;

        /// <summary>
        /// The lever's <see cref="Value"/> has changed.
        /// </summary>
        public UnityEvent<Vector3> ValueChanged => valueChanged;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            pivot = transform.parent;
            if (pivot.IsNull())
            {
                Debug.LogError($"The {GetType().Name} requires the lever to be parented.");
            }

            ranges = new(
                Mathf.Abs(maximumValues.x - minimumValues.x),
                Mathf.Abs(maximumValues.y - minimumValues.y),
                Mathf.Abs(maximumValues.z - minimumValues.z));

            RemapValue();
            UpdateLeverPose();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateLeverPose();
            }
#endif

            if (currentInteractor == null)
            {
                if (isSnapping)
                {
                    SnapIntoPlace();
                }

                return;
            }

            Vector3 currentInteractorPose;
            Vector3 leverPose;

            if (trackingMode == InteractorTrackingMode.Position)
            {
                currentInteractorPose = GetInteractorPosition();
                leverPose = GetLeverPoseFromTrackedPosition(currentInteractorPose);
            }
            else
            {
                currentInteractorPose = GetInteractorRotation();
                leverPose = GetLeverPoseFromTrackedRotation(currentInteractorPose);
            }

            UpdateLeverPose(leverPose);
            UpdateValue();

            previousInteractorPose = currentInteractorPose;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected override void OnValidate() => OnEnable();

        private Vector3 GetLeverPoseFromTrackedPosition(Vector3 currentInteractorPose)
        {
            Vector3 leverPose;

            if (leverType == LeverType.Translate)
            {
                var interactorPositionDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorPositionDelta.x += currentInteractorPose.x - previousInteractorPose.x;
                }

                if (TransformY)
                {
                    interactorPositionDelta.y += currentInteractorPose.y - previousInteractorPose.y;
                }

                if (TransformZ)
                {
                    interactorPositionDelta.z += currentInteractorPose.z - previousInteractorPose.z;
                }

                leverPose = transform.localPosition + interactorPositionDelta;
            }
            else
            {
                var direction = currentInteractorPose;
                var interactorRotationDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorRotationDelta.x = Vector3.SignedAngle(previousInteractorPose, direction, pivot.right);
                }

                if (TransformY)
                {
                    interactorRotationDelta.y = Vector3.SignedAngle(previousInteractorPose, direction, pivot.up);
                }

                if (TransformZ)
                {
                    interactorRotationDelta.z = Vector3.SignedAngle(previousInteractorPose, direction, pivot.forward);
                }

                leverPose = (Quaternion.Euler(interactorRotationDelta) * transform.localRotation).eulerAngles;
            }

            return leverPose;
        }

        private Vector3 GetLeverPoseFromTrackedRotation(Vector3 currentInteractorPose)
        {
            Vector3 leverPose;

            if (leverType == LeverType.Translate)
            {
                var interactorPositionDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorPositionDelta.x += currentInteractorPose.x - previousInteractorPose.x;
                }

                if (TransformY)
                {
                    interactorPositionDelta.y += currentInteractorPose.y - previousInteractorPose.y;
                }

                if (TransformZ)
                {
                    interactorPositionDelta.z += currentInteractorPose.z - previousInteractorPose.z;
                }

                leverPose = transform.localPosition + interactorPositionDelta;
            }
            else
            {
                var interactorRotationDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorRotationDelta.x = currentInteractorPose.x;
                }

                if (TransformY)
                {
                    interactorRotationDelta.y = currentInteractorPose.y;
                }

                if (TransformZ)
                {
                    interactorRotationDelta.z = currentInteractorPose.z;
                }

                leverPose = interactorRotationDelta;
            }

            return leverPose;
        }

        /// <summary>
        /// Sets the lever's <see cref="Value"/> without raising the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="value">The lever's new value.</param>
        public void SetValueWithoutNotify(Vector3 value)
        {
            valueX = MapValue(value.x);
            valueY = MapValue(value.y);
            valueZ = MapValue(value.z);

            UpdateLeverPose();
            OnValueChanged();
        }

        /// <inheritdoc/>
        protected override void OnFirstGrabEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is not IControllerInteractor controllerInteractor)
            {
                return;
            }

            isSnapping = false;
            currentInteractor = controllerInteractor;
            previousInteractorPose = trackingMode == InteractorTrackingMode.Position ? GetInteractorPosition() : GetInteractorRotation();
        }

        /// <inheritdoc/>
        protected override void OnLastGrabExited(InteractionExitEventArgs eventArgs)
        {
            currentInteractor = null;

            if (snapsIntoPlace)
            {
                StartSnapIntoPlace();
            }
        }

        /// <summary>
        /// Updates the lever pose to <paramref name="leverPose"/>.
        /// </summary>
        /// <param name="leverPose">The updated lever position or rotation, depending on the <see cref="leverType"/>.</param>
        private void UpdateLeverPose(Vector3 leverPose)
        {
            if (leverType == LeverType.Translate)
            {
                leverPose.x = Mathf.Clamp(leverPose.x, minimumValues.x, maximumValues.x);
                leverPose.y = Mathf.Clamp(leverPose.y, minimumValues.y, maximumValues.y);
                leverPose.z = Mathf.Clamp(leverPose.z, minimumValues.z, maximumValues.z);
                transform.SetLocalPositionAndRotation(leverPose, Quaternion.identity);
                return;
            }

            leverPose.x = ClampAngle(leverPose.x, minimumValues.x, maximumValues.x);
            leverPose.y = ClampAngle(leverPose.y, minimumValues.y, maximumValues.y);
            leverPose.z = ClampAngle(leverPose.z, minimumValues.z, maximumValues.z);

            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(leverPose));
        }

        /// <summary>
        /// Updates the lever pose based on <see cref="Value"/>.
        /// </summary>
        private void UpdateLeverPose()
        {
            RemapValue();

            Vector3 leverPose;

            if (valueMapping == ValueMapping.Value0To1)
            {
                leverPose = new Vector3(
                    minimumValues.x + (minimumValues.x > maximumValues.x ? -1f : 1f) * Value.x * ranges.x,
                    minimumValues.y + (minimumValues.y > maximumValues.y ? -1f : 1f) * Value.y * ranges.y,
                    minimumValues.z + (minimumValues.z > maximumValues.z ? -1f : 1f) * Value.z * ranges.z);
            }
            else
            {
                leverPose = new Vector3(
                Value.x * ranges.x / 2f,
                Value.y * ranges.y / 2f,
                Value.z * ranges.z / 2f);
            }

            UpdateLeverPose(leverPose);
        }

        /// <summary>
        /// Updates <see cref="Value"/> based on <paramref name="leverPose"/>.
        /// </summary>
        private void UpdateValue()
        {
            var leverPose = leverType == LeverType.Translate ? transform.localPosition : transform.localEulerAngles;

            if (valueMapping == ValueMapping.Value0To1)
            {
                Value = new(
                    TransformX ? leverPose.x / ranges.x + .5f : 0f,
                    TransformY ? leverPose.y / ranges.y + .5f : 0f,
                    TransformZ ? leverPose.z / ranges.z + .5f : 0f);

                return;
            }

            Value = new(
                 TransformX ? leverPose.x / ranges.x * 2f : 0f,
                 TransformY ? leverPose.y / ranges.y * 2f : 0f,
                 TransformZ ? leverPose.z / ranges.z * 2f : 0f);
        }

        /// <summary>
        /// The <see cref="Value"/> has changed.
        /// </summary>
        protected virtual void OnValueChanged() { }

        #region Snap Into Place

        private void StartSnapIntoPlace()
        {
            var halfway = valueMapping == ValueMapping.Value0To1 ? .5f : 0f;
            Vector3 snapTargetValue;

            if (valueMapping == ValueMapping.Value0To1)
            {
                snapTargetValue = new Vector3(
                    TransformX ? (Value.x <= halfway ? 0f : 1f) : Value.x,
                    TransformY ? (Value.y <= halfway ? 0f : 1f) : Value.y,
                    TransformZ ? (Value.z <= halfway ? 0f : 1f) : Value.z);
            }
            else
            {
                const float tolerance = .01f;
                snapTargetValue = new Vector3(
                        TransformX ? (Value.x.Approximately(halfway, tolerance) ? halfway : (Value.x < halfway ? -1f : 1f)) : Value.x,
                        TransformY ? (Value.y.Approximately(halfway, tolerance) ? halfway : (Value.y < halfway ? -1f : 1f)) : Value.y,
                        TransformZ ? (Value.z.Approximately(halfway, tolerance) ? halfway : (Value.z < halfway ? -1f : 1f)) : Value.z);
            }

            if (smoothSnap)
            {
                isSnapping = true;
                this.snapTargetValue = snapTargetValue;
            }
            else
            {
                Value = snapTargetValue;
                UpdateLeverPose();
            }
        }

        private void SnapIntoPlace()
        {
            Value = Vector3.MoveTowards(Value, snapTargetValue, smoothSnapSpeed * Time.deltaTime);
            UpdateLeverPose();
        }

        #endregion Snap Into Place

        #region Utilities

        private Vector3 GetInteractorPosition() => pivot.InverseTransformPoint(currentInteractor.GameObject.transform.position);

        private Vector3 GetInteractorRotation() => (Quaternion.Inverse(pivot.rotation) * currentInteractor.GameObject.transform.rotation).eulerAngles;

        private bool IsAtMinimum()
        {
            var minimumValue = valueMapping == ValueMapping.Value0To1 ? 0 : -1;

            if (TransformX && Value.x != minimumValue)
            {
                return false;
            }

            if (TransformY && Value.y != minimumValue)
            {
                return false;
            }

            if (TransformZ && Value.z != minimumValue)
            {
                return false;
            }

            return true;
        }

        private bool IsInBetween()
        {
            if (valueMapping == ValueMapping.Value0To1)
            {
                return false;
            }

            const float halfway = 0f;
            const float tolerance = .01f;

            if (TransformX && !Value.x.Approximately(halfway, tolerance))
            {
                return false;
            }

            if (TransformY && !Value.y.Approximately(halfway, tolerance))
            {
                return false;
            }

            if (TransformZ && !Value.z.Approximately(halfway, tolerance))
            {
                return false;
            }

            return true;
        }

        private bool IsAtMaximum()
        {
            const float maximumValue = 1f;

            if (TransformX && Value.x != maximumValue)
            {
                return false;
            }

            if (TransformY && Value.y != maximumValue)
            {
                return false;
            }

            if (TransformZ && Value.z != maximumValue)
            {
                return false;
            }

            return true;
        }

        private void RemapValue()
        {
            Value = new(
                TransformX ? Value.x : 0f,
                TransformY ? Value.y : 0f,
                TransformZ ? Value.z : 0f);
        }

        private float MapValue(float value)
        {
            if (valueMapping == ValueMapping.Value0To1)
            {
                return Mathf.Clamp01(value);
            }
            else if (value >= -1 && value <= 1f)
            {
                return value;
            }

            return Mathf.Clamp((value * 2) - 1, -1f, 1f);
        }

        private float ClampAngle(float angle, float from, float to)
        {
            if (from > to)
            {
                var temp = to;
                to = from;
                from = temp;
            }

            if (angle < 0f)
            {
                angle = 360 + angle;
            }

            if (angle > 180f)
            {
                return Mathf.Max(angle, 360 + from);
            }

            return Mathf.Min(angle, to);
        }

        #endregion Utilities
    }
}
