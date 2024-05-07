// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Definitions.Utilities;
using UnityEngine.Rendering;

namespace RealityToolkit.Utilities
{
    public static class RenderPipelineUtilities
    {
        private const string urpAssetTypeName = "UniversalRenderPipelineAsset";
        private const string hdrpAssetTypeName = "HDRenderPipelineAsset";

        /// <summary>
        /// Gets the <see cref="UnityRenderPipeline"/> used by the project.
        /// </summary>
        /// <returns>The <see cref="UnityRenderPipeline"/> used by the project.</returns>
        public static UnityRenderPipeline GetActiveRenderingPipeline()
        {
#if UNITY_6000_0_OR_NEWER
            var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
#else
            var renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
#endif

            if (renderPipelineAsset.IsNull())
            {
                return UnityRenderPipeline.Legacy;
            }

            switch (renderPipelineAsset.GetType().Name)
            {
                case urpAssetTypeName:
                    return UnityRenderPipeline.UniversalRenderPipeline;
                case hdrpAssetTypeName:
                    return UnityRenderPipeline.HighDefinitionRenderPipeline;
            }

            return UnityRenderPipeline.Custom;
        }
    }
}