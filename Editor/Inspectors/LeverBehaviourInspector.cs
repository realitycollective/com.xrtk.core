// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(LeverBehaviour), true)]
    public class LeverBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private VisualElement inspector;
        private Toggle snapsIntoPlace;
        private Toggle smoothSnap;
        private PropertyField smoothSnapSpeed;

        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField
            {
                label = "Lever Axes",
                bindingPath = "axes"
            });

            inspector.Add(new PropertyField
            {
                label = "Tracking Mode",
                bindingPath = "trackingMode"
            });

            inspector.Add(new PropertyField
            {
                label = "Lever Type",
                bindingPath = "leverType"
            });

            inspector.Add(new PropertyField
            {
                label = "Minimum",
                bindingPath = "minimumValues"
            });

            inspector.Add(new PropertyField
            {
                label = "Maximum",
                bindingPath = "maximumValues"
            });

            inspector.Add(new PropertyField
            {
                label = "Value Mapping",
                bindingPath = "valueMapping"
            });

            snapsIntoPlace = new Toggle("Snaps Into Place")
            {
                bindingPath = "snapsIntoPlace"
            };
            snapsIntoPlace.RegisterValueChangedCallback(SnapsIntoPlace_ValueChanged);
            inspector.Add(snapsIntoPlace);

            smoothSnap = new Toggle("Smooth Snap")
            {
                bindingPath = "smoothSnap"
            };
            smoothSnap.RegisterValueChangedCallback(SmoothSnap_ValueChanged);

            if (snapsIntoPlace.value)
            {
                inspector.Add(smoothSnap);
            }

            smoothSnapSpeed = new PropertyField
            {
                label = "Smooth Snap Speed",
                bindingPath = "smoothSnapSpeed"
            };

            if (snapsIntoPlace.value && smoothSnap.value)
            {
                inspector.Add(smoothSnapSpeed);
            }

            inspector.Add(new PropertyField
            {
                label = "Value (X)",
                bindingPath = "valueX"
            });

            inspector.Add(new PropertyField
            {
                label = "Value (Y)",
                bindingPath = "valueY"
            });

            inspector.Add(new PropertyField
            {
                label = "Value (Z)",
                bindingPath = "valueZ"
            });

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField
            {
                label = "On Value Changed",
                bindingPath = "valueChanged"
            });

            return inspector;
        }

        private void SnapsIntoPlace_ValueChanged(ChangeEvent<bool> changeEvent)
        {
            if (changeEvent.newValue)
            {
                inspector.Add(smoothSnap);
                smoothSnap.PlaceInFront(snapsIntoPlace);
                return;
            }

            if (inspector.Contains(smoothSnap))
            {
                inspector.Remove(smoothSnap);
            }
        }

        private void SmoothSnap_ValueChanged(ChangeEvent<bool> changeEvent)
        {
            if (snapsIntoPlace.value && changeEvent.newValue)
            {
                inspector.Add(smoothSnapSpeed);
                smoothSnapSpeed.PlaceInFront(smoothSnap);
                return;
            }

            if (inspector.Contains(smoothSnapSpeed))
            {
                inspector.Remove(smoothSnapSpeed);
            }
        }
    }
}
