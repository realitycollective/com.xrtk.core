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
        private PropertyField syncPositionSpeed;
        private PropertyField syncRotationSpeed;

        private const string grabOffsetPoseBindingPath = "localOffsetPose";
        private const string smoothSyncPoseBindingPath = "smoothSyncPose";
        private const string syncPositionSpeedBindingPath = "syncPositionSpeed";
        private const string syncRotationSpeedBindingPath = "syncRotationSpeed";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(grabOffsetPoseBindingPath)));

            smoothSyncPose = new PropertyField(serializedObject.FindProperty(smoothSyncPoseBindingPath));
            smoothSyncPose.RegisterCallback<ChangeEvent<bool>>(SmoothSyncPose_ValueChanged);
            inspector.Add(smoothSyncPose);

            syncPositionSpeed = new PropertyField(serializedObject.FindProperty(syncPositionSpeedBindingPath));
            syncPositionSpeed.style.paddingLeft = UIElementsUtilities.DefaultInset;

            syncRotationSpeed = new PropertyField(serializedObject.FindProperty(syncRotationSpeedBindingPath));
            syncRotationSpeed.style.paddingLeft = UIElementsUtilities.DefaultInset;

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
                inspector.Add(syncPositionSpeed);
                syncPositionSpeed.PlaceInFront(smoothSyncPose);
                inspector.Add(syncRotationSpeed);
                syncRotationSpeed.PlaceInFront(syncPositionSpeed);
                return;
            }

            if (inspector.Contains(syncPositionSpeed))
            {
                inspector.Remove(syncPositionSpeed);
            }

            if (inspector.Contains(syncRotationSpeed))
            {
                inspector.Remove(syncRotationSpeed);
            }
        }
    }
}
