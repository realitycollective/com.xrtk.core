// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(ButtonBehaviour), true)]
    public class ButtonBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private const string raiseOnInputDown = "raiseOnInputDown";
        private const string click = "click";

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(raiseOnInputDown)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(click)));

            return inspector;
        }
    }
}