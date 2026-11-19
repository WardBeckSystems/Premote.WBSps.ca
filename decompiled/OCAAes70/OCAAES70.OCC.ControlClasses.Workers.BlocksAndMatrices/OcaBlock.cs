using Model.AES70;

namespace OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;

public class OcaBlock : OcaWorker
{
	private const string block_lineage_key = "1.1.3";

	private const ushort worker_class_version = 1;

	public new const ushort worker_tree_level = 3;

	public override string GetRole()
	{
		OcaBlock ocaBlock = new OcaBlock();
		return ocaBlock.Role;
	}

	public override bool SetLabel()
	{
		OcaBlock ocaBlock = new OcaBlock();
		string label = ocaBlock.Label;
		if (label != null)
		{
			return true;
		}
		return false;
	}

	public override string GetLabel()
	{
		OcaBlock ocaBlock = new OcaBlock();
		return ocaBlock.Label;
	}

	public override OcaRootInfo ClassIdentification(ushort version, string key)
	{
		new OcaRootInfo().ClassID = key;
		new OcaRootInfo().ClassVersion = version;
		return new OcaRootInfo();
	}

	public uint GetOcaBlockOno()
	{
		OcaBlock ocaBlock = new OcaBlock();
		return ocaBlock.ONo;
	}

	public uint GetOcaBlockMemberOno()
	{
		OcaBlock ocaBlock = new OcaBlock();
		return ocaBlock.ONo;
	}
}
