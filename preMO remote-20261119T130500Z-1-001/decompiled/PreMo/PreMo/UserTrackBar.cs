using System.ComponentModel;
using System.Windows.Forms;

namespace PreMo;

public class UserTrackBar : UserControl
{
	private IContainer components = null;

	public UserTrackBar()
	{
		InitializeComponent();
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
