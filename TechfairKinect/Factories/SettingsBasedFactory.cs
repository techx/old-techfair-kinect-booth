using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace TechfairKinect.Factories
{
    internal abstract class SettingsBasedFactory<TProduct, TSettingsData>
    {
        protected abstract string SettingsKey { get; }
        protected abstract Dictionary<string, TSettingsData> ImplementationsBySettingsValue { get; }

        protected abstract TProduct Instantiate(TSettingsData settingsData);

        public virtual TProduct Create()
        {
            CheckKeyInSettings();

            return CreateObjectFromSettingsValue(GetSettingsValue());
        }

        protected virtual void CheckKeyInSettings()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(SettingsKey))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in config file.", SettingsKey));
        }

        protected virtual string GetSettingsValue()
        {
            return ConfigurationManager.AppSettings[SettingsKey];
        }

        protected virtual void CheckValidSettingsValue(string value)
        {
            if (!ImplementationsBySettingsValue.ContainsKey(value))
                throw new ConfigurationErrorsException(string.Format("Invalid config value for {0}: {1}.", SettingsKey, value));
        }

        protected virtual TProduct CreateObjectFromSettingsValue(string settingsValue)
        {
            CheckValidSettingsValue(settingsValue);
            return Instantiate(ImplementationsBySettingsValue[settingsValue]);
        }
    }
}
