using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PreMo;

public class Login : Form
{
	private string usrEntered;

	private string pwdEntered;

	private bool credMatch;

	private string xmlUserInfoPath = "C:\\Users\\Anthony P. Kuzub\\Desktop\\PreMo1.15\\PreMo\\Resources\\UserInfo.xml";

	private IContainer components = null;

	private Label label1;

	private Label label2;

	private TextBox txtPwd;

	private Button btnLogIn;

	private Button btnCancel;

	private DataGridView dataGridView1;

	private Label lbError;

	public TextBox txtUsr;

	private BindingSource loginBindingSource;

	private CustomControl1 customControl1;

	public Login()
	{
		InitializeComponent();
	}

	private void btnLogIn_Click(object sender, EventArgs e)
	{
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		DataSet dataSet = new DataSet();
		bool flag;
		try
		{
			dataSet.ReadXml(xmlUserInfoPath);
			flag = true;
		}
		catch (FileNotFoundException)
		{
			((Control)lbError).Text = "Unable to authenticate";
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		dataGridView1.DataSource = dataSet.Tables[0];
		usrEntered = ((Control)txtUsr).Text;
		pwdEntered = ((Control)txtPwd).Text;
		credMatch = false;
		if (usrEntered != "")
		{
			for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
			{
				if (usrEntered == dataSet.Tables[0].Rows[i][0].ToString() && pwdEntered == dataSet.Tables[0].Rows[i][1].ToString())
				{
					credMatch = true;
				}
			}
		}
		if (credMatch)
		{
			((Control)lbError).Text = "";
			Home home = new Home();
			((Control)home.lbUserName).Text = ((Control)txtUsr).Text;
			((Form)home).ShowDialog((IWin32Window)(object)this);
			((Control)Form.ActiveForm).Hide();
		}
		else
		{
			((Control)lbError).Text = "Username or password is incorrect";
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		((Control)txtUsr).Text = "";
		((Control)txtPwd).Text = "";
		((Control)lbError).Text = "";
	}

	private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		if (e.ColumnIndex == 1 && ((ConvertEventArgs)e).Value != null)
		{
			((ConvertEventArgs)e).Value = new string('*', ((ConvertEventArgs)e).Value.ToString().Length);
		}
	}

	private void userTrackBar1_Load(object sender, EventArgs e)
	{
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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected Obj, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected Obj, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Expected Obj, but got Unknown
		components = new Container();
		label1 = new Label();
		label2 = new Label();
		txtPwd = new TextBox();
		txtUsr = new TextBox();
		btnLogIn = new Button();
		btnCancel = new Button();
		dataGridView1 = new DataGridView();
		lbError = new Label();
		customControl1 = new CustomControl1();
		loginBindingSource = new BindingSource(components);
		((ISupportInitialize)dataGridView1).BeginInit();
		((ISupportInitialize)loginBindingSource).BeginInit();
		((Control)this).SuspendLayout();
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(110, 75);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(55, 13);
		((Control)label1).TabIndex = 0;
		((Control)label1).Text = "Username";
		((Control)label2).AutoSize = true;
		((Control)label2).Location = new Point(110, 101);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(53, 13);
		((Control)label2).TabIndex = 2;
		((Control)label2).Text = "Password";
		((Control)txtPwd).Location = new Point(169, 98);
		((Control)txtPwd).Name = "txtPwd";
		txtPwd.PasswordChar = '*';
		((Control)txtPwd).Size = new Size(100, 20);
		((Control)txtPwd).TabIndex = 3;
		((Control)txtUsr).Location = new Point(169, 72);
		((Control)txtUsr).Name = "txtUsr";
		((Control)txtUsr).Size = new Size(100, 20);
		((Control)txtUsr).TabIndex = 1;
		((Control)btnLogIn).Location = new Point(113, 187);
		((Control)btnLogIn).Name = "btnLogIn";
		((Control)btnLogIn).Size = new Size(75, 23);
		((Control)btnLogIn).TabIndex = 4;
		((Control)btnLogIn).Text = "Log In";
		((ButtonBase)btnLogIn).UseVisualStyleBackColor = true;
		((Control)btnLogIn).Click += btnLogIn_Click;
		((Control)btnCancel).Location = new Point(194, 187);
		((Control)btnCancel).Name = "btnCancel";
		((Control)btnCancel).Size = new Size(75, 23);
		((Control)btnCancel).TabIndex = 5;
		((Control)btnCancel).Text = "Cancel";
		((ButtonBase)btnCancel).UseVisualStyleBackColor = true;
		((Control)btnCancel).Click += btnCancel_Click;
		dataGridView1.AllowUserToOrderColumns = true;
		dataGridView1.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode)2;
		((Control)dataGridView1).Location = new Point(478, 46);
		((Control)dataGridView1).Name = "dataGridView1";
		((Control)dataGridView1).Size = new Size(357, 235);
		((Control)dataGridView1).TabIndex = 6;
		dataGridView1.CellFormatting += dataGridView1_CellFormatting;
		((Control)lbError).AutoSize = true;
		((Control)lbError).ForeColor = Color.Red;
		((Control)lbError).Location = new Point(110, 46);
		((Control)lbError).Name = "lbError";
		((Control)lbError).Size = new Size(0, 13);
		((Control)lbError).TabIndex = 7;
		((Control)customControl1).Location = new Point(260, 228);
		((Control)customControl1).Name = "customControl1";
		((Control)customControl1).Size = new Size(102, 72);
		((Control)customControl1).TabIndex = 8;
		((Control)customControl1).Text = "customControl1";
		loginBindingSource.DataSource = typeof(Login);
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(1033, 382);
		((Control)this).Controls.Add((Control)(object)customControl1);
		((Control)this).Controls.Add((Control)(object)lbError);
		((Control)this).Controls.Add((Control)(object)dataGridView1);
		((Control)this).Controls.Add((Control)(object)btnCancel);
		((Control)this).Controls.Add((Control)(object)btnLogIn);
		((Control)this).Controls.Add((Control)(object)txtUsr);
		((Control)this).Controls.Add((Control)(object)txtPwd);
		((Control)this).Controls.Add((Control)(object)label2);
		((Control)this).Controls.Add((Control)(object)label1);
		((Control)this).Name = "Login";
		((Control)this).Text = "Login";
		((ISupportInitialize)dataGridView1).EndInit();
		((ISupportInitialize)loginBindingSource).EndInit();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
