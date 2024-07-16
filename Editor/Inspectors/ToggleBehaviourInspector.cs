// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(ToggleBehaviour), true)]
    public class ToggleBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private const string isOn = "isOn";
        private const string raiseOnInputDown = "raiseOnInputDown";
        private const string valueChanged = "valueChanged";

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(isOn)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(raiseOnInputDown)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(valueChanged)));

            return inspector;
        }
    }
}
