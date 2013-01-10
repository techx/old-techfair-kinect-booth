using System;
using System.Collections.Generic;
using TechfairKinect.Factories;
using TechfairKinect.Gestures.Keyboard;
using TechfairKinect.Gestures.Kinect;

namespace TechfairKinect.Gestures
{
    internal class GestureRecognizerFactory : SettingsBasedSingularFactory<IGestureRecognizer>
    {
        protected override string SettingsKey { get { return "GestureRecognizerImplementation"; } }

        private static Dictionary<string, Type> _implementationsByName = new Dictionary<string, Type>
        {
            { "Keyboard", typeof(KeyboardGestureRecognizer) },
            { "Kinect", typeof(KinectGestureRecognizer) }
        };

        protected override Dictionary<string, Type> ImplementationsBySettingsValue { get { return _implementationsByName; } }
    }
}
