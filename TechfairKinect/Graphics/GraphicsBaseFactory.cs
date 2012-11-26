using System;
using System.Collections.Generic;
using TechfairKinect.Factories;

namespace TechfairKinect.Graphics
{
    internal class GraphicsBaseFactory : SettingsBasedSingularFactory<IGraphicsBase>
    {
        protected override string SettingsKey { get { return "GraphicsImplementation"; } }

        private static Dictionary<string, Type> _subclassesByName = new Dictionary<string, Type>()
        {
            { "Gdi", typeof(GdiGraphicsBase) }
        };

        protected override Dictionary<string, Type> ImplementationsBySettingsValue { get { return _subclassesByName; } }
    }
}
