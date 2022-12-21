using UnityEngine;
using FYFY;

public class TilePopupSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject orientationPopup;
	public UnityEngine.GameObject slotPopup;
	public UnityEngine.GameObject scriptNamePopup;
	public UnityEngine.GameObject furniturePopup;
	public UnityEngine.GameObject scriptMenu;
	public UnityEngine.GameObject rangePopup;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "orientationPopup", orientationPopup);
		MainLoop.initAppropriateSystemField (system, "slotPopup", slotPopup);
		MainLoop.initAppropriateSystemField (system, "scriptNamePopup", scriptNamePopup);
		MainLoop.initAppropriateSystemField (system, "furniturePopup", furniturePopup);
		MainLoop.initAppropriateSystemField (system, "scriptMenu", scriptMenu);
		MainLoop.initAppropriateSystemField (system, "rangePopup", rangePopup);
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
