using System;
using System.Collections.Generic;
using System.Linq;

namespace TechfairKinect.Factories
{
    internal abstract class SettingsBasedCollectionFactory<T> : SettingsBasedFactory<IEnumerable<T>, IEnumerable<Type>>
    {
        protected override IEnumerable<T> Instantiate(IEnumerable<Type> settingsData)
        {
            return settingsData.Select(type => (T)Activator.CreateInstance(type));
        }
    }
}
