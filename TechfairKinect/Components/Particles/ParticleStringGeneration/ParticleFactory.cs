using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;

namespace TechfairKinect.Components.Particles.ParticleStringGeneration
{
    class ParticleFactory
    {
        private const string ParticleStringSettingsKey = "ParticleString";
        private const string ParticleRadiusSettingsKey = "ParticleRadius";
        private const string ParticleFontSettingsKey = "ParticleFont";

        private readonly ParticleStringGenerator _generator;

        public ParticleFactory()
        {
            _generator = new ParticleStringGenerator(GetParticleString(), GetParticleRadius(), GetParticleFontFamily());
        }

        public IEnumerable<Particle> Create(Size screenBounds)
        {
            return _generator.GenerateParticles(screenBounds).ToList();
        }

        private static string GetSettingsValue(string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in settings", key));

            return ConfigurationManager.AppSettings[key];
        }

        private static string GetParticleString()
        {
            return GetSettingsValue(ParticleStringSettingsKey);
        }

        private static double GetParticleRadius()
        {
            var value = GetSettingsValue(ParticleRadiusSettingsKey);
            double radius;

            if (!double.TryParse(value, out radius))
                throw new ConfigurationErrorsException(string.Format("Invalid value \"{0}\" for settings key {1} (expected double)", value, ParticleRadiusSettingsKey));

            return radius;
        }

        private static FontFamily GetParticleFontFamily()
        {
            var fontName = GetSettingsValue(ParticleFontSettingsKey);
            return new FontFamily(fontName);    
        }
    }
}
