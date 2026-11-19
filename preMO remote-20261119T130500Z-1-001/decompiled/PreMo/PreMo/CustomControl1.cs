using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PreMo;

public class CustomControl1 : Control
{
	private IContainer components = null;

	private TrackBar trackBar1;

	public CustomControl1()
	{
		InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected Obj, but got Unknown
		base.OnPaint(pe);
		int num = Math.Abs(((Control)trackBar1).Size.Width);
		int num2 = Math.Abs(((Control)trackBar1).Size.Height);
		int x = ((Control)trackBar1).Location.X;
		int num3 = ((Control)trackBar1).Location.Y + num2;
		int x2 = x + num;
		int y = ((Control)trackBar1).Location.Y;
		Pen val = new Pen(Color.Black, 3f);
		Point point = new Point(x, num3);
		Point point2 = new Point(x + Convert.ToInt32(num / 2), num3 + Convert.ToInt32(num2 / 3));
		Point point3 = point2;
		Point point4 = new Point(x2, y);
		pe.Graphics.DrawBezier(val, point, point2, point3, point4);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		this.trackBar1 = new TrackBar();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		((Control)this).SuspendLayout();
		((Control)this.trackBar1).Location = new System.Drawing.Point(0, 0);
		((Control)this.trackBar1).Name = "trackBar1";
		((Control)this.trackBar1).Size = new System.Drawing.Size(104, 45);
		((Control)this.trackBar1).TabIndex = 0;
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		((Control)this).ResumeLayout(false);
	}
}
