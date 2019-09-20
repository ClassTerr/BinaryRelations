// Test control fronend for WindowsForms for RazorGDIPainter library
//   (c) Mokrov Ivan
// special for habrahabr.ru
// under MIT license

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RazorGDIPainter;
using System.Timers;
using System.Threading;
using System.Diagnostics;

namespace RazorGDIControlWF
{
	public partial class PainterControl : PictureBox
	{
		#region Component Designer generated code
		private System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
            
			lock (this)
			{
				if (Graphics != null) Graphics.Dispose();
				if (Bitmap != null) Bitmap.Dispose();
				if (hDCGraphics != null) hDCGraphics.Dispose();
                if (RP != null) RP.Dispose();
			}
            
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// RazorPainterWFCtl
			// 
			this.Name = "RazorPainterWFCtl";
			this.ResumeLayout(false);

		}
		#endregion

		private readonly HandleRef hDCRef;
		private readonly Graphics hDCGraphics;
		private readonly RazorPainter RP;

		/// <summary>
		/// root Bitmap
		/// </summary>
		public Bitmap Bitmap { get; private set; }

		/// <summary>
		/// Graphics object to paint on RazorBMP
		/// </summary>
		public Graphics Graphics { get; private set; }

		/// <summary>
		/// Lock it to avoid resize/repaint race
		/// </summary>
        public readonly object RazorLock = new object();
        
		public PainterControl()
		{
			InitializeComponent();


			this.MinimumSize = new Size(1, 1);

			SetStyle(ControlStyles.DoubleBuffer, false);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
//			SetStyle(ControlStyles.Opaque, true);

			hDCGraphics = CreateGraphics();
			hDCRef = new HandleRef(hDCGraphics, hDCGraphics.GetHdc());

			RP = new RazorPainter();
            this.SizeChanged += ResizeProc;
            ResizeProc(null,null);
		}

        private void ResizeProc(object sender, EventArgs e)
        {
			lock (RazorLock)
			{
                if (Graphics != null) Graphics.Dispose();
                if (Bitmap != null) Bitmap.Dispose();
                Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                Graphics = Graphics.FromImage(Bitmap);
                Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			}
        }

		/// <summary>
		/// After all in-memory paint on RazorGFX, call it to display it on control
		/// </summary>
		public void RazorPaint()
		{
			RP.Paint(hDCRef, Bitmap);
		}
	}
}
