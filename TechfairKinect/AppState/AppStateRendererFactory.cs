using System;
using System.Collections.Generic;
using TechfairKinect.Factories;
using TechfairKinect.StringDisplay.Rendering;

namespace TechfairKinect.AppState
{
    internal class AppStateRendererFactory : SettingsBasedCollectionFactory<IAppStateRenderer>
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
