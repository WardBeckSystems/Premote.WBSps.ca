using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
using PreMoBLL;

namespace PreMo;

public class Update : Form
{
	private FileInfo fileinfo;

	private IPInfo ipinfo = new IPInfo();

	private UpdateBLL UB = new UpdateBLL();

	private IContainer components = null;

	private Button btnReadFile;

	private Button btnReboot;

	private Button btnUpdate;

	private Label label1;

	private TextBox txtMessage;

	private ProgressBar progressBar1;

	private ComboBox cboIP;

	private BindingSource iPInfoBindingSource;

	private BindingSource iPInfoBindingSource1;

	public Update()
	{
		InitializeComponent();
	}

	private void btnReadFile_Click(object sender, EventArgs e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		fileinfo = new FileInfo();
		OpenFileDialog val = new OpenFileDialog();
		((FileDialog)val).Title = "Select File";
		((FileDialog)val).InitialDirectory = "c:\\";
		((CommonDialog)val).ShowDialog();
		((FileDialog)val).Filter = "Binary|*.bin";
		fileinfo.path = ((FileDialog)val).FileName;
		new ReadFileBLL().SaveFile(fileinfo);
		string str = fileinfo.path + " File Length:" + Convert.ToString(fileinfo.FileLength);
		ShowMsg(str);
		ShowMsg("packnum:" + fileinfo.PackNum);
		ShowMsg("modLength:" + fileinfo.ModLength);
	}

	private void Update_Load(object sender, EventArgs e)
	{
		ShowMsg("Welcome WBS Firmware Update System!");
		IOrderedEnumerable<KeyValuePair<string, TcpClient>> orderedEnumerable = IPInfo.dictIP.OrderBy((KeyValuePair<string, TcpClient> pair) => pair.Key);
		foreach (KeyValuePair<string, TcpClient> item in orderedEnumerable)
		{
			((Control)cboIP).Text = item.Key;
			cboIP.Items.Add((object)item.Key);
		}
	}

	private void ShowMsg(string str)
	{
		((TextBoxBase)txtMessage).AppendText(str + "\r\n");
	}

	private void btnReboot_Click(object sender, EventArgs e)
	{
		TcpClient tcpclient = IPInfo.dictIP[cboIP.SelectedItem.ToString()];
		UB.Reboot(tcpclient, "REBOOT");
		WaitACK(tcpclient);
	}

	public async void WaitACK(TcpClient tcpclient)
	{
		string str = await Task.Run(() => UB.WaitBoot(tcpclient));
		ShowMsg(str);
		UB.UpdateARM(str, tcpclient, fileinfo);
		WaitACK(tcpclient);
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
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected Obj, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected Obj, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Expected Obj, but got Unknown
		components = new Container();
		btnReadFile = new Button();
		btnReboot = new Button();
		btnUpdate = new Button();
		label1 = new Label();
		txtMessage = new TextBox();
		progressBar1 = new ProgressBar();
		cboIP = new ComboBox();
		iPInfoBindingSource = new BindingSource(components);
		iPInfoBindingSource1 = new BindingSource(components);
		((ISupportInitialize)iPInfoBindingSource).BeginInit();
		((ISupportInitialize)iPInfoBindingSource1).BeginInit();
		((Control)this).SuspendLayout();
		((Control)btnReadFile).Location = new Point(12, 50);
		((Control)btnReadFile).Name = "btnReadFile";
		((Control)btnReadFile).Size = new Size(75, 23);
		((Control)btnReadFile).TabIndex = 0;
		((Control)btnReadFile).Text = "READFILE";
		((ButtonBase)btnReadFile).UseVisualStyleBackColor = true;
		((Control)btnReadFile).Click += btnReadFile_Click;
		((Control)btnReboot).Location = new Point(124, 50);
		((Control)btnReboot).Name = "btnReboot";
		((Control)btnReboot).Size = new Size(75, 23);
		((Control)btnReboot).TabIndex = 1;
		((Control)btnReboot).Text = "REBOOT";
		((ButtonBase)btnReboot).UseVisualStyleBackColor = true;
		((Control)btnReboot).Click += btnReboot_Click;
		((Control)btnUpdate).Location = new Point(241, 50);
		((Control)btnUpdate).Name = "btnUpdate";
		((Control)btnUpdate).Size = new Size(75, 23);
		((Control)btnUpdate).TabIndex = 2;
		((Control)btnUpdate).Text = "UPDATE";
		((ButtonBase)btnUpdate).UseVisualStyleBackColor = true;
		((Control)label1).AutoSize = true;
		((Control)label1).Font = new Font("Microsoft Sans Serif", 12f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)label1).Location = new Point(9, 9);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(28, 20);
		((Control)label1).TabIndex = 3;
		((Control)label1).Text = "IP:";
		((Control)txtMessage).Location = new Point(12, 121);
		((TextBoxBase)txtMessage).Multiline = true;
		((Control)txtMessage).Name = "txtMessage";
		((Control)txtMessage).Size = new Size(304, 420);
		((Control)txtMessage).TabIndex = 5;
		((Control)progressBar1).Location = new Point(12, 578);
		((Control)progressBar1).Name = "progressBar1";
		((Control)progressBar1).Size = new Size(304, 23);
		((Control)progressBar1).TabIndex = 6;
		((ListControl)cboIP).FormattingEnabled = true;
		((Control)cboIP).Location = new Point(43, 11);
		((Control)cboIP).Name = "cboIP";
		((Control)cboIP).Size = new Size(137, 21);
		((Control)cboIP).TabIndex = 7;
		iPInfoBindingSource.DataSource = typeof(IPInfo);
		iPInfoBindingSource1.DataSource = typeof(IPInfo);
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(403, 665);
		((Control)this).Controls.Add((Control)(object)cboIP);
		((Control)this).Controls.Add((Control)(object)progressBar1);
		((Control)this).Controls.Add((Control)(object)txtMessage);
		((Control)this).Controls.Add((Control)(object)label1);
		((Control)this).Controls.Add((Control)(object)btnUpdate);
		((Control)this).Controls.Add((Control)(object)btnReboot);
		((Control)this).Controls.Add((Control)(object)btnReadFile);
		((Control)this).Name = "Update";
		((Control)this).Text = "Update";
		((Form)this).Load += Update_Load;
		((ISupportInitialize)iPInfoBindingSource).EndInit();
		((ISupportInitialize)iPInfoBindingSource1).EndInit();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
