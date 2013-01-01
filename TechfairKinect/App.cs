using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TechfairKinect.AppState;
using TechfairKinect.Gestures;
using TechfairKinect.Graphics;
using TechfairKinect.Graphics.SkeletonRenderer;

namespace TechfairKinect
{
    public class App
    {
        private const double FramesPerSecond = 60.0;
        private const double MillisecondsPerFrame = 1000 / FramesPerSecond;
        private const double MaxMillisecondsUpdate = 50.0;

        private bool _running;
        private Stopwatch _timer;

        private readonly Dictionary<AppStateType, IAppState> _appStates;
        private readonly Dictionary<AppStateType, IAppStateRenderer> _appStateRenderers;

        private readonly IGraphicsBase _graphicsBase;

        private IAppState _currentAppState;
        private IAppStateRenderer _currentAppStateRenderer;
        private ISkeletonRenderer _skeletonRenderer;

        private readonly IGestureRecognizer _gestureRecognizer;

        public App()
        {
            _timer = new Stopwatch();

            _graphicsBase = new GraphicsBaseFactory().Create();
            _graphicsBase.OnExit += new EventHandler(ExitHandler);
            _graphicsBase.OnSizeChanged += SizeChangedHandler;
            _graphicsBase.OnKeyPressed += KeyPressHandler;

            _appStates = new AppStateFactory().CreateAppStates(_graphicsBase.ScreenBounds);

            _appStateRenderers = new AppStateRendererFactory().Create().ToDictionary(appStateRenderer => appStateRenderer.AppStateType);

            _skeletonRenderer = new SkeletonRendererFactory().Create();

            _gestureRecognizer = new GestureRecognizerFactory().Create();
            _gestureRecognizer.SkeletonRenderer = _skeletonRenderer;
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            _currentAppState.AppSize = e.Size;
            _skeletonRenderer.AppSize = e.Size;
        }

        private void KeyPressHandler(object sender, KeyEventArgs e)
        {
            if (_gestureRecognizer != null)
                _gestureRecognizer.OnKeyPressed(e);
        }

        private void StateChangeRequestedHandler(object sender, StateChangeRequestedEventArgs args)
        {
            SetCurrentAppState(args.AppStateType);
        }

        private void SetCurrentAppState(AppStateType appStateType)
        {
            Action onReady = () =>
                {
                    _currentAppState = _appStates[appStateType];
                    _currentAppState.StateChangeRequested += StateChangeRequestedHandler;
                    _currentAppState.AppSize = _graphicsBase.ScreenBounds;

                    _currentAppStateRenderer = _appStateRenderers[appStateType];
                    _currentAppStateRenderer.AppState = _currentAppState;
                    _currentAppStateRenderer.GraphicsBase = _graphicsBase;

                    _skeletonRenderer.GraphicsBase = _graphicsBase;

                    _gestureRecognizer.CurrentAppState = _currentAppState;

                    _currentAppState.OnTransitionTo();
                };

            if (_currentAppState != null)
            {
                _currentAppState.StateChangeRequested -= StateChangeRequestedHandler;
                _currentAppState.OnTransitionFrom(onReady);
            }
            else
                onReady();
        }

        public void Run()
        {
            _running = true;
            _timer.Start();

            SetCurrentAppState(AppStateType.StringDisplay);
            AppLoop();
        }

        private void ExitHandler(object sender, EventArgs e)
        {
            _running = false;
        }

        private void AppLoop()
        {
            //http://gafferongames.com/game-physics/fix-your-timestep/
            var accumulator = 0.0;
            var currentTime = _timer.ElapsedMilliseconds;
            while (_running)
            {
                var newTime = _timer.ElapsedMilliseconds;
                var frameSpan = Math.Min(newTime - currentTime, MaxMillisecondsUpdate);
                accumulator += frameSpan;

                currentTime = newTime;

                while (accumulator >= MillisecondsPerFrame)
                {
                    _currentAppState.UpdatePhysics(MillisecondsPerFrame);
                    accumulator -= MillisecondsPerFrame;
                }

                Render(accumulator / MillisecondsPerFrame);

                var cur = (int)(_timer.ElapsedMilliseconds - currentTime);
                if (cur < MillisecondsPerFrame)
                    Thread.Sleep((int)MillisecondsPerFrame - (int)(_timer.ElapsedMilliseconds - currentTime));
            }
        }

        private void Render(double interpolation)
        {
            _currentAppStateRenderer.Render(interpolation);
            _skeletonRenderer.Render();
        }
    }
}
