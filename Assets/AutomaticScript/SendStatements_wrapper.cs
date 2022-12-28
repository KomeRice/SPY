using UnityEngine;
using FYFY;

public class SendStatements_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void initGBLXAPI()
	{
		MainLoop.callAppropriateSystemMethod (system, "initGBLXAPI", null);
	}

	public void templateStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "templateStatement", null);
	}

	public void LevelCompleteStatement(System.String levelNumber)
	{
		MainLoop.callAppropriateSystemMethod (system, "LevelCompleteStatement", levelNumber);
	}

	public void CollectedObjetStatement(System.String ObjectName)
	{
		MainLoop.callAppropriateSystemMethod (system, "CollectedObjetStatement", ObjectName);
	}

	public void ActivatedDoorStatement(System.String ObjectName)
	{
		MainLoop.callAppropriateSystemMethod (system, "ActivatedDoorStatement", ObjectName);
	}

	public void HackedRobotStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "HackedRobotStatement", null);
	}

	public void ReachedTPStatement(System.String time)
	{
		MainLoop.callAppropriateSystemMethod (system, "ReachedTPStatement", time);
	}

}
