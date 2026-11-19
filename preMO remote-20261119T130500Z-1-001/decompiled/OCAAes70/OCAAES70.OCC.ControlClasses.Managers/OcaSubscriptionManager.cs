using System;
using Model.AES70;

namespace OCAAES70.OCC.ControlClasses.Managers;

public class OcaSubscriptionManager : OcaManager
{
	public enum MethodIndex
	{
		ADD_SUBSCRIPTION = 1,
		REMOVE_SUBSCRIPTION,
		DISABLE_NOTIFICATIONS,
		RE_ENABLE_NOTIFICATIONS,
		ADD_PROPERTY_CHANGE_SUBSCRIPTION,
		REMOVE_PROPERTY_CHANGE_SUBSCRIPTION,
		GET_MAXIMUM_SUBSCRIBER_CONTEXT_LENGTH
	}

	public enum PropertyIndex
	{
		OCA_PROP_STATE = 1
	}

	public enum EventIndex
	{
		OCA_EVENT_EVENTS_DISABLED = 1,
		OCA_EVENT_SYNCHRONIZE_STATE
	}

	public const string OCA_SUBSCRIPTION_MANAGER_CLASSID = "1.3.4";

	private const ushort OCA_SUBSCRIPTION_MANAGER_class_version = 1;

	public const ushort OCA_SUBSCRIPTION_MANAGER_tree_level = 3;

	public const uint OCA_SUBSCRIPTION_MANAGER_Ono = 4u;

	public event EventHandler<OcaPropertyParameter> NewNotification;

	public override string GetRole()
	{
		throw new NotImplementedException();
	}

	public override OcaRootInfo ClassIdentification(ushort version, string key)
	{
		throw new NotImplementedException();
	}

	public void OnNewNitification(OcaPropertyParameter e)
	{
		if (e != null)
		{
			NewNotification?.Invoke(this, new OcaPropertyParameter(e.block, e.label, e.parametervalue));
		}
	}
}
