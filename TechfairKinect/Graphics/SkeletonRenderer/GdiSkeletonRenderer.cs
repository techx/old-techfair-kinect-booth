using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using Gdi = System.Drawing;

namespace TechfairKinect.Graphics.SkeletonRenderer
{
    internal class GdiSkeletonRenderer : ISkeletonRenderer
    {
        //maybe make these percentages of the screen size? hmmmmmmmm
        private const float BoxPercentageSize = 0.1f;
        private const float BoxEdgeThickness = 5.0f;

        private const float LimbThickness = 1.0f;
        private const float JointCircleRadius = 2.0f;

        private static List<Tuple<JointType, JointType>> Limbs = new List<Tuple<JointType, JointType>>
        {
            Tuple.Create(JointType.Head, JointType.ShoulderCenter),

            Tuple.Create(JointType.ShoulderCenter, JointType.ShoulderLeft),
            Tuple.Create(JointType.ShoulderLeft, JointType.ElbowLeft),
            Tuple.Create(JointType.ElbowLeft, JointType.WristLeft),
            Tuple.Create(JointType.WristLeft, JointType.HandLeft),

            Tuple.Create(JointType.ShoulderCenter, JointType.ShoulderRight),
            Tuple.Create(JointType.ShoulderRight, JointType.ElbowRight),
            Tuple.Create(JointType.ElbowRight, JointType.WristRight),
            Tuple.Create(JointType.WristRight, JointType.HandRight),

            Tuple.Create(JointType.ShoulderCenter, JointType.Spine),
            Tuple.Create(JointType.Spine, JointType.HipCenter),
            
            Tuple.Create(JointType.HipCenter, JointType.HipLeft),
            Tuple.Create(JointType.HipLeft, JointType.KneeLeft),
            Tuple.Create(JointType.KneeLeft, JointType.AnkleLeft),
            Tuple.Create(JointType.AnkleLeft, JointType.FootLeft),

            Tuple.Create(JointType.HipCenter, JointType.HipRight),
            Tuple.Create(JointType.HipRight, JointType.KneeRight),
            Tuple.Create(JointType.KneeRight, JointType.AnkleRight),
            Tuple.Create(JointType.AnkleRight, JointType.FootRight)
        };

        private Dictionary<JointType, ScaledJoint> _currentSkeleton;

        private Gdi.RectangleF _skeletonBox;

        private GdiGraphicsBase _gdiGraphicsBase;
        public IGraphicsBase GraphicsBase
        {
            get { return _gdiGraphicsBase; }
            set 
            {
                _gdiGraphicsBase = (GdiGraphicsBase)value;
                AppSize = _gdiGraphicsBase.ScreenBounds;
            }
        }

        private Gdi.Size _appSize;
        public Gdi.Size AppSize
        {
            get { return _appSize; }
            set
            {
                _appSize = value;
                _skeletonBox = CreateSkeletonBox();
            }
        }

        public void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton)
        {
            _currentSkeleton = skeleton;
        }

        public void Render()
        {
            _gdiGraphicsBase.Render(this, args => Render(args.Graphics));
        }

        private void Render(Gdi.Graphics graphics)
        { 
            using (var boxPen = new Gdi.Pen(Gdi.Color.Black, BoxEdgeThickness))
            using (var jointBrush = new Gdi.SolidBrush(Gdi.Color.Black))
            using (var limbPen = new Gdi.Pen(Gdi.Color.Black, LimbThickness)) 
            {
                graphics.DrawRectangle(boxPen, _skeletonBox.X, _skeletonBox.Y, _skeletonBox.Width, _skeletonBox.Height);

                if (_currentSkeleton == null) //not initialized yet
                    return;

                var boxJoints = CalculateBoxJoints();

                boxJoints.Values.ToList().ForEach(location =>
                    RenderJoint(graphics, jointBrush, location));

                Limbs.ForEach(tuple => RenderLimb(graphics, limbPen, boxJoints[tuple.Item1], boxJoints[tuple.Item2]));
            }
        }

        private Gdi.RectangleF CreateSkeletonBox()
        {
            var x = (0.5f - BoxPercentageSize / 2) * _appSize.Width;
            var y = (1 - BoxPercentageSize) * _appSize.Height;
            var width = BoxPercentageSize * _appSize.Width;
            var height = BoxPercentageSize * _appSize.Height;

            return new Gdi.RectangleF(x, y, width, height);
        }

        private Dictionary<JointType, Vector3D> CalculateBoxJoints()
        {
            return _currentSkeleton.Select(kvp =>
                    Tuple.Create(
                        kvp.Key, 
                        new Vector3D(
                            kvp.Value.LocationScreenPercent.X * _skeletonBox.Width + _skeletonBox.X, 
                            (1 - kvp.Value.LocationScreenPercent.Y) * _skeletonBox.Height + _skeletonBox.Y, //y is flipped
                            0)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private void RenderJoint(Gdi.Graphics graphics, Gdi.Brush brush, Vector3D location)
        {
            graphics.FillEllipse(
                brush,
                (float)(location.X - JointCircleRadius),
                (float)(location.Y - JointCircleRadius),
                JointCircleRadius * 2,
                JointCircleRadius * 2);
        }

        private void RenderLimb(Gdi.Graphics graphics, Gdi.Pen pen, Vector3D lhs, Vector3D rhs)
        {
            graphics.DrawLine(pen, (float)lhs.X, (float)lhs.Y, (float)rhs.X, (float)rhs.Y); 
        }
    }
}
 