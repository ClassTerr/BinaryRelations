/*
* VISTA CONTROLS FOR .NET 2.0
* ENHANCED BUTTON
* 
* Written by Marco Minerva, mailto:marco.minerva@gmail.com
* 
* This code is released under the Microsoft Community License (Ms-CL).
* A copy of this license is available at
* http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedcommunitylicense.mspx
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsAero
{
    [ToolboxBitmap(typeof(Button))]
    public class Button : System.Windows.Forms.Button
    {
        public Button()
        {
            this.FlatStyle = FlatStyle.System;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private Boolean useicon = true; //Checks if user wants to use an icon instead of a bitmap
        private Bitmap image_;
        //Image alignment is ignored at the moment. Property overrides inherited image property
        //Supports images other than bitmap, supports transparency on .NET 2.0
        [Description("Gets or sets the image that is displayed on a button control."), Category("Appearance"), DefaultValue(null)]
        public new Bitmap Image
        {
            get
            {
                return image_;
            }
            set
            {
                image_ = value;
                if (value != null)
                {
                    this.useicon = false;
                    this.Icon = null;
                }
                this.SetShield(false);
                SetImage();
            }
        }
        private Icon icon_;
        [Description("Gets or sets the icon that is displayed on a button control."), Category("Appearance"), DefaultValue(null)]
        public Icon Icon
        {
            get
            {
                return icon_;
            }
            set
            {
                icon_ = value;
                if (icon_ != null)
                {
                this.useicon = true;
                }
                this.SetShield(false);
                SetImage();
            }
        }
        [Description("Refreshes the image displayed on the button.")]
        public void SetImage()
        {
            IntPtr iconhandle = IntPtr.Zero;
            if (!this.useicon)
            {
                if (this.image_ != null)
                {
                    iconhandle = image_.GetHicon(); //Gets the handle of the bitmap
                }
            }
            else
            {
                if (this.icon_ != null)
                {
                    iconhandle = this.Icon.Handle;
                }
            }

            //Set the button to use the icon. If no icon or bitmap is used, no image is set.
            SendMessage(this.Handle, NativeMethods.BM_SETIMAGE, 1, (int)iconhandle);
        }
        private Boolean showshield_ = false;
        [Description("Gets or sets whether if the control should use an elevated shield icon."), Category("Appearance"), DefaultValue(false)]
        public Boolean ShowShield
        {
            get
            {
                return showshield_;
            }
            set
            {
                showshield_ = value;
                this.SetShield(value);
                if (!value)
                {
                this.SetImage();
                }
            }
        }
        public void SetShield(Boolean Value)
        {
            NativeMethods.SendMessage(this.Handle, NativeMethods.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(showshield_ ? 1 : 0));
        }

        /// <summary>
        /// This procedure will redraw any control, given it's handl as an image on the form.  This is necessary if you want to lay this
        /// control on top of the glass surface of an Aero form.
        /// </summary>
        /// <param name="hwnd"></param>
        public void RedrawControlAsBitmap(IntPtr hwnd)
        {
            Control c = Control.FromHandle(hwnd);
            using (Bitmap bm = new Bitmap(c.Width, c.Height))
            {
                c.DrawToBitmap(bm, c.ClientRectangle);
                using (Graphics g = c.CreateGraphics())
                {
                    Point p = new Point(-1, -1);
                    g.DrawImage(bm, p);
                }
            }
            c = null;
        }

        /// <summary>
        /// Handles incoming Windows Messages.
        /// </summary>
        /// <param name="m"></param>
        /// <remarks>
        /// On the paint event and if the RenderOnGlass is set to true, we will redraw the control as an image directly on
        /// the form.  This has a little extra overhead but also provides the ability to lay this control directly on the
        /// glass and have it rendered correctly.
        /// </remarks>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            int WM_PAINT = 15;
            if ((m.Msg == WM_PAINT))
            {
                this.RedrawControlAsBitmap(this.Handle);
            }
        }
    }
}
