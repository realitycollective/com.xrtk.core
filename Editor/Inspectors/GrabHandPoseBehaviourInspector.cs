// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(GrabHandPoseBehaviour), true)]
    public class GrabHandPoseBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private const string grabPose = "grabPose";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(grabPose)));

            return inspector;
        }
    }
}
