using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    /// <summary>
    /// Base inspector for <see cref="RealityToolkit.Input.InteractionBehaviours.BaseInteractionBehaviour"/>s.
    /// </summary>
    public abstract class BaseInteractionBehaviourInspector : UnityEditor.Editor
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            inspector.Add(new PropertyField
            {
                label = "Sorting Order",
                bindingPath = "sortingOrder"
            });

            inspector.Add(new PropertyField
            {
                label = "Target Handedness",
                bindingPath = "targetHandedness"
            });

            return inspector;
        }
    }
}
