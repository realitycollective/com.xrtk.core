// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Editor.Utilities;
using RealityToolkit.Input.InteractionBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Inspectors
{
    [CustomEditor(typeof(SteeringWheelBehaviour), true)]
    public class SteeringWheelBehaviourInspector : BaseInteractionBehaviourInspector
    {
        private VisualElement inspector;
        private Toggle resetsToNeutral;
        private Toggle smoothReset;
        private PropertyField smoothResetDuration;

        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField
            {
                label = "Steering Angle Limit",
                bindingPath = "steeringAngleLimit"
            });

            inspector.Add(new PropertyField
            {
                label = "Up Transform",
                bindingPath = "upTransform"
            });

            resetsToNeutral = new Toggle("Resets To Neutral")
            {
                bindingPath = "resetsToNeutral"
            };
            resetsToNeutral.RegisterValueChangedCallback(ResetsToNeutral_ValueChanged);
            inspector.Add(resetsToNeutral);

            smoothReset = new Toggle("Smooth Reset")
            {
                bindingPath = "smoothReset"
            };
            smoothReset.RegisterValueChangedCallback(SmoothReset_ValueChanged);

            if (resetsToNeutral.value)
            {
                inspector.Add(smoothReset);
            }

            smoothResetDuration = new PropertyField
            {
                label = "Smooth Reset Duration",
                bindingPath = "smoothResetDuration"
            };

            if (resetsToNeutral.value && smoothReset.value)
            {
                inspector.Add(smoothResetDuration);
            }

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField
            {
                label = "On Steering Changed",
                bindingPath = "SteeringChanged"
            });

            return inspector;
        }

        private void ResetsToNeutral_ValueChanged(ChangeEvent<bool> changeEvent)
        {
            if (changeEvent.newValue)
            {
                inspector.Add(smoothReset);
                smoothReset.PlaceInFront(resetsToNeutral);
                return;
            }

            if (inspector.Contains(smoothReset))
            {
                inspector.Remove(smoothReset);
            }
        }

        private void SmoothReset_ValueChanged(ChangeEvent<bool> changeEvent)
        {
            if (resetsToNeutral.value && changeEvent.newValue)
            {
                inspector.Add(smoothResetDuration);
                smoothResetDuration.PlaceInFront(smoothReset);
                return;
            }

            if (inspector.Contains(smoothResetDuration))
            {
                inspector.Remove(smoothResetDuration);
            }
        }
    }
}
