using System;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bonjour;
using Model;

namespace PreMoBLL;

public class BonjourBLL
{
	private delegate void ReadMessageCallback(string data);

	private DNSSDEventManager m_eventManager = null;

	private DNSSDService m_service = null;

	private DNSSDService m_registrar = null;

	private DNSSDService m_browser = null;

	private DNSSDService m_resolver = null;

	private string m_name;

	private Socket m_socket = null;

	private const int BUFFER_SIZE = 1024;

	public byte[] m_buffer = new byte[1024];

	public bool m_complete = false;

	public StringBuilder m_sb = new StringBuilder();

	private ReadMessageCallback m_readMessageCallback;

	private string localIP;

	private string IPsubnet;

	public bool Bonjour_Init()
	{
		try
		{
			m_service = (DNSSDService)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("24CD4DE9-FF84-4701-9DC1-9B69E0D1090A")));
		}
		catch
		{
			return false;
		}
		m_eventManager = (DNSSDEventManager)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("BEEB932A-8D4A-4619-AEFE-A836F988B221")));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceRegistered").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceRegisteredEventHandler(ServiceRegistered));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceFound").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceFoundEventHandler(ServiceFound));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceLost").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceLostEventHandler(ServiceLost));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "ServiceResolved").AddEventHandler(m_eventManager, new _IDNSSDEvents_ServiceResolvedEventHandler(ServiceResolved));
		new ComAwareEventInfo(typeof(_IDNSSDEvents_Event), "QueryRecordAnswered").AddEventHandler(m_eventManager, new _IDNSSDEvents_QueryRecordAnsweredEventHandler(QueryAnswered));
		m_readMessageCallback = OnReadMessage;
		SimpleChat();
		return true;
	}

	private void SimpleChat()
	{
		IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		m_socket.Bind(localEP);
		localEP = (IPEndPoint)m_socket.LocalEndPoint;
		m_socket.BeginReceive(m_buffer, 0, 1024, SocketFlags.None, OnReadSocket, this);
		try
		{
			m_registrar = m_service.Register((DNSSDFlags)0, 0u, Environment.UserName, "_http._tcp", null, null, (ushort)localEP.Port, null, m_eventManager);
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
			m_browser = m_service.Browse((DNSSDFlags)0, 0u, "_http._tcp", null, m_eventManager);
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

	public async Task<PeerData> Get_PeerData()
	{
		PeerData peer = new PeerData();
		return await Task.Run(() => (PeerData.dictBonjour.Count != 0) ? peer : null);
	}

	public void ServiceLost(DNSSDService sref, DNSSDFlags flags, uint ifIndex, string serviceName, string regType, string domain)
	{
		PeerData peerData = new PeerData();
		peerData.InterfaceIndex = ifIndex;
		peerData.Name = serviceName;
		peerData.Type = regType;
		peerData.Domain = domain;
		peerData.Address = null;
	}

	public void ServiceResolved(DNSSDService sref, DNSSDFlags flags, uint ifIndex, string fullName, string hostName, ushort port, TXTRecord txtRecord)
	{
		m_resolver.Stop();
		m_resolver = null;
		PeerData peerData = new PeerData();
		peerData.Port = port;
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
		m_resolver.Stop();
		m_resolver = null;
		PeerData peerData = new PeerData();
		uint num = BitConverter.ToUInt32((byte[])rdata, 0);
		IPAddress address = new IPAddress(num);
		peerData.Address = address;
	}

	private void OnReadMessage(string msg)
	{
		int num = 0;
		for (int i = 0; i < msg.Length && msg[i] != ':'; i++)
		{
			num ^= (int)((uint)msg[i] << (i % 3 + 2) * 8);
		}
	}

	private void OnReadSocket(IAsyncResult ar)
	{
		try
		{
			m_socket.BeginReceive(m_buffer, 0, 1024, SocketFlags.None, OnReadSocket, this);
		}
		catch
		{
		}
	}

	private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
	{
		PeerData peerData = new PeerData();
		m_resolver = m_service.Resolve((DNSSDFlags)0, peerData.InterfaceIndex, peerData.Name, peerData.Type, peerData.Domain, m_eventManager);
	}

	public int[] GetNetNum(ManagementObjectCollection nics)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		int[] array = new int[4];
		ManagementObjectCollection.ManagementObjectEnumerator enumerator = nics.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				if (Convert.ToBoolean(((ManagementBaseObject)val)["ipEnabled"]))
				{
					localIP = (((ManagementBaseObject)val)["IPAddress"] as string[])[0];
					IPsubnet = (((ManagementBaseObject)val)["IPSubnet"] as string[])[0];
				}
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		int[] array2 = new int[4];
		int[] array3 = new int[4];
		string[] array4 = localIP.Split(new char[1] { '.' });
		string[] array5 = IPsubnet.Split(new char[1] { '.' });
		for (int i = 0; i < 4; i++)
		{
			array[i] = int.Parse(array4[i]) & int.Parse(array5[i]);
		}
		return array;
	}

	public bool FailSubNet(string IPsubnet, string devIP, int[] NetNum)
	{
		string[] array = IPsubnet.Split(new char[1] { '.' });
		string[] array2 = devIP.Split(new char[1] { '.' });
		for (int i = 0; i < array.Length; i++)
		{
			if ((int.Parse(array2[i]) & int.Parse(array[i])) != NetNum[i])
			{
				return false;
			}
		}
		return true;
	}
}
