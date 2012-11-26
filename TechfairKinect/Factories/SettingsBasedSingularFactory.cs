using System;

namespace TechfairKinect.Factories
{
    internal abstract class SettingsBasedSingularFactory<T> : SettingsBasedFactory<T, Type>
    {
        protected override T Instantiate(Type settingsData)
        {
            return (T)Activator.CreateInstance(settingsData);
        }
    }
}
