using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PreMo;

public class PPMMeter : UserControl
{
	public enum Meter_Side
	{
		Left,
		Right
	}

	public enum BarColour_Unlit
	{
		None,
		Coloured,
		Background,
		Gray,
		Gainsboro,
		WhiteSmoke,
		White
	}

	private Font labelFont = new Font("Courier New", 10f, (FontStyle)0, (GraphicsUnit)2);

	private Pen penGray = new Pen(Color.Gray, 1f);

	private Brush brushGray = (Brush)new SolidBrush(Color.Gray);

	private Pen penBlack = new Pen(Color.Black, 1f);

	private Brush brushBlack = (Brush)new SolidBrush(Color.Black);

	private Color unlit = Color.White;

	private Pen penUnlit;

	private Brush brushUnlit;

	private Pen penGreen = new Pen(Color.DarkGreen, 1f);

	private Brush brushGreen = (Brush)new SolidBrush(Color.DarkGreen);

	private Pen penYellow = new Pen(Color.Orange, 1f);

	private Brush brushYellow = (Brush)new SolidBrush(Color.Orange);

	private Pen penRed = new Pen(Color.Red, 1f);

	private Brush brushRed = (Brush)new SolidBrush(Color.Red);

	private Pen penDarkGreen = new Pen(Color.FromArgb(0, 64, 0), 1f);

	private Brush brushDarkGreen = (Brush)new SolidBrush(Color.FromArgb(0, 64, 0));

	private Pen penDarkYellow = new Pen(Color.FromArgb(64, 64, 0), 1f);

	private Brush brushDarkYellow = (Brush)new SolidBrush(Color.FromArgb(64, 64, 0));

	private Pen penDarkRed = new Pen(Color.FromArgb(64, 0, 0), 1f);

	private Brush brushDarkRed = (Brush)new SolidBrush(Color.FromArgb(64, 0, 0));

	private Rectangle[] meterBarArray = new Rectangle[40];

	private List<Rectangle> meterBarList = new List<Rectangle>();

	private byte[] Bar_Number = new byte[61]
	{
		40, 39, 38, 37, 36, 35, 34, 33, 32, 31,
		30, 29, 28, 27, 26, 25, 24, 23, 22, 21,
		20, 19, 18, 17, 16, 15, 14, 13, 12, 12,
		11, 11, 10, 10, 9, 9, 8, 8, 7, 7,
		6, 6, 6, 5, 5, 5, 4, 4, 4, 4,
		3, 3, 3, 3, 2, 2, 2, 2, 1, 1,
		0
	};

	private const int OFFSET = 5;

	private int barX;

	private int barY;

	private Point barPoint;

	private const int BAR_WIDTH = 10;

	private const int BAR_HEIGHT = 3;

	private Size barSize = new Size(10, 3);

	private int tickX;

	private int tickY;

	private const int TICK_LENGTH = 5;

	private int numberX;

	private const int NUM_BARS = 40;

	private int numBarsLit;

	private int peakHoldBar;

	private int barNum;

	private const int YELLOW_LIMIT = 16;

	private const int RED_LIMIT = 29;

	private Rectangle meterBar;

	private Meter_Side meterSide = Meter_Side.Left;

	private int dBvalue;

	private int peakHold;

	private bool showValue;

	private BarColour_Unlit barColourUnlit = BarColour_Unlit.White;

	private Color backColour;

	private IContainer components = null;

	[Description("The location of the meter bars relative to the labelling.")]
	[Category("Appearance")]
	public Meter_Side MeterSide
	{
		get
		{
			return meterSide;
		}
		set
		{
			meterSide = value;
			Calculate();
			((Control)this).Invalidate();
		}
	}

	[Description("The level of the meter.")]
	[Category("Behavior")]
	public int Value
	{
		get
		{
			return dBvalue;
		}
		set
		{
			dBvalue = value;
			((Control)this).Invalidate();
		}
	}

	public int PeakHold
	{
		get
		{
			return peakHold;
		}
		set
		{
			peakHold = value;
			((Control)this).Invalidate();
		}
	}

	[Description("Show the value numerically.")]
	[Category("Appearance")]
	public bool ShowValue
	{
		get
		{
			return showValue;
		}
		set
		{
			showValue = value;
			Calculate();
			((Control)this).Invalidate();
		}
	}

	[Description("The unlit portion of the meter.")]
	[Category("Appearance")]
	public BarColour_Unlit BarColourUnlit
	{
		get
		{
			return barColourUnlit;
		}
		set
		{
			barColourUnlit = value;
			Calculate();
			((Control)this).Invalidate();
		}
	}

	public override Color BackColor
	{
		get
		{
			return backColour;
		}
		set
		{
			backColour = value;
			Calculate();
			((Control)this).Invalidate();
		}
	}

