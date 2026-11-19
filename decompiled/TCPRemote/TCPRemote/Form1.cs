using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPRemote;

public class Form1 : Form
{
	private int icount = 0;

	private TcpClient tcpclient;

	private NetworkStream clientstream;

	private byte[] recBuff = new byte[10];

	private byte[] buffer = new byte[1024000];

	private byte[] buf = new byte[201];

	private int packLength = 0;

	private int modLength = 0;

	private int packCount = 0;

	private Thread thrSend;

	private Thread thrReceive;

	private IContainer components = null;

	private Button btnRead;

	private Button btnBoot;

	private Button btnUpdate;

	private TextBox txtIP;

	private Label label1;

	private ProgressBar pgbUpdate;

	private TextBox txtMessage;

	private System.Windows.Forms.Timer timer1;

	public object DirectoryEntry { get; private set; }

	[DSDescription("DirectoryEntryDesc")]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	[EnvironmentPermission(SecurityAction.Assert, Unrestricted = true)]
	[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public Form1()
	{
		InitializeComponent();
	}

	private void btnRead_Click(object sender, EventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected Obj, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			OpenFileDialog val = new OpenFileDialog();
			((FileDialog)val).Title = "Select File";
			((FileDialog)val).InitialDirectory = "c:\\";
			((CommonDialog)val).ShowDialog();
			((FileDialog)val).Filter = "Binary|*.bin";
			string fileName = ((FileDialog)val).FileName;
			using FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
			int num = fileStream.Read(buffer, 0, 512000);
			packLength = num / 200;
			modLength = num % 200;
			buf[0] = Convert.ToByte(0);
			for (int i = 0; i < 200; i++)
			{
				buf[i + 1] = buffer[i];
			}
			string str = fileName + " File Length:" + Convert.ToString(num);
			ShowMsg(str);
			ShowMsg("packlength:" + packLength);
			ShowMsg("modLength:" + modLength);
		}
		catch
		{
		}
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		Control.CheckForIllegalCrossThreadCalls = false;
		ShowMsg("Welcome WBS Audio Update System !");
	}

	private void ShowMsg(string str)
	{
		((TextBoxBase)txtMessage).AppendText(str + "\r\n");
	}

	private void btnBoot_Click(object sender, EventArgs e)
	{
		BootLoad();
	}

	public async void BootLoad()
	{
		packCount = 0;
		tcpclient = new TcpClient();
		await tcpclient.ConnectAsync(IPAddress.Parse(((Control)txtIP).Text), 58888);
		clientstream = tcpclient.GetStream();
		byte[] buf = new byte[5];
		await clientstream.ReadAsync(buf, 0, buf.Length);
		string str = Encoding.ASCII.GetString(buf, 0, buf.Length);
		if (str == "START")
		{
			ShowMsg(str);
			byte[] bootBuf = Encoding.ASCII.GetBytes("REBOOT");
			try
			{
				await clientstream.WriteAsync(bootBuf, 0, 6);
				await clientstream.FlushAsync();
			}
			catch
			{
			}
		}
		if (await Task.Run(() => WaitNotice()) == "DOWNLOAD")
		{
			StartBoot();
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		UpdateBoot();
	}

	public async void UpdateBoot()
	{
		tcpclient = new TcpClient();
		await tcpclient.ConnectAsync(IPAddress.Parse(((Control)txtIP).Text), 58888);
		clientstream = tcpclient.GetStream();
		byte[] buf = new byte[5];
		await clientstream.ReadAsync(buf, 0, buf.Length);
		string str = Encoding.ASCII.GetString(buf, 0, buf.Length);
		if (str == "START")
		{
			packCount = 0;
			StartBoot();
		}
	}

	private void Connect()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected Obj, but got Unknown
		while (true)
		{
			try
			{
				byte[] array = new byte[5];
				clientstream.Read(array, 0, array.Length);
				string text = Encoding.ASCII.GetString(array, 0, array.Length);
				if (text == "START")
				{
					ShowMsg(text);
					((Control)this).Invoke((Delegate)(MethodInvoker)(() =>
					{
						timer1.Start();
					}));
					break;
				}
			}
			catch
			{
			}
		}
	}

	public async Task<string> WaitNotice()
	{
		while (true)
		{
			try
			{
				byte[] buf = new byte[8];
				await clientstream.ReadAsync(buf, 0, buf.Length);
				string str = Encoding.ASCII.GetString(buf, 0, buf.Length);
				ShowMsg(str);
				if (str == "DOWNLOAD")
				{
					packCount = 0;
					return str;
				}
			}
			catch
			{
			}
		}
	}

	public bool Delay(int delayTime)
	{
		DateTime now = DateTime.Now;
		int seconds;
		do
		{
			seconds = (DateTime.Now - now).Seconds;
			Application.DoEvents();
		}
		while (seconds < delayTime);
		return true;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		StartBoot();
	}

