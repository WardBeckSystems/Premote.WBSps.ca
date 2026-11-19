using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Model.JSON;
using Newtonsoft.Json;

namespace PreMoBLL;

public class JSONBLL
{
	public List<Devices> Device_List(string str)
	{
		string address = "http://" + str + ".local./device_list";
		string value = "";
		try
		{
			WebClient webClient = new WebClient();
			webClient.Credentials = CredentialCache.DefaultCredentials;
			byte[] buffer = webClient.DownloadData(address);
			MemoryStream stream = new MemoryStream(buffer);
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			value = streamReader.ReadLine();
		}
		catch
		{
		}
		return JsonConvert.DeserializeObject<RootObject>(value)?.ArchwaveAES67.devices;
	}
}
