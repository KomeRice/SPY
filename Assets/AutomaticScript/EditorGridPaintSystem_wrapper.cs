using UnityEngine;
using FYFY;

public class EditorGridPaintSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void setBrush(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBrush", go);
	}

}
