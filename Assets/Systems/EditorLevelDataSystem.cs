using System;
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
	public Family f_levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData)));
	
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

			getData().dragdropDisabled = false;
			getData().fogEnabled = false;
			getData().executionLimitEnabled = false;
			getData().scoreEnabled = false;
			getData().scoreThreeStars = 1;
			getData().scoreTwoStars = 0;
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
	protected override void onProcess(int familiesUpdateCount)
	{
		if (!getData().requireRefresh) 
			return;
		
		executionLimitContainer.GetComponentInChildren<Toggle>().isOn = getData().executionLimitEnabled;
		if(getData().executionLimitEnabled)
			executionLimitContainer.GetComponentInChildren<InputField>().text = getData().executionLimit.ToString();
		fogToggle.GetComponent<Toggle>().isOn = getData().fogEnabled;
		dragAndDropToggle.GetComponent<Toggle>().isOn = !getData().dragdropDisabled;
		scoreContainer.GetComponentInChildren<Toggle>().isOn = getData().scoreEnabled;
		if (!getData().scoreEnabled) return;
		scoreContainer.transform.GetChild(1).GetComponent<InputField>().text = getData().scoreTwoStars.ToString(); 
		scoreContainer.transform.GetChild(2).GetComponent<InputField>().text = getData().scoreThreeStars.ToString();
		getData().requireRefresh = false;
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
		var newStatus = executionLimitContainer.GetComponentInChildren<Toggle>().isOn;
		executionLimitContainer.GetComponentInChildren<InputField>().interactable = newStatus;
		getData().executionLimitEnabled = newStatus;
	}
	
	public void scoreToggleChanged()
	{
		var newStatus = scoreContainer.GetComponentInChildren<Toggle>().isOn;
		scoreContainer.transform.GetChild(1).GetComponent<InputField>().interactable = newStatus;
		scoreContainer.transform.GetChild(2).GetComponent<InputField>().interactable = newStatus;
		getData().scoreEnabled = newStatus;
	}

	public void scoreTwoStarsExit()
	{
		var twoStarsText = scoreContainer.transform.GetChild(1).GetComponent<InputField>().text;

		if (string.IsNullOrEmpty(twoStarsText))
		{
			scoreContainer.transform.GetChild(1).GetComponent<InputField>().text = "0";
			getData().scoreTwoStars = 0;
			return;
		}

		var twoStarsScore = int.Parse(twoStarsText);
		var threeStarsScore = getData().scoreThreeStars;
		if (twoStarsScore < threeStarsScore)
		{
			getData().scoreTwoStars = twoStarsScore;
			return;
		}

		scoreContainer.transform.GetChild(1).GetComponent<InputField>().text = (threeStarsScore - 1).ToString();
		getData().scoreTwoStars = threeStarsScore - 1;
	}

	public void scoreThreeStarsExit()
	{
		var threeStarsText = scoreContainer.transform.GetChild(2).GetComponent<InputField>().text;
		var twoStarsScore = getData().scoreTwoStars;

		if (string.IsNullOrEmpty(threeStarsText))
		{
			scoreContainer.transform.GetChild(2).GetComponent<InputField>().text = (twoStarsScore + 1).ToString();
			getData().scoreThreeStars = twoStarsScore + 1;
			return;
		}

		var threeStarsScore = int.Parse(threeStarsText);
		if (twoStarsScore < threeStarsScore)
		{
			getData().scoreThreeStars = threeStarsScore;
			return;
		}

		scoreContainer.transform.GetChild(2).GetComponent<InputField>().text = (twoStarsScore + 1).ToString();
		getData().scoreThreeStars = twoStarsScore + 1;
	}	
	
	public void fogToggleChanged()
	{
		var newStatus = fogToggle.GetComponent<Toggle>().isOn;
		getData().fogEnabled = newStatus;
	}
	
	public void dragDropChanged()
	{
		var newStatus = dragAndDropToggle.GetComponent<Toggle>().isOn;
		getData().dragdropDisabled = !newStatus;
	}

	private LevelData getData()
	{
		return f_levelData.First().GetComponent<LevelData>();
	}
}

public enum BlockCategory
{
	Action = 0,
	Control = 1,
	Operator = 2,
	Sensor = 3
}