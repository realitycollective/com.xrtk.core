﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Interfaces.Events;
using System;
using UnityEngine.EventSystems;

namespace RealityToolkit.EventDatum
{
    /// <summary>
    /// Generic Base Event Data for Sending Events through the Event System.
    /// </summary>
    public class GenericBaseEventData : BaseEventData
    {
        /// <summary>
        /// The Event Source that the event originates from.
        /// </summary>
        public IEventSource EventSource { get; private set; }

        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        /// <remarks>
        /// The value will be in the device's configured time zone.
        /// </remarks>
        public DateTime EventTime { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventSystem">Usually <see cref="EventSystem.current"/></param>
        public GenericBaseEventData(EventSystem eventSystem) : base(eventSystem)
        {
            if (eventSystem.IsNull())
            {
                throw new Exception("Event system cannot be null!");
            }
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="eventSource">The source of the event.</param>
        protected void BaseInitialize(IEventSource eventSource)
        {
            Reset();
            EventTime = DateTime.UtcNow;
            EventSource = eventSource;
        }
    }
}