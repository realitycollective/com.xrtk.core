﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RealityToolkit.Editor.BuildPipeline.Logging
{
    /// <summary>
    /// Logging utility designed to properly output logs to continuous integration workflow logging consoles.
    /// </summary>
    [InitializeOnLoad]
    public static class CILoggingUtility
    {
        /// <summary>
        /// The logger to use.
        /// </summary>
        public static ICILogger Logger { get; set; }

        private static bool loggingEnabled = Application.isBatchMode;

        /// <summary>
        /// Is CI Logging currently enabled?
        /// </summary>
        public static bool LoggingEnabled
        {
            get => loggingEnabled;
            set
            {
                if (loggingEnabled == value) { return; }

                Debug.Log(value ? "CI Logging Enabled" : "CI Logging Disabled");

                loggingEnabled = value;
            }
        }

        /// <summary>
        /// List of ignored log messages.
        /// </summary>
        public static readonly List<string> IgnoredLogs = new List<string>
        {
            @".android\repositories.cfg could not be loaded",
            @"Using symlinks in Unity projects may cause your project to become corrupted",
            @"Cancelling DisplayDialog: Built in VR Detected XR Plug-in Management has detected that this project is using built in VR.",
            @"Reference Rewriter found some errors while running with command"
        };

        static CILoggingUtility()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD", EnvironmentVariableTarget.Process)))
            {
                Logger = new AzurePipelinesLogger();
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACTIONS", EnvironmentVariableTarget.Process)))
            {
                Logger = new GitHubActionsLogger();
            }
        }
    }
}
