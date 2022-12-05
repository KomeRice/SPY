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

	public void LoadLevelStatement(System.String levelNumber)
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadLevelStatement", levelNumber);
	}

	public void LevelCompleteStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "LevelCompleteStatement", null);
	}

	public void ActionDraggredStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "ActionDraggredStatement", null);
	}

	public void CharacterMovedStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "CharacterMovedStatement", null);
	}

	public void CollectedObjetStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "CollectedObjetStatement", null);
	}

	public void ActivatedDoorStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "ActivatedDoorStatement", null);
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
