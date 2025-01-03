// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(HoldOntoBehaviour), true)]
    public class HoldOntoBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private VisualElement inspector;
        private PropertyField smooth;
        private PropertyField smoothingDuration;

        private const string hintBindingPath = "hint";
        private const string smoothSyncPoseBindingPath = nameof(smooth);
        private const string syncPositionSpeedBindingPath = nameof(smoothingDuration);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(hintBindingPath)));

            smooth = new PropertyField(serializedObject.FindProperty(smoothSyncPoseBindingPath));
            smooth.RegisterCallback<ChangeEvent<bool>>(SmoothSyncPose_ValueChanged);
            inspector.Add(smooth);

            smoothingDuration = new PropertyField(serializedObject.FindProperty(syncPositionSpeedBindingPath));
            smoothingDuration.style.paddingLeft = UIElementsUtilities.DefaultInset;

            UpdateSmoothSyncPoseFields(serializedObject.FindProperty(smoothSyncPoseBindingPath).boolValue);

            return inspector;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        private void OnDestroy()
        {
            if (smooth != null)
            {
                smooth.UnregisterCallback<ChangeEvent<bool>>(SmoothSyncPose_ValueChanged);
            }
        }

        private void SmoothSyncPose_ValueChanged(ChangeEvent<bool> changeEvent) => UpdateSmoothSyncPoseFields(changeEvent.newValue);

        private void UpdateSmoothSyncPoseFields(bool showFields)
        {
            if (showFields)
            {
                inspector.Add(smoothingDuration);
                smoothingDuration.PlaceInFront(smooth);
                return;
            }

            if (inspector.Contains(smoothingDuration))
            {
                inspector.Remove(smoothingDuration);
            }
        }
    }
}
