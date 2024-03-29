﻿/*****************************************************
 *            Vista Controls for .NET 2.0
 * 
 * http://www.codeplex.com/vistacontrols
 * 
 * @author: Lorenz Cuno Klopfenstein
 * Licensed under Microsoft Community License (Ms-CL)
 * 
 *****************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsAero.Native;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WindowsFormsAero.Dwm.Helpers
{

    /// <summary>
    /// Form that automatically handles glass margins and mouse dragging.
    /// </summary>
    public class GlassForm : AeroForm
    {

        /// <summary>
        /// Construct a new form without glass margins.
        /// </summary>
        public GlassForm()
        {
            ResizeRedraw = true;
            HandleMouseMove = true;
        }

        #region Properties

        List<Point> tree = new List<System.Drawing.Point>();
        Margins _glassMargins = Margins.Zero;

        /// <summary>Gets or sets the glass margins of the form.</summary>
        /// <remarks>This property should be used when setting the margins from code.</remarks>
        public Margins GlassMargins
        {
            get
            {
                return _glassMargins;
            }
            set
            {
                _glassMargins = value;

                SetGlass();
            }
        }

        /// <summary>Gets or sets the glass margins of the form.</summary>
        /// <remarks>This property should be used when setting the margins through the designer.</remarks>
        [Description("The glass margins which are extended inside the client area of the window."),
            Category("Appearance")]
        public Padding GlassDesignerMargins
        {
            get
            {
                return _glassMargins.AsPadding();
            }
            set
            {
                _glassMargins = new Margins(
                    value.Left, value.Right,
                    value.Top, value.Bottom
                );
            }
        }

        /// <summary>Gets or sets whether mouse dragging should be handled automatically.</summary>
        [Description("True if mouse dragging of the window should be handled automatically."),
            Category("Behavior"), DefaultValue(true)]
        public bool HandleMouseMove
        {
            get;
            set;
        }

        bool _glassEnabled = false;

        /// <summary>Gets or sets whether the extended glass margin is enabled or not.</summary>
        [Description("Enables or disables the glass margin."),
            Category("Appearance"), DefaultValue(false)]
        public bool GlassEnabled
        {
            get
            {
                return _glassEnabled;
            }
            set
            {
                if (value != _glassEnabled)
                {
                    _glassEnabled = value;

                    SetGlass();
                }
            }
        }

        bool _hideTitle = false;

        /// <summary>
        /// Gets or sets whether the window title and icon should be hidden.
        /// </summary>
        /// <remarks>
        /// The window caption will still be visible, but title text and icon will not be.
        /// A form with a hidden title will look like an Explorer window on Windows Vista or Windows 7.
        /// </remarks>
        [Description("Shows or hides the title and icon of the window."),
            Category("Appearance"), DefaultValue(false)]
        public bool HideTitle
        {
            get
            {
                return _hideTitle;
            }
            set
            {
                _hideTitle = value;

                ApplyWindowTheme();
            }
        }

        bool _hideCaption = false;

        /// <summary>
        /// Gets or sets whether the window caption should be hidden altogether.
        /// </summary>
        /// <remarks>
        /// Should be set before handle creation.
        /// </remarks>
        [Description("Shows or hides the window caption completely."),
            Category("Appearance"), DefaultValue(false)]
        public bool HideCaption
        {
            get
            {
                return _hideCaption;
            }
            set
            {
                _hideCaption = value;

                if (IsHandleCreated)
                {
                    RecreateHandle();
                }
            }
        }

        #endregion

        #region Overriding

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;

                if (HideCaption)
                {
                    parms.Style &= ~0x00C00000; //Remove WS_CAPTION
                }

                return parms;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            ApplyWindowTheme();

            base.OnHandleCreated(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Paint glass regions in black
            if (!_glassMargins.IsNull && _glassEnabled)
            {
                if (_glassMargins.IsMarginless)
                    e.Graphics.Clear(Color.Black);
                else
                {
                    e.Graphics.FillRectangles(Brushes.Black, new Rectangle[] {
					    new Rectangle(0, 0, ClientSize.Width, _glassMargins.Top),
					    new Rectangle(ClientSize.Width - _glassMargins.Right, 0, _glassMargins.Right, ClientSize.Height),
					    new Rectangle(0, ClientSize.Height - _glassMargins.Bottom, ClientSize.Width, _glassMargins.Bottom),
					    new Rectangle(0, 0, _glassMargins.Left, ClientSize.Height)
				    });
                }
            }
        }

        private const int WM_DWMCOMPOSITIONCHANGED = 0x31e;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            //Shortcut if disabled
            if (!HandleMouseMove)
                return;

            //Respond to hit test messages with Caption if mouse on glass: will enable form moving/maximization
            //as if mouse is on form caption.
            if (m.Msg == Messaging.WM_NCHITTEST && m.Result.ToInt32() == Messaging.HTCLIENT)
            {
                uint lparam = (uint)m.LParam.ToInt32();
                ushort x = IntHelpers.LowWord(lparam);
                ushort y = IntHelpers.HighWord(lparam);

                //Check if mouse pointer is on glass part of form
                var clientPoint = this.PointToClient(new Point(x, y));
                if (_glassMargins.IsOutsideMargins(clientPoint, ClientSize))
                {
                    m.Result = (IntPtr)Messaging.HTCAPTION;
                    return;
                }
            }

            if (m.Msg == WM_DWMCOMPOSITIONCHANGED)
            {
                bool compositionEnabled = false;
                DwmIsCompositionEnabled(ref compositionEnabled);

                if (Controls.Count == tree.Count)
                    for (int i = 0; i < Controls.Count; i++)
                        Controls[i].Location = tree[i];

                GlassEnabled = compositionEnabled;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern void DwmIsCompositionEnabled(ref bool pfEnabled);

        #endregion

        private void SetGlass()
        {
            if (DesignMode)
                return;

            if (!_glassMargins.IsNull && _glassEnabled)
                DwmManager.EnableGlassFrame(this, _glassMargins);
            else
                DwmManager.DisableGlassFrame(this);

            this.Invalidate();
        }

        private void ApplyWindowTheme()
        {
            var attr = (HideTitle) ?
                WindowThemeNonClientAttributes.NoDrawCaption | WindowThemeNonClientAttributes.NoDrawIcon :
                WindowThemeNonClientAttributes.NullAttribute;

            WindowTheme.SetWindowThemeNonClientAttributes(Handle,
                WindowThemeNonClientAttributes.NoDrawCaption | WindowThemeNonClientAttributes.NoDrawIcon,
                attr);
        }

    }
}
