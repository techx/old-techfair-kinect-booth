using System;
using System.Collections.Generic;
using TechfairKinect.Factories;

namespace TechfairKinect.Graphics.SkeletonRenderer
{
    internal class SkeletonRendererFactory : SettingsBasedSingularFactory<ISkeletonRenderer>
    {
        protected override string SettingsKey { get { return "GraphicsImplementation"; } }

        private static Dictionary<string, Type> _implementationsByName = new Dictionary<string, Type>
        {
            { "Gdi", typeof(GdiSkeletonRenderer) }
        };

        protected override Dictionary<string, Type> ImplementationsBySettingsValue { get { return _implementationsByName; } }
    }
}
