using RealityToolkit.Input.Events;
using RealityToolkit.Input.InteractionBehaviours;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Core.Samples.Interactions
{
    [HelpURL("https://www.realitytoolkit.io/docs/interactions/interaction-behaviours/default-behaviours/translate-lever-behaviour")]
    public class TranslateLeverBehaviour : BaseInteractionBehaviour
    {
        [SerializeField, Tooltip("The axes to translate upon interaction.")]
        private SnapAxis axes = SnapAxis.Z;

        [SerializeField, Tooltip("Translation limits per axis in each direction.")]
        private Vector3 translateThresholds = new Vector3(0f, 0f, .1f);

        [SerializeField, Tooltip("The pivot transform determines the coordinate space to translate in.")]
        private Transform pivot = null;

        private IControllerInteractor currentInteractor;
        private Vector3 leverResetPosition;
        private Vector3 compoundMinimumThresholds;
        private Vector3 compoundMaximumThresholds;
        private Vector3 previousInteractorPosition;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            leverResetPosition = GetLeverlPosition();

            compoundMinimumThresholds = new(
                leverResetPosition.x - translateThresholds.x,
                leverResetPosition.y - translateThresholds.y,
                leverResetPosition.z - translateThresholds.z);

            compoundMaximumThresholds = new(
                leverResetPosition.x + translateThresholds.x,
                leverResetPosition.y + translateThresholds.y,
                leverResetPosition.z + translateThresholds.z);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (currentInteractor == null)
            {
                return;
            }

            var currentInteractorPosition = GetInteractorPosition();
            var delta = Vector3.zero;

            if ((axes & SnapAxis.X) > 0)
            {
                delta.x = currentInteractorPosition.x - previousInteractorPosition.x;
            }

            if ((axes & SnapAxis.Y) > 0)
            {
                delta.y = currentInteractorPosition.y - previousInteractorPosition.y;
            }

            if ((axes & SnapAxis.Z) > 0)
            {
                delta.z = currentInteractorPosition.z - previousInteractorPosition.z;
            }

            var leverPosition = GetLeverlPosition();
            leverPosition.x = Mathf.Clamp(leverPosition.x + delta.x, compoundMinimumThresholds.x, compoundMaximumThresholds.x);
            leverPosition.y = Mathf.Clamp(leverPosition.y + delta.y, compoundMinimumThresholds.y, compoundMaximumThresholds.y);
            leverPosition.z = Mathf.Clamp(leverPosition.z + delta.z, compoundMinimumThresholds.z, compoundMaximumThresholds.z);
            transform.localPosition = leverPosition;

            previousInteractorPosition = currentInteractorPosition;
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

        private Vector3 GetLeverlPosition() => pivot.InverseTransformPoint(transform.position);

        private Vector3 GetInteractorPosition() => pivot.InverseTransformPoint(currentInteractor.GameObject.transform.position);
    }
}
