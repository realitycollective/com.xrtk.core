using RealityCollective.Extensions;
using RealityToolkit.Input.Events;
using RealityToolkit.Input.InteractionBehaviours;
using RealityToolkit.Input.Interactors;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace RealityToolkit.Core.Samples.Interactions
{
    [ExecuteInEditMode]
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/translate-lever-behaviour")]
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
        [SerializeField, Tooltip("The lever's minimum local position on each axis.")]
        private Vector3 minimumValues = new Vector3(0f, 0f, -.1f);

        [SerializeField, Tooltip("The lever's maximum local position on each axis.")]
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

        [SerializeField, Space, Tooltip("A normalized value per axis indicating the levers position.")]
        private UnityEvent<Vector3> valueChanged = null;

        private Transform pivot = null;
        private IControllerInteractor currentInteractor;
        private Vector3 previousInteractorPosition;
        private Vector3 ranges;
        private Vector3 delta;

        /// <summary>
        /// A normalized value per axis indicating the levers position.
        /// </summary>
        public Vector3 Value
        {
            get => new(valueX, valueY, valueZ);
            private set
            {
                valueX = value.x;
                valueY = value.y;
                valueZ = value.z;
            }
        }

        /// <summary>
        /// Should we translate the x-axis on interaction?
        /// </summary>
        protected bool TranslateX => (axes & SnapAxis.X) > 0;

        /// <summary>
        /// Should we translate the y-axis on interaction?
        /// </summary>
        protected bool TranslateY => (axes & SnapAxis.Y) > 0;

        /// <summary>
        /// Should we translate the z-axis on interaction?
        /// </summary>
        protected bool TranslateZ => (axes & SnapAxis.Z) > 0;

        /// <summary>
        /// The lever's <see cref="Value"/> has changed.
        /// </summary>
        public UnityEvent<Vector3> ValueChanged => valueChanged;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

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
            UpdateLeverPosition();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateLeverPosition();
            }
#endif

            if (currentInteractor == null)
            {
                return;
            }

            var currentInteractorPosition = GetInteractorPosition();

            if (TranslateX)
            {
                delta.x += currentInteractorPosition.x - previousInteractorPosition.x;
            }

            if (TranslateY)
            {
                delta.y += currentInteractorPosition.y - previousInteractorPosition.y;
            }

            if (TranslateZ)
            {
                delta.z += currentInteractorPosition.z - previousInteractorPosition.z;
            }

            var leverPosition = pivot.localPosition + delta;
            leverPosition.x = Mathf.Clamp(leverPosition.x, minimumValues.x, maximumValues.x);
            leverPosition.y = Mathf.Clamp(leverPosition.y, minimumValues.y, maximumValues.y);
            leverPosition.z = Mathf.Clamp(leverPosition.z, minimumValues.z, maximumValues.z);

            UpdateLeverPosition(leverPosition);
            UpdateValue(leverPosition);

            previousInteractorPosition = currentInteractorPosition;
        }

        /// <summary>
        /// Sets the lever's <see cref="Value"/> without raising the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="value">The lever's new value.</param>
        public void SetValueWithoutNotify(Vector3 value)
        {
            Value = value;
            UpdateLeverPosition();
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
            delta = Vector3.zero;
        }

        /// <summary>
        /// Updates the lever position to <paramref name="leverPosition"/>.
        /// </summary>
        /// <param name="leverPosition">The updated lever position.</param>
        private void UpdateLeverPosition(Vector3 leverPosition) => transform.localPosition = leverPosition;

        /// <summary>
        /// Updates the lever positio based on <see cref="Value"/>.
        /// </summary>
        private void UpdateLeverPosition()
        {
            RemapValue();

            Vector3 leverPosition;

            if (valueMapping == ValueMapping.Value0To1)
            {
                leverPosition = new Vector3(
                TranslateX ? minimumValues.x + Value.x * ranges.x : 0f,
                TranslateY ? minimumValues.y + Value.y * ranges.y : 0f,
                TranslateZ ? minimumValues.z + Value.z * ranges.z : 0f);

                UpdateLeverPosition(leverPosition);
                return;
            }

            leverPosition = new Vector3(
                TranslateX ? Value.x * ranges.x / 2f : 0f,
                TranslateY ? Value.y * ranges.y / 2f : 0f,
                TranslateZ ? Value.z * ranges.z / 2f : 0f);

            UpdateLeverPosition(leverPosition);
        }

        /// <summary>
        /// Updates <see cref="Value"/> based on <paramref name="leverPosition"/>.
        /// </summary>
        /// <param name="leverPosition">The updated lever position.</param>
        private void UpdateValue(Vector3 leverPosition)
        {
            Value = new(
                 ranges.x / (leverPosition.x - pivot.localPosition.x),
                 ranges.y / (leverPosition.y - pivot.localPosition.y),
                 ranges.z / (leverPosition.z - pivot.localPosition.z));

            RemapValue();
            OnValueChanged();

            ValueChanged?.Invoke(Value);
        }

        /// <summary>
        /// The <see cref="Value"/> has changed.
        /// </summary>
        protected virtual void OnValueChanged() { }

        private Vector3 GetInteractorPosition() => pivot.InverseTransformPoint(currentInteractor.GameObject.transform.position);

        private void RemapValue()
        {
            Value = new(
                MapValue(Value.x),
                MapValue(Value.y),
                MapValue(Value.z));
        }

        private float MapValue(float value)
        {
            if (valueMapping == ValueMapping.Value0To1)
            {
                return Mathf.Clamp01(value);
            }

            return Mathf.Clamp((value * 2) - 1, -1f, 1f);
        }
    }
}
