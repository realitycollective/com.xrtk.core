// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Settings
{
    public class RealityToolkitSettingsProvider : SettingsProvider
    {
        RealityToolkitSettingsProvider(string path = realityToolkitSettingsPath, SettingsScope scopes = SettingsScope.Project,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        private const string realityToolkitSettingsPath = "Project/XR Plug-in Management/Reality Toolkit";
        private UnityEditor.Editor realityToolkitSettingsEditor;

        [SettingsProvider]
        public static SettingsProvider CreateRealityToolkitSettingsProvider() => new RealityToolkitSettingsProvider();

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            realityToolkitSettingsEditor = UnityEditor.Editor.CreateEditor(RealityToolkitEditorSettings.Instance);
            rootElement.Add(realityToolkitSettingsEditor.CreateInspectorGUI());
        }

        /// <inheritdoc />
        public override void OnDeactivate()
        {
            if (realityToolkitSettingsEditor.IsNotNull())
            {
                Object.DestroyImmediate(realityToolkitSettingsEditor);
            }

            base.OnDeactivate();
        }
    }
}
