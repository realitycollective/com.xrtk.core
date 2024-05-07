// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Editor;
using RealityToolkit.Editor.Utilities;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RealityToolkit.Editor.Settings
{
    [CustomEditor(typeof(RealityToolkitEditorSettings))]
    public class RealityToolkitEditorSettingsEditor : UnityEditor.Editor
    {
        public static string UxmlPath => $"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(CorePathFinder))}{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}Settings{Path.DirectorySeparatorChar}RealityToolkitEditorSettingsEditor.uxml";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            var visualElement = visualTreeAsset.Instantiate();

            root.Add(visualElement);

            var settings = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RealityToolkitEditorSettings.AssetPath));
            root.Bind(settings);

            return root;
        }
    }
}
