﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Hands.Poses;
using UnityEditor;
using UnityEngine.UIElements;

#if UNITY_2021
using UnityEditor.UIElements;
#endif

namespace RealityToolkit.Editor.Input.Hands.Poses
{
    [CustomEditor(typeof(HandPose), true)]
    public class HandPoseInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            var recorder = (HandPose)target;

            var recordedHandednessField = new EnumField("Recorded Handedness", Handedness.Left)
            {
                bindingPath = "recordedHandedness"
            };

            inspector.Add(recordedHandednessField);

            var posesListView = new ListView
            {
                bindingPath = "poses",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = false,
                showFoldoutHeader = true,
                headerTitle = "Recorded Joint Poses"
            };
            posesListView.style.flexGrow = 1;

            inspector.Add(posesListView);
            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new Button(Mirror) { text = "Create Mirror Pose" });

            return inspector;
        }

        private void Mirror()
        {
            var pose = (HandPose)target;
            pose.Mirror();
        }
    }
}