	public async void StartBoot()
	{
		byte[] recBuff = new byte[2];
		try
		{
			if (packCount <= packLength - 1)
			{
				await clientstream.WriteAsync(buf, 0, 201);
				await clientstream.FlushAsync();
			}
			else
			{
				await clientstream.WriteAsync(buf, 0, modLength + 1);
				await clientstream.FlushAsync();
				packCount++;
				buf[0] = Convert.ToByte(0);
				for (int i = 0; i < 200; i++)
				{
					buf[i + 1] = buffer[i];
				}
			}
			await clientstream.ReadAsync(recBuff, 0, 2);
			ShowMsg(Convert.ToInt32(recBuff[0]) + " LENGTH: " + Convert.ToInt32(recBuff[1]));
			if (packCount <= packLength - 2)
			{
				if (Convert.ToInt32(recBuff[1]) == 201 && Convert.ToInt32(recBuff[0]) + packCount / 200 * 200 == packCount)
				{
					packCount++;
					buf[0] = Convert.ToByte(packCount % 200);
					for (int j = 0; j < 200; j++)
					{
						buf[j + 1] = buffer[j + 200 * packCount];
					}
				}
			}
			else if (packCount == packLength - 1)
			{
				packCount++;
				buf[0] = Convert.ToByte(packCount % 200);
				for (int k = 0; k < modLength; k++)
				{
					buf[k + 1] = buffer[k + 200 * packLength];
				}
			}
		}
		catch (Exception)
		{
			throw;
		}
		pgbUpdate.Value = (int)(Convert.ToDouble(packCount) / Convert.ToDouble(packLength) * 100.0);
		if (packCount == packLength + 1)
		{
			if (tcpclient != null)
			{
				clientstream.Dispose();
				tcpclient.Close();
			}
		}
		else
		{
			StartBoot();
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
		components = new Container();
		btnRead = new Button();
		btnBoot = new Button();
		btnUpdate = new Button();
		txtIP = new TextBox();
		label1 = new Label();
		pgbUpdate = new ProgressBar();
		txtMessage = new TextBox();
		timer1 = new System.Windows.Forms.Timer(components);
		((Control)this).SuspendLayout();
		((Control)btnRead).Location = new Point(228, 45);
		((Control)btnRead).Name = "btnRead";
		((Control)btnRead).Size = new Size(75, 23);
		((Control)btnRead).TabIndex = 0;
		((Control)btnRead).Text = "READFILE";
		((ButtonBase)btnRead).UseVisualStyleBackColor = true;
		((Control)btnRead).Click += btnRead_Click;
		((Control)btnBoot).Location = new Point(353, 45);
		((Control)btnBoot).Name = "btnBoot";
		((Control)btnBoot).Size = new Size(75, 23);
		((Control)btnBoot).TabIndex = 1;
		((Control)btnBoot).Text = "REBOOT";
		((ButtonBase)btnBoot).UseVisualStyleBackColor = true;
		((Control)btnBoot).Click += btnBoot_Click;
		((Control)btnUpdate).Location = new Point(453, 45);
		((Control)btnUpdate).Name = "btnUpdate";
		((Control)btnUpdate).Size = new Size(75, 23);
		((Control)btnUpdate).TabIndex = 2;
		((Control)btnUpdate).Text = "UPDATE";
		((ButtonBase)btnUpdate).UseVisualStyleBackColor = true;
		((Control)btnUpdate).Click += btnUpdate_Click;
		((Control)txtIP).Location = new Point(87, 48);
		((Control)txtIP).Name = "txtIP";
		((Control)txtIP).Size = new Size(114, 20);
		((Control)txtIP).TabIndex = 3;
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(37, 51);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(20, 13);
		((Control)label1).TabIndex = 5;
		((Control)label1).Text = "IP:";
		((Control)pgbUpdate).Location = new Point(12, 636);
		((Control)pgbUpdate).Name = "pgbUpdate";
		((Control)pgbUpdate).Size = new Size(488, 23);
		((Control)pgbUpdate).TabIndex = 6;
		((Control)txtMessage).Location = new Point(40, 104);
		((TextBoxBase)txtMessage).Multiline = true;
		((Control)txtMessage).Name = "txtMessage";
		((Control)txtMessage).Size = new Size(287, 526);
		((Control)txtMessage).TabIndex = 7;
		timer1.Interval = 120;
		timer1.Tick += timer1_Tick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(571, 671);
		((Control)this).Controls.Add((Control)(object)txtMessage);
		((Control)this).Controls.Add((Control)(object)pgbUpdate);
		((Control)this).Controls.Add((Control)(object)label1);
		((Control)this).Controls.Add((Control)(object)txtIP);
		((Control)this).Controls.Add((Control)(object)btnUpdate);
		((Control)this).Controls.Add((Control)(object)btnBoot);
		((Control)this).Controls.Add((Control)(object)btnRead);
		((Control)this).Name = "Form1";
		((Control)this).Text = "PreMoUpdate";
		((Form)this).Load += Form1_Load;
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
