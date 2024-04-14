using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace kWindows.Core
{
    public enum ShowWindowsCommands
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        SW_HIDE = 0,

        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
        /// </summary>
        SW_SHOWNORMAL = 1,

        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        SW_SHOWMINIMIZED = 2,

        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        SW_MAXIMIZE = 3,

        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        SW_SHOWMAXIMIZED = 3,

        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar to <see cref="SW_SHOWNORMAL"/>, except that the window is not activated.
        /// </summary>
        SW_SHOWNOACTIVATE = 4,

        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        SW_SHOW = 5,

        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        SW_MINIMIZE = 6,

        /// <summary>
        /// Displays the window as a minimized window. This value is similar to <see cref="SW_SHOWMINIMIZED"/>, except the window is not activated.
        /// </summary>
        SW_SHOWMINNOACTIVE = 7,

        /// <summary>
        /// Displays the window in its current size and position. This value is similar to <see cref="SW_SHOW"/>, except that the window is not activated.
        /// </summary>
        SW_SHOWNA = 8,

        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
        /// </summary>
        SW_RESTORE = 9,

        /// <summary>
        /// Sets the show state based on the SW_ value specified in the <see cref="STARTUPINFO"/> structure passed to the <see cref="CreateProcess"/> function by the program that started the application.
        /// </summary>
        SW_SHOWDEFAULT = 10,

        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
        /// </summary>
        SW_FORCEMINIMIZE = 11
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    /// <summary>
    /// <para>The <see cref="MONITORINFO"/> structure contains information about a display monitor.</para>
    /// <para>The <see cref="MONITORINFO"/> structure is a subset of the <see cref="MONITORINFOEX"/> structure. The <see cref="MONITORINFOEX"/> structure adds a string member to contain a name for the display monitor.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    /// <summary>
    /// Represents an x- and y-coordinate pair in two-dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Describes the width, height, and location of a rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public static readonly RECT Empty = new RECT();

        public int Width
        {
            get { return Math.Abs(right - left); }
        }

        public int Height
        {
            get { return bottom - top; }
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public bool IsEmpty
        {
            get { return left >= right || top >= bottom; }
        }

        public RECT(RECT rcSrc)
        {
            left = rcSrc.left;
            top = rcSrc.top;
            right = rcSrc.right;
            bottom = rcSrc.bottom;
        }

        public override string ToString()
        {
            if (this == Empty)
            {
                return "RECT {Empty}";
            }

            return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RECT))
            {
                return false;
            }

            return (this == (RECT)obj);
        }

        public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();

        public static bool operator ==(RECT rect1, RECT rect2)
        {
            return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
        }

        public static bool operator !=(RECT rect1, RECT rect2)
        {
            return !(rect1 == rect2);
        }
    }

    /// <summary>
    /// Contains information about the placement of a window on the screen.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int Length;
        public int Flags;
        public int ShowCmd;
        public POINT MinPosition;
        public POINT MaxPosition;
        public RECT NormalPosition;

        public override bool Equals(object obj)
        {
            if (!(obj is WINDOWPLACEMENT))
            {
                return false;
            }

            WINDOWPLACEMENT windowPlacement = (WINDOWPLACEMENT)obj;
            return Length == windowPlacement.Length &&
                   Flags == windowPlacement.Flags &&
                   ShowCmd == windowPlacement.ShowCmd &&
                   EqualityComparer<POINT>.Default.Equals(MinPosition, windowPlacement.MinPosition) &&
                   EqualityComparer<POINT>.Default.Equals(MaxPosition, windowPlacement.MaxPosition) &&
                   EqualityComparer<RECT>.Default.Equals(NormalPosition, windowPlacement.NormalPosition);
        }

        /// <summary>
        /// Serves as the default hash function. Returns a value, based on the current instance, that is suited for hashing algorithms and data structures such as a hash table. https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2218-override-gethashcode-on-overriding-equals?view=vs-2017
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = -2001258044;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + Flags.GetHashCode();
            hashCode = hashCode * -1521134295 + ShowCmd.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<POINT>.Default.GetHashCode(MinPosition);
            hashCode = hashCode * -1521134295 + EqualityComparer<POINT>.Default.GetHashCode(MaxPosition);
            hashCode = hashCode * -1521134295 + EqualityComparer<RECT>.Default.GetHashCode(NormalPosition);
            return hashCode;
        }

        public static bool operator ==(WINDOWPLACEMENT windowPlacement1, WINDOWPLACEMENT windowPlacement2)
        {
            return windowPlacement1.Equals(windowPlacement2);
        }

        public static bool operator !=(WINDOWPLACEMENT windowPlacement1, WINDOWPLACEMENT windowPlacement2)
        {
            return !(windowPlacement1 == windowPlacement2);
        }
    }
}
