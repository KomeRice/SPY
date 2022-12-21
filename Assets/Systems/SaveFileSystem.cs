using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using FYFY;
using UnityEngine.UI;
using ArgumentNullException = System.ArgumentNullException;

public class SaveFileSystem : FSystem
{
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	private Family f_levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData)));

	public InputField executionLimitField;

	public static SaveFileSystem instance;

	public SaveFileSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart(){
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

	public void saveXmlFile()
	{
		var xDocument = new XDocument();
		var rootNode = new XElement("level");
		var levelData = getLevelData();

		var mapNode = new XElement("map");
		for (var i = 0; i < getPaintableGrid().grid.GetLength(1); i++)
		{
			var lineNode = new XElement("line");
			for (var j = 0; j < getPaintableGrid().grid.GetLength(0); j++)
			{
				lineNode.Add(new XElement("cell", new XAttribute("value", (int) getPaintableGrid().grid[j, i])));
			}
			mapNode.Add(lineNode);
		}
		rootNode.Add(mapNode);

		if (levelData.dragdropDisabled)
			rootNode.Add(new XElement("dragdropDisabled"));

		if (levelData.executionLimitEnabled)
		{
			var amount = string.IsNullOrEmpty(executionLimitField.text) ? "1" : executionLimitField.text;
			rootNode.Add(new XElement("executionLimit", new XAttribute("amount", amount == "0" ? "1" : amount)));
		}
		
		if(levelData.fogEnabled)
			rootNode.Add(new XElement("fog"));
		
		var blockLimits = getBLockLimits();
		var xmlBlockLimits = new XElement("blockLimits");
		foreach (var key in blockLimits.Keys)
		{
			xmlBlockLimits.Add(new XElement("blockLimit", new XAttribute("blockType", key), new XAttribute("limit", blockLimits[key])));
		}
		rootNode.Add(xmlBlockLimits);

		foreach (var foCoords in getPaintableGrid().floorObjects.Keys)
		{
			var fo = getPaintableGrid().floorObjects[foCoords];
			switch (fo)
			{
				case Console c:
					rootNode.Add(
						new XElement("console", 
							new XAttribute("state", c.state ? 1 : 0),
							new XAttribute("posY", c.y),
							new XAttribute("posX", c.x),
							new XAttribute("direction", (int) c.orientation),
							new XElement("slot", new XAttribute("slotId", c.slot))));
					break;
				case Door d:
					rootNode.Add(
						new XElement("door", 
							new XAttribute("posY", d.y),
							new XAttribute("posX", d.x),
							new XAttribute("direction", (int) d.orientation),
							new XElement("slot", new XAttribute("slotId", d.slot))));
					break;
				case PlayerRobot pr:
					rootNode.Add(
						new XElement("player",
							new XAttribute("associatedScriptName", pr.associatedScriptName),
							new XAttribute("posY", pr.y),
							new XAttribute("posX", pr.x),
							new XAttribute("direction", (int) pr.orientation)));
					break;
				case EnemyRobot er:
					rootNode.Add(
						new XElement("enemy",
							new XAttribute("associatedScriptName", er.associatedScriptName),
							new XAttribute("posY", er.y),
							new XAttribute("posX", er.x),
							new XAttribute("direction", (int) er.orientation),
							new XAttribute("range", er.range),
							new XAttribute("selfRange", er.selfRange ? "True" : "False"),
							new XAttribute("typeRange", (int) er.typeRange)));
					break;
				case DecorationObject deco:
					rootNode.Add(
						new XElement("decoration",
							new XAttribute("name", deco.path),
							new XAttribute("posY", deco.y),
							new XAttribute("posX", deco.x),
							new XAttribute("direction", (int) deco.orientation)));
					break;

				default:
					if (fo.type != Cell.Coin)
					{
						Debug.Log("Unexpected floor object type, object ignored: " + fo.type);
						break;
					}

					rootNode.Add(new XElement("coin", 
						new XAttribute("posY", fo.y), 
						new XAttribute("posX", fo.x)));
					break;
			}
		}

		if (levelData.scoreEnabled)
		{
			rootNode.Add(new XElement("score", new XAttribute("twoStars", levelData.scoreTwoStars),
				new XAttribute("threeStars", levelData.scoreThreeStars)));
		}
		
		
		
		xDocument.Add(rootNode);
		Debug.Log($"Done: \n{xDocument}");
	}

	private Dictionary<string, int> getBLockLimits()
	{
		var result = new Dictionary<string, int>();
		foreach (var go in f_editorblocks)
		{
			var data = go.GetComponent<EditorBlockData>();
			var blockName = data.blockName;
			var hideToggled = go.GetComponentsInChildren<Toggle>()[0].isOn;
			var limitToggled = go.GetComponentsInChildren<Toggle>()[1].isOn;

			if (hideToggled)
			{
				result[blockName] = 0;
				continue;
			}

			if (!limitToggled)
			{
				result[blockName] = -1;
			}
			
			var limitStr = go.GetComponentInChildren<InputField>().text;
			var limit = !string.IsNullOrEmpty(limitStr) ? int.Parse(limitStr) : 1;

			result[blockName] = limit;
		}

		return result;
	}

	private PaintableGrid getPaintableGrid()
	{
		return f_paintables.First().GetComponent<PaintableGrid>();
	}

	private LevelData getLevelData()
	{
		return f_levelData.First().GetComponent<LevelData>();
	}
}