using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    public abstract class BaseInteractionBehaviourInspector : UnityEditor.Editor
    {
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
