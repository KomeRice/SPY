using UnityEngine;
using FYFY;

public class TilePopupSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject orientationPopup;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "orientationPopup", orientationPopup);
	}

	public void rotateSelectionUp()
	{
		MainLoop.callAppropriateSystemMethod (system, "rotateSelectionUp", null);
	}

	public void rotateSelectionRight()
	{
		MainLoop.callAppropriateSystemMethod (system, "rotateSelectionRight", null);
	}

	public void rotateSelectionLeft()
	{
		MainLoop.callAppropriateSystemMethod (system, "rotateSelectionLeft", null);
	}

	public void rotateSelectionDown()
	{
		MainLoop.callAppropriateSystemMethod (system, "rotateSelectionDown", null);
	}

}
