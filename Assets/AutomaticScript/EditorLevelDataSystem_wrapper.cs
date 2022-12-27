using UnityEngine;
using FYFY;

public class EditorLevelDataSystem_wrapper : BaseWrapper
{
	public UnityEngine.Sprite backgroundAction;
	public UnityEngine.Sprite backgroundControl;
	public UnityEngine.Sprite backgroundOperator;
	public UnityEngine.Sprite backgroundSensor;
	public UnityEngine.Color actionColor;
	public UnityEngine.Color controlColor;
	public UnityEngine.Color operatorColor;
	public UnityEngine.Color sensorColor;
	public UnityEngine.GameObject scrollViewContent;
	public UnityEngine.GameObject executionLimitContainer;
	public UnityEngine.UI.Toggle fogToggle;
	public UnityEngine.UI.Toggle dragAndDropToggle;
	public UnityEngine.GameObject scoreContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "backgroundAction", backgroundAction);
		MainLoop.initAppropriateSystemField (system, "backgroundControl", backgroundControl);
		MainLoop.initAppropriateSystemField (system, "backgroundOperator", backgroundOperator);
		MainLoop.initAppropriateSystemField (system, "backgroundSensor", backgroundSensor);
		MainLoop.initAppropriateSystemField (system, "actionColor", actionColor);
		MainLoop.initAppropriateSystemField (system, "controlColor", controlColor);
		MainLoop.initAppropriateSystemField (system, "operatorColor", operatorColor);
		MainLoop.initAppropriateSystemField (system, "sensorColor", sensorColor);
		MainLoop.initAppropriateSystemField (system, "scrollViewContent", scrollViewContent);
		MainLoop.initAppropriateSystemField (system, "executionLimitContainer", executionLimitContainer);
		MainLoop.initAppropriateSystemField (system, "fogToggle", fogToggle);
		MainLoop.initAppropriateSystemField (system, "dragAndDropToggle", dragAndDropToggle);
		MainLoop.initAppropriateSystemField (system, "scoreContainer", scoreContainer);
	}

	public void hideToggleChanged(System.Int32 index)
	{
		MainLoop.callAppropriateSystemMethod (system, "hideToggleChanged", index);
	}

	public void limitToggleChanged(System.Int32 index)
	{
		MainLoop.callAppropriateSystemMethod (system, "limitToggleChanged", index);
	}

	public void preventMinusSign(System.Int32 index)
	{
		MainLoop.callAppropriateSystemMethod (system, "preventMinusSign", index);
	}

	public void preventMinusSign(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "preventMinusSign", go);
	}

	public void executionLimitChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "executionLimitChanged", null);
	}

	public void scoreToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "scoreToggleChanged", null);
	}

	public void scoreTwoStarsExit()
	{
		MainLoop.callAppropriateSystemMethod (system, "scoreTwoStarsExit", null);
	}

	public void scoreThreeStarsExit()
	{
		MainLoop.callAppropriateSystemMethod (system, "scoreThreeStarsExit", null);
	}

	public void fogToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "fogToggleChanged", null);
	}

	public void dragDropChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "dragDropChanged", null);
	}

}
