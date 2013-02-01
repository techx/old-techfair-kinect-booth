using System;
using System.Collections.Generic;
using TechfairKinect.Factories;
using TechfairKinect.Gestures.Keyboard;
using TechfairKinect.Gestures.Kinect;

namespace TechfairKinect.Gestures
{
    internal class SkeletonUpdaterFactory : SettingsBasedSingularFactory<ISkeletonUpdater>
    {
        protected override string SettingsKey { get { return "SkeletonUpdaterImplementation"; } }

        private static Dictionary<string, Type> _implementationsByName = new Dictionary<string, Type>
        {
            { "Keyboard", typeof(KeyboardSkeletonUpdater) },
            { "Kinect", typeof(KinectSkeletonUpdater) }
        };

        protected override Dictionary<string, Type> ImplementationsBySettingsValue { get { return _implementationsByName; } }
    }
}
