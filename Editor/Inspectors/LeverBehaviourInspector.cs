using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(LeverBehaviour), true)]
    public class LeverBehaviourInspector : BaseInteractionBehaviourInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            return inspector;
        }
    }
}
