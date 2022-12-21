using UnityEngine;
using FYFY;

public class SaveFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.InputField executionLimitField;
	public UnityEngine.GameObject editableContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "executionLimitField", executionLimitField);
		MainLoop.initAppropriateSystemField (system, "editableContainer", editableContainer);
	}

	public void saveXmlFile()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveXmlFile", null);
	}

}
