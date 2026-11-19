using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Bonjour;
using Model;
using Model.AES70;
using OCAAES70.OCC.ControlClasses.Managers;
using OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;
using PreMo.Properties;
using PreMoBLL;

namespace PreMo;

public class Home : Form
{
	protected delegate void ReceiveARMDelegate(TcpClient client);

	public class AutoClosingMessageBox
	{
		private System.Threading.Timer _timeoutTimer;

		private string _caption;

		private const int WM_CLOSE = 16;

		private AutoClosingMessageBox(string text, string caption, int timeout)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			_caption = caption;
			_timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, -1);
			MessageBox.Show(text, caption);
		}

		public static void Show(string text, string caption, int timeout)
		{
			new AutoClosingMessageBox(text, caption, timeout);
		}

		private void OnTimerElapsed(object state)
		{
			IntPtr intPtr = FindWindow(null, _caption);
			if (intPtr != IntPtr.Zero)
			{
				SendMessage(intPtr, 16u, IntPtr.Zero, IntPtr.Zero);
			}
			_timeoutTimer.Dispose();
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}

	private OcaBlock block = new OcaBlock();

	private GainBLL gainbll = new GainBLL();

	private SwitchBLL switchbll = new SwitchBLL();

	private DSPBLL dspbll = new DSPBLL();

	private ResponseBLL responsebll;

	private BonjourBLL bonjourbll = new BonjourBLL();

	private OcaFileInfo fileinfo;

	private FileBLL filebll;

	private IPInfo ipinfo;

	private TcpClient tcpclient = null;

	private System.Timers.Timer timer = new System.Timers.Timer();

	private int childFormNumber = 0;

	private bool flaginit = false;

	private byte flag = 1;

	private bool load = false;

	private byte[] buf = new byte[200];

	private CancellationTokenSource _cts;

	private static bool boolstop = true;

	private DNSSDEventManager m_eventManager = null;

	private DNSSDService m_service = null;

	private DNSSDService m_registrar = null;

	private DNSSDService m_browser = null;

	private DNSSDService m_resolver = null;

	private DNSSDService m_getaddrinfo = null;

	private int linkstate = 0;

	private string m_name;

	private Socket m_socket = null;

	private const int BUFFER_SIZE = 1024;

	public byte[] m_buffer = new byte[1024];

	public bool m_complete = false;

	public StringBuilder m_sb = new StringBuilder();

	private string localIP;

	private string IPsubnet;

	private int[] NetNumber = new int[4];

	private ConnectionBLL CB;

	private NoticeBLL noticebll;

	private OcaPropertyParameter ocaparameter;

	private OcaSubscriptionManager ocasubmanager = new OcaSubscriptionManager();

	private OcaUnmarshalByte OcaStruct = new OcaUnmarshalByte();

	private Ocp1NotificationPdu ocp1pdu;

	private OcaSwitchInfo switchinfo = new OcaSwitchInfo();

	private PeerData peer = new PeerData();

	private bool failsubnet = true;

	private IContainer components = null;

	private MenuStrip menuStrip;

	private ToolStrip toolStrip;

	private StatusStrip statusStrip;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripMenuItem printSetupToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripSeparator toolStripSeparator8;

	private ToolStripStatusLabel toolStripStatusLabel;

	private ToolStripMenuItem aboutToolStripMenuItem;

	private ToolStripMenuItem tileHorizontalToolStripMenuItem;

	private ToolStripMenuItem fileMenu;

	private ToolStripMenuItem newToolStripMenuItem;

	private ToolStripMenuItem openToolStripMenuItem;

	private ToolStripMenuItem saveToolStripMenuItem;

	private ToolStripMenuItem saveAsToolStripMenuItem;

	private ToolStripMenuItem printToolStripMenuItem;

	private ToolStripMenuItem printPreviewToolStripMenuItem;

	private ToolStripMenuItem exitToolStripMenuItem;

	private ToolStripMenuItem editMenu;

	private ToolStripMenuItem redoToolStripMenuItem;

	private ToolStripMenuItem copyToolStripMenuItem;

	private ToolStripMenuItem viewMenu;

	private ToolStripMenuItem toolBarToolStripMenuItem;

	private ToolStripMenuItem toolsMenu;

	private ToolStripMenuItem optionsToolStripMenuItem;

	private ToolStripMenuItem windowsMenu;

	private ToolStripMenuItem newWindowToolStripMenuItem;

	private ToolStripMenuItem cascadeToolStripMenuItem;

	private ToolStripMenuItem tileVerticalToolStripMenuItem;

	private ToolStripMenuItem closeAllToolStripMenuItem;

	private ToolStripMenuItem arrangeIconsToolStripMenuItem;

	private ToolStripMenuItem helpMenu;

	private ToolStripMenuItem contentsToolStripMenuItem;

	private ToolStripMenuItem indexToolStripMenuItem;

	private ToolStripMenuItem searchToolStripMenuItem;

	private ToolStripButton newToolStripButton;

	private ToolStripButton openToolStripButton;

	private ToolStripButton saveToolStripButton;

	private ToolStripButton printToolStripButton;

	private ToolStripButton printPreviewToolStripButton;

	private ToolStripButton helpToolStripButton;

	private ToolTip toolTip;

	private Panel panTop;

	private Panel panBottom;

	private Panel panel2;

	private Panel panel3;

	private Panel panel4;

	private TextBox txtIP;

	private Label label1;

	private Button btnPreSet;

	private TextBox txtDownload2;

	private TextBox txtDownload1;

	private TrackBar trbDownload2;

	private TrackBar trbDownload1;

	private Label label7;

	private Label label6;

	private TextBox txtVolume2;

	private TextBox txtVolume1;

	private TrackBar trbVolume2;

	private TrackBar trbVolume1;

	private Label label14;

	private NumericUpDown nUDDown2_BASS;

	private NumericUpDown nUDDown2_Treble;

	private NumericUpDown nUDDown2_HPF;

	private NumericUpDown nUDDown1_BASS;

	private NumericUpDown nUDDown1_Treble;

	private NumericUpDown nUDDown1_HPF;

	private Button btnSave;

	private Label label24;

	private Label label23;

	private TrackBar trbBright;

	private Label label22;

	private NumericUpDown numUpload;

	private NumericUpDown numHeadset;

	private Label label26;

	private Label label25;

	private TrackBar trbHeadGM;

	private Label lbUpGM;

	private Label lbHeadGM;

	private TextBox txtUpGM;

	private TextBox txtHeadGM;

	private TrackBar trbUpGM;

	private PictureBox picUpload;

	private PictureBox picHeadset;

	private Label label30;

	private TextBox txtDown2;

	private TextBox txtDown1;

	private ToolTip toolTipCustomer;

	private NumericUpDown nUDFileNum;

	private ComboBox comboBox1;

	private Label label3;

	private Label label2;

	private ErrorProvider errorProvider1;

	private Button btnSaveAs;

	private Label label4;

	private OnOffButton CheckDownRight_Phase;

	private OnOffButton CheckDownRight_Mute;

	private OnOffButton CheckDownLeft_Phase;

	private OnOffButton CheckDownLeft_Mute;

	private OnOffButton Mute_Headset;

	private OnOffButton MicMute;

	private OnOffButton Mute_Upload;

	private OnOffButton DisplayMod;

	private OnOffButton CheckRearHeadset_Mute;

	private OnOffButton CheckFrontHeadset_Mute;

	private Label label11;

	private Label label10;

	private Label label5;

	public Label lbUserName;

	public Label label27;

	private Button btnUsrMng;

	private ScrollBarEx scrollBarEx1;

	private Panel panel5;

	private Label label32;

	private System.Windows.Forms.Timer timDevice;

	private TreeView trvDevice;

	private PPMMeter DLR_PPM;

	private PPMMeter DLL_PPM;

	private Panel panel1;

	private PPMMeter Channel2_PPM;

	private PPMMeter Channel1_PPM;

	private MICLINE micline2;

	private MICLINE micline1;

	private OnOffButton CheckLeft_Phantom;

	private OnOffButton CheckLeft_Pad;

	private OnOffButton CheckLeft_Mute;

	private OnOffButton CheckRight_Phase;

	private OnOffButton CheckRight_Mute;

	private OnOffButton CheckRight_Pad;

	private OnOffButton CheckRight_Phantom;

	private OnOffButton CheckLeft_Phase;

	private TextBox txtCH2;

	private TextBox txtCH1;

	private Label label29;

	private Label label21;

	private Label label20;

	private Label label19;

	private Label label18;

	private Label label17;

	private Label label16;

	private Label label15;

	private NumericUpDown nUD2_BASS;

	private NumericUpDown nUD2_Treble;

	private NumericUpDown nUD2_HPF;

	private NumericUpDown nUD1_BASS;

	private NumericUpDown nUD1_Treble;

	private NumericUpDown nUD1_HPF;

	private Label label13;

	private Label label12;

	private Label label9;

	private Label label8;

	private TextBox txtChannel2;

	private TextBox txtChannel1;

	private TrackBar trbInput2;

	private TrackBar trbInput1;

	public Home()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected Obj, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Expected Obj, but got Unknown
		InitializeComponent();
		((Control)txtChannel1).Text = "0 dB";
		((Control)txtChannel2).Text = "0 dB";
		((Control)txtDownload1).Text = "0 dB";
		((Control)txtDownload2).Text = "0 dB";
		((Control)txtVolume1).Text = "0 dB";
		((Control)txtVolume2).Text = "0 dB";
		picHeadset.Image = (Image)(object)Resources._1;
		picUpload.Image = (Image)(object)Resources._7;
		CheckFrontHeadset_Mute.Checked = true;
		CheckRearHeadset_Mute.Checked = true;
		ManagementClass val = new ManagementClass("Win32_NetworkAdapterConfiguration");
		ManagementObjectCollection instances = val.GetInstances();
		ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val2 = (ManagementObject)enumerator.Current;
				if (Convert.ToBoolean(((ManagementBaseObject)val2)["ipEnabled"]))
				{
					localIP = (((ManagementBaseObject)val2)["IPAddress"] as string[])[0];
					IPsubnet = (((ManagementBaseObject)val2)["IPSubnet"] as string[])[0];
				}
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		NetNumber = bonjourbll.GetNetNum(instances);
		Bonjour_Init();
	}

	private void ShowNewForm(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected Obj, but got Unknown
		Form val = new Form();
		val.MdiParent = (Form)(object)this;
		((Control)val).Text = "Window " + childFormNumber++;
		((Control)val).Show();
	}

	private void OpenFile(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		OpenFileDialog val = new OpenFileDialog();
		((FileDialog)val).InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		((FileDialog)val).Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
		if ((int)((CommonDialog)val).ShowDialog((IWin32Window)(object)this) == 1)
		{
			string fileName = ((FileDialog)val).FileName;
		}
	}

	private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		SaveFileDialog val = new SaveFileDialog();
		((FileDialog)val).InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		((FileDialog)val).Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
		if ((int)((CommonDialog)val).ShowDialog((IWin32Window)(object)this) == 1)
		{
			string fileName = ((FileDialog)val).FileName;
		}
	}

	private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).Close();
	}

	private void CutToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Control)toolStrip).Visible = toolBarToolStripMenuItem.Checked;
	}

	private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).LayoutMdi((MdiLayout)0);
	}

	private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).LayoutMdi((MdiLayout)2);
	}

	private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).LayoutMdi((MdiLayout)1);
	}

	private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).LayoutMdi((MdiLayout)3);
	}

	private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Form[] mdiChildren = ((Form)this).MdiChildren;
		foreach (Form val in mdiChildren)
		{
			val.Close();
		}
	}

	private void panel1_Paint(object sender, PaintEventArgs e)
	{
		ControlPaint.DrawBorder(e.Graphics, ((Control)panel1).ClientRectangle, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3);
	}

	private void panel2_Paint(object sender, PaintEventArgs e)
	{
		ControlPaint.DrawBorder(e.Graphics, ((Control)panel2).ClientRectangle, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3);
	}

	private void panel3_Paint(object sender, PaintEventArgs e)
	{
		ControlPaint.DrawBorder(e.Graphics, ((Control)panel3).ClientRectangle, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3);
	}

	private void panel4_Paint(object sender, PaintEventArgs e)
	{
		ControlPaint.DrawBorder(e.Graphics, ((Control)panel4).ClientRectangle, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3);
	}

	private void panBottom_Paint(object sender, PaintEventArgs e)
	{
		ControlPaint.DrawBorder(e.Graphics, ((Control)panBottom).ClientRectangle, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3, Color.Green, 5, (ButtonBorderStyle)3);
	}

	private void txtIP_TextChanged(object sender, EventArgs e)
	{
		((Control)txtIP).BackColor = Color.White;
	}

	private void editMenu_Click(object sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Form)new Update()).ShowDialog();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		SwitchOn(144u, "ppm");
	}

	private void trvDevice_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (((Control)txtIP).Text != "")
		{
			SwitchOn(144u, "ppm");
			Thread.Sleep(500);
		}
		if (!boolstop)
		{
			Task task = Task.Run(() =>
			{
				boolstop = true;
				_cts?.Cancel();
			});
			AutoClosingMessageBox.Show("Change Device", "Caption", 5000);
			task.ContinueWith((Task t) =>
			{
				boolstop = false;
			});
		}
		tcpclient = new TcpClient();
		_cts = new CancellationTokenSource();
		ipinfo = new IPInfo();
		ipinfo.IPAddress = trvDevice.SelectedNode.Name;
		ipinfo.intPort = 58888;
		if (ConnectionBLL.IPCheck(ipinfo.IPAddress))
		{
			CallWithAsync(ipinfo);
		}
	}

	public async void CallWithAsync(IPInfo ipinfo)
	{
		CB = new ConnectionBLL();
		if (IPInfo.dictIP.ContainsKey(ipinfo.IPAddress))
		{
			((Control)txtIP).Text = ipinfo.IPAddress;
			((Control)txtIP).BackColor = Color.Green;
			tcpclient = IPInfo.dictIP[ipinfo.IPAddress.ToString()];
			load = true;
			fileinfo = new OcaFileInfo();
			filebll = new FileBLL();
			fileinfo.fileNum = Convert.ToByte(nUDFileNum.Value);
			block.ONo = 160u;
			block.Label = "read";
			filebll.CommandMessage(tcpclient, fileinfo, block);
			Thread.Sleep(100);
		}
		else
		{
			switch (await CB.Connection(ipinfo, tcpclient))
			{
			case ConnectionState.Connected:
				((Control)txtIP).Text = ipinfo.IPAddress;
				((Control)txtIP).BackColor = Color.Green;
				if (!IPInfo.dictIP.ContainsKey(ipinfo.IPAddress))
				{
					IPInfo.dictIP.Add(ipinfo.IPAddress, tcpclient);
				}
				load = true;
				fileinfo = new OcaFileInfo();
				filebll = new FileBLL();
				fileinfo.fileNum = Convert.ToByte(nUDFileNum.Value);
				block.ONo = 160u;
				block.Label = "read";
				filebll.CommandMessage(tcpclient, fileinfo, block);
				Thread.Sleep(100);
				break;
			case ConnectionState.Disconnect:
				MessageBox.Show("Retry or check IP");
				break;
			case ConnectionState.IPEmpty:
				MessageBox.Show("Enter IP ");
				break;
			case ConnectionState.IllegalIP:
				MessageBox.Show("Enter correct IP");
				break;
			}
		}
		boolstop = false;
		WaitACK(tcpclient);
	}

	private void trbInput1_Scroll(object sender, EventArgs e)
	{
		if (trbInput1.Value < 8)
		{
			trbInput1.Value = 0;
		}
		((Control)txtChannel1).Text = $"{trbInput1.Value.ToString(),-3}dB";
	}

	private void trbInput2_Scroll(object sender, EventArgs e)
	{
		if (trbInput2.Value < 8)
		{
			trbInput2.Value = 0;
		}
		((Control)txtChannel2).Text = trbInput2.Value + "dB";
	}

	private void trbDownload1_Scroll(object sender, EventArgs e)
	{
		((Control)txtDownload1).Text = trbDownload1.Value + "dB";
	}

	private void trbDownload2_Scroll(object sender, EventArgs e)
	{
		((Control)txtDownload2).Text = trbDownload2.Value + "dB";
	}

	private void trbVolume1_Scroll(object sender, EventArgs e)
	{
		((Control)txtVolume1).Text = trbVolume1.Value + "dB";
	}

	private void trbVolume2_Scroll(object sender, EventArgs e)
	{
		((Control)txtVolume2).Text = trbVolume2.Value + "dB";
	}

	public async void WaitACK(TcpClient tcpclient)
	{
		if (boolstop)
		{
			boolstop = false;
			return;
		}
		try
		{
			noticebll = new NoticeBLL();
			byte[] initbyte = new byte[8];
			ocaparameter = new OcaPropertyParameter(0u, 0u, initbyte);
			ocp1pdu = new Ocp1NotificationPdu();
			Home home = this;
			_ = home.buf;
			home.buf = await Task.Run(() => noticebll.WaitNoticeBLL(tcpclient), _cts.Token);
			if (buf[7] == 2)
			{
				ocp1pdu = OcaStruct.Onpacket(buf);
				ocaparameter.block = ocp1pdu.notifications.targetOno;
				ocaparameter.label = ocp1pdu.notifications.parameters.eventData.ocaevent.emitterOno;
				ocaparameter.parametervalue = ocp1pdu.notifications.parameters.eventData.eventParameters;
				Array.Clear(buf, 0, 200);
				ocasubmanager.NewNotification += ControlValueChange;
				ocasubmanager.OnNewNitification(ocaparameter);
			}
			else if (buf[7] == 3)
			{
				WaitResponse(buf);
			}
		}
		catch
		{
		}
		WaitACK(tcpclient);
	}

	public void ControlValueChange(object sender, OcaPropertyParameter ocaparameter)
	{
		switch (ocaparameter.block)
		{
		case 16u:
			Channel1Value(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 32u:
			Download1Value(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 48u:
			Channel2Value(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 64u:
			Download2Value(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 80u:
			HeadphoneFrontValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 96u:
			HeadphoneRearValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 112u:
			StreamValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 128u:
			ToneGenValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 144u:
			SystemLCDMicMuteValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 160u:
			FileValue(ocaparameter.label, ocaparameter.parametervalue[0]);
			break;
		case 176u:
			PPMValue(ocaparameter.label, ocaparameter.parametervalue);
			break;
		}
	}

	public void Channel1Value(uint label, byte param)
	{
		switch (label)
		{
		case 17u:
			CheckLeft_Phantom.Checked = Convert.ToBoolean(param);
			break;
		case 18u:
			CheckLeft_Phase.Checked = Convert.ToBoolean(param);
			break;
		case 19u:
			micline1.Checked = Convert.ToBoolean(param);
			break;
		case 20u:
			CheckLeft_Mute.Checked = Convert.ToBoolean(param);
			if (CheckRight_Mute.Checked || CheckRight_Mute.Checked)
			{
				MicMute.Checked = false;
			}
			else if (!CheckRight_Mute.Checked && !CheckRight_Mute.Checked)
			{
				MicMute.Checked = true;
			}
			break;
		case 21u:
			((Control)txtChannel1).Text = Convert.ToInt32(param) + "dB";
			trbInput1.Value = Convert.ToInt32(param);
			break;
		case 22u:
			CheckLeft_Pad.Checked = Convert.ToBoolean(param);
			break;
		case 23u:
			nUD1_HPF.Value = Convert.ToInt32(param);
			break;
		case 24u:
			nUD1_Treble.Value = (sbyte)param;
			break;
		case 25u:
			nUD1_BASS.Value = (sbyte)param;
			break;
		}
	}

	public void Download1Value(uint label, byte param)
	{
		switch (label)
		{
		case 33u:
			CheckDownLeft_Phase.Checked = Convert.ToBoolean(param);
			break;
		case 34u:
			CheckDownLeft_Mute.Checked = Convert.ToBoolean(param);
			break;
		case 35u:
			((Control)txtDownload1).Text = Convert.ToInt32(param) + "dB";
			trbDownload1.Value = Convert.ToInt32(param);
			break;
		case 36u:
			nUDDown1_HPF.Value = Convert.ToInt32(param);
			break;
		case 37u:
			nUDDown1_Treble.Value = (sbyte)param;
			break;
		case 38u:
			nUDDown1_BASS.Value = (sbyte)param;
			break;
		}
	}

	public void Channel2Value(uint label, byte param)
	{
		switch (label)
		{
		case 49u:
			CheckRight_Phantom.Checked = Convert.ToBoolean(param);
			break;
		case 50u:
			CheckRight_Phase.Checked = Convert.ToBoolean(param);
			break;
		case 51u:
			micline2.Checked = Convert.ToBoolean(param);
			break;
		case 52u:
			CheckRight_Mute.Checked = Convert.ToBoolean(param);
			if (CheckRight_Mute.Checked || CheckRight_Mute.Checked)
			{
				MicMute.Checked = false;
			}
			else if (!CheckRight_Mute.Checked && !CheckRight_Mute.Checked)
			{
				MicMute.Checked = true;
			}
			break;
		case 53u:
			((Control)txtChannel2).Text = Convert.ToInt32(param) + "dB";
			trbInput2.Value = Convert.ToInt32(param);
			break;
		case 54u:
			CheckRight_Pad.Checked = Convert.ToBoolean(param);
			break;
		case 55u:
			nUD2_HPF.Value = Convert.ToInt32(param);
			break;
		case 56u:
			nUD2_Treble.Value = (sbyte)param;
			break;
		case 57u:
			nUD2_BASS.Value = (sbyte)param;
			break;
		}
	}

	public void Download2Value(uint label, byte param)
	{
		switch (label)
		{
		case 65u:
			CheckDownRight_Phase.Checked = Convert.ToBoolean(param);
			break;
		case 66u:
			CheckDownRight_Mute.Checked = Convert.ToBoolean(param);
			break;
		case 67u:
			((Control)txtDownload2).Text = Convert.ToInt32(param) + "dB";
			trbDownload2.Value = Convert.ToInt32(param);
			break;
		case 68u:
			nUDDown2_HPF.Value = Convert.ToInt32(param);
			break;
		case 69u:
			nUDDown2_Treble.Value = (sbyte)param;
			break;
		case 70u:
			nUDDown2_BASS.Value = (sbyte)param;
			break;
		}
	}

	public void HeadphoneFrontValue(uint label, byte param)
	{
		switch (label)
		{
		case 81u:
			CheckFrontHeadset_Mute.Checked = Convert.ToBoolean(param);
			break;
		case 82u:
			((Control)txtVolume1).Text = Convert.ToInt32(param) + "dB";
			trbVolume1.Value = Convert.ToInt32(param);
			break;
		case 83u:
			break;
		}
	}

	public void HeadphoneRearValue(uint label, byte param)
	{
		switch (label)
		{
		case 97u:
			CheckRearHeadset_Mute.Checked = Convert.ToBoolean(param);
			break;
		case 98u:
			((Control)txtVolume2).Text = Convert.ToInt32(param) + "dB";
			trbVolume2.Value = Convert.ToInt32(param);
			break;
		case 99u:
			break;
		}
	}

	public void StreamValue(uint label, byte param)
	{
		switch (label)
		{
		case 113u:
			numHeadset.Value = Convert.ToInt32(param);
			switch (Convert.ToInt32(param))
			{
			case 0:
				picHeadset.Image = (Image)(object)Resources._1;
				((Control)lbHeadGM).Visible = false;
				((Control)trbHeadGM).Visible = false;
				((Control)txtHeadGM).Visible = false;
				break;
			case 1:
				picHeadset.Image = (Image)(object)Resources._2;
				((Control)lbHeadGM).Visible = false;
				((Control)trbHeadGM).Visible = false;
				((Control)txtHeadGM).Visible = false;
				break;
			case 2:
				picHeadset.Image = (Image)(object)Resources._3;
				((Control)lbHeadGM).Visible = false;
				((Control)trbHeadGM).Visible = false;
				((Control)txtHeadGM).Visible = false;
				break;
			case 3:
				picHeadset.Image = (Image)(object)Resources._4;
				((Control)lbHeadGM).Visible = true;
				((Control)trbHeadGM).Visible = true;
				((Control)txtHeadGM).Visible = true;
				break;
			case 4:
				picHeadset.Image = (Image)(object)Resources._5;
				((Control)lbHeadGM).Visible = false;
				((Control)trbHeadGM).Visible = false;
				((Control)txtHeadGM).Visible = false;
				break;
			case 5:
				picHeadset.Image = (Image)(object)Resources._6;
				((Control)lbHeadGM).Visible = false;
				((Control)trbHeadGM).Visible = false;
				((Control)txtHeadGM).Visible = false;
				break;
			}
			break;
		case 114u:
			numUpload.Value = Convert.ToInt32(param);
			switch (Convert.ToInt32(param))
			{
			case 0:
				picUpload.Image = (Image)(object)Resources._7;
				((Control)lbUpGM).Visible = false;
				((Control)trbUpGM).Visible = false;
				((Control)txtUpGM).Visible = false;
				break;
			case 1:
				picUpload.Image = (Image)(object)Resources._8;
				((Control)lbUpGM).Visible = false;
				((Control)trbUpGM).Visible = false;
				((Control)txtUpGM).Visible = false;
				break;
			case 2:
				picUpload.Image = (Image)(object)Resources._9;
				((Control)lbUpGM).Visible = false;
				((Control)trbUpGM).Visible = false;
				((Control)txtUpGM).Visible = false;
				break;
			case 3:
				picUpload.Image = (Image)(object)Resources._10;
				((Control)lbUpGM).Visible = true;
				((Control)trbUpGM).Visible = true;
				((Control)txtUpGM).Visible = true;
				break;
			case 4:
				picUpload.Image = (Image)(object)Resources._11;
				((Control)lbUpGM).Visible = false;
				((Control)trbUpGM).Visible = false;
				((Control)txtUpGM).Visible = false;
				break;
			case 5:
				picUpload.Image = (Image)(object)Resources._12;
				((Control)lbUpGM).Visible = false;
				((Control)trbUpGM).Visible = false;
				((Control)txtUpGM).Visible = false;
				break;
			}
			break;
		case 115u:
		{
			int value = Convert.ToInt32(param);
			trbHeadGM.Value = value;
			((Control)txtHeadGM).Text = value + "dB";
			trbUpGM.Value = value;
			((Control)txtUpGM).Text = value + "dB";
			break;
		}
		}
	}

	public void ToneGenValue(uint label, byte param)
	{
		switch (label)
		{
		case 129u:
			break;
		case 130u:
			break;
		case 131u:
			break;
		}
	}

	public void SystemLCDMicMuteValue(uint label, byte param)
	{
		switch (label)
		{
		case 145u:
			trbBright.Value = Convert.ToInt32(param);
			break;
		case 146u:
			DisplayMod.Checked = Convert.ToBoolean(param);
			break;
		case 147u:
			MicMute.Checked = Convert.ToBoolean(param);
			if (MicMute.Checked)
			{
				CheckLeft_Mute.Checked = false;
				CheckRight_Mute.Checked = false;
			}
			else if (!MicMute.Checked)
			{
				CheckLeft_Mute.Checked = true;
				CheckRight_Mute.Checked = true;
			}
			break;
		}
	}

	public void FileValue(uint label, byte param)
	{
		switch (label)
		{
		case 161u:
			break;
		case 162u:
			break;
		case 163u:
			break;
		case 164u:
			break;
		}
	}

	public void PPMValue(uint label, byte[] param)
	{
		Channel1_PPM.Value = (sbyte)param[0];
		Channel1_PPM.PeakHold = (sbyte)param[1];
		Channel2_PPM.Value = (sbyte)param[2];
		Channel2_PPM.PeakHold = (sbyte)param[3];
		DLL_PPM.Value = (sbyte)param[4];
		DLL_PPM.PeakHold = (sbyte)param[5];
		DLR_PPM.Value = (sbyte)param[6];
		DLR_PPM.PeakHold = (sbyte)param[7];
	}

	public TcpClient GetTcpInfo()
	{
		if (IPInfo.dictIP.ContainsKey(((Control)txtIP).Text))
		{
			tcpclient = IPInfo.dictIP[((Control)txtIP).Text];
		}
		return tcpclient;
	}

	public void SwitchOn(uint blocknumber, string label)
	{
		switchinfo.SwitchStatus = 1;
		tcpclient = GetTcpInfo();
		block.ONo = blocknumber;
		block.Label = label;
		switchbll.CommandMessage(tcpclient, switchinfo, block);
	}

	public void SwitchOff(uint blocknumber, string label)
	{
		switchinfo.SwitchStatus = 0;
		tcpclient = GetTcpInfo();
		block.ONo = blocknumber;
		block.Label = label;
		switchbll.CommandMessage(tcpclient, switchinfo, block);
	}

	private void trbInput1_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		if (trbInput1.Value > 0 && trbInput1.Value < 8)
		{
			trbInput1.Value = 0;
		}
		ocaGainInfo.Gain = (byte)trbInput1.Value;
		tcpclient = GetTcpInfo();
		block.Label = "CH1";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(30);
	}

	private void micline1_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (micline1.Checked)
			{
				SwitchOn(16u, "source");
			}
			else
			{
				SwitchOff(16u, "source");
			}
			Thread.Sleep(30);
		}
	}

	private void CheckLeft_MICLIN_Click(object sender, EventArgs e)
	{
	}

	private void CheckLeft_Phantom_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckLeft_Phantom.Checked)
			{
				SwitchOn(16u, "phantom");
			}
			else
			{
				SwitchOff(16u, "phantom");
			}
			Thread.Sleep(30);
		}
	}

	private void CheckLeft_Pad_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckLeft_Pad.Checked)
			{
				SwitchOn(16u, "pad");
			}
			else
			{
				SwitchOff(16u, "pad");
			}
			Thread.Sleep(100);
		}
	}

	private void CheckLeft_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckLeft_Mute.Checked)
			{
				SwitchOn(16u, "mute");
			}
			else
			{
				SwitchOff(16u, "mute");
			}
			if (CheckRight_Mute.Checked || CheckLeft_Mute.Checked)
			{
				MicMute.Checked = false;
			}
			else if (!CheckRight_Mute.Checked && !CheckLeft_Mute.Checked)
			{
				MicMute.Checked = true;
			}
			Thread.Sleep(30);
		}
	}

	private void CheckLeft_Phase_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckLeft_Phase.Checked)
			{
				SwitchOn(16u, "phase");
			}
			else
			{
				SwitchOff(16u, "phase");
			}
		}
		Thread.Sleep(30);
	}

	private void nUD1_HPF_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			ocaDSPInfo.dspvalue = (byte)nUD1_HPF.Value;
			tcpclient = GetTcpInfo();
			block.ONo = 16u;
			block.Label = "hpf";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(100);
		}
		Thread.Sleep(200);
	}

	private void nUD1_Treble_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUD1_Treble.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUD1_Treble.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUD1_Treble.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 16u;
			block.Label = "treble";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void nUD1_BASS_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUD1_BASS.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUD1_BASS.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUD1_BASS.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 16u;
			block.Label = "bass";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void trbInput2_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		if (trbInput2.Value < 8)
		{
			trbInput2.Value = 0;
		}
		ocaGainInfo.Gain = (byte)trbInput2.Value;
		tcpclient = GetTcpInfo();
		block.Label = "CH2";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(30);
	}

	private void micline2_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (micline2.Checked)
			{
				SwitchOn(48u, "source");
			}
			else
			{
				SwitchOff(48u, "source");
			}
			Thread.Sleep(30);
		}
	}

	private void CheckRight_MICLIN_Click(object sender, EventArgs e)
	{
	}

	private void CheckRight_Phantom_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckRight_Phantom.Checked)
			{
				SwitchOn(48u, "phantom");
			}
			else
			{
				SwitchOff(48u, "phantom");
			}
			Thread.Sleep(30);
		}
	}

	private void CheckRight_Pad_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckRight_Pad.Checked)
			{
				SwitchOn(48u, "pad");
			}
			else
			{
				SwitchOff(48u, "pad");
			}
			Thread.Sleep(100);
		}
	}

	private void CheckRight_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckRight_Mute.Checked)
			{
				SwitchOn(48u, "mute");
			}
			else
			{
				SwitchOff(48u, "mute");
			}
			if (CheckRight_Mute.Checked || CheckLeft_Mute.Checked)
			{
				MicMute.Checked = false;
			}
			else if (!CheckRight_Mute.Checked && !CheckLeft_Mute.Checked)
			{
				MicMute.Checked = true;
			}
			Thread.Sleep(30);
		}
	}

	private void CheckRight_Phase_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckRight_Phase.Checked)
			{
				SwitchOn(48u, "phase");
			}
			else
			{
				SwitchOff(48u, "phase");
			}
			Thread.Sleep(30);
		}
	}

	private void nUD2_HPF_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			ocaDSPInfo.dspvalue = (byte)nUD2_HPF.Value;
			tcpclient = GetTcpInfo();
			block.ONo = 48u;
			block.Label = "hpf";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void nUD2_Treble_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUD2_Treble.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUD2_Treble.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUD2_Treble.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 48u;
			block.Label = "treble";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void nUD2_BASS_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUD2_BASS.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUD2_BASS.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUD2_BASS.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 48u;
			block.Label = "bass";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void trbDownload1_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbDownload1.Value;
		tcpclient = GetTcpInfo();
		block.Label = "DOWNLOAD1";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(10);
	}

	private void CheckDownLeft_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckDownLeft_Mute.Checked)
			{
				SwitchOn(32u, "mute");
			}
			else
			{
				SwitchOff(32u, "mute");
			}
		}
	}

	private void CheckDownLeft_Phase_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckDownLeft_Phase.Checked)
			{
				SwitchOn(32u, "phase");
			}
			else
			{
				SwitchOff(32u, "phase");
			}
		}
	}

	private void nUDDown1_HPF_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			ocaDSPInfo.dspvalue = (byte)nUDDown1_HPF.Value;
			tcpclient = GetTcpInfo();
			block.ONo = 32u;
			block.Label = "hpf";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(50);
		}
	}

	private void nUDDown1_Treble_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUDDown1_Treble.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUDDown1_Treble.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUDDown1_Treble.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 32u;
			block.Label = "treble";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void nUDDown1_BASS_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUDDown1_BASS.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUDDown1_BASS.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUDDown1_BASS.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 32u;
			block.Label = "bass";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void trbDownload2_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbDownload2.Value;
		tcpclient = GetTcpInfo();
		block.Label = "DOWNLOAD2";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(10);
	}

	private void CheckDownRight_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckDownRight_Mute.Checked)
			{
				SwitchOn(64u, "mute");
			}
			else
			{
				SwitchOff(64u, "mute");
			}
		}
	}

	private void CheckDownRight_Phase_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckDownRight_Phase.Checked)
			{
				SwitchOn(64u, "phase");
			}
			else
			{
				SwitchOff(64u, "phase");
			}
		}
	}

	private void nUDDown2_HPF_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			ocaDSPInfo.dspvalue = (byte)nUDDown2_HPF.Value;
			tcpclient = GetTcpInfo();
			block.ONo = 64u;
			block.Label = "hpf";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(50);
		}
	}

	private void nUDDown2_Treble_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUDDown2_Treble.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUDDown2_Treble.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUDDown2_Treble.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 64u;
			block.Label = "treble";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void nUDDown2_BASS_ValueChanged(object sender, EventArgs e)
	{
		if (!load)
		{
			OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
			if (nUDDown2_BASS.Value < 0m)
			{
				ocaDSPInfo.dspvalue = (byte)(int.Parse(nUDDown2_BASS.Value.ToString()) & 0xFF);
			}
			else
			{
				ocaDSPInfo.dspvalue = (byte)nUDDown2_BASS.Value;
			}
			tcpclient = GetTcpInfo();
			block.ONo = 64u;
			block.Label = "bass";
			dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
			Thread.Sleep(200);
		}
	}

	private void trbVolume1_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbVolume1.Value;
		tcpclient = GetTcpInfo();
		block.Label = "FRONT";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(10);
	}

	private void CheckFrontHeadset_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckFrontHeadset_Mute.Checked)
			{
				SwitchOn(80u, "push");
			}
			else
			{
				SwitchOff(80u, "push");
			}
		}
	}

	private void trbVolume2_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbVolume2.Value;
		tcpclient = GetTcpInfo();
		block.Label = "REAR";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
		Thread.Sleep(10);
	}

	private void CheckRearHeadset_Mute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (CheckRearHeadset_Mute.Checked)
			{
				SwitchOn(96u, "push");
			}
			else
			{
				SwitchOff(96u, "push");
			}
		}
	}

	private void trbBright_MouseUp(object sender, MouseEventArgs e)
	{
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbBright.Value;
		tcpclient = GetTcpInfo();
		block.Label = "SYSTEM";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
	}

	private void MicMute_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (MicMute.Checked)
			{
				SwitchOn(144u, "mute");
				CheckLeft_Mute.Checked = false;
				CheckRight_Mute.Checked = false;
			}
			else
			{
				SwitchOff(144u, "mute");
				CheckLeft_Mute.Checked = true;
				CheckRight_Mute.Checked = true;
			}
		}
	}

	private void DisplayMod_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (DisplayMod.Checked)
			{
				SwitchOn(144u, "display");
			}
			else
			{
				SwitchOff(144u, "display");
			}
		}
	}

	private void numHeadset_ValueChanged(object sender, EventArgs e)
	{
		switch ((int)numHeadset.Value)
		{
		case 0:
			picHeadset.Image = (Image)(object)Resources._1;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 1:
			picHeadset.Image = (Image)(object)Resources._2;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 2:
			picHeadset.Image = (Image)(object)Resources._3;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 3:
			picHeadset.Image = (Image)(object)Resources._4;
			((Control)lbHeadGM).Visible = true;
			((Control)trbHeadGM).Visible = true;
			((Control)txtHeadGM).Visible = true;
			break;
		case 4:
			picHeadset.Image = (Image)(object)Resources._5;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 5:
			picHeadset.Image = (Image)(object)Resources._6;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		}
		OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
		ocaDSPInfo.dspvalue = (byte)numHeadset.Value;
		tcpclient = GetTcpInfo();
		block.ONo = 112u;
		block.Label = "headsetrout";
		dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
		Thread.Sleep(20);
	}

	private void numUpload_ValueChanged(object sender, EventArgs e)
	{
		switch ((int)numUpload.Value)
		{
		case 0:
			picUpload.Image = (Image)(object)Resources._7;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 1:
			picUpload.Image = (Image)(object)Resources._8;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 2:
			picUpload.Image = (Image)(object)Resources._9;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 3:
			picUpload.Image = (Image)(object)Resources._10;
			((Control)lbUpGM).Visible = true;
			((Control)trbUpGM).Visible = true;
			((Control)txtUpGM).Visible = true;
			break;
		case 4:
			picUpload.Image = (Image)(object)Resources._11;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 5:
			picUpload.Image = (Image)(object)Resources._12;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		}
		OcaDSPInfo ocaDSPInfo = new OcaDSPInfo();
		ocaDSPInfo.dspvalue = (byte)numUpload.Value;
		tcpclient = GetTcpInfo();
		block.ONo = 112u;
		block.Label = "uploadrout";
		dspbll.CommandMessage(tcpclient, ocaDSPInfo, block);
		Thread.Sleep(20);
	}

	private void trbHeadGM_Scroll(object sender, EventArgs e)
	{
		((Control)txtHeadGM).Text = trbHeadGM.Value + "dB";
		((Control)txtUpGM).Text = ((Control)txtHeadGM).Text;
		trbUpGM.Value = trbHeadGM.Value;
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbHeadGM.Value;
		tcpclient = GetTcpInfo();
		block.Label = "STREAM";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
	}

	private void trbUpGM_Scroll(object sender, EventArgs e)
	{
		((Control)txtUpGM).Text = trbUpGM.Value + "dB";
		((Control)txtHeadGM).Text = ((Control)txtUpGM).Text;
		trbHeadGM.Value = trbUpGM.Value;
		OcaGainInfo ocaGainInfo = new OcaGainInfo();
		ocaGainInfo.Gain = (byte)trbUpGM.Value;
		tcpclient = GetTcpInfo();
		block.Label = "STREAM";
		gainbll.CommandMessage(tcpclient, ocaGainInfo, block);
	}

	private void Mute_Headset_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (Mute_Headset.Checked)
			{
				SwitchOn(112u, "hmute");
			}
			else
			{
				SwitchOff(112u, "hmute");
			}
		}
	}

	private void Mute_Upload_Click(object sender, EventArgs e)
	{
		if (!load)
		{
			if (Mute_Upload.Checked)
			{
				SwitchOn(112u, "umute");
			}
			else
			{
				SwitchOff(112u, "umute");
			}
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
	}

	private void btnSaveAs_Click(object sender, EventArgs e)
	{
	}

	public async void WaitResponse(byte[] buf)
	{
		responsebll = new ResponseBLL();
		fileinfo = new OcaFileInfo();
		Home home = this;
		_ = home.fileinfo;
		home.fileinfo = await Task.Run(() => responsebll.WaitResponseBLL(buf));
		if (fileinfo == null)
		{
			load = false;
			return;
		}
		flag = 2;
		if (fileinfo.fileParam[20] == 0)
		{
			trbInput1.Value = fileinfo.fileParam[0];
			((Control)txtChannel1).Text = trbInput1.Value + "dB";
		}
		else if (fileinfo.fileParam[20] == 1)
		{
			trbInput1.Value = fileinfo.fileParam[18];
			((Control)txtChannel1).Text = trbInput1.Value + "dB";
		}
		if (fileinfo.fileParam[21] == 0)
		{
			trbInput2.Value = fileinfo.fileParam[1];
			((Control)txtChannel2).Text = trbInput2.Value + "dB";
		}
		else if (fileinfo.fileParam[21] == 1)
		{
			trbInput2.Value = fileinfo.fileParam[19];
			((Control)txtChannel2).Text = trbInput2.Value + "dB";
		}
		trbBright.Value = fileinfo.fileParam[2];
		trbDownload1.Value = fileinfo.fileParam[3];
		((Control)txtDownload1).Text = trbDownload1.Value + "dB";
		trbDownload2.Value = fileinfo.fileParam[4];
		((Control)txtDownload2).Text = trbDownload2.Value + "dB";
		trbHeadGM.Value = fileinfo.fileParam[5];
		trbUpGM.Value = fileinfo.fileParam[5];
		((Control)txtHeadGM).Text = fileinfo.fileParam[5] + "dB";
		((Control)txtUpGM).Text = fileinfo.fileParam[5] + "dB";
		numHeadset.Value = fileinfo.fileParam[6];
		switch ((int)numHeadset.Value)
		{
		case 0:
			picHeadset.Image = (Image)(object)Resources._1;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 1:
			picHeadset.Image = (Image)(object)Resources._2;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 2:
			picHeadset.Image = (Image)(object)Resources._3;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 3:
			picHeadset.Image = (Image)(object)Resources._4;
			((Control)lbHeadGM).Visible = true;
			((Control)trbHeadGM).Visible = true;
			((Control)txtHeadGM).Visible = true;
			break;
		case 4:
			picHeadset.Image = (Image)(object)Resources._5;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		case 5:
			picHeadset.Image = (Image)(object)Resources._6;
			((Control)lbHeadGM).Visible = false;
			((Control)trbHeadGM).Visible = false;
			((Control)txtHeadGM).Visible = false;
			break;
		}
		numUpload.Value = fileinfo.fileParam[7];
		switch ((int)numUpload.Value)
		{
		case 0:
			picUpload.Image = (Image)(object)Resources._7;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 1:
			picUpload.Image = (Image)(object)Resources._8;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 2:
			picUpload.Image = (Image)(object)Resources._9;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 3:
			picUpload.Image = (Image)(object)Resources._10;
			((Control)lbUpGM).Visible = true;
			((Control)trbUpGM).Visible = true;
			((Control)txtUpGM).Visible = true;
			break;
		case 4:
			picUpload.Image = (Image)(object)Resources._11;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		case 5:
			picUpload.Image = (Image)(object)Resources._12;
			((Control)lbUpGM).Visible = false;
			((Control)trbUpGM).Visible = false;
			((Control)txtUpGM).Visible = false;
			break;
		}
		trbVolume1.Value = fileinfo.fileParam[8];
		((Control)txtVolume1).Text = fileinfo.fileParam[8] + "dB";
		trbVolume2.Value = fileinfo.fileParam[9];
		((Control)txtVolume2).Text = fileinfo.fileParam[9] + "dB";
		nUD1_HPF.Value = fileinfo.fileParam[10];
		nUD2_HPF.Value = fileinfo.fileParam[11];
		nUDDown1_HPF.Value = fileinfo.fileParam[12];
		nUDDown2_HPF.Value = fileinfo.fileParam[13];
		CheckLeft_Phase.Checked = Convert.ToBoolean(fileinfo.fileParam[14]);
		CheckRight_Phase.Checked = Convert.ToBoolean(fileinfo.fileParam[15]);
		CheckDownLeft_Phase.Checked = Convert.ToBoolean(fileinfo.fileParam[16]);
		CheckDownRight_Phase.Checked = Convert.ToBoolean(fileinfo.fileParam[17]);
		micline1.Checked = Convert.ToBoolean(fileinfo.fileParam[20]);
		micline2.Checked = Convert.ToBoolean(fileinfo.fileParam[21]);
		CheckLeft_Phantom.Checked = Convert.ToBoolean(fileinfo.fileParam[31]);
		CheckRight_Phantom.Checked = Convert.ToBoolean(fileinfo.fileParam[32]);
		CheckLeft_Pad.Checked = Convert.ToBoolean(fileinfo.fileParam[22]);
		CheckRight_Pad.Checked = Convert.ToBoolean(fileinfo.fileParam[23]);
		Mute_Headset.Checked = Convert.ToBoolean(fileinfo.fileParam[24]);
		CheckLeft_Mute.Checked = Convert.ToBoolean(fileinfo.fileParam[25]);
		CheckRight_Mute.Checked = Convert.ToBoolean(fileinfo.fileParam[26]);
		Mute_Upload.Checked = Convert.ToBoolean(fileinfo.fileParam[27]);
		CheckDownLeft_Mute.Checked = Convert.ToBoolean(fileinfo.fileParam[28]);
		CheckDownRight_Mute.Checked = Convert.ToBoolean(fileinfo.fileParam[29]);
		DisplayMod.Checked = Convert.ToBoolean(fileinfo.fileParam[30]);
		MicMute.Checked = Convert.ToBoolean(fileinfo.fileParam[41]);
		nUD1_BASS.Value = (sbyte)fileinfo.fileParam[33];
		nUD2_BASS.Value = (sbyte)fileinfo.fileParam[34];
		nUDDown1_BASS.Value = (sbyte)fileinfo.fileParam[35];
		nUDDown2_BASS.Value = (sbyte)fileinfo.fileParam[36];
		nUD1_Treble.Value = (sbyte)fileinfo.fileParam[37];
		nUD2_Treble.Value = (sbyte)fileinfo.fileParam[38];
		nUDDown1_Treble.Value = (sbyte)fileinfo.fileParam[39];
		nUDDown2_Treble.Value = (sbyte)fileinfo.fileParam[40];
		load = false;
		flag = 1;
	}

	private void Switch_Checked(int param, RadioButton ctrName1, RadioButton ctrName2)
	{
		if (fileinfo.fileParam[param] == 0)
		{
			ctrName2.Checked = true;
		}
		else
		{
			ctrName1.Checked = true;
		}
	}

	private void btnUsrMng_Click(object sender, EventArgs e)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		UserManagement userManagement = new UserManagement();
		((Form)userManagement).ShowDialog((IWin32Window)(object)this);
	}

	private void viewMenu_Click(object sender, EventArgs e)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (((Control)txtIP).Text != null)
		{
			Process.Start("IEXplore.exe", "http://" + ((Control)txtIP).Text);
		}
		else
		{
			MessageBox.Show("Check IP");
		}
	}

	private void label32_Click(object sender, EventArgs e)
	{
		IOrderedEnumerable<KeyValuePair<string, string>> orderedEnumerable = PeerData.dictBonjour.OrderBy((KeyValuePair<string, string> pair) => pair.Key);
	}

	private void timDevice_Tick(object sender, EventArgs e)
	{
		IOrderedEnumerable<KeyValuePair<string, string>> orderedEnumerable = PeerData.dictBonjour.OrderBy((KeyValuePair<string, string> pair) => pair.Key);
		if (PeerData.dictBonjour.Count != 0)
		{
			timDevice.Enabled = false;
		}
	}

	public bool Bonjour_Init()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			m_service = (DNSSDService)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("24CD4DE9-FF84-4701-9DC1-9B69E0D1090A")));
		}
		catch
		{
			MessageBox.Show("Bonjour Service is not available", "Error");
			Application.Exit();
			return false;
		}
		m_eventManager = (DNSSDEventManager)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("BEEB932A-8D4A-4619-AEFE-A836F988B221")));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceRegistered").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceRegisteredEventHandler(ServiceRegistered));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceFound").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceFoundEventHandler(ServiceFound));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceResolved").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceResolvedEventHandler(ServiceResolved));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "QueryRecordAnswered").AddEventHandler(m_eventManager, new _IDNSSDEvents_QueryRecordAnsweredEventHandler(QueryAnswered));
		SimpleChat();
		return true;
	}

	private void SimpleChat()
	{
		IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		m_socket.Bind(localEP);
		localEP = (IPEndPoint)m_socket.LocalEndPoint;
		try
		{
			m_registrar = m_service.Register((DNSSDFlags)0, 0u, Environment.UserName, "_rtsp._tcp", null, null, (ushort)localEP.Port, null, m_eventManager);
		}
		catch
		{
		}
	}

	public void ServiceRegistered(DNSSDService service, DNSSDFlags flags, string name, string regType, string domain)
	{
		m_name = name;
		try
		{
			m_browser = m_service.Browse((DNSSDFlags)0, 0u, "_rtsp._tcp", null, m_eventManager);
		}
		catch
		{
		}
	}

	public void ServiceFound(DNSSDService sref, DNSSDFlags flags, uint ifIndex, string serviceName, string regType, string domain)
	{
		if (serviceName != m_name)
		{
			PeerData peerData = new PeerData();
			peerData.InterfaceIndex = ifIndex;
			peerData.Name = serviceName;
			peerData.Type = regType;
			peerData.Domain = domain;
			peerData.Address = null;
			m_resolver = m_service.Resolve((DNSSDFlags)0, peerData.InterfaceIndex, peerData.Name, peerData.Type, peerData.Domain, m_eventManager);
		}
	}

	public void ServiceResolved(DNSSDService sref, DNSSDFlags flags, uint ifIndex, string fullName, string hostName, ushort port, TXTRecord txtRecord)
	{
		m_resolver = null;
		peer.Port = port;
		try
		{
			m_resolver = m_service.QueryRecord((DNSSDFlags)0, ifIndex, hostName, DNSSDRRType.kDNSSDType_A, DNSSDRRClass.kDNSSDClass_IN, m_eventManager);
		}
		catch
		{
		}
	}

	public void QueryAnswered(DNSSDService service, DNSSDFlags flags, uint ifIndex, string fullName, DNSSDRRType rrtype, DNSSDRRClass rrclass, object rdata, uint ttl)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		m_resolver = null;
		uint num = BitConverter.ToUInt32((byte[])rdata, 0);
		IPAddress iPAddress = new IPAddress(num);
		string[] array = fullName.Split(new char[1] { '.' });
		if (!PeerData.dictBonjour.ContainsKey(iPAddress.ToString()))
		{
			peer.Name = array[0];
			peer.Address = iPAddress;
			PeerData.dictBonjour.Add(iPAddress.ToString(), array[0]);
			TreeNode val = new TreeNode();
			failsubnet = bonjourbll.FailSubNet(IPsubnet, peer.Address.ToString(), NetNumber);
			if (!failsubnet)
			{
				val.ForeColor = Color.Gray;
			}
			val.Name = peer.Address.ToString();
			val.Text = peer.Name;
			trvDevice.Nodes["PreMo"].Nodes.Add(val);
			val.ToolTipText = peer.Address.ToString();
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
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected Obj, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected Obj, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected Obj, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected Obj, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected Obj, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected Obj, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected Obj, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected Obj, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected Obj, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected Obj, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected Obj, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected Obj, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected Obj, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected Obj, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected Obj, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected Obj, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected Obj, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected Obj, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected Obj, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected Obj, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected Obj, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected Obj, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected Obj, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected Obj, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected Obj, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected Obj, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected Obj, but got Unknown
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected Obj, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected Obj, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Expected Obj, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected Obj, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected Obj, but got Unknown
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected Obj, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected Obj, but got Unknown
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected Obj, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected Obj, but got Unknown
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Expected Obj, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected Obj, but got Unknown
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Expected Obj, but got Unknown
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected Obj, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected Obj, but got Unknown
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected Obj, but got Unknown
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Expected Obj, but got Unknown
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Expected Obj, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Expected Obj, but got Unknown
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected Obj, but got Unknown
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Expected Obj, but got Unknown
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Expected Obj, but got Unknown
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Expected Obj, but got Unknown
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Expected Obj, but got Unknown
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected Obj, but got Unknown
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Expected Obj, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Expected Obj, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected Obj, but got Unknown
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Expected Obj, but got Unknown
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected Obj, but got Unknown
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Expected Obj, but got Unknown
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Expected Obj, but got Unknown
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Expected Obj, but got Unknown
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Expected Obj, but got Unknown
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected Obj, but got Unknown
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Expected Obj, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Expected Obj, but got Unknown
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Expected Obj, but got Unknown
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Expected Obj, but got Unknown
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Expected Obj, but got Unknown
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected Obj, but got Unknown
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Expected Obj, but got Unknown
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Expected Obj, but got Unknown
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Expected Obj, but got Unknown
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Expected Obj, but got Unknown
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Expected Obj, but got Unknown
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Expected Obj, but got Unknown
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Expected Obj, but got Unknown
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Expected Obj, but got Unknown
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Expected Obj, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Expected Obj, but got Unknown
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Expected Obj, but got Unknown
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Expected Obj, but got Unknown
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Expected Obj, but got Unknown
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Expected Obj, but got Unknown
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Expected Obj, but got Unknown
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Expected Obj, but got Unknown
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Expected Obj, but got Unknown
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Expected Obj, but got Unknown
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Expected Obj, but got Unknown
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Expected Obj, but got Unknown
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Expected Obj, but got Unknown
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Expected Obj, but got Unknown
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Expected Obj, but got Unknown
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Expected Obj, but got Unknown
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Expected Obj, but got Unknown
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Expected Obj, but got Unknown
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Expected Obj, but got Unknown
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Expected Obj, but got Unknown
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Expected Obj, but got Unknown
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Expected Obj, but got Unknown
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Expected Obj, but got Unknown
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Expected Obj, but got Unknown
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Expected Obj, but got Unknown
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Expected Obj, but got Unknown
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Expected Obj, but got Unknown
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Expected Obj, but got Unknown
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Expected Obj, but got Unknown
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected Obj, but got Unknown
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Expected Obj, but got Unknown
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Expected Obj, but got Unknown
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Expected Obj, but got Unknown
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Expected Obj, but got Unknown
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Expected Obj, but got Unknown
		//IL_0573: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Expected Obj, but got Unknown
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Expected Obj, but got Unknown
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Expected Obj, but got Unknown
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Expected Obj, but got Unknown
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Expected Obj, but got Unknown
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Expected Obj, but got Unknown
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Expected Obj, but got Unknown
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Expected Obj, but got Unknown
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Expected Obj, but got Unknown
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Expected Obj, but got Unknown
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Expected Obj, but got Unknown
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Expected Obj, but got Unknown
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Expected Obj, but got Unknown
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Expected Obj, but got Unknown
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Expected Obj, but got Unknown
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a86: Expected Obj, but got Unknown
		//IL_0b0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b16: Expected Obj, but got Unknown
		//IL_0bc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bce: Expected Obj, but got Unknown
		//IL_0cb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc0: Expected Obj, but got Unknown
		//IL_0d2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d38: Expected Obj, but got Unknown
		//IL_0ed3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edd: Expected Obj, but got Unknown
		//IL_0f73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7d: Expected Obj, but got Unknown
		//IL_14d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e1: Expected Obj, but got Unknown
		//IL_153e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1548: Expected Obj, but got Unknown
		//IL_16ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d8: Expected Obj, but got Unknown
		//IL_1757: Unknown result type (might be due to invalid IL or missing references)
		//IL_1761: Expected Obj, but got Unknown
		//IL_17e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ea: Expected Obj, but got Unknown
		//IL_1876: Unknown result type (might be due to invalid IL or missing references)
		//IL_1880: Expected Obj, but got Unknown
		//IL_18e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_18f1: Expected Obj, but got Unknown
		//IL_197d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1987: Expected Obj, but got Unknown
		//IL_1e8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e97: Expected Obj, but got Unknown
		//IL_244c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2456: Expected Obj, but got Unknown
		//IL_250d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2517: Expected Obj, but got Unknown
		//IL_25fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2607: Expected Obj, but got Unknown
		//IL_268e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2698: Expected Obj, but got Unknown
		//IL_280e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2818: Expected Obj, but got Unknown
		//IL_296b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2975: Expected Obj, but got Unknown
		//IL_29e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_29ef: Expected Obj, but got Unknown
		//IL_2d30: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d3a: Expected Obj, but got Unknown
		//IL_3126: Unknown result type (might be due to invalid IL or missing references)
		//IL_3130: Expected Obj, but got Unknown
		//IL_3447: Unknown result type (might be due to invalid IL or missing references)
		//IL_3451: Expected Obj, but got Unknown
		//IL_34fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_3508: Expected Obj, but got Unknown
		//IL_368d: Unknown result type (might be due to invalid IL or missing references)
		//IL_3697: Expected Obj, but got Unknown
		//IL_3f77: Unknown result type (might be due to invalid IL or missing references)
		//IL_3f81: Expected Obj, but got Unknown
		//IL_3fb3: Unknown result type (might be due to invalid IL or missing references)
		//IL_3fbd: Expected Obj, but got Unknown
		//IL_410d: Unknown result type (might be due to invalid IL or missing references)
		//IL_4117: Expected Obj, but got Unknown
		//IL_41c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_41ce: Expected Obj, but got Unknown
		//IL_4f62: Unknown result type (might be due to invalid IL or missing references)
		//IL_4f6c: Expected Obj, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Home));
		TreeNode val = new TreeNode("PreMo");
		TreeNode val2 = new TreeNode("OtherDevice");
		menuStrip = new MenuStrip();
		fileMenu = new ToolStripMenuItem();
		newToolStripMenuItem = new ToolStripMenuItem();
		openToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator3 = new ToolStripSeparator();
		saveToolStripMenuItem = new ToolStripMenuItem();
		saveAsToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator4 = new ToolStripSeparator();
		printToolStripMenuItem = new ToolStripMenuItem();
		printPreviewToolStripMenuItem = new ToolStripMenuItem();
		printSetupToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator5 = new ToolStripSeparator();
		exitToolStripMenuItem = new ToolStripMenuItem();
		editMenu = new ToolStripMenuItem();
		redoToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator6 = new ToolStripSeparator();
		copyToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator7 = new ToolStripSeparator();
		viewMenu = new ToolStripMenuItem();
		toolBarToolStripMenuItem = new ToolStripMenuItem();
		toolsMenu = new ToolStripMenuItem();
		optionsToolStripMenuItem = new ToolStripMenuItem();
		windowsMenu = new ToolStripMenuItem();
		newWindowToolStripMenuItem = new ToolStripMenuItem();
		cascadeToolStripMenuItem = new ToolStripMenuItem();
		tileVerticalToolStripMenuItem = new ToolStripMenuItem();
		tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
		closeAllToolStripMenuItem = new ToolStripMenuItem();
		arrangeIconsToolStripMenuItem = new ToolStripMenuItem();
		helpMenu = new ToolStripMenuItem();
		contentsToolStripMenuItem = new ToolStripMenuItem();
		indexToolStripMenuItem = new ToolStripMenuItem();
		searchToolStripMenuItem = new ToolStripMenuItem();
		toolStripSeparator8 = new ToolStripSeparator();
		aboutToolStripMenuItem = new ToolStripMenuItem();
		toolStrip = new ToolStrip();
		newToolStripButton = new ToolStripButton();
		openToolStripButton = new ToolStripButton();
		saveToolStripButton = new ToolStripButton();
		toolStripSeparator1 = new ToolStripSeparator();
		printToolStripButton = new ToolStripButton();
		printPreviewToolStripButton = new ToolStripButton();
		toolStripSeparator2 = new ToolStripSeparator();
		helpToolStripButton = new ToolStripButton();
		statusStrip = new StatusStrip();
		toolStripStatusLabel = new ToolStripStatusLabel();
		toolTip = new ToolTip(components);
		panTop = new Panel();
		btnUsrMng = new Button();
		label27 = new Label();
		panel2 = new Panel();
		txtDown2 = new TextBox();
		txtDown1 = new TextBox();
		nUDDown2_BASS = new NumericUpDown();
		nUDDown2_Treble = new NumericUpDown();
		nUDDown2_HPF = new NumericUpDown();
		nUDDown1_BASS = new NumericUpDown();
		nUDDown1_Treble = new NumericUpDown();
		nUDDown1_HPF = new NumericUpDown();
		txtDownload2 = new TextBox();
		txtDownload1 = new TextBox();
		trbDownload2 = new TrackBar();
		trbDownload1 = new TrackBar();
		lbUserName = new Label();
		btnPreSet = new Button();
		txtIP = new TextBox();
		label1 = new Label();
		panBottom = new Panel();
		btnSaveAs = new Button();
		nUDFileNum = new NumericUpDown();
		comboBox1 = new ComboBox();
		label3 = new Label();
		label2 = new Label();
		btnSave = new Button();
		panel3 = new Panel();
		label11 = new Label();
		label10 = new Label();
		label5 = new Label();
		label4 = new Label();
		label30 = new Label();
		label24 = new Label();
		label23 = new Label();
		trbBright = new TrackBar();
		label22 = new Label();
		label14 = new Label();
		label7 = new Label();
		label6 = new Label();
		txtVolume2 = new TextBox();
		txtVolume1 = new TextBox();
		trbVolume2 = new TrackBar();
		trbVolume1 = new TrackBar();
		panel4 = new Panel();
		picUpload = new PictureBox();
		picHeadset = new PictureBox();
		txtUpGM = new TextBox();
		txtHeadGM = new TextBox();
		trbUpGM = new TrackBar();
		trbHeadGM = new TrackBar();
		lbUpGM = new Label();
		lbHeadGM = new Label();
		numUpload = new NumericUpDown();
		numHeadset = new NumericUpDown();
		label26 = new Label();
		label25 = new Label();
		toolTipCustomer = new ToolTip(components);
		txtCH1 = new TextBox();
		txtCH2 = new TextBox();
		errorProvider1 = new ErrorProvider(components);
		panel5 = new Panel();
		trvDevice = new TreeView();
		label32 = new Label();
		timDevice = new System.Windows.Forms.Timer(components);
		trbInput1 = new TrackBar();
		trbInput2 = new TrackBar();
		txtChannel1 = new TextBox();
		txtChannel2 = new TextBox();
		label8 = new Label();
		label9 = new Label();
		label12 = new Label();
		label13 = new Label();
		nUD1_HPF = new NumericUpDown();
		nUD1_Treble = new NumericUpDown();
		nUD1_BASS = new NumericUpDown();
		nUD2_HPF = new NumericUpDown();
		nUD2_Treble = new NumericUpDown();
		nUD2_BASS = new NumericUpDown();
		label15 = new Label();
		label16 = new Label();
		label17 = new Label();
		label18 = new Label();
		label19 = new Label();
		label20 = new Label();
		label21 = new Label();
		label29 = new Label();
		panel1 = new Panel();
		CheckRearHeadset_Mute = new OnOffButton();
		CheckFrontHeadset_Mute = new OnOffButton();
		Mute_Headset = new OnOffButton();
		MicMute = new OnOffButton();
		Mute_Upload = new OnOffButton();
		DisplayMod = new OnOffButton();
		Channel2_PPM = new PPMMeter();
		Channel1_PPM = new PPMMeter();
		micline2 = new MICLINE();
		micline1 = new MICLINE();
		CheckLeft_Phantom = new OnOffButton();
		CheckLeft_Pad = new OnOffButton();
		CheckLeft_Mute = new OnOffButton();
		CheckRight_Phase = new OnOffButton();
		CheckRight_Mute = new OnOffButton();
		CheckRight_Pad = new OnOffButton();
		CheckRight_Phantom = new OnOffButton();
		CheckLeft_Phase = new OnOffButton();
		scrollBarEx1 = new ScrollBarEx();
		DLR_PPM = new PPMMeter();
		DLL_PPM = new PPMMeter();
		CheckDownRight_Phase = new OnOffButton();
		CheckDownRight_Mute = new OnOffButton();
		CheckDownLeft_Phase = new OnOffButton();
		CheckDownLeft_Mute = new OnOffButton();
		((Control)menuStrip).SuspendLayout();
		((Control)toolStrip).SuspendLayout();
		((Control)statusStrip).SuspendLayout();
		((Control)panTop).SuspendLayout();
		((Control)panel2).SuspendLayout();
		((ISupportInitialize)nUDDown2_BASS).BeginInit();
		((ISupportInitialize)nUDDown2_Treble).BeginInit();
		((ISupportInitialize)nUDDown2_HPF).BeginInit();
		((ISupportInitialize)nUDDown1_BASS).BeginInit();
		((ISupportInitialize)nUDDown1_Treble).BeginInit();
		((ISupportInitialize)nUDDown1_HPF).BeginInit();
		((ISupportInitialize)trbDownload2).BeginInit();
		((ISupportInitialize)trbDownload1).BeginInit();
		((Control)panBottom).SuspendLayout();
		((ISupportInitialize)nUDFileNum).BeginInit();
		((Control)panel3).SuspendLayout();
		((ISupportInitialize)trbBright).BeginInit();
		((ISupportInitialize)trbVolume2).BeginInit();
		((ISupportInitialize)trbVolume1).BeginInit();
		((Control)panel4).SuspendLayout();
		((ISupportInitialize)picUpload).BeginInit();
		((ISupportInitialize)picHeadset).BeginInit();
		((ISupportInitialize)trbUpGM).BeginInit();
		((ISupportInitialize)trbHeadGM).BeginInit();
		((ISupportInitialize)numUpload).BeginInit();
		((ISupportInitialize)numHeadset).BeginInit();
		((ISupportInitialize)errorProvider1).BeginInit();
		((Control)panel5).SuspendLayout();
		((ISupportInitialize)trbInput1).BeginInit();
		((ISupportInitialize)trbInput2).BeginInit();
		((ISupportInitialize)nUD1_HPF).BeginInit();
		((ISupportInitialize)nUD1_Treble).BeginInit();
		((ISupportInitialize)nUD1_BASS).BeginInit();
		((ISupportInitialize)nUD2_HPF).BeginInit();
		((ISupportInitialize)nUD2_Treble).BeginInit();
		((ISupportInitialize)nUD2_BASS).BeginInit();
		((Control)panel1).SuspendLayout();
		((Control)this).SuspendLayout();
		((ToolStrip)menuStrip).Items.AddRange(new ToolStripItem[6]
		{
			(ToolStripItem)fileMenu,
			(ToolStripItem)editMenu,
			(ToolStripItem)viewMenu,
			(ToolStripItem)toolsMenu,
			(ToolStripItem)windowsMenu,
			(ToolStripItem)helpMenu
		});
		((Control)menuStrip).Location = new Point(0, 0);
		menuStrip.MdiWindowListItem = windowsMenu;
		((Control)menuStrip).Name = "menuStrip";
		((Control)menuStrip).Size = new Size(1430, 24);
		((Control)menuStrip).TabIndex = 0;
		((Control)menuStrip).Text = "MenuStrip";
		((ToolStripDropDownItem)fileMenu).DropDownItems.AddRange(new ToolStripItem[11]
		{
			(ToolStripItem)newToolStripMenuItem,
			(ToolStripItem)openToolStripMenuItem,
			(ToolStripItem)toolStripSeparator3,
			(ToolStripItem)saveToolStripMenuItem,
			(ToolStripItem)saveAsToolStripMenuItem,
			(ToolStripItem)toolStripSeparator4,
			(ToolStripItem)printToolStripMenuItem,
			(ToolStripItem)printPreviewToolStripMenuItem,
			(ToolStripItem)printSetupToolStripMenuItem,
			(ToolStripItem)toolStripSeparator5,
			(ToolStripItem)exitToolStripMenuItem
		});
		((ToolStripItem)fileMenu).ImageTransparentColor = SystemColors.ActiveBorder;
		((ToolStripItem)fileMenu).Name = "fileMenu";
		((ToolStripItem)fileMenu).Size = new Size(37, 20);
		((ToolStripItem)fileMenu).Text = "&File";
		((ToolStripItem)newToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("newToolStripMenuItem.Image");
		((ToolStripItem)newToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)newToolStripMenuItem).Name = "newToolStripMenuItem";
		newToolStripMenuItem.ShortcutKeys = (Keys)131150;
		((ToolStripItem)newToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)newToolStripMenuItem).Text = "&New";
		((ToolStripItem)newToolStripMenuItem).Click += ShowNewForm;
		((ToolStripItem)openToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("openToolStripMenuItem.Image");
		((ToolStripItem)openToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)openToolStripMenuItem).Name = "openToolStripMenuItem";
		openToolStripMenuItem.ShortcutKeys = (Keys)131151;
		((ToolStripItem)openToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)openToolStripMenuItem).Text = "&Open";
		((ToolStripItem)openToolStripMenuItem).Click += OpenFile;
		((ToolStripItem)toolStripSeparator3).Name = "toolStripSeparator3";
		((ToolStripItem)toolStripSeparator3).Size = new Size(143, 6);
		((ToolStripItem)saveToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("saveToolStripMenuItem.Image");
		((ToolStripItem)saveToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)saveToolStripMenuItem).Name = "saveToolStripMenuItem";
		saveToolStripMenuItem.ShortcutKeys = (Keys)131155;
		((ToolStripItem)saveToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)saveToolStripMenuItem).Text = "&Save";
		((ToolStripItem)saveAsToolStripMenuItem).Name = "saveAsToolStripMenuItem";
		((ToolStripItem)saveAsToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)saveAsToolStripMenuItem).Text = "Save &As";
		((ToolStripItem)saveAsToolStripMenuItem).Click += SaveAsToolStripMenuItem_Click;
		((ToolStripItem)toolStripSeparator4).Name = "toolStripSeparator4";
		((ToolStripItem)toolStripSeparator4).Size = new Size(143, 6);
		((ToolStripItem)printToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("printToolStripMenuItem.Image");
		((ToolStripItem)printToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)printToolStripMenuItem).Name = "printToolStripMenuItem";
		printToolStripMenuItem.ShortcutKeys = (Keys)131152;
		((ToolStripItem)printToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)printToolStripMenuItem).Text = "&Print";
		((ToolStripItem)printPreviewToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("printPreviewToolStripMenuItem.Image");
		((ToolStripItem)printPreviewToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)printPreviewToolStripMenuItem).Name = "printPreviewToolStripMenuItem";
		((ToolStripItem)printPreviewToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)printPreviewToolStripMenuItem).Text = "Print Pre&view";
		((ToolStripItem)printSetupToolStripMenuItem).Name = "printSetupToolStripMenuItem";
		((ToolStripItem)printSetupToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)printSetupToolStripMenuItem).Text = "Print Setup";
		((ToolStripItem)toolStripSeparator5).Name = "toolStripSeparator5";
		((ToolStripItem)toolStripSeparator5).Size = new Size(143, 6);
		((ToolStripItem)exitToolStripMenuItem).Name = "exitToolStripMenuItem";
		((ToolStripItem)exitToolStripMenuItem).Size = new Size(146, 22);
		((ToolStripItem)exitToolStripMenuItem).Text = "E&xit";
		((ToolStripItem)exitToolStripMenuItem).Click += ExitToolsStripMenuItem_Click;
		((ToolStripDropDownItem)editMenu).DropDownItems.AddRange(new ToolStripItem[4]
		{
			(ToolStripItem)redoToolStripMenuItem,
			(ToolStripItem)toolStripSeparator6,
			(ToolStripItem)copyToolStripMenuItem,
			(ToolStripItem)toolStripSeparator7
		});
		((ToolStripItem)editMenu).Name = "editMenu";
		((ToolStripItem)editMenu).Size = new Size(57, 20);
		((ToolStripItem)editMenu).Text = "&Update";
		((ToolStripItem)editMenu).Click += editMenu_Click;
		((ToolStripItem)redoToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("redoToolStripMenuItem.Image");
		((ToolStripItem)redoToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)redoToolStripMenuItem).Name = "redoToolStripMenuItem";
		redoToolStripMenuItem.ShortcutKeys = (Keys)131161;
		((ToolStripItem)redoToolStripMenuItem).Size = new Size(152, 22);
		((ToolStripItem)redoToolStripMenuItem).Text = "&Redo";
		((ToolStripItem)toolStripSeparator6).Name = "toolStripSeparator6";
		((ToolStripItem)toolStripSeparator6).Size = new Size(149, 6);
		((ToolStripItem)copyToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("copyToolStripMenuItem.Image");
		((ToolStripItem)copyToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)copyToolStripMenuItem).Name = "copyToolStripMenuItem";
		copyToolStripMenuItem.ShortcutKeys = (Keys)131139;
		((ToolStripItem)copyToolStripMenuItem).Size = new Size(152, 22);
		((ToolStripItem)copyToolStripMenuItem).Text = "Cancel";
		((ToolStripItem)copyToolStripMenuItem).Click += CopyToolStripMenuItem_Click;
		((ToolStripItem)toolStripSeparator7).Name = "toolStripSeparator7";
		((ToolStripItem)toolStripSeparator7).Size = new Size(149, 6);
		((ToolStripDropDownItem)viewMenu).DropDownItems.AddRange(new ToolStripItem[1] { (ToolStripItem)toolBarToolStripMenuItem });
		((ToolStripItem)viewMenu).Name = "viewMenu";
		((ToolStripItem)viewMenu).Size = new Size(42, 20);
		((ToolStripItem)viewMenu).Text = "&ARC";
		((ToolStripItem)viewMenu).Click += viewMenu_Click;
		toolBarToolStripMenuItem.Checked = true;
		toolBarToolStripMenuItem.CheckOnClick = true;
		toolBarToolStripMenuItem.CheckState = (CheckState)1;
		((ToolStripItem)toolBarToolStripMenuItem).Name = "toolBarToolStripMenuItem";
		((ToolStripItem)toolBarToolStripMenuItem).Size = new Size(134, 22);
		((ToolStripItem)toolBarToolStripMenuItem).Text = "InVokeWeb";
		((ToolStripItem)toolBarToolStripMenuItem).Click += ToolBarToolStripMenuItem_Click;
		((ToolStripDropDownItem)toolsMenu).DropDownItems.AddRange(new ToolStripItem[1] { (ToolStripItem)optionsToolStripMenuItem });
		((ToolStripItem)toolsMenu).Name = "toolsMenu";
		((ToolStripItem)toolsMenu).Size = new Size(48, 20);
		((ToolStripItem)toolsMenu).Text = "&Tools";
		((ToolStripItem)optionsToolStripMenuItem).Name = "optionsToolStripMenuItem";
		((ToolStripItem)optionsToolStripMenuItem).Size = new Size(116, 22);
		((ToolStripItem)optionsToolStripMenuItem).Text = "&Options";
		((ToolStripDropDownItem)windowsMenu).DropDownItems.AddRange(new ToolStripItem[6]
		{
			(ToolStripItem)newWindowToolStripMenuItem,
			(ToolStripItem)cascadeToolStripMenuItem,
			(ToolStripItem)tileVerticalToolStripMenuItem,
			(ToolStripItem)tileHorizontalToolStripMenuItem,
			(ToolStripItem)closeAllToolStripMenuItem,
			(ToolStripItem)arrangeIconsToolStripMenuItem
		});
		((ToolStripItem)windowsMenu).Name = "windowsMenu";
		((ToolStripItem)windowsMenu).Size = new Size(68, 20);
		((ToolStripItem)windowsMenu).Text = "&Windows";
		((ToolStripItem)newWindowToolStripMenuItem).Name = "newWindowToolStripMenuItem";
		((ToolStripItem)newWindowToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)newWindowToolStripMenuItem).Text = "&New Window";
		((ToolStripItem)newWindowToolStripMenuItem).Click += ShowNewForm;
		((ToolStripItem)cascadeToolStripMenuItem).Name = "cascadeToolStripMenuItem";
		((ToolStripItem)cascadeToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)cascadeToolStripMenuItem).Text = "&Cascade";
		((ToolStripItem)cascadeToolStripMenuItem).Click += CascadeToolStripMenuItem_Click;
		((ToolStripItem)tileVerticalToolStripMenuItem).Name = "tileVerticalToolStripMenuItem";
		((ToolStripItem)tileVerticalToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)tileVerticalToolStripMenuItem).Text = "Tile &Vertical";
		((ToolStripItem)tileVerticalToolStripMenuItem).Click += TileVerticalToolStripMenuItem_Click;
		((ToolStripItem)tileHorizontalToolStripMenuItem).Name = "tileHorizontalToolStripMenuItem";
		((ToolStripItem)tileHorizontalToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)tileHorizontalToolStripMenuItem).Text = "Tile &Horizontal";
		((ToolStripItem)tileHorizontalToolStripMenuItem).Click += TileHorizontalToolStripMenuItem_Click;
		((ToolStripItem)closeAllToolStripMenuItem).Name = "closeAllToolStripMenuItem";
		((ToolStripItem)closeAllToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)closeAllToolStripMenuItem).Text = "C&lose All";
		((ToolStripItem)closeAllToolStripMenuItem).Click += CloseAllToolStripMenuItem_Click;
		((ToolStripItem)arrangeIconsToolStripMenuItem).Name = "arrangeIconsToolStripMenuItem";
		((ToolStripItem)arrangeIconsToolStripMenuItem).Size = new Size(151, 22);
		((ToolStripItem)arrangeIconsToolStripMenuItem).Text = "&Arrange Icons";
		((ToolStripItem)arrangeIconsToolStripMenuItem).Click += ArrangeIconsToolStripMenuItem_Click;
		((ToolStripDropDownItem)helpMenu).DropDownItems.AddRange(new ToolStripItem[5]
		{
			(ToolStripItem)contentsToolStripMenuItem,
			(ToolStripItem)indexToolStripMenuItem,
			(ToolStripItem)searchToolStripMenuItem,
			(ToolStripItem)toolStripSeparator8,
			(ToolStripItem)aboutToolStripMenuItem
		});
		((ToolStripItem)helpMenu).Name = "helpMenu";
		((ToolStripItem)helpMenu).Size = new Size(44, 20);
		((ToolStripItem)helpMenu).Text = "&Help";
		((ToolStripItem)contentsToolStripMenuItem).Name = "contentsToolStripMenuItem";
		contentsToolStripMenuItem.ShortcutKeys = (Keys)131184;
		((ToolStripItem)contentsToolStripMenuItem).Size = new Size(168, 22);
		((ToolStripItem)contentsToolStripMenuItem).Text = "&Contents";
		((ToolStripItem)indexToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("indexToolStripMenuItem.Image");
		((ToolStripItem)indexToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)indexToolStripMenuItem).Name = "indexToolStripMenuItem";
		((ToolStripItem)indexToolStripMenuItem).Size = new Size(168, 22);
		((ToolStripItem)indexToolStripMenuItem).Text = "&Index";
		((ToolStripItem)searchToolStripMenuItem).Image = (Image)componentResourceManager.GetObject("searchToolStripMenuItem.Image");
		((ToolStripItem)searchToolStripMenuItem).ImageTransparentColor = Color.Black;
		((ToolStripItem)searchToolStripMenuItem).Name = "searchToolStripMenuItem";
		((ToolStripItem)searchToolStripMenuItem).Size = new Size(168, 22);
		((ToolStripItem)searchToolStripMenuItem).Text = "&Search";
		((ToolStripItem)toolStripSeparator8).Name = "toolStripSeparator8";
		((ToolStripItem)toolStripSeparator8).Size = new Size(165, 6);
		((ToolStripItem)aboutToolStripMenuItem).Name = "aboutToolStripMenuItem";
		((ToolStripItem)aboutToolStripMenuItem).Size = new Size(168, 22);
		((ToolStripItem)aboutToolStripMenuItem).Text = "&About ... ...";
		toolStrip.Items.AddRange(new ToolStripItem[8]
		{
			(ToolStripItem)newToolStripButton,
			(ToolStripItem)openToolStripButton,
			(ToolStripItem)saveToolStripButton,
			(ToolStripItem)toolStripSeparator1,
			(ToolStripItem)printToolStripButton,
			(ToolStripItem)printPreviewToolStripButton,
			(ToolStripItem)toolStripSeparator2,
			(ToolStripItem)helpToolStripButton
		});
		((Control)toolStrip).Location = new Point(0, 24);
		((Control)toolStrip).Name = "toolStrip";
		((Control)toolStrip).Size = new Size(1430, 25);
		((Control)toolStrip).TabIndex = 1;
		((Control)toolStrip).Text = "ToolStrip";
		((ToolStripItem)newToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)newToolStripButton).Image = (Image)componentResourceManager.GetObject("newToolStripButton.Image");
		((ToolStripItem)newToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)newToolStripButton).Name = "newToolStripButton";
		((ToolStripItem)newToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)newToolStripButton).Text = "New";
		((ToolStripItem)newToolStripButton).Click += ShowNewForm;
		((ToolStripItem)openToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)openToolStripButton).Image = (Image)componentResourceManager.GetObject("openToolStripButton.Image");
		((ToolStripItem)openToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)openToolStripButton).Name = "openToolStripButton";
		((ToolStripItem)openToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)openToolStripButton).Text = "Open";
		((ToolStripItem)openToolStripButton).Click += OpenFile;
		((ToolStripItem)saveToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)saveToolStripButton).Image = (Image)componentResourceManager.GetObject("saveToolStripButton.Image");
		((ToolStripItem)saveToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)saveToolStripButton).Name = "saveToolStripButton";
		((ToolStripItem)saveToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)saveToolStripButton).Text = "Save";
		((ToolStripItem)toolStripSeparator1).Name = "toolStripSeparator1";
		((ToolStripItem)toolStripSeparator1).Size = new Size(6, 25);
		((ToolStripItem)printToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)printToolStripButton).Image = (Image)componentResourceManager.GetObject("printToolStripButton.Image");
		((ToolStripItem)printToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)printToolStripButton).Name = "printToolStripButton";
		((ToolStripItem)printToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)printToolStripButton).Text = "Print";
		((ToolStripItem)printPreviewToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)printPreviewToolStripButton).Image = (Image)componentResourceManager.GetObject("printPreviewToolStripButton.Image");
		((ToolStripItem)printPreviewToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)printPreviewToolStripButton).Name = "printPreviewToolStripButton";
		((ToolStripItem)printPreviewToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)printPreviewToolStripButton).Text = "Print Preview";
		((ToolStripItem)toolStripSeparator2).Name = "toolStripSeparator2";
		((ToolStripItem)toolStripSeparator2).Size = new Size(6, 25);
		((ToolStripItem)helpToolStripButton).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)helpToolStripButton).Image = (Image)componentResourceManager.GetObject("helpToolStripButton.Image");
		((ToolStripItem)helpToolStripButton).ImageTransparentColor = Color.Black;
		((ToolStripItem)helpToolStripButton).Name = "helpToolStripButton";
		((ToolStripItem)helpToolStripButton).Size = new Size(23, 22);
		((ToolStripItem)helpToolStripButton).Text = "Help";
		((ToolStrip)statusStrip).Items.AddRange(new ToolStripItem[1] { (ToolStripItem)toolStripStatusLabel });
		((Control)statusStrip).Location = new Point(0, 834);
		((Control)statusStrip).Name = "statusStrip";
		((Control)statusStrip).Size = new Size(1430, 22);
		((Control)statusStrip).TabIndex = 2;
		((Control)statusStrip).Text = "StatusStrip";
		((ToolStripItem)toolStripStatusLabel).Name = "toolStripStatusLabel";
		((ToolStripItem)toolStripStatusLabel).Size = new Size(39, 17);
		((ToolStripItem)toolStripStatusLabel).Text = "Status";
		((Control)panTop).AutoSize = true;
		((Control)panTop).BackColor = Color.Gainsboro;
		((Control)panTop).Controls.Add((Control)(object)btnUsrMng);
		((Control)panTop).Controls.Add((Control)(object)label27);
		((Control)panTop).Controls.Add((Control)(object)panel2);
		((Control)panTop).Controls.Add((Control)(object)lbUserName);
		((Control)panTop).Controls.Add((Control)(object)btnPreSet);
		((Control)panTop).Controls.Add((Control)(object)txtIP);
		((Control)panTop).Controls.Add((Control)(object)label1);
		((Control)panTop).Location = new Point(129, 50);
		((Control)panTop).Name = "panTop";
		((Control)panTop).Size = new Size(1296, 595);
		((Control)panTop).TabIndex = 4;
		((Control)btnUsrMng).Location = new Point(500, 7);
		((Control)btnUsrMng).Name = "btnUsrMng";
		((Control)btnUsrMng).Size = new Size(103, 23);
		((Control)btnUsrMng).TabIndex = 13;
		((Control)btnUsrMng).Text = "User Management";
		((ButtonBase)btnUsrMng).UseVisualStyleBackColor = true;
		((Control)btnUsrMng).Click += btnUsrMng_Click;
		((Control)label27).AutoSize = true;
		((Control)label27).Location = new Point(354, 11);
		((Control)label27).Name = "label27";
		((Control)label27).Size = new Size(74, 13);
		((Control)label27).TabIndex = 12;
		((Control)label27).Text = "Logged in as: ";
		((Control)panel2).BackColor = Color.WhiteSmoke;
		((Control)panel2).Controls.Add((Control)(object)DLR_PPM);
		((Control)panel2).Controls.Add((Control)(object)DLL_PPM);
		((Control)panel2).Controls.Add((Control)(object)CheckDownRight_Phase);
		((Control)panel2).Controls.Add((Control)(object)CheckDownRight_Mute);
		((Control)panel2).Controls.Add((Control)(object)CheckDownLeft_Phase);
		((Control)panel2).Controls.Add((Control)(object)CheckDownLeft_Mute);
		((Control)panel2).Controls.Add((Control)(object)txtDown2);
		((Control)panel2).Controls.Add((Control)(object)txtDown1);
		((Control)panel2).Controls.Add((Control)(object)nUDDown2_BASS);
		((Control)panel2).Controls.Add((Control)(object)nUDDown2_Treble);
		((Control)panel2).Controls.Add((Control)(object)nUDDown2_HPF);
		((Control)panel2).Controls.Add((Control)(object)nUDDown1_BASS);
		((Control)panel2).Controls.Add((Control)(object)nUDDown1_Treble);
		((Control)panel2).Controls.Add((Control)(object)nUDDown1_HPF);
		((Control)panel2).Controls.Add((Control)(object)txtDownload2);
		((Control)panel2).Controls.Add((Control)(object)txtDownload1);
		((Control)panel2).Controls.Add((Control)(object)trbDownload2);
		((Control)panel2).Controls.Add((Control)(object)trbDownload1);
		((Control)panel2).Location = new Point(281, 37);
		((Control)panel2).Name = "panel2";
		((Control)panel2).Size = new Size(270, 555);
		((Control)panel2).TabIndex = 7;
		((Control)panel2).Paint += panel2_Paint;
		((Control)txtDown2).Location = new Point(192, 7);
		((Control)txtDown2).Name = "txtDown2";
		((Control)txtDown2).Size = new Size(52, 20);
		((Control)txtDown2).TabIndex = 35;
		((Control)txtDown2).Text = "DLOAD2";
		toolTipCustomer.SetToolTip((Control)(object)txtDown2, "The first letter is letter,\r\n the length is greater than 2 and less than or equal to 8\r\n");
		((Control)txtDown1).Location = new Point(61, 7);
		((Control)txtDown1).Name = "txtDown1";
		((Control)txtDown1).Size = new Size(54, 20);
		((Control)txtDown1).TabIndex = 36;
		((Control)txtDown1).Text = "DLOAD1";
		toolTipCustomer.SetToolTip((Control)(object)txtDown1, "The first letter is letter,\r\n the length is greater than 2 and less than or equal to 8\r\n");
		((Control)nUDDown2_BASS).Location = new Point(183, 462);
		nUDDown2_BASS.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUDDown2_BASS.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUDDown2_BASS).Name = "nUDDown2_BASS";
		((Control)nUDDown2_BASS).Size = new Size(52, 20);
		((Control)nUDDown2_BASS).TabIndex = 22;
		nUDDown2_BASS.ValueChanged += nUDDown2_BASS_ValueChanged;
		((Control)nUDDown2_Treble).Location = new Point(183, 436);
		nUDDown2_Treble.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUDDown2_Treble.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUDDown2_Treble).Name = "nUDDown2_Treble";
		((Control)nUDDown2_Treble).Size = new Size(52, 20);
		((Control)nUDDown2_Treble).TabIndex = 21;
		nUDDown2_Treble.ValueChanged += nUDDown2_Treble_ValueChanged;
		((Control)nUDDown2_HPF).Location = new Point(183, 410);
		nUDDown2_HPF.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		((Control)nUDDown2_HPF).Name = "nUDDown2_HPF";
		((Control)nUDDown2_HPF).Size = new Size(52, 20);
		((Control)nUDDown2_HPF).TabIndex = 20;
		nUDDown2_HPF.ValueChanged += nUDDown2_HPF_ValueChanged;
		((Control)nUDDown1_BASS).Location = new Point(55, 462);
		nUDDown1_BASS.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUDDown1_BASS.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUDDown1_BASS).Name = "nUDDown1_BASS";
		((Control)nUDDown1_BASS).Size = new Size(52, 20);
		((Control)nUDDown1_BASS).TabIndex = 19;
		nUDDown1_BASS.ValueChanged += nUDDown1_BASS_ValueChanged;
		((Control)nUDDown1_Treble).Location = new Point(55, 436);
		nUDDown1_Treble.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUDDown1_Treble.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUDDown1_Treble).Name = "nUDDown1_Treble";
		((Control)nUDDown1_Treble).Size = new Size(52, 20);
		((Control)nUDDown1_Treble).TabIndex = 18;
		nUDDown1_Treble.ValueChanged += nUDDown1_Treble_ValueChanged;
		((Control)nUDDown1_HPF).Location = new Point(55, 410);
		nUDDown1_HPF.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		((Control)nUDDown1_HPF).Name = "nUDDown1_HPF";
		((Control)nUDDown1_HPF).Size = new Size(52, 20);
		((Control)nUDDown1_HPF).TabIndex = 17;
		nUDDown1_HPF.ValueChanged += nUDDown1_HPF_ValueChanged;
		((Control)txtDownload2).Location = new Point(192, 193);
		((Control)txtDownload2).Name = "txtDownload2";
		((Control)txtDownload2).Size = new Size(52, 20);
		((Control)txtDownload2).TabIndex = 6;
		((Control)txtDownload1).Location = new Point(63, 193);
		((Control)txtDownload1).Name = "txtDownload1";
		((Control)txtDownload1).Size = new Size(54, 20);
		((Control)txtDownload1).TabIndex = 5;
		((Control)trbDownload2).AutoSize = false;
		((Control)trbDownload2).BackColor = Color.White;
		trbDownload2.LargeChange = 1;
		((Control)trbDownload2).Location = new Point(192, 33);
		trbDownload2.Maximum = 30;
		((Control)trbDownload2).Name = "trbDownload2";
		trbDownload2.Orientation = (Orientation)1;
		((Control)trbDownload2).Size = new Size(52, 155);
		((Control)trbDownload2).TabIndex = 2;
		((Control)trbDownload2).MouseUp += trbDownload2_MouseUp;
		((Control)trbDownload1).AutoSize = false;
		((Control)trbDownload1).BackColor = Color.White;
		trbDownload1.LargeChange = 1;
		((Control)trbDownload1).Location = new Point(63, 33);
		trbDownload1.Maximum = 30;
		((Control)trbDownload1).Name = "trbDownload1";
		trbDownload1.Orientation = (Orientation)1;
		((Control)trbDownload1).Size = new Size(52, 155);
		((Control)trbDownload1).TabIndex = 1;
		trbDownload1.Scroll += trbDownload1_Scroll;
		((Control)trbDownload1).MouseUp += trbDownload1_MouseUp;
		((Control)lbUserName).AutoSize = true;
		((Control)lbUserName).Location = new Point(434, 11);
		((Control)lbUserName).Name = "lbUserName";
		((Control)lbUserName).Size = new Size(53, 13);
		((Control)lbUserName).TabIndex = 11;
		((Control)lbUserName).Text = "username";
		((Control)btnPreSet).Location = new Point(263, 1);
		((Control)btnPreSet).Name = "btnPreSet";
		((Control)btnPreSet).Size = new Size(85, 28);
		((Control)btnPreSet).TabIndex = 3;
		((Control)btnPreSet).Text = "SET PREMO";
		((ButtonBase)btnPreSet).UseVisualStyleBackColor = true;
		((Control)txtIP).Font = new Font("Microsoft Sans Serif", 15.75f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)txtIP).Location = new Point(54, 3);
		((Control)txtIP).Name = "txtIP";
		((Control)txtIP).Size = new Size(189, 31);
		((Control)txtIP).TabIndex = 1;
		((Control)txtIP).TextChanged += txtIP_TextChanged;
		((Control)label1).BackColor = Color.White;
		((Control)label1).Font = new Font("Microsoft Sans Serif", 15.75f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)label1).Location = new Point(12, 3);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(36, 31);
		((Control)label1).TabIndex = 0;
		((Control)label1).Text = "IP:";
		label1.TextAlign = (ContentAlignment)32;
		((Control)panBottom).BackColor = Color.Gainsboro;
		((Control)panBottom).Controls.Add((Control)(object)scrollBarEx1);
		((Control)panBottom).Controls.Add((Control)(object)btnSaveAs);
		((Control)panBottom).Controls.Add((Control)(object)nUDFileNum);
		((Control)panBottom).Controls.Add((Control)(object)comboBox1);
		((Control)panBottom).Controls.Add((Control)(object)label3);
		((Control)panBottom).Controls.Add((Control)(object)label2);
		((Control)panBottom).Controls.Add((Control)(object)btnSave);
		((Control)panBottom).Location = new Point(0, 651);
		((Control)panBottom).Name = "panBottom";
		((Control)panBottom).Size = new Size(1418, 180);
		((Control)panBottom).TabIndex = 5;
		((Control)panBottom).Paint += panBottom_Paint;
		((Control)btnSaveAs).Location = new Point(199, 79);
		((Control)btnSaveAs).Name = "btnSaveAs";
		((Control)btnSaveAs).Size = new Size(101, 33);
		((Control)btnSaveAs).TabIndex = 21;
		((Control)btnSaveAs).Text = "Save As Preset";
		((ButtonBase)btnSaveAs).UseVisualStyleBackColor = true;
		((Control)btnSaveAs).Click += btnSaveAs_Click;
		((Control)nUDFileNum).Location = new Point(249, 41);
		nUDFileNum.Maximum = new decimal(new int[4] { 9, 0, 0, 0 });
		((Control)nUDFileNum).Name = "nUDFileNum";
		((Control)nUDFileNum).Size = new Size(51, 20);
		((Control)nUDFileNum).TabIndex = 20;
		((ListControl)comboBox1).FormattingEnabled = true;
		((Control)comboBox1).Location = new Point(73, 43);
		((Control)comboBox1).Name = "comboBox1";
		((Control)comboBox1).Size = new Size(121, 21);
		((Control)comboBox1).TabIndex = 5;
		((Control)label3).Font = new Font("Microsoft Sans Serif", 12f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)label3).Location = new Point(208, 44);
		((Control)label3).Name = "label3";
		((Control)label3).Size = new Size(35, 25);
		((Control)label3).TabIndex = 4;
		((Control)label3).Text = "File";
		((Control)label2).Font = new Font("Microsoft Sans Serif", 12f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)label2).Location = new Point(46, 41);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(35, 23);
		((Control)label2).TabIndex = 3;
		((Control)label2).Text = "IP";
		((Control)btnSave).Location = new Point(73, 79);
		((Control)btnSave).Name = "btnSave";
		((Control)btnSave).Size = new Size(64, 33);
		((Control)btnSave).TabIndex = 2;
		((Control)btnSave).Text = "Save ";
		((ButtonBase)btnSave).UseVisualStyleBackColor = true;
		((Control)btnSave).Click += btnSave_Click;
		((Control)panel3).BackColor = Color.WhiteSmoke;
		((Control)panel3).Controls.Add((Control)(object)CheckRearHeadset_Mute);
		((Control)panel3).Controls.Add((Control)(object)CheckFrontHeadset_Mute);
		((Control)panel3).Controls.Add((Control)(object)label11);
		((Control)panel3).Controls.Add((Control)(object)label10);
		((Control)panel3).Controls.Add((Control)(object)label5);
		((Control)panel3).Controls.Add((Control)(object)Mute_Headset);
		((Control)panel3).Controls.Add((Control)(object)MicMute);
		((Control)panel3).Controls.Add((Control)(object)Mute_Upload);
		((Control)panel3).Controls.Add((Control)(object)DisplayMod);
		((Control)panel3).Controls.Add((Control)(object)label4);
		((Control)panel3).Controls.Add((Control)(object)label30);
		((Control)panel3).Controls.Add((Control)(object)label24);
		((Control)panel3).Controls.Add((Control)(object)label23);
		((Control)panel3).Controls.Add((Control)(object)trbBright);
		((Control)panel3).Controls.Add((Control)(object)label22);
		((Control)panel3).Controls.Add((Control)(object)label14);
		((Control)panel3).Controls.Add((Control)(object)label7);
		((Control)panel3).Controls.Add((Control)(object)label6);
		((Control)panel3).Controls.Add((Control)(object)txtVolume2);
		((Control)panel3).Controls.Add((Control)(object)txtVolume1);
		((Control)panel3).Controls.Add((Control)(object)trbVolume2);
		((Control)panel3).Controls.Add((Control)(object)trbVolume1);
		((Control)panel3).Location = new Point(686, 90);
		((Control)panel3).Name = "panel3";
		((Control)panel3).Size = new Size(270, 555);
		((Control)panel3).TabIndex = 8;
		((Control)panel3).Paint += panel3_Paint;
		((Control)label11).BackColor = Color.Transparent;
		((Control)label11).Location = new Point(193, 222);
		((Control)label11).Name = "label11";
		((Control)label11).Size = new Size(64, 20);
		((Control)label11).TabIndex = 50;
		((Control)label11).Text = "On        Off";
		label11.TextAlign = (ContentAlignment)32;
		((Control)label10).BackColor = Color.Transparent;
		((Control)label10).Location = new Point(101, 222);
		((Control)label10).Name = "label10";
		((Control)label10).Size = new Size(63, 20);
		((Control)label10).TabIndex = 49;
		((Control)label10).Text = "On        Off";
		label10.TextAlign = (ContentAlignment)32;
		((Control)label5).BackColor = Color.Transparent;
		((Control)label5).Location = new Point(3, 245);
		((Control)label5).Name = "label5";
		((Control)label5).Size = new Size(90, 24);
		((Control)label5).TabIndex = 48;
		((Control)label5).Text = "Headset";
		label5.TextAlign = (ContentAlignment)32;
		((Control)label4).BackColor = Color.Transparent;
		((Control)label4).Location = new Point(3, 405);
		((Control)label4).Name = "label4";
		((Control)label4).Size = new Size(100, 24);
		((Control)label4).TabIndex = 25;
		((Control)label4).Text = "Mute_Upload";
		label4.TextAlign = (ContentAlignment)32;
		((Control)label30).BackColor = Color.Transparent;
		((Control)label30).Location = new Point(3, 431);
		((Control)label30).Name = "label30";
		((Control)label30).Size = new Size(100, 24);
		((Control)label30).TabIndex = 19;
		((Control)label30).Text = "MicMute";
		label30.TextAlign = (ContentAlignment)32;
		((Control)label24).BackColor = Color.Transparent;
		((Control)label24).Location = new Point(107, 351);
		((Control)label24).Name = "label24";
		((Control)label24).Size = new Size(63, 20);
		((Control)label24).TabIndex = 17;
		((Control)label24).Text = "On        Off";
		label24.TextAlign = (ContentAlignment)32;
		((Control)label23).BackColor = Color.Transparent;
		((Control)label23).Location = new Point(3, 457);
		((Control)label23).Name = "label23";
		((Control)label23).Size = new Size(100, 24);
		((Control)label23).TabIndex = 16;
		((Control)label23).Text = "DisplayMode";
		label23.TextAlign = (ContentAlignment)32;
		((Control)trbBright).AutoSize = false;
		((Control)trbBright).BackColor = Color.White;
		((Control)trbBright).Location = new Point(106, 519);
		trbBright.Maximum = 127;
		((Control)trbBright).Name = "trbBright";
		((Control)trbBright).Size = new Size(155, 24);
		((Control)trbBright).TabIndex = 15;
		((Control)trbBright).MouseUp += trbBright_MouseUp;
		((Control)label22).BackColor = Color.Transparent;
		((Control)label22).Location = new Point(11, 519);
		((Control)label22).Name = "label22";
		((Control)label22).Size = new Size(67, 24);
		((Control)label22).TabIndex = 14;
		((Control)label22).Text = "Brightness";
		label22.TextAlign = (ContentAlignment)32;
		((Control)label14).BackColor = Color.Transparent;
		((Control)label14).Location = new Point(3, 379);
		((Control)label14).Name = "label14";
		((Control)label14).Size = new Size(100, 24);
		((Control)label14).TabIndex = 11;
		((Control)label14).Text = "Mute_Headphones";
		label14.TextAlign = (ContentAlignment)32;
		((Control)label7).BackColor = Color.Transparent;
		((Control)label7).Location = new Point(181, 9);
		((Control)label7).Name = "label7";
		((Control)label7).Size = new Size(67, 17);
		((Control)label7).TabIndex = 8;
		((Control)label7).Text = "HEADSET2";
		((Control)label6).BackColor = Color.Transparent;
		((Control)label6).Location = new Point(90, 9);
		((Control)label6).Name = "label6";
		((Control)label6).Size = new Size(66, 17);
		((Control)label6).TabIndex = 7;
		((Control)label6).Text = "HEADSET1";
		((Control)txtVolume2).Location = new Point(196, 193);
		((Control)txtVolume2).Name = "txtVolume2";
		((Control)txtVolume2).Size = new Size(52, 20);
		((Control)txtVolume2).TabIndex = 6;
		((Control)txtVolume1).Location = new Point(104, 193);
		((Control)txtVolume1).Name = "txtVolume1";
		((Control)txtVolume1).Size = new Size(52, 20);
		((Control)txtVolume1).TabIndex = 5;
		((Control)trbVolume2).AutoSize = false;
		((Control)trbVolume2).BackColor = Color.White;
		((Control)trbVolume2).Location = new Point(196, 33);
		trbVolume2.Maximum = 255;
		((Control)trbVolume2).Name = "trbVolume2";
		trbVolume2.Orientation = (Orientation)1;
		((Control)trbVolume2).Size = new Size(52, 155);
		((Control)trbVolume2).TabIndex = 4;
		trbVolume2.Scroll += trbVolume2_Scroll;
		((Control)trbVolume2).MouseUp += trbVolume2_MouseUp;
		((Control)trbVolume1).AutoSize = false;
		((Control)trbVolume1).BackColor = Color.White;
		((Control)trbVolume1).Location = new Point(104, 33);
		trbVolume1.Maximum = 255;
		((Control)trbVolume1).Name = "trbVolume1";
		trbVolume1.Orientation = (Orientation)1;
		((Control)trbVolume1).Size = new Size(52, 155);
		((Control)trbVolume1).TabIndex = 3;
		trbVolume1.Scroll += trbVolume1_Scroll;
		((Control)trbVolume1).MouseUp += trbVolume1_MouseUp;
		((Control)panel4).BackColor = Color.WhiteSmoke;
		((Control)panel4).Controls.Add((Control)(object)picUpload);
		((Control)panel4).Controls.Add((Control)(object)picHeadset);
		((Control)panel4).Controls.Add((Control)(object)txtUpGM);
		((Control)panel4).Controls.Add((Control)(object)txtHeadGM);
		((Control)panel4).Controls.Add((Control)(object)trbUpGM);
		((Control)panel4).Controls.Add((Control)(object)trbHeadGM);
		((Control)panel4).Controls.Add((Control)(object)lbUpGM);
		((Control)panel4).Controls.Add((Control)(object)lbHeadGM);
		((Control)panel4).Controls.Add((Control)(object)numUpload);
		((Control)panel4).Controls.Add((Control)(object)numHeadset);
		((Control)panel4).Controls.Add((Control)(object)label26);
		((Control)panel4).Controls.Add((Control)(object)label25);
		((Control)panel4).Location = new Point(962, 90);
		((Control)panel4).Name = "panel4";
		((Control)panel4).Size = new Size(466, 555);
		((Control)panel4).TabIndex = 9;
		((Control)panel4).Paint += panel4_Paint;
		((Control)picUpload).BackColor = Color.White;
		((Control)picUpload).Location = new Point(38, 287);
		((Control)picUpload).Name = "picUpload";
		((Control)picUpload).Size = new Size(385, 219);
		picUpload.SizeMode = (PictureBoxSizeMode)4;
		picUpload.TabIndex = 27;
		picUpload.TabStop = false;
		((Control)picHeadset).BackColor = Color.White;
		((Control)picHeadset).Location = new Point(38, 10);
		((Control)picHeadset).Name = "picHeadset";
		((Control)picHeadset).Size = new Size(385, 222);
		picHeadset.SizeMode = (PictureBoxSizeMode)4;
		picHeadset.TabIndex = 26;
		picHeadset.TabStop = false;
		((Control)txtUpGM).Location = new Point(375, 518);
		((Control)txtUpGM).Name = "txtUpGM";
		((Control)txtUpGM).Size = new Size(48, 20);
		((Control)txtUpGM).TabIndex = 25;
		((Control)txtUpGM).Visible = false;
		((Control)txtHeadGM).Location = new Point(373, 241);
		((Control)txtHeadGM).Name = "txtHeadGM";
		((Control)txtHeadGM).Size = new Size(41, 20);
		((Control)txtHeadGM).TabIndex = 24;
		((Control)txtHeadGM).Visible = false;
		((Control)trbUpGM).AutoSize = false;
		((Control)trbUpGM).BackColor = Color.White;
		trbUpGM.LargeChange = 1;
		((Control)trbUpGM).Location = new Point(215, 512);
		trbUpGM.Maximum = 9;
		((Control)trbUpGM).Name = "trbUpGM";
		((Control)trbUpGM).Size = new Size(154, 24);
		((Control)trbUpGM).TabIndex = 23;
		((Control)trbUpGM).Visible = false;
		trbUpGM.Scroll += trbUpGM_Scroll;
		((Control)trbHeadGM).AutoSize = false;
		((Control)trbHeadGM).BackColor = Color.White;
		trbHeadGM.LargeChange = 1;
		((Control)trbHeadGM).Location = new Point(213, 238);
		trbHeadGM.Maximum = 9;
		((Control)trbHeadGM).Name = "trbHeadGM";
		((Control)trbHeadGM).Size = new Size(154, 24);
		((Control)trbHeadGM).TabIndex = 22;
		((Control)trbHeadGM).Visible = false;
		trbHeadGM.Scroll += trbHeadGM_Scroll;
		((Control)lbUpGM).BackColor = Color.Transparent;
		((Control)lbUpGM).Location = new Point(168, 515);
		((Control)lbUpGM).Name = "lbUpGM";
		((Control)lbUpGM).Size = new Size(41, 24);
		((Control)lbUpGM).TabIndex = 21;
		((Control)lbUpGM).Text = "GM";
		lbUpGM.TextAlign = (ContentAlignment)32;
		((Control)lbUpGM).Visible = false;
		((Control)lbHeadGM).BackColor = Color.Transparent;
		((Control)lbHeadGM).Location = new Point(166, 238);
		((Control)lbHeadGM).Name = "lbHeadGM";
		((Control)lbHeadGM).Size = new Size(41, 24);
		((Control)lbHeadGM).TabIndex = 20;
		((Control)lbHeadGM).Text = "GM";
		lbHeadGM.TextAlign = (ContentAlignment)32;
		((Control)lbHeadGM).Visible = false;
		((Control)numUpload).Location = new Point(110, 519);
		numUpload.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		((Control)numUpload).Name = "numUpload";
		((Control)numUpload).Size = new Size(52, 20);
		((Control)numUpload).TabIndex = 19;
		numUpload.ValueChanged += numUpload_ValueChanged;
		((Control)numHeadset).Location = new Point(108, 242);
		numHeadset.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		((Control)numHeadset).Name = "numHeadset";
		((Control)numHeadset).Size = new Size(52, 20);
		((Control)numHeadset).TabIndex = 18;
		numHeadset.ValueChanged += numHeadset_ValueChanged;
		((Control)label26).BackColor = Color.Transparent;
		((Control)label26).Location = new Point(37, 515);
		((Control)label26).Name = "label26";
		((Control)label26).Size = new Size(67, 24);
		((Control)label26).TabIndex = 13;
		((Control)label26).Text = "UPLOAD";
		label26.TextAlign = (ContentAlignment)32;
		((Control)label25).BackColor = Color.Transparent;
		((Control)label25).Location = new Point(35, 238);
		((Control)label25).Name = "label25";
		((Control)label25).Size = new Size(67, 24);
		((Control)label25).TabIndex = 12;
		((Control)label25).Text = "HEADSET";
		label25.TextAlign = (ContentAlignment)32;
		toolTipCustomer.AutoPopDelay = 5000;
		toolTipCustomer.BackColor = Color.Lime;
		toolTipCustomer.ForeColor = Color.Red;
		toolTipCustomer.InitialDelay = 2000;
		toolTipCustomer.IsBalloon = true;
		toolTipCustomer.ReshowDelay = 100;
		((Control)txtCH1).Location = new Point(95, 7);
		((Control)txtCH1).Name = "txtCH1";
		((Control)txtCH1).RightToLeft = (RightToLeft)0;
		((Control)txtCH1).Size = new Size(52, 20);
		((Control)txtCH1).TabIndex = 34;
		((Control)txtCH1).Text = "INPUT1";
		toolTipCustomer.SetToolTip((Control)(object)txtCH1, "The first letter is letter,\r\n the length is greater than 2 and less than or equal to 8");
		((Control)txtCH2).Location = new Point(189, 7);
		((Control)txtCH2).Name = "txtCH2";
		((Control)txtCH2).Size = new Size(52, 20);
		((Control)txtCH2).TabIndex = 37;
		((Control)txtCH2).Text = "INPUT2";
		toolTipCustomer.SetToolTip((Control)(object)txtCH2, "The first letter is letter,\r\n the length is greater than 2 and less than or equal to 8\r\n");
		errorProvider1.ContainerControl = (ContainerControl)(object)this;
		((Control)panel5).BackColor = Color.WhiteSmoke;
		((Control)panel5).Controls.Add((Control)(object)trvDevice);
		((Control)panel5).Controls.Add((Control)(object)label32);
		((Control)panel5).Location = new Point(2, 50);
		((Control)panel5).Name = "panel5";
		((Control)panel5).Size = new Size(124, 595);
		((Control)panel5).TabIndex = 13;
		((Control)trvDevice).ForeColor = SystemColors.WindowText;
		((Control)trvDevice).Location = new Point(3, 37);
		((Control)trvDevice).Name = "trvDevice";
		val.Name = "PreMo";
		val.Tag = "1";
		val.Text = "PreMo";
		val2.Name = "OtherDevice";
		val2.Tag = "2";
		val2.Text = "OtherDevice";
		trvDevice.Nodes.AddRange(new TreeNode[2] { val, val2 });
		trvDevice.ShowNodeToolTips = true;
		((Control)trvDevice).Size = new Size(121, 555);
		((Control)trvDevice).TabIndex = 1;
		trvDevice.AfterSelect += trvDevice_AfterSelect;
		((Control)label32).BackColor = Color.DeepSkyBlue;
		label32.BorderStyle = (BorderStyle)2;
		((Control)label32).Font = new Font("Microsoft Sans Serif", 14.25f, (FontStyle)0, (GraphicsUnit)3, (byte)0);
		((Control)label32).Location = new Point(3, 3);
		((Control)label32).Name = "label32";
		((Control)label32).Size = new Size(118, 31);
		((Control)label32).TabIndex = 0;
		((Control)label32).Text = "DEVICE";
		label32.TextAlign = (ContentAlignment)32;
		((Control)label32).Click += label32_Click;
		timDevice.Interval = 2000;
		timDevice.Tick += timDevice_Tick;
		((Control)trbInput1).AutoSize = false;
		((Control)trbInput1).BackColor = Color.White;
		((Control)trbInput1).Location = new Point(96, 33);
		trbInput1.Maximum = 63;
		((Control)trbInput1).Name = "trbInput1";
		trbInput1.Orientation = (Orientation)1;
		((Control)trbInput1).Size = new Size(52, 155);
		((Control)trbInput1).TabIndex = 1;
		trbInput1.Scroll += trbInput1_Scroll;
		((Control)trbInput1).MouseUp += trbInput1_MouseUp;
		((Control)trbInput2).AutoSize = false;
		((Control)trbInput2).BackColor = Color.White;
		((Control)trbInput2).Location = new Point(189, 33);
		trbInput2.Maximum = 63;
		((Control)trbInput2).Name = "trbInput2";
		trbInput2.Orientation = (Orientation)1;
		((Control)trbInput2).Size = new Size(52, 155);
		((Control)trbInput2).TabIndex = 1;
		trbInput2.Scroll += trbInput2_Scroll;
		((Control)trbInput2).MouseUp += trbInput2_MouseUp;
		((Control)txtChannel1).Location = new Point(95, 193);
		((Control)txtChannel1).Name = "txtChannel1";
		((Control)txtChannel1).Size = new Size(53, 20);
		((Control)txtChannel1).TabIndex = 4;
		((Control)txtChannel2).Location = new Point(189, 193);
		((Control)txtChannel2).Name = "txtChannel2";
		((Control)txtChannel2).Size = new Size(54, 20);
		((Control)txtChannel2).TabIndex = 5;
		((Control)label8).BackColor = Color.Transparent;
		((Control)label8).Location = new Point(95, 216);
		((Control)label8).Name = "label8";
		((Control)label8).Size = new Size(53, 20);
		((Control)label8).TabIndex = 6;
		((Control)label8).Text = "Line   Mic";
		((Control)label9).BackColor = Color.Transparent;
		((Control)label9).Location = new Point(189, 216);
		((Control)label9).Name = "label9";
		((Control)label9).Size = new Size(54, 20);
		((Control)label9).TabIndex = 7;
		((Control)label9).Text = "Line   Mic";
		((Control)label12).BackColor = Color.Transparent;
		((Control)label12).Location = new Point(92, 267);
		((Control)label12).Name = "label12";
		((Control)label12).Size = new Size(60, 20);
		((Control)label12).TabIndex = 10;
		((Control)label12).Text = "On        Off";
		label12.TextAlign = (ContentAlignment)32;
		((Control)label13).BackColor = Color.Transparent;
		((Control)label13).Location = new Point(180, 267);
		((Control)label13).Name = "label13";
		((Control)label13).Size = new Size(63, 20);
		((Control)label13).TabIndex = 11;
		((Control)label13).Text = "On        Off";
		label13.TextAlign = (ContentAlignment)32;
		((Control)nUD1_HPF).Location = new Point(96, 410);
		nUD1_HPF.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		((Control)nUD1_HPF).Name = "nUD1_HPF";
		((Control)nUD1_HPF).Size = new Size(52, 20);
		((Control)nUD1_HPF).TabIndex = 16;
		nUD1_HPF.ValueChanged += nUD1_HPF_ValueChanged;
		((Control)nUD1_Treble).Location = new Point(96, 436);
		nUD1_Treble.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUD1_Treble.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUD1_Treble).Name = "nUD1_Treble";
		((Control)nUD1_Treble).Size = new Size(52, 20);
		((Control)nUD1_Treble).TabIndex = 17;
		nUD1_Treble.ValueChanged += nUD1_Treble_ValueChanged;
		((Control)nUD1_BASS).Location = new Point(96, 462);
		nUD1_BASS.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUD1_BASS.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUD1_BASS).Name = "nUD1_BASS";
		((Control)nUD1_BASS).Size = new Size(52, 20);
		((Control)nUD1_BASS).TabIndex = 18;
		nUD1_BASS.ValueChanged += nUD1_BASS_ValueChanged;
		((Control)nUD2_HPF).Location = new Point(189, 410);
		nUD2_HPF.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		((Control)nUD2_HPF).Name = "nUD2_HPF";
		((Control)nUD2_HPF).Size = new Size(52, 20);
		((Control)nUD2_HPF).TabIndex = 19;
		nUD2_HPF.ValueChanged += nUD2_HPF_ValueChanged;
		((Control)nUD2_Treble).Location = new Point(189, 436);
		nUD2_Treble.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUD2_Treble.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUD2_Treble).Name = "nUD2_Treble";
		((Control)nUD2_Treble).Size = new Size(52, 20);
		((Control)nUD2_Treble).TabIndex = 20;
		nUD2_Treble.ValueChanged += nUD2_Treble_ValueChanged;
		((Control)nUD2_BASS).Location = new Point(189, 462);
		nUD2_BASS.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		nUD2_BASS.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		((Control)nUD2_BASS).Name = "nUD2_BASS";
		((Control)nUD2_BASS).Size = new Size(52, 20);
		((Control)nUD2_BASS).TabIndex = 21;
		nUD2_BASS.ValueChanged += nUD2_BASS_ValueChanged;
		((Control)label15).BackColor = Color.Transparent;
		((Control)label15).Location = new Point(12, 267);
		((Control)label15).Name = "label15";
		((Control)label15).Size = new Size(67, 20);
		((Control)label15).TabIndex = 24;
		((Control)label15).Text = "Source";
		label15.TextAlign = (ContentAlignment)32;
		((Control)label16).BackColor = Color.Transparent;
		((Control)label16).Location = new Point(12, 289);
		((Control)label16).Name = "label16";
		((Control)label16).Size = new Size(67, 24);
		((Control)label16).TabIndex = 25;
		((Control)label16).Text = "+48V";
		label16.TextAlign = (ContentAlignment)32;
		((Control)label17).BackColor = Color.Transparent;
		((Control)label17).Location = new Point(12, 319);
		((Control)label17).Name = "label17";
		((Control)label17).Size = new Size(67, 24);
		((Control)label17).TabIndex = 26;
		((Control)label17).Text = "Pad";
		label17.TextAlign = (ContentAlignment)32;
		((Control)label18).BackColor = Color.Transparent;
		((Control)label18).Location = new Point(12, 349);
		((Control)label18).Name = "label18";
		((Control)label18).Size = new Size(67, 22);
		((Control)label18).TabIndex = 27;
		((Control)label18).Text = "Mute";
		label18.TextAlign = (ContentAlignment)32;
		((Control)label19).BackColor = Color.Transparent;
		((Control)label19).Location = new Point(12, 405);
		((Control)label19).Name = "label19";
		((Control)label19).Size = new Size(67, 24);
		((Control)label19).TabIndex = 28;
		((Control)label19).Text = "HPF";
		label19.TextAlign = (ContentAlignment)32;
		((Control)label20).BackColor = Color.Transparent;
		((Control)label20).Location = new Point(12, 431);
		((Control)label20).Name = "label20";
		((Control)label20).Size = new Size(67, 24);
		((Control)label20).TabIndex = 29;
		((Control)label20).Text = "Treble";
		label20.TextAlign = (ContentAlignment)32;
		((Control)label21).BackColor = Color.Transparent;
		((Control)label21).Location = new Point(12, 457);
		((Control)label21).Name = "label21";
		((Control)label21).Size = new Size(67, 24);
		((Control)label21).TabIndex = 30;
		((Control)label21).Text = "BASS";
		label21.TextAlign = (ContentAlignment)32;
		((Control)label29).BackColor = Color.Transparent;
		((Control)label29).Location = new Point(12, 379);
		((Control)label29).Name = "label29";
		((Control)label29).Size = new Size(67, 24);
		((Control)label29).TabIndex = 31;
		((Control)label29).Text = "Phase";
		label29.TextAlign = (ContentAlignment)32;
		((Control)panel1).BackColor = Color.WhiteSmoke;
		((Control)panel1).Controls.Add((Control)(object)Channel2_PPM);
		((Control)panel1).Controls.Add((Control)(object)Channel1_PPM);
		((Control)panel1).Controls.Add((Control)(object)micline2);
		((Control)panel1).Controls.Add((Control)(object)micline1);
		((Control)panel1).Controls.Add((Control)(object)CheckLeft_Phantom);
		((Control)panel1).Controls.Add((Control)(object)CheckLeft_Pad);
		((Control)panel1).Controls.Add((Control)(object)CheckLeft_Mute);
		((Control)panel1).Controls.Add((Control)(object)CheckRight_Phase);
		((Control)panel1).Controls.Add((Control)(object)CheckRight_Mute);
		((Control)panel1).Controls.Add((Control)(object)CheckRight_Pad);
		((Control)panel1).Controls.Add((Control)(object)CheckRight_Phantom);
		((Control)panel1).Controls.Add((Control)(object)CheckLeft_Phase);
		((Control)panel1).Controls.Add((Control)(object)txtCH2);
		((Control)panel1).Controls.Add((Control)(object)txtCH1);
		((Control)panel1).Controls.Add((Control)(object)label29);
		((Control)panel1).Controls.Add((Control)(object)label21);
		((Control)panel1).Controls.Add((Control)(object)label20);
		((Control)panel1).Controls.Add((Control)(object)label19);
		((Control)panel1).Controls.Add((Control)(object)label18);
		((Control)panel1).Controls.Add((Control)(object)label17);
		((Control)panel1).Controls.Add((Control)(object)label16);
		((Control)panel1).Controls.Add((Control)(object)label15);
		((Control)panel1).Controls.Add((Control)(object)nUD2_BASS);
		((Control)panel1).Controls.Add((Control)(object)nUD2_Treble);
		((Control)panel1).Controls.Add((Control)(object)nUD2_HPF);
		((Control)panel1).Controls.Add((Control)(object)nUD1_BASS);
		((Control)panel1).Controls.Add((Control)(object)nUD1_Treble);
		((Control)panel1).Controls.Add((Control)(object)nUD1_HPF);
		((Control)panel1).Controls.Add((Control)(object)label13);
		((Control)panel1).Controls.Add((Control)(object)label12);
		((Control)panel1).Controls.Add((Control)(object)label9);
		((Control)panel1).Controls.Add((Control)(object)label8);
		((Control)panel1).Controls.Add((Control)(object)txtChannel2);
		((Control)panel1).Controls.Add((Control)(object)txtChannel1);
		((Control)panel1).Controls.Add((Control)(object)trbInput2);
		((Control)panel1).Controls.Add((Control)(object)trbInput1);
		((Control)panel1).Location = new Point(132, 87);
		((Control)panel1).Name = "panel1";
		((Control)panel1).Size = new Size(275, 555);
		((Control)panel1).TabIndex = 6;
		((Control)panel1).Paint += panel1_Paint;
		((Control)CheckRearHeadset_Mute).BackColor = Color.Transparent;
		CheckRearHeadset_Mute.Checked = false;
		CheckRearHeadset_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckRearHeadset_Mute).Cursor = Cursors.Hand;
		((Control)CheckRearHeadset_Mute).Location = new Point(191, 260);
		((Control)CheckRearHeadset_Mute).Name = "CheckRearHeadset_Mute";
		((Control)CheckRearHeadset_Mute).Size = new Size(65, 24);
		((Control)CheckRearHeadset_Mute).TabIndex = 52;
		((Control)CheckRearHeadset_Mute).Click += CheckRearHeadset_Mute_Click;
		((Control)CheckFrontHeadset_Mute).BackColor = Color.Transparent;
		CheckFrontHeadset_Mute.Checked = false;
		CheckFrontHeadset_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckFrontHeadset_Mute).Cursor = Cursors.Hand;
		((Control)CheckFrontHeadset_Mute).Location = new Point(107, 260);
		((Control)CheckFrontHeadset_Mute).Name = "CheckFrontHeadset_Mute";
		((Control)CheckFrontHeadset_Mute).Size = new Size(65, 24);
		((Control)CheckFrontHeadset_Mute).TabIndex = 51;
		((Control)CheckFrontHeadset_Mute).Click += CheckFrontHeadset_Mute_Click;
		((Control)Mute_Headset).BackColor = Color.Transparent;
		Mute_Headset.Checked = false;
		Mute_Headset.CheckStyleX = CheckStyle.style3;
		((Control)Mute_Headset).Cursor = Cursors.Hand;
		((Control)Mute_Headset).Location = new Point(110, 379);
		((Control)Mute_Headset).Name = "Mute_Headset";
		((Control)Mute_Headset).Size = new Size(65, 24);
		((Control)Mute_Headset).TabIndex = 45;
		((Control)Mute_Headset).Click += Mute_Headset_Click;
		((Control)MicMute).BackColor = Color.Transparent;
		MicMute.Checked = false;
		MicMute.CheckStyleX = CheckStyle.style3;
		((Control)MicMute).Cursor = Cursors.Hand;
		((Control)MicMute).Location = new Point(110, 431);
		((Control)MicMute).Name = "MicMute";
		((Control)MicMute).Size = new Size(65, 24);
		((Control)MicMute).TabIndex = 46;
		((Control)MicMute).Click += MicMute_Click;
		((Control)Mute_Upload).BackColor = Color.Transparent;
		Mute_Upload.Checked = false;
		Mute_Upload.CheckStyleX = CheckStyle.style3;
		((Control)Mute_Upload).Cursor = Cursors.Hand;
		((Control)Mute_Upload).Location = new Point(110, 405);
		((Control)Mute_Upload).Name = "Mute_Upload";
		((Control)Mute_Upload).Size = new Size(65, 24);
		((Control)Mute_Upload).TabIndex = 44;
		((Control)Mute_Upload).Click += Mute_Upload_Click;
		((Control)DisplayMod).BackColor = Color.Transparent;
		DisplayMod.Checked = false;
		DisplayMod.CheckStyleX = CheckStyle.style3;
		((Control)DisplayMod).Cursor = Cursors.Hand;
		((Control)DisplayMod).Location = new Point(110, 457);
		((Control)DisplayMod).Name = "DisplayMod";
		((Control)DisplayMod).Size = new Size(65, 24);
		((Control)DisplayMod).TabIndex = 47;
		((Control)DisplayMod).Click += DisplayMod_Click;
		Channel2_PPM.BarColourUnlit = PPMMeter.BarColour_Unlit.White;
		((Control)Channel2_PPM).Cursor = Cursors.Hand;
		((Control)Channel2_PPM).Location = new Point(136, 13);
		Channel2_PPM.MeterSide = PPMMeter.Meter_Side.Right;
		((Control)Channel2_PPM).Name = "Channel2_PPM";
		Channel2_PPM.PeakHold = 0;
		Channel2_PPM.ShowValue = false;
		((Control)Channel2_PPM).Size = new Size(57, 191);
		((Control)Channel2_PPM).TabIndex = 51;
		Channel2_PPM.Value = 60;
		Channel1_PPM.BarColourUnlit = PPMMeter.BarColour_Unlit.White;
		((Control)Channel1_PPM).Cursor = Cursors.Hand;
		((Control)Channel1_PPM).Location = new Point(32, 13);
		Channel1_PPM.MeterSide = PPMMeter.Meter_Side.Right;
		((Control)Channel1_PPM).Name = "Channel1_PPM";
		Channel1_PPM.PeakHold = 0;
		Channel1_PPM.ShowValue = false;
		((Control)Channel1_PPM).Size = new Size(57, 191);
		((Control)Channel1_PPM).TabIndex = 50;
		Channel1_PPM.Value = 60;
		((Control)micline2).BackColor = Color.Transparent;
		micline2.Checked = false;
		micline2.CheckStyle = ChoseStyle.style1;
		((Control)micline2).Cursor = Cursors.Hand;
		((Control)micline2).Location = new Point(183, 237);
		((Control)micline2).Name = "micline2";
		((Control)micline2).Size = new Size(60, 24);
		((Control)micline2).TabIndex = 45;
		((Control)micline2).Click += micline2_Click;
		((Control)micline1).BackColor = Color.Transparent;
		micline1.Checked = false;
		micline1.CheckStyle = ChoseStyle.style1;
		((Control)micline1).Cursor = Cursors.Hand;
		((Control)micline1).Location = new Point(90, 237);
		((Control)micline1).Name = "micline1";
		((Control)micline1).Size = new Size(60, 24);
		((Control)micline1).TabIndex = 38;
		((Control)micline1).Click += micline1_Click;
		((Control)CheckLeft_Phantom).BackColor = Color.Transparent;
		CheckLeft_Phantom.Checked = false;
		CheckLeft_Phantom.CheckStyleX = CheckStyle.style3;
		((Control)CheckLeft_Phantom).Cursor = Cursors.Hand;
		((Control)CheckLeft_Phantom).Location = new Point(85, 289);
		((Control)CheckLeft_Phantom).Name = "CheckLeft_Phantom";
		((Control)CheckLeft_Phantom).Size = new Size(65, 24);
		((Control)CheckLeft_Phantom).TabIndex = 40;
		((Control)CheckLeft_Phantom).Click += CheckLeft_Phantom_Click;
		((Control)CheckLeft_Pad).BackColor = Color.Transparent;
		CheckLeft_Pad.Checked = false;
		CheckLeft_Pad.CheckStyleX = CheckStyle.style3;
		((Control)CheckLeft_Pad).Cursor = Cursors.Hand;
		((Control)CheckLeft_Pad).Location = new Point(85, 319);
		((Control)CheckLeft_Pad).Name = "CheckLeft_Pad";
		((Control)CheckLeft_Pad).Size = new Size(65, 24);
		((Control)CheckLeft_Pad).TabIndex = 41;
		((Control)CheckLeft_Pad).Click += CheckLeft_Pad_Click;
		((Control)CheckLeft_Mute).BackColor = Color.Transparent;
		CheckLeft_Mute.Checked = false;
		CheckLeft_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckLeft_Mute).Cursor = Cursors.Hand;
		((Control)CheckLeft_Mute).Location = new Point(85, 351);
		((Control)CheckLeft_Mute).Name = "CheckLeft_Mute";
		((Control)CheckLeft_Mute).Size = new Size(65, 24);
		((Control)CheckLeft_Mute).TabIndex = 42;
		((Control)CheckLeft_Mute).Click += CheckLeft_Mute_Click;
		((Control)CheckRight_Phase).BackColor = Color.Transparent;
		CheckRight_Phase.Checked = false;
		CheckRight_Phase.CheckStyleX = CheckStyle.style3;
		((Control)CheckRight_Phase).Cursor = Cursors.Hand;
		((Control)CheckRight_Phase).Location = new Point(178, 381);
		((Control)CheckRight_Phase).Name = "CheckRight_Phase";
		((Control)CheckRight_Phase).Size = new Size(65, 24);
		((Control)CheckRight_Phase).TabIndex = 43;
		((Control)CheckRight_Phase).Click += CheckRight_Phase_Click;
		((Control)CheckRight_Mute).BackColor = Color.Transparent;
		CheckRight_Mute.Checked = false;
		CheckRight_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckRight_Mute).Cursor = Cursors.Hand;
		((Control)CheckRight_Mute).Location = new Point(178, 351);
		((Control)CheckRight_Mute).Name = "CheckRight_Mute";
		((Control)CheckRight_Mute).Size = new Size(65, 24);
		((Control)CheckRight_Mute).TabIndex = 42;
		((Control)CheckRight_Mute).Click += CheckRight_Mute_Click;
		((Control)CheckRight_Pad).BackColor = Color.Transparent;
		CheckRight_Pad.Checked = false;
		CheckRight_Pad.CheckStyleX = CheckStyle.style3;
		((Control)CheckRight_Pad).Cursor = Cursors.Hand;
		((Control)CheckRight_Pad).Location = new Point(178, 319);
		((Control)CheckRight_Pad).Name = "CheckRight_Pad";
		((Control)CheckRight_Pad).Size = new Size(65, 24);
		((Control)CheckRight_Pad).TabIndex = 41;
		((Control)CheckRight_Pad).Click += CheckRight_Pad_Click;
		((Control)CheckRight_Phantom).BackColor = Color.Transparent;
		CheckRight_Phantom.Checked = false;
		CheckRight_Phantom.CheckStyleX = CheckStyle.style3;
		((Control)CheckRight_Phantom).Cursor = Cursors.Hand;
		((Control)CheckRight_Phantom).Location = new Point(178, 289);
		((Control)CheckRight_Phantom).Name = "CheckRight_Phantom";
		((Control)CheckRight_Phantom).Size = new Size(65, 24);
		((Control)CheckRight_Phantom).TabIndex = 40;
		((Control)CheckRight_Phantom).Click += CheckRight_Phantom_Click;
		((Control)CheckLeft_Phase).BackColor = Color.Transparent;
		CheckLeft_Phase.Checked = false;
		CheckLeft_Phase.CheckStyleX = CheckStyle.style3;
		((Control)CheckLeft_Phase).Cursor = Cursors.Hand;
		((Control)CheckLeft_Phase).Location = new Point(85, 381);
		((Control)CheckLeft_Phase).Name = "CheckLeft_Phase";
		((Control)CheckLeft_Phase).Size = new Size(65, 24);
		((Control)CheckLeft_Phase).TabIndex = 39;
		((Control)CheckLeft_Phase).Click += CheckLeft_Phase_Click;
		scrollBarEx1.BordBrush = Color.FromArgb(98, 98, 98);
		scrollBarEx1.FillEndColor = Color.White;
		scrollBarEx1.FillStartColor = Color.FromArgb(98, 98, 98);
		scrollBarEx1.FillThumbEndColor = Color.FromArgb(200, 255, 255, 255);
		scrollBarEx1.FillThumbStartColor = Color.FromArgb(220, 65, 182, 254);
		((Control)scrollBarEx1).Location = new Point(464, 66);
		scrollBarEx1.MaxNum = 10;
		scrollBarEx1.MinNum = 0;
		((Control)scrollBarEx1).Name = "scrollBarEx1";
		((Control)scrollBarEx1).Size = new Size(268, 26);
		((Control)scrollBarEx1).TabIndex = 22;
		((Control)scrollBarEx1).Text = "scrollBarEx1";
		scrollBarEx1.ThumbBordBrush = Color.FromArgb(60, 136, 202);
		scrollBarEx1.TickBrush = Color.FromArgb(230, 236, 236);
		scrollBarEx1.Value = 0;
		DLR_PPM.BarColourUnlit = PPMMeter.BarColour_Unlit.White;
		((Control)DLR_PPM).Cursor = Cursors.Hand;
		((Control)DLR_PPM).Location = new Point(129, 12);
		DLR_PPM.MeterSide = PPMMeter.Meter_Side.Right;
		((Control)DLR_PPM).Name = "DLR_PPM";
		DLR_PPM.PeakHold = 0;
		DLR_PPM.ShowValue = false;
		((Control)DLR_PPM).Size = new Size(57, 192);
		((Control)DLR_PPM).TabIndex = 52;
		DLR_PPM.Value = 60;
		DLL_PPM.BarColourUnlit = PPMMeter.BarColour_Unlit.White;
		((Control)DLL_PPM).Cursor = Cursors.Hand;
		((Control)DLL_PPM).Location = new Point(13, 12);
		DLL_PPM.MeterSide = PPMMeter.Meter_Side.Right;
		((Control)DLL_PPM).Name = "DLL_PPM";
		DLL_PPM.PeakHold = 0;
		DLL_PPM.ShowValue = false;
		((Control)DLL_PPM).Size = new Size(54, 191);
		((Control)DLL_PPM).TabIndex = 51;
		DLL_PPM.Value = 60;
		((Control)CheckDownRight_Phase).BackColor = Color.Transparent;
		CheckDownRight_Phase.Checked = false;
		CheckDownRight_Phase.CheckStyleX = CheckStyle.style3;
		((Control)CheckDownRight_Phase).Cursor = Cursors.Hand;
		((Control)CheckDownRight_Phase).Location = new Point(170, 381);
		((Control)CheckDownRight_Phase).Name = "CheckDownRight_Phase";
		((Control)CheckDownRight_Phase).Size = new Size(65, 24);
		((Control)CheckDownRight_Phase).TabIndex = 43;
		((Control)CheckDownRight_Phase).Click += CheckDownRight_Phase_Click;
		((Control)CheckDownRight_Mute).BackColor = Color.Transparent;
		CheckDownRight_Mute.Checked = false;
		CheckDownRight_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckDownRight_Mute).Cursor = Cursors.Hand;
		((Control)CheckDownRight_Mute).Location = new Point(170, 351);
		((Control)CheckDownRight_Mute).Name = "CheckDownRight_Mute";
		((Control)CheckDownRight_Mute).Size = new Size(65, 24);
		((Control)CheckDownRight_Mute).TabIndex = 42;
		((Control)CheckDownRight_Mute).Click += CheckDownRight_Mute_Click;
		((Control)CheckDownLeft_Phase).BackColor = Color.Transparent;
		CheckDownLeft_Phase.Checked = false;
		CheckDownLeft_Phase.CheckStyleX = CheckStyle.style3;
		((Control)CheckDownLeft_Phase).Cursor = Cursors.Hand;
		((Control)CheckDownLeft_Phase).Location = new Point(42, 381);
		((Control)CheckDownLeft_Phase).Name = "CheckDownLeft_Phase";
		((Control)CheckDownLeft_Phase).Size = new Size(65, 24);
		((Control)CheckDownLeft_Phase).TabIndex = 41;
		((Control)CheckDownLeft_Phase).Click += CheckDownLeft_Phase_Click;
		((Control)CheckDownLeft_Mute).BackColor = Color.Transparent;
		CheckDownLeft_Mute.Checked = false;
		CheckDownLeft_Mute.CheckStyleX = CheckStyle.style3;
		((Control)CheckDownLeft_Mute).Cursor = Cursors.Hand;
		((Control)CheckDownLeft_Mute).Location = new Point(42, 351);
		((Control)CheckDownLeft_Mute).Name = "CheckDownLeft_Mute";
		((Control)CheckDownLeft_Mute).Size = new Size(65, 24);
		((Control)CheckDownLeft_Mute).TabIndex = 40;
		((Control)CheckDownLeft_Mute).Click += CheckDownLeft_Mute_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((ContainerControl)this).AutoValidate = (AutoValidate)0;
		((Control)this).BackColor = Color.FromArgb(128, 255, 128);
		((Form)this).ClientSize = new Size(1430, 856);
		((Control)this).Controls.Add((Control)(object)panel5);
		((Control)this).Controls.Add((Control)(object)panel4);
		((Control)this).Controls.Add((Control)(object)panel3);
		((Control)this).Controls.Add((Control)(object)panel1);
		((Control)this).Controls.Add((Control)(object)panBottom);
		((Control)this).Controls.Add((Control)(object)panTop);
		((Control)this).Controls.Add((Control)(object)statusStrip);
		((Control)this).Controls.Add((Control)(object)toolStrip);
		((Control)this).Controls.Add((Control)(object)menuStrip);
		((Form)this).IsMdiContainer = true;
		((Form)this).MainMenuStrip = menuStrip;
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "Home";
		((Control)this).Text = "PreMo";
		((Control)menuStrip).ResumeLayout(false);
		((Control)menuStrip).PerformLayout();
		((Control)toolStrip).ResumeLayout(false);
		((Control)toolStrip).PerformLayout();
		((Control)statusStrip).ResumeLayout(false);
		((Control)statusStrip).PerformLayout();
		((Control)panTop).ResumeLayout(false);
		((Control)panTop).PerformLayout();
		((Control)panel2).ResumeLayout(false);
		((Control)panel2).PerformLayout();
		((ISupportInitialize)nUDDown2_BASS).EndInit();
		((ISupportInitialize)nUDDown2_Treble).EndInit();
		((ISupportInitialize)nUDDown2_HPF).EndInit();
		((ISupportInitialize)nUDDown1_BASS).EndInit();
		((ISupportInitialize)nUDDown1_Treble).EndInit();
		((ISupportInitialize)nUDDown1_HPF).EndInit();
		((ISupportInitialize)trbDownload2).EndInit();
		((ISupportInitialize)trbDownload1).EndInit();
		((Control)panBottom).ResumeLayout(false);
		((ISupportInitialize)nUDFileNum).EndInit();
		((Control)panel3).ResumeLayout(false);
		((Control)panel3).PerformLayout();
		((ISupportInitialize)trbBright).EndInit();
		((ISupportInitialize)trbVolume2).EndInit();
		((ISupportInitialize)trbVolume1).EndInit();
		((Control)panel4).ResumeLayout(false);
		((Control)panel4).PerformLayout();
		((ISupportInitialize)picUpload).EndInit();
		((ISupportInitialize)picHeadset).EndInit();
		((ISupportInitialize)trbUpGM).EndInit();
		((ISupportInitialize)trbHeadGM).EndInit();
		((ISupportInitialize)numUpload).EndInit();
		((ISupportInitialize)numHeadset).EndInit();
		((ISupportInitialize)errorProvider1).EndInit();
		((Control)panel5).ResumeLayout(false);
		((ISupportInitialize)trbInput1).EndInit();
		((ISupportInitialize)trbInput2).EndInit();
		((ISupportInitialize)nUD1_HPF).EndInit();
		((ISupportInitialize)nUD1_Treble).EndInit();
		((ISupportInitialize)nUD1_BASS).EndInit();
		((ISupportInitialize)nUD2_HPF).EndInit();
		((ISupportInitialize)nUD2_Treble).EndInit();
		((ISupportInitialize)nUD2_BASS).EndInit();
		((Control)panel1).ResumeLayout(false);
		((Control)panel1).PerformLayout();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
