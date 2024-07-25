using RealityToolkit.Input.Events;
using RealityToolkit.Input.Hands.Poses;
using RealityToolkit.Input.Hands.Visualizers;
using RealityToolkit.Input.Interactors;
using UnityEngine;

namespace RealityToolkit.Input.InteractionBehaviours
{
    /// <summary>
    /// The <see cref="SelectHandPoseBehaviour"/> will animate the <see cref="RiggedHandControllerVisualizer"/>
    /// into the assigned <see cref="selectPose"/>, when the <see cref="Interactables.IInteractable"/> is selected.
    [HelpURL(RealityToolkitRuntimePreferences.Toolkit_Docs_BaseUrl + "docs/interactions/interaction-behaviours/default-behaviours/select-hand-pose-behaviour")]
    [AddComponentMenu(RealityToolkitRuntimePreferences.Toolkit_InteractionsAddComponentMenu + "/" + nameof(SelectHandPoseBehaviour))]
    public class SelectHandPoseBehaviour : BaseInteractionBehaviour, IProvideHandPose
    {
        [SerializeField, Tooltip("Select pose applied when selecting the interactable.")]
        private HandPose selectPose = null;

        /// <inheritdoc/>
        public HandPose FocusPose { get; } = null;

        /// <inheritdoc/>
        public HandPose SelectPose => selectPose;

        /// <inheritdoc/>
        public HandPose GrabPose { get; } = null;

        /// <inheritdoc/>
        protected override void OnSelectEntered(InteractionEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = selectPose;
            }
        }

        /// <inheritdoc/>
        protected override void OnSelectExited(InteractionExitEventArgs eventArgs)
        {
            if (eventArgs.Interactor is IDirectInteractor directInteractor &&
                directInteractor.Controller.Visualizer is RiggedHandControllerVisualizer riggedHandControllerVisualizer)
            {
                riggedHandControllerVisualizer.OverridePose = null;
            }
        }
    }
}
