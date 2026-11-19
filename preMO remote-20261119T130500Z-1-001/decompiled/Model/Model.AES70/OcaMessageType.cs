namespace Model.AES70;

public enum OcaMessageType : byte
{
	OcaCmd,
	OcaCmdRrq,
	OcaNtf,
	OcaRsp,
	OcaKeepAlive
}
