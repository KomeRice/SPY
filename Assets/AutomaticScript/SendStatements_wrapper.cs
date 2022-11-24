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

}
