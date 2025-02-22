﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces;
using UnityEngine.EventSystems;

namespace RealityToolkit.EventDatum.Input
{
    /// <summary>
    /// Describes an Input Event that has a source id.
    /// </summary>
    public class InputEventData : BaseInputEventData
    {
        /// <summary>
        /// Handedness of the <see cref="IInputSource"/>.
        /// </summary>
        public Handedness Handedness { get; private set; } = Handedness.None;

        /// <inheritdoc />
        public InputEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="inputSource"></param>
        /// <param name="handedness"></param>
        /// <param name="inputAction"></param>
        public void Initialize(IInputSource inputSource, Handedness handedness, InputAction inputAction)
        {
            BaseInitialize(inputSource, inputAction);
            Handedness = handedness;
        }
    }

    /// <summary>
    /// Describes and input event with a specific type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InputEventData<T> : InputEventData
    {
        /// <summary>
        /// The input data of the event.
        /// </summary>
        public T InputData { get; private set; }

        /// <inheritdoc />
        public InputEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="inputSource"></param>
        /// <param name="handedness"></param>
        /// <param name="inputAction"></param>
        /// <param name="data"></param>
        public void Initialize(IInputSource inputSource, Handedness handedness, InputAction inputAction, T data)
        {
            Initialize(inputSource, handedness, inputAction);
            InputData = data;
        }
    }
}
