using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TechfairKinect.Graphics
{
    //creates a new thread to run the form on (using Application.Run() to allow the form to respond normally to messages)
    //have to jump through a couple threading hoops to ensure form is created and maximized before continuing 
    //we hook Form.Paint and Form.Closed to pass those back to the main app (for rendering and exiting respectively)
    internal class GdiGraphicsBase : GraphicsBase<PaintEventArgs>, IDisposable
    {
        private Form _form;
        private Thread _thread;
        private Action<PaintEventArgs> _paint;

        public override event EventHandler OnExit;
        public override Size ScreenBounds
        {
            get { return _form.Size; }
        }

        public GdiGraphicsBase()
        {
            _thread = new Thread(CreateForm);
            _thread.Start();

            while (_form == null)
                Thread.Sleep(10);

            var formScreenSize = GetFormScreenSize();
            while (!FormIsReady(formScreenSize)) //let the form load/maximize
                Thread.Sleep(10); 
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _thread != null)
            {
                if (_form != null)
                    _form.Close();
                _thread.Abort();
            }
        }

        private void CreateForm()
        {
            _form = new GdiDisplay();
            _form.Paint += new PaintEventHandler(OnPaint);
            _form.FormClosed += new FormClosedEventHandler(OnClosed);

            Application.Run(_form);
        }

        private Size GetFormScreenSize()
        {
            return _form.InvokeRequired ?
                (Size)_form.Invoke((Func<Size>)(() => GetScreenSizeFromFormHandle(_form.Handle))) :
                GetScreenSizeFromFormHandle(_form.Handle);
        }

        private Size GetScreenSizeFromFormHandle(IntPtr formHandle)
        {
            return Screen.FromHandle(formHandle).Bounds.Size;            
        }

        private bool FormIsReady(Size screenBounds)
        {
            if (_form.InvokeRequired)
                return (bool)_form.Invoke((Func<bool>)(() => _form.Size == screenBounds));
            return _form.Size == screenBounds;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (_paint == null)
                return;

            if (_form.InvokeRequired)
                _form.Invoke(_paint, e);
            else
                _paint(e);
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            if (OnExit != null)
                OnExit(this, null);
        }

        public override void Render(Action<PaintEventArgs> paint)
        {
            _paint = paint;
            _form.Invalidate();
        }
    }
}
