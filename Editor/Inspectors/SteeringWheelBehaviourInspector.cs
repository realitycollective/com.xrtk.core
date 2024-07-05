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
        private PropertyField resetsToNeutral;
        private PropertyField smoothReset;
        private PropertyField smoothResetDuration;

        private const string steeringAngleLimitBindingPath = "steeringAngleLimit";
        private const string upTransformBindingPath = "upTransform";
        private const string resetsToNeutralBindingPath = "resetsToNeutral";
        private const string smoothResetBindingPath = "smoothReset";
        private const string smoothResetDurationBindingPath = "smoothResetDuration";
        private const string steeringChangedBindingPath = "SteeringChanged";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            inspector = base.CreateInspectorGUI();

            inspector.Add(new PropertyField(serializedObject.FindProperty(steeringAngleLimitBindingPath)));
            inspector.Add(new PropertyField(serializedObject.FindProperty(upTransformBindingPath)));

            resetsToNeutral = new PropertyField(serializedObject.FindProperty(resetsToNeutralBindingPath));
            resetsToNeutral.RegisterCallback<ChangeEvent<bool>>(ResetsToNeutral_ValueChanged);
            inspector.Add(resetsToNeutral);

            smoothReset = new PropertyField(serializedObject.FindProperty(smoothResetBindingPath));
            smoothReset.RegisterCallback<ChangeEvent<bool>>(SmoothReset_ValueChanged);
            smoothReset.style.paddingLeft = UIElementsUtilities.DefaultInset;

            smoothResetDuration = new PropertyField(serializedObject.FindProperty(smoothResetDurationBindingPath));
            smoothResetDuration.style.paddingLeft = 2 * UIElementsUtilities.DefaultInset;

            UpdateResetsToNeutralFields(serializedObject.FindProperty(resetsToNeutralBindingPath).boolValue);
            UpdateSmoothResetFields(serializedObject.FindProperty(smoothResetBindingPath).boolValue);

            inspector.Add(UIElementsUtilities.VerticalSpace());
            inspector.Add(new PropertyField(serializedObject.FindProperty(steeringChangedBindingPath)));

            return inspector;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        private void OnDestroy()
        {
            if (resetsToNeutral != null)
            {
                resetsToNeutral.UnregisterCallback<ChangeEvent<bool>>(ResetsToNeutral_ValueChanged);
            }

            if (smoothReset != null)
            {
                smoothReset.UnregisterCallback<ChangeEvent<bool>>(SmoothReset_ValueChanged);
            }
        }

        private void ResetsToNeutral_ValueChanged(ChangeEvent<bool> changeEvent) => UpdateResetsToNeutralFields(changeEvent.newValue);

        private void UpdateResetsToNeutralFields(bool showFields)
        {
            if (showFields)
            {
                inspector.Add(smoothReset);
                smoothReset.PlaceInFront(resetsToNeutral);
                return;
            }

            if (inspector.Contains(smoothReset))
            {
                inspector.Remove(smoothReset);
            }

            if (inspector.Contains(smoothResetDuration))
            {
                inspector.Remove(smoothResetDuration);
            }
        }

        private void SmoothReset_ValueChanged(ChangeEvent<bool> changeEvent) => UpdateSmoothResetFields(changeEvent.newValue);

        private void UpdateSmoothResetFields(bool showFields)
        {
            if (serializedObject.FindProperty(resetsToNeutralBindingPath).boolValue && showFields)
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
