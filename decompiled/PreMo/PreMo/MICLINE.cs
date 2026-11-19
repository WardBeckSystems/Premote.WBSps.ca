using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PreMo.Properties;

namespace PreMo;

public class MICLINE : UserControl
{
	private bool isCheck = false;

	private ChoseStyle checkStyle = ChoseStyle.style1;

	private IContainer components = null;

	public bool Checked
	{
		get
		{
			return isCheck;
		}
		set
		{
			isCheck = value;
			((Control)this).Invalidate();
		}
	}

	public ChoseStyle CheckStyle
	{
		get
		{
			return checkStyle;
		}
		set
		{
			checkStyle = value;
			((Control)this).Invalidate();
		}
	}

	public MICLINE()
	{
		InitializeComponent();
		base.SetStyle((ControlStyles)8192, true);
		base.SetStyle((ControlStyles)65536, true);
		base.SetStyle((ControlStyles)16, true);
		base.SetStyle((ControlStyles)512, true);
		base.SetStyle((ControlStyles)2048, true);
		base.SetStyle((ControlStyles)2, true);
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Cursor = Cursors.Hand;
		((Control)this).Size = new Size(87, 27);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Bitmap val = null;
		Bitmap val2 = null;
		if (checkStyle == ChoseStyle.style1)
		{
			val = Resources.LINEbutton;
			val2 = Resources.MICbutton;
		}
		Graphics graphics = e.Graphics;
		Rectangle rectangle = new Rectangle(0, 0, ((Control)this).Size.Width, ((Control)this).Size.Height);
		if (isCheck)
		{
			graphics.DrawImage((Image)(object)val, rectangle);
		}
		else
		{
			graphics.DrawImage((Image)(object)val2, rectangle);
		}
	}

	private void MICLINE_Click(object sender, EventArgs e)
	{
		isCheck = !isCheck;
		((Control)this).Invalidate();
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
		((Control)this).SuspendLayout();
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Name = "MICLINE";
		((Control)this).Click += MICLINE_Click;
		((Control)this).ResumeLayout(false);
	}
}
