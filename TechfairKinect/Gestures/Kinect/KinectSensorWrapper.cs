using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TechfairKinect.Gestures.Kinect
{
    //what happens if kinect sensor gets unplugged?
    //do I need to implement a message interface? draws big text over the screen
    //app.cs would just have like if !string.isnullorempty(message) messagerenderer.render(message)
    //with some event in the interface that has "setmessage" and "removemessage"???
    //would have to stop updates as well
    //should probably find a cleaner way to handle exceptions as well
    internal class KinectSensorWrapper : IDisposable
    {
        public event EventHandler<SkeletonReadEventArgs> OnSkeletonRead;

        private KinectSensor _sensor;
        private int _currentTrackingId;

        public KinectSensorWrapper()
        {
            _currentTrackingId = -1;
            InitializeSensor();
        }

        public void Dispose()
        {
            if (_sensor != null)
                _sensor.Stop();
        }

        private void InitializeSensor()
        {
            _sensor = FindSensor();

            if (_sensor == null)
                throw new Exception("Unable to find Kinect");

            try
            {
                _sensor.SkeletonStream.Enable();
                _sensor.SkeletonStream.AppChoosesSkeletons = true;
                _sensor.SkeletonFrameReady += OnSkeletonFrameReady;
                
                _sensor.Start();
            }
            catch
            {
                _sensor = null; //so dispose doesn't try to stop it
                throw;
            }
        }

        private static KinectSensor FindSensor()
        {
            return KinectSensor.KinectSensors.FirstOrDefault(
                sensor => sensor.Status == KinectStatus.Connected);
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame.SkeletonArrayLength == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No skeletons");
                    return;
                }

                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                {
                    System.Diagnostics.Debug.WriteLine("No skeletons");
                    return;
                }

                var skeleton = FindSkeleton(skeletons);
                var scaled = ScaleSkeleton(skeleton);

                System.Diagnostics.Debug.WriteLine("Skeleton: {0}", skeleton.TrackingId);

                OnSkeletonFrameRead(skeleton.TrackingId, scaled, frame.Timestamp);
            }
        }

        private void OnSkeletonFrameRead(int trackingId, Dictionary<JointType, ScaledJoint> scaledSkeleton, long timestamp)
        {
            var args = new SkeletonReadEventArgs(trackingId, scaledSkeleton, timestamp);
            if (OnSkeletonRead != null)
                OnSkeletonRead(this, args);
        }

        private Skeleton FindSkeleton(Skeleton[] skeletons)
        { 
            var current = skeletons.FirstOrDefault(skeleton => skeleton.TrackingId == _currentTrackingId);

            if (current != null)
                return current;

            var closest = FindClosestSkeleton(skeletons);

            _currentTrackingId = closest.TrackingId;
            _sensor.SkeletonStream.ChooseSkeletons(_currentTrackingId);

            return closest;
        }

        private Skeleton FindClosestSkeleton(Skeleton[] skeletons)
        {
            var seed = new
            {
                Distance = float.PositiveInfinity,
                Skeleton = new Skeleton()
            };

            var result = skeletons.Where(skeleton => skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                .Aggregate(seed, (running, cur) =>
            {
                if (cur.Position.Z >= running.Distance)
                    return running;

                return new
                {
                    Distance = cur.Position.Z,
                    Skeleton = cur
                };
            });

            return result.Skeleton;
        }

        private Dictionary<JointType, ScaledJoint> ScaleSkeleton(Skeleton skeleton)
        {
            return skeleton.Joints.ToDictionary(
                joint => joint.JointType,
                CreateScaledJoint);
        }

        private ScaledJoint CreateScaledJoint(Joint joint)
        {
            return new ScaledJoint()
            {
                JointType = joint.JointType,
                LocationScreenPercent = ScaleSkeletonPoint(joint.Position)
            };
        }

        private Vector3D ScaleSkeletonPoint(SkeletonPoint point)
        {
            var depth = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point, DepthImageFormat.Resolution640x480Fps30);
            return Constrain(new Vector3D(
                depth.X / 640.0,
                1 - depth.Y / 480.0,
                depth.Depth
            ));
        }

        private Vector3D Constrain(Vector3D vector)
        {
            return Vector3D.ComponentMax(Vector3D.Zero, 
                Vector3D.ComponentMin(vector, new Vector3D(1.0, 1.5, 1.0)));
        }
    }
}
