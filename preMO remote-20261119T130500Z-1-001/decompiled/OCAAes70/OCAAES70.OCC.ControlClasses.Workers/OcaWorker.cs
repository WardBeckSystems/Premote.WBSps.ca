using System.Collections.Generic;
using Model.AES70;

namespace OCAAES70.OCC.ControlClasses.Workers;

public abstract class OcaWorker : OcaRoot
{
	public enum MethodIndex
	{
		GET_ENABLED = 1,
		SET_ENABLED,
		ADD_PORT,
		DELETE_PORT,
		GET_PORTS,
		GET_PORT_NAME,
		SET_PORT_NAME,
		GET_LABEL,
		SET_LABEL,
		GET_OWNER,
		GET_LATENCY,
		SET_LATENCY,
		GET_PATH
	}

	public enum PropertyIndex
	{
		OCA_PROP_ENABLED = 1,
		OCA_PROP_PORTS,
		OCA_PROP_LABEL,
		OCA_PROP_OWNER,
		OCA_PROP_LATENCY
	}

	private const string worker_lineage_key = "1.1";

	private const ushort worker_class_version = 1;

	public const ushort worker_tree_level = 2;

	public string ClassId { get; }

	public new ushort ClassVersion { get; set; }

	public bool Enabled { get; set; }

	public List<string> Ports { get; set; }

	public string Label { get; set; }

	public ushort Owner { get; set; }

	public uint Latency { get; set; }

	public bool GetEnabled(OcaWorkerInfo info)
	{
		return true;
	}

	public bool SetEnabled(OcaWorkerInfo info)
	{
		return true;
	}

	public bool GetPorts(OcaWorkerInfo info)
	{
		return true;
	}

	public abstract bool SetLabel();

	public abstract string GetLabel();
}
