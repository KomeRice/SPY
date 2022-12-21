using UnityEngine;
using FYFY;

public class SaveFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.InputField executionLimitField;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "executionLimitField", executionLimitField);
	}

	public void saveXmlFile()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveXmlFile", null);
	}

}
