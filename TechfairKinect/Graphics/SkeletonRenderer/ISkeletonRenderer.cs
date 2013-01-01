using System.Collections.Generic;
using Microsoft.Kinect;
using System.Drawing;

namespace TechfairKinect.Graphics.SkeletonRenderer
{
    internal interface ISkeletonRenderer
    {
        IGraphicsBase GraphicsBase { get; set; }
        Size AppSize { get; set; }
        void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton);
        void Render();
    }
}
