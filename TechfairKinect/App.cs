using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TechfairKinect.AppState;
using TechfairKinect.Gestures;
using TechfairKinect.Graphics;

namespace TechfairKinect
{
    public class App
    {
        private const double FramesPerSecond = 30.0;
        private const double MillisecondsPerFrame = 1000 / FramesPerSecond;
        private const double MaxMillisecondsUpdate = 50.0;

        private bool _running;
        private Stopwatch _timer;

        private readonly Dictionary<AppStateType, IAppState> _appStates;
        private readonly Dictionary<AppStateType, IAppStateRenderer> _renderers;

        private readonly IGraphicsBase _graphicsBase;

        private IAppState _currentAppState;
        private IAppStateRenderer _currentRenderer;

        private readonly IGestureRecognizer _gestureRecognizer;

        public App()
        {
            _timer = new Stopwatch();

            _graphicsBase = new GraphicsBaseFactory().Create();
            _graphicsBase.OnExit += new EventHandler(ExitHandler);

            _appStates = new AppStateFactory().CreateAppStates(_graphicsBase.ScreenBounds);

            _renderers = new AppStateRendererFactory().Create().ToDictionary(appStateRenderer => appStateRenderer.AppStateType);

            _gestureRecognizer = new GestureRecognizerFactory().Create();
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

                    _currentRenderer = _renderers[appStateType];
                    _currentRenderer.AppState = _currentAppState;
                    _currentRenderer.GraphicsBase = _graphicsBase;

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
                currentTime = newTime;

                while (accumulator >= MillisecondsPerFrame)
                {
                    _currentAppState.UpdatePhysics(MillisecondsPerFrame);
                    accumulator -= MillisecondsPerFrame;
                }

                _currentRenderer.Render(accumulator / MillisecondsPerFrame);
                Thread.Sleep((int)MillisecondsPerFrame - (int)(_timer.ElapsedMilliseconds - currentTime));
            }
        }
    }
}
