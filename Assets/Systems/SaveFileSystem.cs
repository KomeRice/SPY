using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;

public class SaveFileSystem : FSystem
{
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	private Family f_levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData)));

	public InputField executionLimitField;
	public GameObject editableContainer;

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
		if((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) ||
		   Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.S))
			saveXmlFile();
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

		var familyDialogs = 
			FamilyManager.getFamily(new AllOfComponents(typeof(DialogListEntry)))
				.Select(go => go.GetComponent<DialogListEntry>()).ToList();

		if (familyDialogs.Count > 0)
		{
			var dialogNode = new XElement("dialogs");
			foreach (var dialog in familyDialogs)
			{
				var dialogSubNode = new XElement("dialog", new XAttribute("text", dialog.dialogText));
				if (dialog.cameraMove)
				{
					dialogSubNode.Add(new XAttribute("camX", dialog.cameraMoveX));
					dialogSubNode.Add(new XAttribute("camY", dialog.cameraMoveY));
				}
				dialogNode.Add(dialogSubNode);
			}

			rootNode.Add(dialogNode);
		}
		
		

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

		var robots = new Dictionary<string, Robot>();

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
					robots[pr.associatedScriptName] = pr;
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
					robots[er.associatedScriptName] = er;
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

		for (var i = 0; i < editableContainer.transform.childCount; i++)
		{
			var editorViewportScriptContainer = editableContainer.transform.GetChild(i).Find("ScriptContainer");
			var header = editorViewportScriptContainer.Find("Header");
			var scriptName = header.Find("ContainerName").GetComponent<TMP_InputField>().text;
			var scriptNode = (getXmlScript(editorViewportScriptContainer.gameObject, "script"));
			if (!robots.ContainsKey(scriptName))
			{
				scriptNode.Add(new XAttribute("name", scriptName),
					new XAttribute("editMode", 0),
					new XAttribute("type", 3));
				rootNode.Add(scriptNode);
				continue;
			}

			var robot = robots[scriptName];
			scriptNode.Add(new XAttribute("name", robot.associatedScriptName), 
				new XAttribute("editMode", (int) robot.scriptEditMode),
				new XAttribute("type", (int) robot.scriptType));
			
			rootNode.Add(scriptNode);
		}
		
		xDocument.Add(rootNode);

		if (levelData.filePath.Contains(Application.streamingAssetsPath))
		{
			Debug.Log("Trying to write to streaming assets...");
			try
			{
				xDocument.Save(levelData.filePath);
			}
			catch (Exception e)
			{
				Debug.Log($"Caught {e.Message} while trying to write to streaming assets");
			}
		}
		else
		{
			xDocument.Save(levelData.filePath);
		}

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

	private XElement getXmlScript(GameObject cur, string name, XAttribute attribute = null)
	{
		var result = new XElement(name);
		if(attribute != null)
			result.Add(attribute);
		
		for (var i = 0; i < cur.transform.childCount; i++)
		{
			var child = cur.transform.GetChild(i).gameObject;
			var actionComponent = child.GetComponent<BasicAction>();
			if (actionComponent)
			{
				result.Add(getXmlAction(actionComponent.actionType));
				continue;
			}
			
			// IfElse inherits from IfControl so it must be checked first
			
			var ifElseComponent = child.GetComponent<IfElseControl>();
			if (ifElseComponent)
			{
				var conditionXml = new XElement("condition", getXmlCondition(child.transform.Find("ConditionContainer").GetChild(0).gameObject));
				var thenContainer = child.transform.Find("Container");
				var elseContainer = child.transform.Find("ElseContainer");
				var thenXml = getXmlScript(thenContainer.gameObject, "thenContainer");
				var elseXml = getXmlScript(elseContainer.gameObject, "elseContainer");
				result.Add(new XElement("ifElse", conditionXml, thenXml, elseXml));
				
				continue;
			}
			
			var ifThenComponent = child.GetComponent<IfControl>();
			if (ifThenComponent)
			{
				var conditionXml = new XElement("condition", getXmlCondition(child.transform.Find("ConditionContainer").GetChild(0).gameObject));
				var thenContainer = child.transform.Find("Container");
				var thenXml = getXmlScript(thenContainer.gameObject, "container");
				result.Add(new XElement("if", conditionXml, thenXml));
				continue;
			}
			
			// WhileComponent inherits from ForControl so it must be checked before the latter
			
			var whileComponent = child.GetComponent<WhileControl>();
			if (whileComponent)
			{
				var conditionXml = new XElement("condition", getXmlCondition(child.transform.Find("ConditionContainer").GetChild(0).gameObject));
				var thenContainer = child.transform.Find("Container");
				var thenXml = getXmlScript(thenContainer.gameObject, "container");
				result.Add(new XElement("while", conditionXml, thenXml));
				continue;
			}
			
			var forComponent = child.GetComponent<ForControl>();
			if (forComponent)
			{
				var nbForStr = forComponent.transform.Find("Header").GetComponentInChildren<TMP_InputField>().text;
				var nbFor = string.IsNullOrEmpty(nbForStr) ? "0" : nbForStr;
				result.Add(getXmlScript(child.transform.Find("Container").gameObject, "for", new XAttribute("nbFor", nbFor)));
				continue;
			}

			var foreverComponent = child.GetComponent<ForeverControl>();
			if (foreverComponent)
			{
				var thenContainer = child.transform.Find("Container");
				var thenXml = getXmlScript(thenContainer.gameObject, "container");
				result.Add(new XElement("forever", thenXml));
			}
		}
		return result;
	}

	private XElement getXmlCondition(GameObject cur)
	{
		var captor = cur.GetComponent<BaseCaptor>();
		string name;
		if (captor)
		{
			name = captor.captorType switch
			{
				BaseCaptor.CaptorType.WallFront => "WallFront",
				BaseCaptor.CaptorType.WallLeft => "WallLeft",
				BaseCaptor.CaptorType.WallRight => "WallRight",
				BaseCaptor.CaptorType.Enemy => "Enemy",
				BaseCaptor.CaptorType.RedArea => "RedArea",
				BaseCaptor.CaptorType.FieldGate => "FieldGate",
				BaseCaptor.CaptorType.Terminal => "Terminal",
				BaseCaptor.CaptorType.Exit => "Exit",
				_ => throw new ArgumentOutOfRangeException()
			};
			return new XElement("captor", new XAttribute("type", name));
		}

		var operatorComponent = cur.GetComponent<BaseOperator>();
		if (!operatorComponent)
		{
			Debug.Log("Invalid condition");
			return new XElement("InvalidOperator");
		}

		name = operatorComponent.operatorType switch
		{
			BaseOperator.OperatorType.AndOperator => "and",
			BaseOperator.OperatorType.OrOperator => "or",
			BaseOperator.OperatorType.NotOperator => "not",
			_ => throw new ArgumentOutOfRangeException()
		};

		var result = new XElement(name);

		var container = cur.transform.Find("Container");
		switch (operatorComponent.operatorType)
		{
			case BaseOperator.OperatorType.AndOperator:
			case BaseOperator.OperatorType.OrOperator:
				
				var conditions = new List<GameObject>();
				for (var i = 0; i < container.childCount; i++)
				{
					var child = container.GetChild(i);
					if (child.GetComponent<BaseOperator>())
					{
						conditions.Add(child.gameObject);
						continue;
					}

					if (child.GetComponent<BaseCondition>())
					{
						conditions.Add(child.gameObject);
					}
				}

				if (conditions.Count != 2)
				{
					Debug.Log("Invalid condition");
					return new XElement("InvalidCondition");
				}
				result.Add(new XElement("conditionLeft", getXmlCondition(conditions[0])));
				result.Add(new XElement("conditionRight", getXmlCondition(conditions[1])));
				
				break;
			case BaseOperator.OperatorType.NotOperator:
				for (var i = 0; i < container.childCount; i++)
				{
					var child = container.GetChild(i);
					if (child.GetComponent<BaseOperator>())
					{
						result.Add(getXmlCondition(child.gameObject));
						break;
					}

					if (!child.GetComponent<BaseCondition>()) 
						continue;
					
					result.Add(getXmlCondition(child.gameObject));
					break;
				}

				if (result.Elements().Count() != 1)
				{
					Debug.Log("Invalid condition");
					return new XElement("InvalidCondition");
				}
				
				break;
			default:
				Debug.Log("Invalid condition");
				return new XElement("InvalidCondition");
		}

		return result;
	}

	private XElement getXmlAction(BasicAction.ActionType type)
	{
		var actionString = type switch
		{
			BasicAction.ActionType.Activate => "Activate",
			BasicAction.ActionType.Forward => "Forward",
			BasicAction.ActionType.TurnLeft => "TurnLeft",
			BasicAction.ActionType.TurnRight => "TurnRight",
			BasicAction.ActionType.Wait => "Wait",
			BasicAction.ActionType.TurnBack => "TurnBack",
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};

		return new XElement("action", new XAttribute("type", actionString));
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