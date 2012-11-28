using System;
using System.Drawing;

namespace TechfairKinect.Graphics
{
    internal class SizeChangedEventArgs : EventArgs
    {
        public Size Size { get; set; }

        public SizeChangedEventArgs(Size size)
        {
            Size = size;
        }
    }
}
