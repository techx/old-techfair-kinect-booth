using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdi = System.Drawing;
using TechfairKinect.Graphics;
using System.Configuration;
using Microsoft.Kinect;

namespace TechfairKinect.Components.Skeleton
{
    internal class GdiSkeletonComponentRenderer : SkeletonComponentRenderer
    {
        private static float ThresholdHeight = float.Parse(ConfigurationManager.AppSettings["ScreenThresholdHeightPercentage"]);

        private const float BoxPercentageSize = 0.1f;
        private const float BoxEdgeThickness = 5.0f;

        private const float LimbThickness = 1.0f;
        private const float LineThickness = 2.0f;
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

        protected GdiGraphicsBase GdiGraphicsBase;
        public override IGraphicsBase GraphicsBase
        {
            get { return GdiGraphicsBase; }
            set { GdiGraphicsBase = (GdiGraphicsBase)value; }
        }

        public override void Render(double interpolation)
        {
            GdiGraphicsBase.Render(this, args => Render(args.Graphics));
        }

        private void Render(Gdi.Graphics graphics)
        {
            using (var boxPen = new Gdi.Pen(Gdi.Color.White, BoxEdgeThickness))
            using (var jointBrush = new Gdi.SolidBrush(Gdi.Color.White))
            using (var limbPen = new Gdi.Pen(Gdi.Color.White, LimbThickness))
            using (var linePen = new Gdi.Pen(Gdi.Color.Red, LineThickness))
            {
                var skeletonBox = CreateSkeletonBox();
                graphics.DrawRectangle(boxPen, skeletonBox.X, skeletonBox.Y, skeletonBox.Width, skeletonBox.Height);

                if (base.SkeletonComponent.CurrentSkeleton == null) //not initialized yet
                    return;

                var boxJoints = CalculateBoxJoints(skeletonBox);

                boxJoints.Values.ToList().ForEach(location =>
                    RenderJoint(graphics, jointBrush, location));

                Limbs.ForEach(tuple => RenderLimb(graphics, limbPen, boxJoints[tuple.Item1], boxJoints[tuple.Item2]));

                var thresholdLineY = skeletonBox.Top + (1 - ThresholdHeight) * skeletonBox.Height;
                graphics.DrawLine(linePen, skeletonBox.Left, thresholdLineY, skeletonBox.Right, thresholdLineY);
            }
        }

        private Gdi.RectangleF CreateSkeletonBox()
        {
            var appSize = GdiGraphicsBase.ScreenBounds;
            var x = (0.5f - BoxPercentageSize / 2) * appSize.Width;
            var y = (1 - BoxPercentageSize) * appSize.Height;
            var width = BoxPercentageSize * appSize.Width;
            var height = BoxPercentageSize * appSize.Height;

            return new Gdi.RectangleF(x, y, width, height);
        }

        private Dictionary<JointType, Vector3D> CalculateBoxJoints(Gdi.RectangleF skeletonBox)
        {
            return base.SkeletonComponent.CurrentSkeleton.Select(kvp =>
                    Tuple.Create(
                        kvp.Key,
                        new Vector3D(
                            kvp.Value.LocationScreenPercent.X * skeletonBox.Width + skeletonBox.X,
                            (1 - kvp.Value.LocationScreenPercent.Y) * skeletonBox.Height + skeletonBox.Y, //y is flipped
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
