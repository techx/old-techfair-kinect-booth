using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;

namespace TechfairKinect.StringDisplay.ParticleStringGeneration
{
    class ParticleFactory
    {
        private const string ParticleStringSettingsKey = "ParticleString";
        private const string ParticleRadiusSettingsKey = "ParticleRadius";

        private readonly ParticleStringGenerator _generator;

        public ParticleFactory()
        {
            _generator = new ParticleStringGenerator(GetParticleString(), GetParticleRadius());
        }

        public IEnumerable<Particle> Create(Size screenBounds)
        {
            return _generator.GenerateParticles(screenBounds).ToList();
        }

        private static string GetParticleString()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(ParticleStringSettingsKey))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in settings", ParticleStringSettingsKey));

            return ConfigurationManager.AppSettings[ParticleStringSettingsKey];
        }

        private static double GetParticleRadius()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(ParticleRadiusSettingsKey))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in settings", ParticleRadiusSettingsKey));

            var value = ConfigurationManager.AppSettings[ParticleRadiusSettingsKey];
            double radius;

            if (!double.TryParse(value, out radius))
                throw new ConfigurationErrorsException(string.Format("Invalid value \"{0}\" for settings key {1} (expected double)", value, ParticleRadiusSettingsKey));

            return radius;
        }
    }
}
