// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(LockControllerVisualizerBehaviour), true)]
    public class LockControllerVisualizerBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private VisualElement inspector;
        private PropertyField smoothSyncPose;
        private PropertyField syncDuration;

        private const string hintBindingPath = "hint";
        private const string smoothSyncPoseBindingPath = nameof(smoothSyncPose);
        private const string syncPositionSpeedBindingPath = nameof(syncDuration);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(hintBindingPath)));

            smoothSyncPose = new PropertyField(serializedObject.FindProperty(smoothSyncPoseBindingPath));
            smoothSyncPose.RegisterCallback<ChangeEvent<bool>>(SmoothSyncPose_ValueChanged);
            inspector.Add(smoothSyncPose);

            syncDuration = new PropertyField(serializedObject.FindProperty(syncPositionSpeedBindingPath));
            syncDuration.style.paddingLeft = UIElementsUtilities.DefaultInset;

            UpdateSmoothSyncPoseFields(serializedObject.FindProperty(smoothSyncPoseBindingPath).boolValue);

            return inspector;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        private void OnDestroy()
        {
            if (smoothSyncPose != null)
            {
                smoothSyncPose.UnregisterCallback<ChangeEvent<bool>>(SmoothSyncPose_ValueChanged);
            }
        }

        private void SmoothSyncPose_ValueChanged(ChangeEvent<bool> changeEvent) => UpdateSmoothSyncPoseFields(changeEvent.newValue);

        private void UpdateSmoothSyncPoseFields(bool showFields)
        {
            if (showFields)
            {
                inspector.Add(syncDuration);
                syncDuration.PlaceInFront(smoothSyncPose);
                return;
            }

            if (inspector.Contains(syncDuration))
            {
                inspector.Remove(syncDuration);
            }
        }
    }
}
