using Model.AES70;

namespace OCAAES70.OCC.ControlClasses.Workers.Actuators;

public class OcaGain : OcaActuators
{
	private const string gain_lineage_key = "1.1.1.5";

	private const ushort gain_class_version = 1;

	public const ushort gain_tree_level = 4;

	public override bool SetLabel()
	{
		OcaGain ocaGain = new OcaGain();
		string label = ocaGain.Label;
		if (label != null)
		{
			return true;
		}
		return false;
	}

	public override string GetLabel()
	{
		OcaGain ocaGain = new OcaGain();
		return ocaGain.Label;
	}

	public override string GetRole()
	{
		OcaGain ocaGain = new OcaGain();
		return ocaGain.Role;
	}

	public override OcaRootInfo ClassIdentification(ushort version, string key)
	{
		new OcaRootInfo().ClassID = key;
		new OcaRootInfo().ClassVersion = version;
		return new OcaRootInfo();
	}

	public byte GetGain(byte gainvalue)
	{
		return gainvalue;
	}

	public virtual bool SetGain(byte gainvalue)
	{
		byte b = gainvalue;
		bool flag = true;
		return true;
	}
}
