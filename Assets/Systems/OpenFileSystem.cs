using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FYFY;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

public class OpenFileSystem : FSystem {

	public static OpenFileSystem instance;

	public GameObject screen;
	public InputField newCampaignField;
	public Dropdown campaignDropdown;
	public Toggle newCampaignToggle;
	public InputField newLevelField;
	public Dropdown levelDropdown;
	public Toggle newLevelToggle;
	public Text warningMessage;
	public GameObject height;
	public GameObject width;
	public Button confirmButton;

	private readonly string _baseLevelsPath = Application.streamingAssetsPath + "/Scenario/";
	private readonly string _customLevelsPath = Application.persistentDataPath + "/Scenario/";
	private SortedDictionary<string, List<Level>> campaignDict;
	private string curCampaign;

	public OpenFileSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		if (!Directory.Exists(_customLevelsPath))
			Directory.CreateDirectory(_customLevelsPath);

		campaignDict = getLevelDict();
		if (campaignDict.Keys.Count == 0)
		{
			newCampaignToggle.isOn = true;
			newCampaignToggle.interactable = false;
			campaignDropdownChanged();
			return;
		}
		newCampaignToggleChanged();
		newLevelToggleChanged();
		
		curCampaign = campaignDict.Keys.First();
		warningMessage.gameObject.SetActive(false);
		campaignDropdown.AddOptions(campaignDict.Keys.ToList());
		refreshLevelDropdown();
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

	private SortedDictionary<string, List<Level>> getLevelDict()
	{
		var levelDict = new SortedDictionary<string, List<Level>>();
		foreach (var campaignPath in Directory.EnumerateFiles(_baseLevelsPath, "*.xml"))
		{
			var levels = new List<Level>();
			var xmlFile = XDocument.Load(campaignPath).Descendants("scenario");
			foreach (var level in xmlFile.Descendants())
			{
				levels.Add(new Level
				{
					Name = Path.GetFileNameWithoutExtension(level.Attribute("name")!.Value),
					Path = Application.streamingAssetsPath + "/" + level.Attribute("name")!.Value
				});
			}
			
			levelDict[Path.GetFileNameWithoutExtension(campaignPath)] = levels;
		}
		
		foreach (var campaignPath in Directory.EnumerateFiles(_customLevelsPath, "*.xml"))
		{
			if(levelDict.Keys.Contains(Path.GetFileNameWithoutExtension(campaignPath)))
				continue;
			var levels = new List<Level>();
			var xmlFile = XDocument.Load(campaignPath).Descendants("scenario");
			foreach (var level in xmlFile.Descendants())
			{
				levels.Add(new Level
				{
					Name = Path.GetFileNameWithoutExtension(level.Attribute("name")!.Value),
					Path = level.Attribute("name")!.Value
				});
			}
			
			levelDict[Path.GetFileNameWithoutExtension(campaignPath)] = levels;
		}
		
		return levelDict;
	}

	public void campaignDropdownChanged()
	{
		newLevelToggle.isOn = false;
		refreshLevelDropdown();
		newLevelToggleChanged();
	}

	public void newLevelToggleChanged()
	{
		newLevelField.gameObject.SetActive(newLevelToggle.isOn);
		levelDropdown.gameObject.SetActive(!newLevelToggle.isOn);
		width.SetActive(newLevelToggle.isOn);
		width.GetComponentInChildren<InputField>().text = "5";
		height.SetActive(newLevelToggle.isOn);
		height.GetComponentInChildren<InputField>().text = "5";
		confirmButton.interactable = !newLevelToggle.isOn;
		validateName();
	}

	public void guaranteeOne(GameObject go)
	{
		var text = go.GetComponent<InputField>();
		if (string.IsNullOrEmpty(text.text) || text.text == "0")
			text.text = "1";
		if (int.Parse(text.text) > 10)
			text.text = "10";
	}

	public void newCampaignToggleChanged()
	{
		newCampaignField.gameObject.SetActive(newCampaignToggle.isOn);
		campaignDropdown.gameObject.SetActive(!newCampaignToggle.isOn);
		if (newCampaignToggle.isOn)
		{
			newLevelToggle.isOn = true;
			newLevelToggle.interactable = false;
			confirmButton.interactable = false;
			validateName();
		}
		else
		{
			newLevelToggle.isOn = false;
			newLevelToggle.interactable = true;
			confirmButton.interactable = true;
		}
	}

