using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TechfairKinect.Components;
using TechfairKinect.StringDisplay;

namespace TechfairKinect.AppState
{
    internal class AppStateFactory
    {
        private static List<Type> AppStateTypes = new List<Type>
        {
            typeof(StringDisplayAppState)
        };

        public Dictionary<ComponentType, IAppState> CreateAppStates(Size screenBounds)
        {
            return AppStateTypes
                .Select(type => 
                    (IAppState)Activator.CreateInstance(type, screenBounds))
                .ToDictionary(state => state.ComponentType);
        }
    }
}
