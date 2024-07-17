// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Utilities.Extensions;
using RealityToolkit.EventDatum.Input;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Hands.Poses;
using System;
using UnityEngine;

namespace RealityToolkit.Input.Hands.Visualizers
{
    /// <summary>
    /// Base <see cref="IControllerVisualizer"/> for <see cref="IController"/> visualizations that resemble a hand-like appearance.
    /// </summary>
    public abstract class BaseHandControllerVisualizer : BaseControllerVisualizer
    {
        protected IHandJointTransformProvider jointTransformProvider;
        protected int jointCount;

        protected virtual void Awake()
        {
            jointCount = Enum.GetNames(typeof(HandJoint)).Length;

            if (!TryGetComponent(out jointTransformProvider))
            {
                Debug.LogError($"{GetType().Name} requires an {nameof(IHandJointTransformProvider)} on the {nameof(UnityEngine.GameObject)}.", this);
                return;
            }
        }

        /// <summary>
        /// Checks whether any of the <see cref="Interactors.IInteractor"/>s on this <see cref="BaseHandControllerVisualizer"/> is targeting
        /// an object that has a <see cref="IProvideHandPose"/> implementation attached to it.
        /// </summary>
        /// <param name="eventData">The input event data received.</param>
        /// <param name="checkFocus">Check whether there is a <see cref="IProvideHandPose"/> that provides a focus pose.</param>
        /// <param name="checkSelect">Check whether there is a <see cref="IProvideHandPose"/> that provides a select pose.</param>
        /// <param name="checkGrab">Check whether there is a <see cref="IProvideHandPose"/> that provides a grab pose.</param>
        /// <returns></returns>
        protected bool IsTargetingHandPoseProvider(InputEventData eventData, bool checkFocus, bool checkSelect, bool checkGrab)
        {
            for (var i = 0; i < eventData.InputSource.Pointers.Length; i++)
            {
                var interactor = eventData.InputSource.Pointers[i];
                if (interactor.Result.CurrentTarget.IsNotNull())
                {
                    var poseProviders = interactor.Result.CurrentTarget.GetComponents<IProvideHandPose>();
                    for (var j = 0; j < poseProviders.Length; j++)
                    {
                        var poseProvider = poseProviders[j];
                        if (checkFocus && poseProvider.FocusPose.IsNotNull())
                        {
                            return true;
                        }

                        if (checkSelect && poseProvider.SelectPose.IsNotNull())
                        {
                            return true;
                        }

                        if (checkGrab && poseProvider.GrabPose.IsNotNull())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
