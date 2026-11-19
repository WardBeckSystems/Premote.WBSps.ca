using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PreMo.Properties;

namespace PreMo;

public class OnOffButton : UserControl
{
	private bool isCheck = false;

	private CheckStyle checkStyle = CheckStyle.style1;

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

	public CheckStyle CheckStyleX
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

	public OnOffButton()
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
		if (checkStyle == CheckStyle.style1)
		{
			val = Resources.btncheckon1;
			val2 = Resources.btncheckoff1;
		}
		else if (checkStyle == CheckStyle.style2)
		{
			val = Resources.btncheckon2;
			val2 = Resources.btncheckoff2;
		}
		else if (checkStyle == CheckStyle.style3)
		{
			val = Resources.btncheckon3;
			val2 = Resources.btncheckoff3;
		}
		else if (checkStyle == CheckStyle.style4)
		{
			val = Resources.btncheckon4;
			val2 = Resources.btncheckoff4;
		}
		else if (checkStyle == CheckStyle.style5)
		{
			val = Resources.btncheckon5;
			val2 = Resources.btncheckoff5;
		}
		else if (checkStyle == CheckStyle.style6)
		{
			val = Resources.btncheckon6;
			val2 = Resources.btncheckoff6;
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

	private void OnOffButton_Click(object sender, EventArgs e)
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
		((Control)this).Name = "OnOffButton";
		((Control)this).Click += OnOffButton_Click;
		((Control)this).ResumeLayout(false);
	}
}
