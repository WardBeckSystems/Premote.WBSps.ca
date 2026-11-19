namespace OCAAES70.OCC.ControlClasses.Workers.Actuators;

public abstract class OcaActuators : OcaWorker
{
	private const string actuator_lineage_key = "1.1.1";

	private const ushort actuator_class_version = 1;

	public const ushort actuator_tree_level = 3;
}
