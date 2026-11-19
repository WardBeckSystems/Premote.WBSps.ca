using System;
using Model.AES70;

namespace OCAAES70.OCC.ControlClasses;

public abstract class OcaRoot
{
	private enum PropertyIndex
	{
		OCA_PROP_CLASS_ID = 1,
		OCA_PROP_CLASS_VERSION,
		OCA_PROP_OBJECT_NUMBER,
		OCA_PROP_LOCKABLE,
		OCA_PROP_ROLE
	}

	private enum MethodIndex
	{
		GET_CLASS_IDENTIFICATION = 1,
		GET_LOCKABLE,
		LOCK,
		UNLOCK,
		GET_ROLE,
		LOCK_READONLY
	}

	private enum EventIndex
	{
		OCA_EVENT_PROPERTY_CHANGED = 1
	}

	private const string root_lineage_key = "1";

	private const ushort root_version = 1;

	public const ushort root_tree_level = 1;

	private const uint OCA_MINIMUM_DEVICE_OBJECT_ONO = 4096u;

	public bool Lockable { get; set; }

	public string Role { get; set; }

	public ushort ClassVersion { get; set; }

	public string ClassID { get; set; }

	public uint ONo { get; set; }

	public event EventHandler<EventArgs> PropertyChanged;

	public bool GetLockable(OcaRootInfo info)
	{
		return true;
	}

	public bool Lock(OcaRootInfo info)
	{
		return true;
	}

	public bool UnLock(OcaRootInfo info)
	{
		return true;
	}

	public abstract string GetRole();

	public abstract OcaRootInfo ClassIdentification(ushort version, string key);
}
