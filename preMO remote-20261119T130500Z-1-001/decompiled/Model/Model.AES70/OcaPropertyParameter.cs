using System;

namespace Model.AES70;

public class OcaPropertyParameter : EventArgs
{
	public uint block { get; set; }

	public uint label { get; set; }

	public byte[] parametervalue { get; set; }

	public OcaPropertyParameter(uint block, uint label, byte[] parametervalue)
	{
		this.block = block;
		this.label = label;
		this.parametervalue = parametervalue;
	}
}
