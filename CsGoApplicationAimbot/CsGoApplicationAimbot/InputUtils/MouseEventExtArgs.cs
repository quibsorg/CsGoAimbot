using System.Windows.Forms;

namespace CsGoApplicationAimbot.InputUtils
{
    public class MouseEventExtArgs : MouseEventArgs
    {
        public enum UpDown
        {
            None,
            Up,
            Down
        }

        /// <summary>
        ///     Used by UI to save cursor position on current form
        /// </summary>
        public object PosOnForm;

        /// <summary>
        ///     Used to check if button is released or pressed
        ///     If Wheel equals true then shows which way wheel is being turned
        /// </summary>
        public UpDown UpOrDown = UpDown.None;

        /// <summary>
        ///     If mouse wheel moved
        /// </summary>
        public bool Wheel;

        public MouseEventExtArgs()
            : base(MouseButtons.None, 0, 0, 0, 0)
        {
        }

        public MouseEventExtArgs(MouseButtons b, int clickcount, WinAPI.POINT point, int delta)
            : base(b, clickcount, point.X, point.Y, delta)
        {
        }

        public MouseEventExtArgs(MouseButtons b, int clickcount, int x, int y, int delta)
            : base(b, clickcount, x, y, delta)
        {
        }
    }
}