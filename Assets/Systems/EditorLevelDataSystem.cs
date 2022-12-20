using System;
using System.Linq;
using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class EditorLevelDataSystem : FSystem {

	public static EditorLevelDataSystem instance;

	public Sprite backgroundAction;
	public Sprite backgroundControl;
	public Sprite backgroundOperator;
	public Sprite backgroundSensor;

	public Color actionColor;
	public Color controlColor;
	public Color operatorColor;
	public Color sensorColor;

	public Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	public GameObject scrollViewContent;
	public GameObject executionLimitContainer;
	public Toggle fogToggle;
	public Toggle dragAndDropToggle;
	public GameObject scoreContainer;

	public EditorLevelDataSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart(){
		foreach (var editorBlock in f_editorblocks)
		{
			var blockData = editorBlock.GetComponent<EditorBlockData>();
			var bgImage = editorBlock.transform.GetChild(0).GetComponent<Image>();
			var spriteImage = editorBlock.transform.GetChild(1).GetComponent<Image>();
			switch (blockData.blockCategory)
			{
				case BlockCategory.Action:
					bgImage.sprite = backgroundAction;
					bgImage.color = actionColor;
					break;
				case BlockCategory.Control:
					bgImage.sprite = backgroundControl;
					bgImage.color = controlColor;
					break;
				case BlockCategory.Operator:
					bgImage.sprite = backgroundOperator;
					bgImage.color = operatorColor;
					break;
				case BlockCategory.Sensor:
					bgImage.sprite = backgroundSensor;
					bgImage.color = sensorColor;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			spriteImage.sprite = blockData.blockIcon;
			editorBlock.transform.GetChild(5).GetComponent<Text>().text = blockData.blockName;
		}
	}

	// Use to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}

	public void hideToggleChanged(int index)
	{
		var newStatus = f_editorblocks.getAt(index).transform.GetChild(4).GetComponent<Toggle>().isOn;
		f_editorblocks.getAt(index).transform.GetChild(3).GetComponent<Toggle>().interactable = !newStatus;
		f_editorblocks.getAt(index).transform.GetChild(2).GetComponent<InputField>().interactable = 
			!f_editorblocks.getAt(index).transform.GetChild(3).GetComponent<Toggle>().isOn && !newStatus;
	}

	public void limitToggleChanged(int index)
	{
		var newStatus = f_editorblocks.getAt(index).transform.GetChild(3).GetComponent<Toggle>().isOn;
		f_editorblocks.getAt(index).transform.GetChild(2).GetComponent<InputField>().interactable = !newStatus;
	}

	public void preventMinusSign(int index)
	{
		var text = f_editorblocks.getAt(index).transform.GetChild(2).GetComponent<InputField>().text;
		if (text.StartsWith("-"))
			f_editorblocks.getAt(index).transform.GetChild(2).GetComponent<InputField>().text = text.Trim('-');
	}
	
	public void preventMinusSign(GameObject go)
	{
		var text = go.GetComponent<InputField>().text;
		if (text.StartsWith("-"))
			go.GetComponent<InputField>().text = text.Trim('-');
	}

	public void executionLimitChanged()
	{
		executionLimitContainer.GetComponentInChildren<InputField>().interactable =
			executionLimitContainer.GetComponentInChildren<Toggle>().isOn;
	}
	
	public void scoreToggleChanged()
	{
		var newStatus = scoreContainer.GetComponentInChildren<Toggle>().isOn;
		scoreContainer.transform.GetChild(1).GetComponent<InputField>().interactable = newStatus;
		scoreContainer.transform.GetChild(2).GetComponent<InputField>().interactable = newStatus;
	}
}

public enum BlockCategory
{
	Action = 0,
	Control = 1,
	Operator = 2,
	Sensor = 3
}