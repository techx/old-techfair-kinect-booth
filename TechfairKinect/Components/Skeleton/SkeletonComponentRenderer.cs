using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechfairKinect.Graphics;

namespace TechfairKinect.Components.Skeleton
{
    internal abstract class SkeletonComponentRenderer : IComponentRenderer
    {
        public ComponentType ComponentType { get { return ComponentType.Skeleton; } }

        protected SkeletonComponent SkeletonComponent;
        public IComponent Component
        {
            get { return SkeletonComponent; }
            set { SkeletonComponent = (SkeletonComponent)value; }
        }

        public abstract IGraphicsBase GraphicsBase { get; set; }
        public abstract void Render(double interpolation);
    }
}
