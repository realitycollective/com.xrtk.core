// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Settings
{
    [CustomEditor(typeof(RealityToolkitEditorSettings))]
    public class RealityToolkitEditorSettingsEditor : UnityEditor.Editor
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            inspector.Add(new PropertyField
            {
                label = "Asset Import Path",
                bindingPath = "assetImportPath"
            });

            return inspector;
        }
    }
}
