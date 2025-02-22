﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.Interfaces.Audio;
using UnityEngine;

namespace RealityToolkit.Utilities.Audio.Influencers
{
    /// <summary>
    /// Class that implements <see cref="IAudioInfluencer"/> to provide an audio occlusion effect, similar
    /// to listening to sound from outside of an enclosed space.
    /// </summary>
    /// <remarks>
    /// Ensure that all sound emitting objects have an attached <see cref="AudioInfluencerController"/>. 
    /// Failing to do so will result in the desired effect not being applied to the sound.
    /// </remarks>
    [DisallowMultipleComponent]
    public class AudioOccluder : MonoBehaviour, IAudioInfluencer
    {
        [SerializeField]
        [Range(10.0f, 22000.0f)]
        [Tooltip("Frequency above which sound will not be heard after applying occlusion.")]
        private float cutoffFrequency = 5000.0f;

        /// <summary>
        /// Frequency above which sound will not be heard after applying occlusion.
        /// Setting this value to 22000.0 effectively disables the effect.
        /// </summary>
        /// <remarks>
        /// Chaining occluders will result in the lowest of the cutoff frequencies being applied to the sound.
        /// The CutoffFrequency range is 0.0 - 22000.0 (0 - 22kHz), inclusive.
        /// The default value is 5000.0 (5kHz).
        /// </remarks>
        public float CutoffFrequency
        {
            get => cutoffFrequency;
            set => cutoffFrequency = Mathf.Clamp(value, 10.0f, 22000.0f);
        }

        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Percentage of the audio source volume that will be heard after applying occlusion.")]
        private float volumePassThrough = 1.0f;

        /// <summary>
        /// Percentage of the audio source volume that will be heard after applying occlusion.
        /// </summary>
        /// <remarks>
        /// VolumePassThrough is cumulative. It is applied to the current volume of the object at the time
        /// the effect is applied.
        /// The VolumePassThrough range is from 0.0 - 1.0 (0-100%), inclusive.
        /// The default value is 1.0.
        /// </remarks>
        public float VolumePassThrough
        {
            get => volumePassThrough;
            set => cutoffFrequency = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        // Update is not used, but is kept so that this component can be enabled/disabled.
        private void Update() { }

        /// <inheritdoc />
        public void ApplyEffect(GameObject soundEmittingObject)
        {
            if (!isActiveAndEnabled) { return; }

            var audioSource = soundEmittingObject.GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogWarning("The specified emitter does not have an attached AudioSource component.");
                return;
            }

            // Audio occlusion is performed using a low pass filter.
            var lowPass = soundEmittingObject.EnsureComponent<AudioLowPassFilter>();
            lowPass.enabled = true;

            // In the real world, chaining multiple low-pass filters will result in the 
            // lowest of the cutoff frequencies being the highest pitches heard.
            lowPass.cutoffFrequency = Mathf.Min(lowPass.cutoffFrequency, CutoffFrequency);

            // Unlike the cutoff frequency, volume pass-through is cumulative.
            audioSource.volume *= VolumePassThrough;
        }

        /// <inheritdoc />
        public void RemoveEffect(GameObject soundEmittingObject)
        {
            // Audio occlusion is performed using a low pass filter.
            var lowPass = soundEmittingObject.GetComponent<AudioLowPassFilter>();

            if (lowPass == null) { return; }

            float neutralFrequency = AudioInfluencerController.NeutralHighFrequency;
            var influencerController = soundEmittingObject.GetComponent<AudioInfluencerController>();

            if (influencerController != null)
            {
                neutralFrequency = influencerController.NativeLowPassCutoffFrequency;
            }

            lowPass.cutoffFrequency = neutralFrequency;
            lowPass.enabled = false;

            // Note: Volume attenuation is reset in the AudioInfluencerController, which is attached to the sound emitting object.
        }
    }
}