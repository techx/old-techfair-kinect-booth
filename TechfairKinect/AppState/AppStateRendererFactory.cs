using System;
using System.Collections.Generic;
using TechfairKinect.Components;
using TechfairKinect.Factories;
using TechfairKinect.StringDisplay;

namespace TechfairKinect.AppState
{
    internal class AppStateRendererFactory : SettingsBasedCollectionFactory<IComponentRenderer>
    {
        private static Dictionary<string, IEnumerable<Type>> AppStateRendererCollectionsByImplementation =
            new Dictionary<string, IEnumerable<Type>>()
            {
                { "Gdi", new[] { typeof(GdiStringDisplayRenderer) } }
            };

        protected override string SettingsKey { get { return "GraphicsImplementation"; } }

        protected override Dictionary<string, IEnumerable<Type>> ImplementationsBySettingsValue
        {
            get { return AppStateRendererCollectionsByImplementation; }
        }
    }
}
