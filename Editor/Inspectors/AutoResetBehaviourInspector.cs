// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(AutoResetBehaviour), true)]
    public class AutoResetBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private const string resetDelay = "resetDelay";
        private const string resetPose = "resetPose";

        /// <summary>
        /// The <see cref="AutoResetBehaviour"/> does not care what the interactor handedness
        /// is and thus we can hide this setting from the user.
        /// </summary>
        protected override bool ShowHandedness => false;

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(resetDelay)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(resetPose)));

            return inspector;
        }
    }
}
