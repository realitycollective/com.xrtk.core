using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    /// <summary>
    /// Base inspector for <see cref="RealityToolkit.Input.InteractionBehaviours.BaseInteractionBehaviour"/>s.
    /// </summary>
    public abstract class BaseInteractionBehaviourInspector : UnityEditor.Editor
    {
        private const string sortingOrderBindingPath = "sortingOrder";
        private const string targetHandednessBindingPath = "targetHandedness";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            inspector.Add(new PropertyField(serializedObject.FindProperty(sortingOrderBindingPath)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(targetHandednessBindingPath)));

            return inspector;
        }
    }
}
