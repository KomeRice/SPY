using UnityEngine;
using FYFY;

public class CoinManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void CollectedObjetStatement(System.String ObjectName)
	{
		MainLoop.callAppropriateSystemMethod (system, "CollectedObjetStatement", ObjectName);
	}

}
