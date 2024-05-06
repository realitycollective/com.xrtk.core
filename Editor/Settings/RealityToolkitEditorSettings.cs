// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Editor;
using UnityEngine;

namespace RealityToolkit.Editor.Settings
{
    /// <summary>
    /// Reality Toolkit specific Unity editor settings for the open project.
    /// </summary>
    [ScriptableSettingsPath("Assets/RealityToolkit/Settings")]
    public class RealityToolkitEditorSettings : EditorScriptableSettings<RealityToolkitEditorSettings>
    {
        private const string defaultAssetImportPath = "Assets/RealityToolkit.Generated/";
        [SerializeField, Tooltip("This is the path default toolkit assets will be imported to, to make them mutable.")]
        private string assetImportPath = defaultAssetImportPath;

        /// <summary>
        /// This is the path default toolkit assets will be imported to, to make them mutable.
        /// </summary>
        public string AssetImportPath
        {
            get => assetImportPath;
            set => assetImportPath = value;
        }
    }
}
