﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Editor;
using RealityToolkit.Input.Definitions;
using System;
using UnityEditor;
using UnityEngine;

namespace RealityToolkit.Editor.PropertyDrawers
{
    /// <summary>
    /// Draws the <see cref="InputAction"/> property field in custom inspectors.
    /// </summary>
    /// <example>
    /// <code language="csharp"><![CDATA[
    /// public class CustomScript : Monobehaviour
    /// {
    ///     [SerializeField]
    ///     private InputAction testAction = InputAction.None;
    ///
    ///     [SerializeField]
    ///     [AxisConstraint(AxisType.DualAxis)]
    ///     private InputAction test2Action = InputAction.None;
    /// }
    ///
    /// [CustomEditor(typeof(CustomScript))]
    /// public class CustomInspector : Editor
    /// {
    ///     private readonly InputActionDropdown inputActionDropdown = new InputActionDropdown();
    ///
    ///     private SerializedProperty testAction;
    ///     private SerializedProperty test2Action;
    ///
    ///     private void OnEnable()
    ///     {
    ///         testAction = serializedObject.FindProperty(nameof(testAction));
    ///         test2Action = serializedObject.FindProperty(nameof(test2Action));
    ///     }
    ///
    ///     public override void OnInspectorGUI()
    ///     {
    ///         inputActionDropdown.OnGui(GUIContent.none, testAction);
    ///         inputActionDropdown.OnGui(GUIContent.none, test2Action, AxisType.DualAxis);
    ///
    ///         // The same as:
    ///         EditorGUILayout.PropertyField(testAction);
    ///         EditorGUILayout.PropertyField(test2Action);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public class InputActionDropdown
    {
        private static readonly string DefaultGuidString = default(Guid).ToString("N");

        private InputActionsProfile[] allInputActionProfiles;

        public void OnGui(GUIContent content, SerializedProperty property, AxisType axisConstraintFilter = AxisType.None, params GUILayoutOption[] layoutOptions)
        {
            var rect = GUILayoutUtility.GetRect(content, "MiniPullDown", layoutOptions);
            OnGui(rect, property, content, axisConstraintFilter);
        }

        public void OnGui(Rect rect, SerializedProperty property, GUIContent content, AxisType axisConstraintFilter = AxisType.None)
        {
            var label = EditorGUI.BeginProperty(rect, content, property);

            var id = property.FindPropertyRelative("id");
            var profileGuid = property.FindPropertyRelative("profileGuid");
            var description = property.FindPropertyRelative("description");
            var axisConstraint = property.FindPropertyRelative("axisConstraint");

            if (string.IsNullOrWhiteSpace(profileGuid.stringValue))
            {
                profileGuid.stringValue = DefaultGuidString;
            }

            var currentAction = new InputAction(Guid.Parse(profileGuid.stringValue), (uint)id.intValue, description.stringValue, (AxisType)axisConstraint.intValue);

            if (allInputActionProfiles == null)
            {
                allInputActionProfiles = ScriptableObjectExtensions.GetAllInstances<InputActionsProfile>();
            }

            var dropdownMenu = new GenericMenu { allowDuplicateNames = true };
            dropdownMenu.AddItem(new GUIContent("None"), false, data => ResetAction(), null);

            foreach (var inputActionProfile in allInputActionProfiles)
            {
                dropdownMenu.AddSeparator($"{inputActionProfile.name}/");

                foreach (var inputAction in inputActionProfile.InputActions)
                {
                    if (axisConstraintFilter != AxisType.None &&
                        axisConstraintFilter != inputAction.AxisConstraint)
                    {
                        if (inputAction == currentAction)
                        {
                            ResetAction();
                        }

                        continue;
                    }

                    dropdownMenu.AddItem(
                        new GUIContent($"{inputActionProfile.name}/{inputAction.Description}"),
                        inputAction == currentAction,
                        OnItemSelect,
                        null);

                    void OnItemSelect(object _)
                    {
                        id.intValue = (int)inputAction.Id;
                        description.stringValue = inputAction.Description;
                        axisConstraint.intValue = (int)inputAction.AxisConstraint;
                        profileGuid.stringValue = inputAction.ProfileGuid.ToString("N");
                        property.serializedObject.ApplyModifiedProperties();
                        GUI.changed = true;
                    }
                }
            }

            void ResetAction()
            {
                id.intValue = 0;
                description.stringValue = "None";
                axisConstraint.intValue = 0;
                profileGuid.stringValue = DefaultGuidString;
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
            }

            var prefix = EditorGUI.PrefixLabel(rect, label);

            if (EditorGUI.DropdownButton(prefix, new GUIContent(description.stringValue), FocusType.Passive))
            {
                dropdownMenu.DropDown(prefix);
            }

            EditorGUI.EndProperty();
        }
    }
}