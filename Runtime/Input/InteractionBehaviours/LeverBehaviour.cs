using RealityCollective.Extensions;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.InteractionBehaviours;
using RealityToolkit.Input.Interactors;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Core.Samples.Interactions
{
    /// <summary>
    /// The <see cref="LeverBehaviour"/> is as versatile <see cref="IInteractionBehaviour"/> component used to
    /// simulate all kinds of levers the user can interacth with in a virtual world.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/lever-behaviour")]
    public class LeverBehaviour : BaseInteractionBehaviour
    {
        /// <summary>
        /// Supported lever types.
        /// </summary>
        private enum LeverType
        {
            Translate,
            Rotate
        }

        /// <summary>
        /// Supported value mappings.
        /// </summary>
        private enum ValueMapping
        {
            [Description("[0, 1]")]
            Value0To1,
            [Description("[-1, 1]")]
            ValueNegative1To1
        }

        [Header("Setup")]
        [SerializeField, Tooltip("The axes to translate upon interaction.")]
        private SnapAxis axes = SnapAxis.Z;

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
        private Vector3 previousInteractorPosition;
        private Vector3 ranges;

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
                ValueChanged?.Invoke(Value);
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
                return;
            }

            var currentInteractorPosition = GetInteractorPosition();
            Vector3 leverPose = Vector3.zero;

            if (leverType == LeverType.Translate)
            {
                var interactorPositionDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorPositionDelta.x += currentInteractorPosition.x - previousInteractorPosition.x;
                }

                if (TransformY)
                {
                    interactorPositionDelta.y += currentInteractorPosition.y - previousInteractorPosition.y;
                }

                if (TransformZ)
                {
                    interactorPositionDelta.z += currentInteractorPosition.z - previousInteractorPosition.z;
                }

                leverPose = transform.localPosition + interactorPositionDelta;
            }
            else
            {
                var direction = currentInteractorPosition;
                var interactorRotationDelta = Vector3.zero;

                if (TransformX)
                {
                    interactorRotationDelta.x = Vector3.SignedAngle(previousInteractorPosition, direction, Vector3.right);
                }

                if (TransformY)
                {
                    interactorRotationDelta.y = Vector3.SignedAngle(previousInteractorPosition, direction, Vector3.up);
                }

                if (TransformZ)
                {
                    interactorRotationDelta.z = Vector3.SignedAngle(previousInteractorPosition, direction, Vector3.forward);
                }

                leverPose = interactorRotationDelta;
            }

            leverPose.x = Mathf.Clamp(leverPose.x, minimumValues.x, maximumValues.x);
            leverPose.y = Mathf.Clamp(leverPose.y, minimumValues.y, maximumValues.y);
            leverPose.z = Mathf.Clamp(leverPose.z, minimumValues.z, maximumValues.z);

            UpdateLeverPose(leverPose);
            UpdateValue(leverPose);

            previousInteractorPosition = currentInteractorPosition;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected override void OnValidate() => OnEnable();

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

            currentInteractor = controllerInteractor;
            previousInteractorPosition = GetInteractorPosition();
        }

        /// <inheritdoc/>
        protected override void OnLastGrabExited(InteractionExitEventArgs eventArgs)
        {
            currentInteractor = null;
        }

        /// <summary>
        /// Updates the lever pose to <paramref name="leverPose"/>.
        /// </summary>
        /// <param name="leverPose">The updated lever position or rotation, depending on the <see cref="leverType"/>.</param>
        private void UpdateLeverPose(Vector3 leverPose)
        {
            if (leverType == LeverType.Translate)
            {
                transform.SetLocalPositionAndRotation(leverPose, Quaternion.identity);
                return;
            }

            leverPose.x = WrapAngle(leverPose.x);
            leverPose.y = WrapAngle(leverPose.y);
            leverPose.z = WrapAngle(leverPose.z);

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
                if (leverType == LeverType.Translate)
                {
                    leverPose = new Vector3(
                        minimumValues.x + Value.x * ranges.x,
                        minimumValues.y + Value.y * ranges.y,
                        minimumValues.z + Value.z * ranges.z);
                }
                else
                {
                    leverPose = new Vector3(
                        Value.x * ranges.x,
                        Value.y * ranges.y,
                        Value.z * ranges.z);
                }
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
        /// <param name="leverPose">The updated lever position or rotation, depending on the <see cref="leverType"/>.</param>
        private void UpdateValue(Vector3 leverPose)
        {
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

        private Vector3 GetInteractorPosition() => pivot.InverseTransformPoint(currentInteractor.GameObject.transform.position);

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

        private static float WrapAngle(float angle)
        {
            angle %= 360;

            if (angle > 180)
            {
                return angle - 360;
            }

            return angle;
        }
    }
}
