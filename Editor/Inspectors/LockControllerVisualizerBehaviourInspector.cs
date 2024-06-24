// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private Toggle snapToLockPose;
        private PropertyField syncPositionSpeed;
        private PropertyField syncRotationSpeed;

        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField
            {
                label = "Grab Offset Pose",
                bindingPath = "localOffsetPose"
            });

            snapToLockPose = new Toggle("Snap To Lock Pose")
            {
                bindingPath = "snapToLockPose"
            };
            snapToLockPose.RegisterValueChangedCallback(SnapToLockPose_ValueChanged);
            inspector.Add(snapToLockPose);

            syncPositionSpeed = new PropertyField
            {
                label = "Sync Position Speed",
                bindingPath = "syncPositionSpeed"
            };

            syncRotationSpeed = new PropertyField
            {
                label = "Sync Rotation Speed",
                bindingPath = "syncRotationSpeed"
            };

            if (!snapToLockPose.value)
            {
                inspector.Add(syncPositionSpeed);
                inspector.Add(syncRotationSpeed);
            }

            return inspector;
        }

        private void SnapToLockPose_ValueChanged(ChangeEvent<bool> changeEvent)
        {
            if (!changeEvent.newValue)
            {
                inspector.Add(syncPositionSpeed);
                syncPositionSpeed.PlaceInFront(snapToLockPose);
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
