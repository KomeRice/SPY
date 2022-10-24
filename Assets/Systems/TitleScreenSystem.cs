﻿using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using Object = UnityEngine.Object;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	public GameData prefabGameData;
	public GameObject mainCanvas;
	public GameObject campagneMenu;
	public GameObject compLevelButton;
	public GameObject listOfCampaigns;
	public GameObject listOfLevels;
	public GameObject loadingScenarioContent;
	public GameObject scenarioContent;

	private GameObject selectedScenario;
	private Dictionary<string, List<string>> defaultCampaigns; // List of levels for each default campaign

	// L'instance
	public static TitleScreenSystem instance;

	public TitleScreenSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		if (!GameObject.Find("GameData"))
		{
			gameData = UnityEngine.Object.Instantiate(prefabGameData);
			gameData.name = "GameData";
			GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);
		}
		else
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}

		gameData.levels = new Dictionary<string, XmlNode>();
		gameData.scenario = new List<string>();

		defaultCampaigns = new Dictionary<string, List<string>>();
		selectedScenario = null;

		GameObjectManager.setGameObjectState(campagneMenu, false);
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			for (int i = 1; i <= 22; i++)
				defaultCampaigns["Campagne infiltration"].Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + "Campagne infiltration" + Path.DirectorySeparatorChar +"Niveau" + i + ".xml");
			// Hide Competence button
			GameObjectManager.setGameObjectState(compLevelButton, false);
			createScenarioButtons();
		}
	}

	public void updateScenarioList()
	{
		if (Application.platform != RuntimePlatform.WebGLPlayer)
		{
			loadLevelsAndScenarios(Application.streamingAssetsPath);
			loadLevelsAndScenarios(Application.persistentDataPath);
			createScenarioButtons();
		}
	}

	private void createScenarioButtons()
	{
		// remove all old scenario
		foreach(Transform child in listOfCampaigns.transform)
        {
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
        }

		//create level directory buttons
		foreach (string key in defaultCampaigns.Keys)
		{
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, listOfCampaigns.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(key);
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(key); });
		}
	}

	private void loadLevelsAndScenarios(string path)
	{
		// try to load all child files
		foreach (string fileName in Directory.GetFiles(path))
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(fileName);
				EditingUtility.removeComments(doc);
				// a valid level must have only one tag "level"
				if (doc.GetElementsByTagName("level").Count == 1)
					gameData.levels.Add(fileName, doc.GetElementsByTagName("level")[0]);
				// a valid scenario must have only one tag "scenario"
				if (doc.GetElementsByTagName("scenario").Count == 1)
				{
					List<string> levelList = new List<string>();
					foreach (XmlNode child in doc.GetElementsByTagName("scenario")[0])
						if (child.Name.Equals("level"))
							levelList.Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
					defaultCampaigns[Path.GetFileName(fileName)] = levelList; //key = directory name
				}
			}
			catch { }
		}

		// explore subdirectories
		foreach (string directory in Directory.GetDirectories(path))
			loadLevelsAndScenarios(directory);
	}

	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetButtonDown("Cancel")) {
			Application.Quit();
		}
	}

	private void showLevels(string campaignKey) {
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("SPYMenu").Find("MenuCampaigns").gameObject, false);
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("SPYMenu").Find("MenuLevels").gameObject, true);
		// delete all old level buttons
		foreach (Transform child in listOfLevels.transform)
        {
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
        }

		// create level buttons for this campaign
		for (int i = 0; i < defaultCampaigns[campaignKey].Count; i++)
		{
			string levelKey = defaultCampaigns[campaignKey][i];
			GameObject button = Object.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, listOfLevels.transform);
			button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(levelKey);
			button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(campaignKey, levelKey); });
			GameObjectManager.bind(button);
			//locked levels
			if (i <= PlayerPrefs.GetInt(campaignKey, 0)) //by default first level of directory is the only unlocked level of directory
				button.GetComponentInChildren<Button>().interactable = true;
			//unlocked levels
			else
				button.GetComponentInChildren<Button>().interactable = false;
			//scores
			int scoredStars = PlayerPrefs.GetInt(levelKey + gameData.scoreKey, 0); //0 star by default
			Transform scoreCanvas = button.transform.Find("ScoreCanvas");
			for (int nbStar = 0; nbStar < 4; nbStar++)
			{
				if (nbStar == scoredStars)
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
				else
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
			}
		}
	}

	public void launchLevel(string campaignKey, string levelToLoad) {
		gameData.scenarioName = campaignKey;
		gameData.levelToLoad = levelToLoad;
		gameData.scenario = defaultCampaigns[campaignKey];
		GameObjectManager.loadScene("MainScene");
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}

	public void displayLoadingPanel()
	{
		selectedScenario = null;
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("LoadingPanel").gameObject, true);
		// remove all old scenario
		foreach (Transform child in loadingScenarioContent.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create level directory buttons
		foreach (string key in defaultCampaigns.Keys)
		{
			GameObject scenarioItem = Object.Instantiate<GameObject>(Resources.Load("Prefabs/ScenarioAvailable") as GameObject, loadingScenarioContent.transform);
			scenarioItem.GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(scenarioItem);
		}
	}

	public void onScenarioSelected(GameObject go)
    {
		selectedScenario = go;
    }

	public void loadScenario()
    {
		if (selectedScenario != null && defaultCampaigns.ContainsKey(selectedScenario.GetComponentInChildren<TMP_Text>().text))
		{
			//remove all old scenario
			foreach (Transform child in scenarioContent.transform)
            {
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
            }

			foreach (string levelPath in defaultCampaigns[selectedScenario.GetComponentInChildren<TMP_Text>().text])
			{
				GameObject newLevel = GameObject.Instantiate(Resources.Load("Prefabs/deletableElement") as GameObject, scenarioContent.transform);
				newLevel.GetComponentInChildren<TMP_Text>().text = levelPath.Replace(Application.streamingAssetsPath + Path.DirectorySeparatorChar, "");
				LayoutRebuilder.ForceRebuildLayoutImmediate(newLevel.transform as RectTransform);
				GameObjectManager.bind(newLevel);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(scenarioContent.transform as RectTransform);
		}
    }
}