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
        /// Used internally by some <see cref="BaseInteractionBehaviourInspector"/>
        /// implementations to hide the handedness field from the inspector, likely because that
        /// behaviour works without that setting impacting it.
        /// </summary>
        protected virtual bool ShowHandedness => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            inspector.Add(new PropertyField(serializedObject.FindProperty(sortingOrderBindingPath)));

            if (ShowHandedness)
            {
                inspector.Add(new PropertyField(serializedObject.FindProperty(targetHandednessBindingPath)));
            }

            return inspector;
        }
    }
}
