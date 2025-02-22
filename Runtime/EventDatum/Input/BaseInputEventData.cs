﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;
using System;
using UnityEvents = UnityEngine.EventSystems;

namespace RealityToolkit.EventDatum.Input
{
    /// <summary>
    /// Base class of all Input Events.
    /// </summary>
    public abstract class BaseInputEventData : UnityEvents.BaseEventData
    {
        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        /// <remarks>
        /// The value will be in the device's configured time zone.
        /// </remarks>
        public DateTime EventTime { get; private set; }

        /// <summary>
        /// The source the input event originates from.
        /// </summary>
        public IInputSource InputSource { get; private set; }

        /// <summary>
        /// The id of the source the event is from, for instance the hand id.
        /// </summary>
        public uint SourceId { get; private set; }

        /// <summary>
        /// The Input Action for this event.
        /// </summary>
        public InputAction InputAction { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventSystem">Typically will be <see cref="EventSystem.current"/></param>
        protected BaseInputEventData(UnityEvents.EventSystem eventSystem) : base(eventSystem)
        {
            if (eventSystem.IsNull())
            {
                throw new Exception($"{nameof(UnityEvents.EventSystem)} cannot be null!");
            }
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="inputSource"></param>
        /// <param name="inputAction"></param>
        protected void BaseInitialize(IInputSource inputSource, InputAction inputAction)
        {
            Reset();
            EventTime = DateTime.UtcNow;
            InputSource = inputSource;
            InputAction = inputAction;
            SourceId = InputSource.SourceId;
        }
    }
}