	public void refreshLevelDropdown()
	{
		curCampaign = campaignDropdown.options[campaignDropdown.value].text;
		levelDropdown.ClearOptions();
		if (campaignDict[curCampaign].Count == 0)
		{
			newLevelToggle.isOn = true;
			newLevelToggle.interactable = false;
		}
		else
		{
			levelDropdown.AddOptions(campaignDict[curCampaign].Select(level => level.Name).ToList());
			newLevelToggle.isOn = false;
			newLevelToggle.interactable = true;
		}
		newLevelToggleChanged();
	}

	public void validateName()
	{
		// TODO: Check for case insensitive
		if (newCampaignToggle.isOn && 
			(string.IsNullOrEmpty(newCampaignField.text) || string.IsNullOrWhiteSpace(newCampaignField.text) ||
			 campaignDict.ContainsKey(newCampaignField.text)))
		{
			confirmButton.interactable = false;
			warningMessage.text = "Campaign name must be filled and not exist";
			warningMessage.gameObject.SetActive(true);
			return;
		}
		if (newLevelToggle.isOn && 
		    (string.IsNullOrEmpty(newLevelField.text) || string.IsNullOrWhiteSpace(newLevelField.text) || 
		     (!newCampaignToggle.isOn && campaignDict[curCampaign].Select(level => level.Name).Contains(newLevelField.text))))
		{
			confirmButton.interactable = false;
			warningMessage.text = "Level name must be filled and not exist";
			warningMessage.gameObject.SetActive(true);
			return;
		}

		warningMessage.gameObject.SetActive(false);
		confirmButton.interactable = true;
	}

	public void confirmButtonClicked()
	{
		var levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData))).First().GetComponent<LevelData>();
		levelData.height = int.Parse(height.GetComponentInChildren<InputField>().text); 
		levelData.width = int.Parse(width.GetComponentInChildren<InputField>().text); 
		levelData.campaignName = newCampaignField.text; 
		levelData.levelName = newLevelField.text;
		
		// TODO: Prevent a bug where if a scenario is created or editted with a new unsaved level it'd cause desync in the scenario xml
		if (newCampaignToggle.isOn)
		{
			// TODO: Currently borrowing streaming assets but clarifications needed as it is supposedly read only
			//var levelsPath = Application.persistentDataPath + "/Levels/" + newCampaignField.text + "/";
			var levelsPath = Application.streamingAssetsPath + "/Levels/" + newCampaignField.text + "/";
			Directory.CreateDirectory(levelsPath);
			// Change the path written to scenario file once streaming assets are dealt with
			var levelPath = levelsPath + newLevelField.text + ".xml";
			var writtenPath = levelPath.Substring(levelPath.IndexOf("Levels/", StringComparison.Ordinal));
			var xmlScenario = new XElement("scenario", new XElement("level", new XAttribute("name", writtenPath)));
			//var xmlScenarioPath = _customLevelsPath + newCampaignField.text + ".xml"; 
			var xmlScenarioPath = _baseLevelsPath + newCampaignField.text + ".xml"; 
			xmlScenario.Save(xmlScenarioPath);
			levelData.filePath = levelPath;
		}
		else if (newLevelToggle.isOn)
		{
			var levelsPath = Application.streamingAssetsPath + "/Levels/" + campaignDict.Keys.ToList()[campaignDropdown.value] + "/";
			var levelPath = levelsPath + newLevelField.text + ".xml";
			var writtenPath = levelPath.Substring(levelPath.IndexOf("Levels/", StringComparison.Ordinal));
			var scenarioPath = _baseLevelsPath + curCampaign + ".xml";
			var xdoc = XDocument.Load(scenarioPath);
			var xdocInnerNode = xdoc.Element("scenario");
			xdocInnerNode?.Add(new XElement("level", new XAttribute("name", writtenPath)));
			xdoc.Save(scenarioPath);
			levelData.filePath = levelPath;
		}
		else 
		{
			levelData.filePath = campaignDict[curCampaign][levelDropdown.value].Path;
		}
		screen.SetActive(false);
	}

	public void quitButtonClicked()
	{
		GameObjectManager.loadScene("TitleScreen");
	}

	private struct Level
	{
		public string Name { get; set; }
		public string Path { get; set; }
	}

}