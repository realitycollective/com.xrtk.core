// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(InteractionEventsBehaviour), true)]
    public class InteractionEventsBehaviourInspector : BaseInteractionBehaviourInspector
    {
        /// <summary>
        /// The events behaviour does not need to display handedness as it will and should
        /// forward events for any handedness and leave it to the user to decide what to do with it.
        /// </summary>
        protected override bool ShowHandedness => false;

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty("firstFocusEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("focusEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("focusExited")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("lastFocusExited")));

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField(serializedObject.FindProperty("firstSelectEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("selectEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("selectExited")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("lastSelectExited")));

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField(serializedObject.FindProperty("firstGrabEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("grabEntered")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("grabExited")));
            inspector.Add(new PropertyField(serializedObject.FindProperty("lastGrabExited")));

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField(serializedObject.FindProperty("reset")));

            return inspector;
        }
    }
}