	public void Calculate()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected Obj, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected Obj, but got Unknown
		if (barColourUnlit == BarColour_Unlit.Background)
		{
			unlit = backColour;
		}
		else
		{
			unlit = Color.FromName(barColourUnlit.ToString());
		}
		penUnlit = new Pen(unlit, 1f);
		brushUnlit = (Brush)new SolidBrush(unlit);
		if (meterSide == Meter_Side.Left)
		{
			barX = ((Control)this).DisplayRectangle.X;
		}
		else
		{
			barX = ((Control)this).DisplayRectangle.Width - 10 - 1;
		}
	}

	public PPMMeter()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected Obj, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected Obj, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected Obj, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected Obj, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected Obj, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected Obj, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected Obj, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected Obj, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected Obj, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected Obj, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected Obj, but got Unknown
		InitializeComponent();
		base.SetStyle((ControlStyles)8192, true);
		base.SetStyle((ControlStyles)65536, true);
		base.SetStyle((ControlStyles)16, true);
		base.SetStyle((ControlStyles)512, true);
		base.SetStyle((ControlStyles)2048, true);
		base.SetStyle((ControlStyles)2, true);
		((Control)this).BackColor = Color.White;
		ShowValue = false;
		Value = 0;
		((Control)this).Cursor = Cursors.Arrow;
		((Control)this).Size = new Size(60, 190);
		Calculate();
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (showValue)
		{
			ShowValue = false;
		}
		else
		{
			ShowValue = true;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Expected Obj, but got Unknown
		Graphics graphics = e.Graphics;
		if (showValue)
		{
			if (meterSide == Meter_Side.Left)
			{
				graphics.DrawString(dBvalue.ToString(), labelFont, brushBlack, (float)(((Control)this).DisplayRectangle.Width - 7 * dBvalue.ToString().Length), (float)(((Control)this).DisplayRectangle.Height / 2));
			}
			else
			{
				graphics.DrawString(dBvalue.ToString(), labelFont, brushBlack, (float)((Control)this).DisplayRectangle.X, (float)(((Control)this).DisplayRectangle.Height / 2));
			}
		}
		Console.WriteLine(Math.Abs(dBvalue));
		numBarsLit = Bar_Number[Math.Abs(dBvalue)];
		peakHoldBar = Bar_Number[Math.Abs(peakHold)];
		for (int i = 0; i < 40; i++)
		{
			barY = ((Control)this).DisplayRectangle.Height - 3 * (i + 1) - 5 - i;
			barPoint = new Point(barX, barY);
			meterBar = new Rectangle(barPoint, barSize);
			if (i <= numBarsLit || i == peakHoldBar)
			{
				if (i < 16)
				{
					graphics.DrawRectangle(penGreen, meterBar);
					graphics.FillRectangle(brushGreen, meterBar);
				}
				else if (i < 29)
				{
					graphics.DrawRectangle(penYellow, meterBar);
					graphics.FillRectangle(brushYellow, meterBar);
				}
				else
				{
					graphics.DrawRectangle(penRed, meterBar);
					graphics.FillRectangle(brushRed, meterBar);
				}
			}
			else if (barColourUnlit == BarColour_Unlit.Coloured)
			{
				if (i < 21)
				{
					graphics.DrawRectangle(penDarkGreen, meterBar);
					graphics.FillRectangle(brushDarkGreen, meterBar);
				}
				else if (i < 29)
				{
					graphics.DrawRectangle(penDarkYellow, meterBar);
					graphics.FillRectangle(brushDarkYellow, meterBar);
				}
				else
				{
					graphics.DrawRectangle(penDarkRed, meterBar);
					graphics.FillRectangle(brushDarkRed, meterBar);
				}
			}
			else if (barColourUnlit == BarColour_Unlit.Background)
			{
				graphics.DrawRectangle(penUnlit, meterBar);
				graphics.FillRectangle(brushUnlit, meterBar);
			}
			else if (barColourUnlit != BarColour_Unlit.None)
			{
				graphics.DrawRectangle(penUnlit, meterBar);
				graphics.FillRectangle(brushUnlit, meterBar);
			}
			graphics.DrawLine(new Pen(backColour), barPoint, new Point(barPoint.X + 10, barPoint.Y));
			barNum = i + 1;
			if (meterSide == Meter_Side.Left)
			{
				tickX = meterBar.X + meterBar.Width;
				numberX = tickX + 5 + 1 + 6 * (3 - (barNum - 37).ToString().Length);
			}
			else
			{
				tickX = meterBar.X - 5;
				numberX = tickX - 23 + 6 * (3 - (barNum - 37).ToString().Length);
			}
			tickY = meterBar.Y - 1;
			if (barNum == 40)
			{
				graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
				graphics.DrawString("3", labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
			}
			else if (barNum > 11 && barNum <= 40)
			{
				if ((barNum + 3) % 5 == 0)
				{
					graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
					graphics.DrawString((barNum - 37).ToString(), labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
				}
				else
				{
					graphics.DrawLine(penGray, tickX, tickY, tickX + 5, tickY);
				}
			}
			else if (barNum == 10)
			{
				graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
				graphics.DrawString("-30", labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
			}
			else if (barNum == 7)
			{
				graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
				graphics.DrawString("-35", labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
			}
			else if (barNum == 4)
			{
				graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
				graphics.DrawString("-45", labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
			}
			else if (barNum == 1)
			{
				graphics.DrawLine(penBlack, tickX, tickY, tickX + 5, tickY);
				graphics.DrawString("-55", labelFont, brushBlack, (float)numberX, (float)(tickY - 5));
			}
		}
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
		components = new Container();
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
	}
}
