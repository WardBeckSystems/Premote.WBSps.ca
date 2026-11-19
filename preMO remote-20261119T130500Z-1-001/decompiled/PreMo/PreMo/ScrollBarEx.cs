using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PreMo;

[ToolboxBitmap(typeof(TrackBar))]
public class ScrollBarEx : Control
{
	private int maxNum;

	private int value;

	private int minNum;

	private Color bordBrush = Color.FromArgb(98, 98, 98);

	private Color thumbBordBrush = Color.FromArgb(60, 136, 202);

	private Color tickBrush = Color.FromArgb(230, 236, 236);

	private Color bordFillStartColor = Color.FromArgb(98, 98, 98);

	private Color bordFileEndColor = Color.White;

	private Color fillThumbStartColor = Color.FromArgb(220, 65, 182, 254);

	private Color fillThumbEndColor = Color.FromArgb(200, Color.White);

	private IContainer components = null;

	[Description("最大值")]
	[Category("值")]
	public int MaxNum
	{
		get
		{
			return maxNum;
		}
		set
		{
			if (value < MinNum || value < 0)
			{
				throw new Exception("最大值必须大于0. 且大于最小值!");
			}
			maxNum = value;
			((Control)this).Refresh();
		}
	}

	[Description("值")]
	[Category("值")]
	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value < MinNum || value < 0 || value > MaxNum)
			{
				throw new Exception("值必须是MinNum与MaxNum之间的值");
			}
			if (this.value != value)
			{
				this.value = value;
				((Control)this).Refresh();
				OnValueChange();
			}
		}
	}

	[Description("最小值")]
	[Category("值")]
	public int MinNum
	{
		get
		{
			return minNum;
		}
		set
		{
			if (value < 0 || value > MaxNum)
			{
				throw new Exception("最大值必须大于0. 且小于最大值!");
			}
			minNum = value;
			((Control)this).Refresh();
		}
	}

	[Description("边框颜色")]
	[Category("外观")]
	public Color BordBrush
	{
		get
		{
			return bordBrush;
		}
		set
		{
			bordBrush = value;
			((Control)this).Refresh();
		}
	}

	[Description("滑块边框")]
	[Category("外观")]
	public Color ThumbBordBrush
	{
		get
		{
			return thumbBordBrush;
		}
		set
		{
			thumbBordBrush = value;
			((Control)this).Refresh();
		}
	}

	[Description("刻度颜色")]
	[Category("外观")]
	public Color TickBrush
	{
		get
		{
			return tickBrush;
		}
		set
		{
			tickBrush = value;
			((Control)this).Refresh();
		}
	}

	[Description("刻度渐变起始色")]
	[Category("外观")]
	public Color FillStartColor
	{
		get
		{
			return bordFillStartColor;
		}
		set
		{
			bordFillStartColor = value;
			((Control)this).Refresh();
		}
	}

	[Description("刻度渐变结束色")]
	[Category("外观")]
	public Color FillEndColor
	{
		get
		{
			return bordFileEndColor;
		}
		set
		{
			bordFileEndColor = value;
			((Control)this).Refresh();
		}
	}

	[Description("滑块渐变起始色")]
	[Category("外观")]
	public Color FillThumbStartColor
	{
		get
		{
			return fillThumbStartColor;
		}
		set
		{
			fillThumbStartColor = value;
		}
	}

	[Description("滑块渐变结束色")]
	[Category("外观")]
	public Color FillThumbEndColor
	{
		get
		{
			return fillThumbEndColor;
		}
		set
		{
			fillThumbEndColor = value;
		}
	}

	public AutoScaleMode AutoScaleMode
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[Description("当Value发生变化时触发该事件")]
	[Category("值")]
	public event EventHandler ValueChanged = null;

	public ScrollBarEx()
	{
		((Control)this).Width = 100;
		((Control)this).Height = 20;
		MaxNum = 10;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected Obj, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected Obj, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected Obj, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected Obj, but got Unknown
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Expected Obj, but got Unknown
		Bitmap val = new Bitmap(((Control)this).Width, ((Control)this).Height);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				val2.Clear(Color.Transparent);
				val2.SmoothingMode = (SmoothingMode)4;
				float num = (float)(((Control)this).Width - 9) / (float)MaxNum;
				Rectangle rectangle = new Rectangle(0, ((Control)this).Height - 15, ((Control)this).Width - 1, 5);
				Pen val3 = new Pen(BordBrush);
				try
				{
					val2.DrawRectangle(val3, rectangle);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				LinearGradientBrush val4 = new LinearGradientBrush(new Point(0, ((Control)this).Height - 15), new Point(((Control)this).Width - 1, 5), FillStartColor, FillEndColor);
				try
				{
					rectangle.Inflate(-1, -1);
					val2.FillRectangle((Brush)(object)val4, rectangle);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
				Pen val5 = new Pen(TickBrush, 0.2f);
				try
				{
					for (int i = 0; i <= MaxNum; i++)
					{
						int num2 = (int)(4f + (float)i * num);
						int num3 = ((Control)this).Height - 3;
						val2.DrawLine(val5, num2, num3, num2, num3 + 3);
					}
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
				LinearGradientBrush val6 = new LinearGradientBrush(new Point((int)(4f + (float)Value * num) - 3, ((Control)this).Height - 20), new Point(4 + (int)((float)Value * num) + 3, ((Control)this).Height - 2), FillThumbStartColor, FillThumbEndColor);
				try
				{
					int num4 = (int)(4f + (float)Value * num);
					int num5 = num4 - 3;
					int num6 = 6;
					int num7 = 12;
					int num8 = ((Control)this).Height - 20;
					GraphicsPath val7 = new GraphicsPath();
					val7.AddLines(new Point[6]
					{
						new Point(num5, num8),
						new Point(num5 + num6, num8),
						new Point(num5 + num6, num8 + num7),
						new Point(num4, ((Control)this).Height - 3),
						new Point(num5, num8 + num7),
						new Point(num5, num8)
					});
					val2.FillPath((Brush)(object)val6, val7);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
				Pen val8 = new Pen(ThumbBordBrush);
				try
				{
					int num9 = (int)(4f + (float)Value * num);
					int num10 = num9 - 3;
					int num11 = 6;
					int num12 = 12;
					int num13 = ((Control)this).Height - 20;
					val2.DrawLines(val8, new Point[6]
					{
						new Point(num10, num13),
						new Point(num10 + num11, num13),
						new Point(num10 + num11, num13 + num12),
						new Point(num9, ((Control)this).Height - 3),
						new Point(num10, num13 + num12),
						new Point(num10, num13)
					});
				}
				finally
				{
					((IDisposable)val8)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			e.Graphics.DrawImage((Image)(object)val, 0, 0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (e.Y >= ((Control)this).Height - 15)
		{
			float num = (float)(((Control)this).Width - 8) / (float)MaxNum;
			int num2 = (int)(((float)(e.X - 4) + num / 2f) / num);
			if (num2 <= MaxNum)
			{
				Value = ((num2 > 0) ? num2 : 0);
			}
			else
			{
				value = MaxNum;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		base.OnMouseMove(e);
		if ((int)e.Button == 1048576)
		{
			float num = (float)(((Control)this).Width - 9) / (float)MaxNum;
			int num2 = (int)(((float)(e.X - 4) + num / 2f) / num);
			if (num2 <= MaxNum)
			{
				Value = ((num2 > 0) ? num2 : 0);
			}
			else
			{
				value = MaxNum;
			}
		}
	}

	public virtual void OnValueChange()
	{
		if (ValueChanged != null)
		{
			EventArgs e = new EventArgs();
			ValueChanged(this, e);
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		((Control)this).Refresh();
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
		this.components = new System.ComponentModel.Container();
		this.AutoScaleMode = (AutoScaleMode)1;
	}
}
