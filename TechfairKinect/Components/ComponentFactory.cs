using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using TechfairKinect.Components.Particles;
using TechfairKinect.Components.Skeleton;

namespace TechfairKinect.Components
{
    internal class ComponentFactory
    {
        private static Dictionary<string, Type> _particleComponentsByImplementation = new Dictionary<string, Type>()
        {
            { "Circular", typeof(CircularParticleComponent) }
        };

        private static Dictionary<string, Type> _particleComponentRenderersByImplementation = new Dictionary<string,Type>()
        {
            { "Gdi", typeof(GdiParticleComponentRenderer) }
        };

        private static Tuple<Type, Dictionary<string, Type>>[] _typesByRendererImplementation = new[]
            {
                Tuple.Create(
                    typeof(SkeletonComponent),
                    new Dictionary<string, Type>
                    {
                        { "Gdi", typeof(GdiSkeletonComponentRenderer) }
                    })
            };

        public IEnumerable<Tuple<IComponent, IComponentRenderer>> CreateStringDisplayComponentRendererTupes(Size appSize)
        {
            var graphicsImplementation = ConfigurationManager.AppSettings["GraphicsImplementation"];

            return _typesByRendererImplementation.Select(
                    tuple => CreateTuple(tuple, graphicsImplementation))
                .Concat(new[] { CreateParticleComponentTuple(appSize, graphicsImplementation) });
        }

        private Tuple<IComponent, IComponentRenderer> CreateTuple(Tuple<Type, Dictionary<string, Type>> types, string implementation)
        {
            var component = Instantiate<IComponent>(types.Item1);
            var renderer = Instantiate<IComponentRenderer>(types.Item2[implementation]);
            renderer.Component = component;

            return Tuple.Create(component, renderer);
        }

        private T Instantiate<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }

        private Tuple<IComponent, IComponentRenderer> CreateParticleComponentTuple(Size appSize, string graphicsImplementation)
        {
            var particleImplementation = ConfigurationManager.AppSettings["ParticleImplementation"];

            var component = (IComponent)Activator.CreateInstance(
                _particleComponentsByImplementation[particleImplementation], 
                appSize);

            var renderer = Instantiate<IComponentRenderer>(_particleComponentRenderersByImplementation[graphicsImplementation]);
            renderer.Component = component;

            return Tuple.Create(component, renderer);
        }
    }
}
