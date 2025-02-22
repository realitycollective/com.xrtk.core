﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using System;
using UnityEngine;
using UnityEvents = UnityEngine.EventSystems;

namespace RealityToolkit.EventDatum.Input
{
    /// <summary>
    /// Describes a uGUI event that was generated by a specific pointer.
    /// </summary>
    public class GraphicInputEventData : UnityEvents.PointerEventData
    {
        /// <inheritdoc />
        public GraphicInputEventData(UnityEvents.EventSystem eventSystem) : base(eventSystem)
        {
            if (eventSystem.IsNull())
            {
                throw new Exception("Event system cannot be null!");
            }
        }

        /// <summary>
        /// Clears the pointer data and calls the base <see cref="PointerEventData"/>'s Reset method.
        /// </summary>
        public void Clear()
        {
            Reset();
            button = InputButton.Left;
            clickCount = 0;
            clickTime = 0;
            delta = Vector2.zero;
            dragging = false;
            eligibleForClick = false;
            pointerCurrentRaycast = default;
            pointerDrag = null;
            pointerEnter = null;
            pointerId = 0;
            pointerPress = null;
            pointerPressRaycast = default;
            position = Vector2.zero;
            pressPosition = Vector2.zero;
            rawPointerPress = null;
            scrollDelta = Vector2.zero;
            useDragThreshold = false;
        }
    }
}