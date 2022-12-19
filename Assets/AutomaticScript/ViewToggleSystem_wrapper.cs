using UnityEngine;
using FYFY;

public class ViewToggleSystem_wrapper : BaseWrapper
{
	public UnityEngine.Canvas mainCanvas;
	public UnityEngine.Canvas metadataCanvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "metadataCanvas", metadataCanvas);
	}

	public void toggleCanvas()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleCanvas", null);
	}

}
