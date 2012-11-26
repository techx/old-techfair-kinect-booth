using System;
using System.Collections.Generic;
using System.Drawing;
using TechfairKinect.Factories;

namespace TechfairKinect.StringDisplay
{
    internal class ParticleControllerFactory : SettingsBasedFactory<IParticleController, Type>
    {
        protected override string SettingsKey { get { return "ParticleImplementation"; } }

        private static Dictionary<string, Type> _subclassesByName = new Dictionary<string, Type>
        {
            { "Circular", typeof(CircularParticleController) }
        };

        protected override Dictionary<string, Type> ImplementationsBySettingsValue { get { return _subclassesByName; } }

        private readonly Size _screenBounds;

        public ParticleControllerFactory(Size screenBounds)
        {
            _screenBounds = screenBounds;
        }

        protected override IParticleController Instantiate(Type settingsData)
        {
            return (IParticleController)Activator.CreateInstance(settingsData, _screenBounds);
        }
    }
}
