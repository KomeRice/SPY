using UnityEngine;
using FYFY;

public class EditorEscMenu_wrapper : BaseWrapper
{
	public UnityEngine.GameObject escCanvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "escCanvas", escCanvas);
	}

	public void toggleMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleMenu", null);
	}

	public void closeEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeEditor", null);
	}

}
