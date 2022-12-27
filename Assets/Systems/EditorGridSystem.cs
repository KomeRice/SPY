using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class EditorGridSystem : FSystem {
	public static EditorGridSystem instance;
	public Tile voidTile;
	public Tile floorTile;
	public Tile wallTile;
	public Tile spawnTile;
	public Tile teleportTile;
	public Tile playerTile;
	public Tile enemyTile;
	public Tile decoTile;
	public Tile doorTile;
	public Tile consoleTile;
	public Tile coinTile;
	public Texture2D placingCursor;
	public string defaultDecoration;
	public GameObject mainCanvas;
	public GameObject dialogViewPortContent;
	public GameObject listEntryPrefab;
	
	private Vector2Int _gridSize;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	public Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	private LevelData _levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData))).First().GetComponent<LevelData>();

	public EditorGridSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		getTilemap().SetActive(false);
		_levelData.isReady = false;
		MainLoop.instance.StartCoroutine(waitForLevelLoad());
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
		if (!getTilemap().GetComponent<PaintableGrid>().gridActive)
		{
			return;
		}

		var pos = mousePosToGridPos();
		if (0 > pos.x || pos.x >= _gridSize.x || 0 > pos.y || pos.y >= _gridSize.y || !canBePlaced(getActiveBrush(), pos.x, pos.y))
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		var posTuple = new Tuple<int, int>(pos.x, pos.y);
		
		if (Input.GetMouseButtonDown(1) &&
		    getTilemap().GetComponent<PaintableGrid>().floorObjects.ContainsKey(posTuple))
		{
			resetTile(pos.x, pos.y, -1);
			getTilemap().GetComponent<PaintableGrid>().selectedObject = null;
			return;
		}
		
		if (getActiveBrush() == Cell.Select)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			if (Input.GetMouseButtonDown(0) && getTilemap().GetComponent<PaintableGrid>().floorObjects.ContainsKey(posTuple)
			    && getTilemap().GetComponent<PaintableGrid>().selectedObject == null && getTilemap().GetComponent<PaintableGrid>().floorObjects[posTuple].selectable)
			{
				getTilemap().GetComponent<PaintableGrid>().selectedObject =
					getTilemap().GetComponent<PaintableGrid>().floorObjects[posTuple];
			}
			return;
		}

		if(placingCursor != null)
			Cursor.SetCursor(placingCursor, new Vector2(placingCursor.width / 2.0f, placingCursor.height / 2.0f), CursorMode.Auto);

		if (Input.GetMouseButtonDown(0) && (int) getActiveBrush() >= 10000)
		{
			setTile(pos.x, pos.y, getActiveBrush());
		}
		else if(Input.GetMouseButton(0) && (int) getActiveBrush() > -2)
			setTile(pos.x, pos.y, getActiveBrush());
	}

	private void initGrid(int width = 10, int height = 10)
	{
		getTilemap().GetComponent<PaintableGrid>().floorObjects = new Dictionary<Tuple<int, int>, FloorObject>();
		getTilemap().GetComponent<PaintableGrid>().gridActive = true;
		_gridSize = new Vector2Int(width, height);
		getTilemap().GetComponent<PaintableGrid>().grid = new Cell[width, height];
		for (var i = 0; i < width; ++i)
		{
			for (var j = 0; j < height; ++j)
			{
				if (i == 0 && j == 0)
				{
					setTile(i, j, Cell.Spawn);
					continue;
				}
				setTile(i, j, Cell.Void);
			}
		}
	}

	private IEnumerator waitForLevelLoad()
	{
		while (string.IsNullOrEmpty(_levelData.filePath))
		{
			yield return null;
		}

		if (File.Exists(_levelData.filePath))
		{
			var xmlScriptGenerator = new XmlScriptGenerator(mainCanvas);
			var paintableGrid = getTilemap().GetComponent<PaintableGrid>();
			var xmlFile = XDocument.Load(_levelData.filePath);
			var scripts = new List<XElement>();
			var robots = new Dictionary<string, Tuple<int, int>>();
			foreach (var element in xmlFile.Descendants("level").Elements())
			{
				Tuple<int, int> position;
				ObjectDirection orientation;
				int slotId;
				string associatedScriptName;
				switch (element.Name.LocalName)
				{
					case "map":
						var lines = element.Elements().ToList();
						var height = lines.Count;
						var width = lines[0].Elements().ToList().Count;
						initGrid(width, height);
						_levelData.height = height;
						_levelData.width = width;
						for(var i = 0; i < height; i++)
						{
							var cols = lines[i].Elements().ToList();
							for (var j = 0; j < cols.Count; j++)
							{
								if (int.TryParse(cols[j].Attribute("value")!.Value, out var cellValue))
								{
									setTile(j, i, (Cell) cellValue);
								}
								else
								{
									Debug.Log($"Warning: Skipped cell ({j},{i}) from file {_levelData.filePath}");
								}
							}
						}

						break;
					
					case "dialogs":
						foreach (var dialog in element.Descendants())
						{
							var newEntry = Object.Instantiate(listEntryPrefab, dialogViewPortContent.transform);
							GameObjectManager.bind(newEntry);
							var component = newEntry.GetComponent<DialogListEntry>();
							if (dialog.Attribute("text") != null)
							{
								component.dialogText = dialog.Attribute("text")?.Value;
								component.GetComponentInChildren<TMP_Text>().text = component.dialogText;
							}

							if (dialog.Attribute("camX") != null && dialog.Attribute("camY") != null)
							{
								component.cameraMove = true;
								int.TryParse(dialog.Attribute("camX")?.Value, out var camX);
								int.TryParse(dialog.Attribute("camY")?.Value, out var camY);
								component.cameraMoveX = camX;
								component.cameraMoveY = camY;
							}
							else
							{
								component.cameraMove = false;
							}
						}

						break;
					case "dragdropDisabled":
						_levelData.dragdropDisabled = true;
						
						break;
					case "executionLimit":
						_levelData.executionLimitEnabled = true;
						int.TryParse(element.Attribute("amount")?.Value, out _levelData.executionLimit);
						
						break;
					case "fog":
						_levelData.fogEnabled = true;
						
						break;
					case "blockLimits":
						var dict = new Dictionary<string, EditorBlockData>();
						foreach (var editorBlock in f_editorblocks)
						{
							var component = editorBlock.GetComponent<EditorBlockData>();
							dict[component.blockName] = component;
						}

						foreach (var blockLimit in element.Elements())
						{
							var blockName = blockLimit.Attribute("blockType").Value;
							var blockAmount = int.Parse(blockLimit.Attribute("limit").Value);
							if (blockAmount == 0)
								continue;
							
							dict[blockName].transform.GetChild(4).GetComponent<Toggle>().isOn = false;
							dict[blockName].transform.GetChild(3).GetComponent<Toggle>().interactable = true;

							if (blockAmount <= 0) 
								continue;
							
							dict[blockName].transform.GetChild(3).GetComponent<Toggle>().isOn = false;
							var field = dict[blockName].transform.GetChild(2).GetComponent<InputField>();
							field.interactable = true;
							field.text = blockAmount.ToString();
						}

						break;
					case "coin":
						position = getPositionFromXElement(element);
						setTile(position.Item1, position.Item2, Cell.Coin);
						
						break;
					case "console":
						position = getPositionFromXElement(element);
						var state = int.Parse(element.Attribute("state").Value);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);

						setTile(position.Item1, position.Item2, Cell.Console, orientation);

						if (element.HasElements)
						{
							slotId = int.Parse(element.Element("slot").Attribute("slotId").Value); 
							((Console)paintableGrid.floorObjects[position]).slot = slotId;
						}

						((Console)paintableGrid.floorObjects[position]).state = state == 1;
						
						break;
					case "door":
						position = getPositionFromXElement(element);
						slotId = int.Parse(element.Attribute("slotId").Value);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Door, orientation, slotId: slotId);
						((Door)paintableGrid.floorObjects[position]).slot = slotId;
						
						break;
					case "player":
						// TODO: player script type
						// Factorise
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Player, orientation);
						
						if (element.Attribute("associatedScriptName") != null)
						{
							associatedScriptName = element.Attribute("associatedScriptName").Value;
							((PlayerRobot)paintableGrid.floorObjects[position]).associatedScriptName =
								associatedScriptName;
							robots[associatedScriptName] = position;
						}
						
						break;
					case "enemy":
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Enemy, orientation);
						
						if (element.Attribute("associatedScriptName") != null)
						{
							associatedScriptName = element.Attribute("associatedScriptName").Value;
							((EnemyRobot)paintableGrid.floorObjects[position]).associatedScriptName =
								associatedScriptName;
							robots[associatedScriptName] = position;
						}
						
						var enemyRange = int.Parse(element.Attribute("range").Value);
						var selfRange = element.Attribute("selfRange").Value == "True";
						var typeRange = (EnemyTypeRange)int.Parse(element.Attribute("typeRange").Value);
						((EnemyRobot)paintableGrid.floorObjects[position]).range = enemyRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).selfRange = selfRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).typeRange = typeRange;
						
						break;
					case "script":
						// TODO: handle script types
						scripts.Add(element);
						
						break;
					case "decoration":
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						var decoPath = element.Attribute("name").Value;
						setTile(position.Item1, position.Item2, Cell.Decoration, orientation);
						((DecorationObject)paintableGrid.floorObjects[position]).path = decoPath;
						
						break;
					case "score":
						_levelData.scoreEnabled = true;
						_levelData.scoreTwoStars = int.Parse(element.Attribute("twoStars").Value);
						_levelData.scoreThreeStars = int.Parse(element.Attribute("threeStars").Value);
						
						break;
					default:
						Debug.Log($"Ignored unexpected node type: {element.Name}");
						
						break;
				}
			}

			foreach (var scriptElement in scripts)
			{
				var node = new XmlDocument().ReadNode(scriptElement.CreateReader());
				xmlScriptGenerator.readXMLScript(node, node.Attributes.GetNamedItem("name").Value, UIRootContainer.EditMode.Editable, UIRootContainer.SolutionType.Undefined);
			}

			_levelData.requireRefresh = true;
		}
		else
		{
			initGrid(_levelData.width, _levelData.height);
		}

		_levelData.isReady = true;
		getTilemap().SetActive(true);
	}

	private Tuple<int, int> getPositionFromXElement(XElement element)
	{
		if (element.Attribute("posX") == null || element.Attribute("posY") == null)
			return null;

		return new Tuple<int, int>(
			int.Parse(element.Attribute("posX")?.Value ?? throw new InvalidOperationException()),
			int.Parse(element.Attribute("posY")?.Value ?? throw new InvalidOperationException()));
	}
	
	private Vector2Int mousePosToGridPos()
	{
		var pos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
		var tilePos = getTilemap().GetComponent<Tilemap>().WorldToCell(pos);
		return vector3ToGridPos(tilePos);
	}

	private Vector2Int vector3ToGridPos(Vector3Int vec)
	{
		return new Vector2Int(vec.x + _gridSize.x / 2, _gridSize.y / 2 + vec.y * -1);
	}

	public void setTile(int x, int y, Cell cell, ObjectDirection rotation = ObjectDirection.Up, bool state = true, int slotId = 0,
		ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable,
		int enemyRange = 3, bool selfRange = false)
	{
		var tilemapGo = getTilemap();
		var tuplePos = new Tuple<int, int>(x, y);
		if ((int)cell < 10000)
		{
			tilemapGo.GetComponent<PaintableGrid>().grid[x, y] = cell;
			if (cell != Cell.Ground)
			{
				resetTile(x, y, -1);
			}
		}
		else if(!tilemapGo.GetComponent<PaintableGrid>().floorObjects.ContainsKey(tuplePos) ||
			tilemapGo.GetComponent<PaintableGrid>().floorObjects[tuplePos].type != cell)
		{
			tilemapGo.GetComponent<PaintableGrid>().floorObjects[tuplePos] = 
				cell switch
				{
					Cell.Player => new PlayerRobot("Bob", rotation, x, y, scriptType: scriptType, editMode: editMode),
					Cell.Enemy => new EnemyRobot("Eve", rotation, x, y, scriptType: scriptType, editMode: editMode, range: enemyRange, selfRange: selfRange),
					Cell.Decoration => new DecorationObject(defaultDecoration, rotation, x, y),
					Cell.Door => new Door(rotation, x, y, slotId),
					Cell.Console => new Console(rotation, x, y, slotId, state),
					Cell.Coin => new FloorObject(Cell.Coin, ObjectDirection.Up, x, y, orientable:false, selectable: false),
					_ => null
				};
		}
		else
		{
			return;
		}
		
		tilemapGo.GetComponent<Tilemap>().SetTile(new Vector3Int(x - _gridSize.x / 2,
			_gridSize.y / 2 - y, 
			(int) cell < 10000 ? 0 : -1), 
			cellToTile(cell));
		
		if((int) cell >= 10000)	
			rotateObject(rotation, x, y);
	}

	public void resetTile(int x, int y, int z)
	{
		var tilemapGo = getTilemap();
		var tuplePos = new Tuple<int, int>(x, y);
		tilemapGo.GetComponent<PaintableGrid>().floorObjects.Remove(tuplePos);
		tilemapGo.GetComponent<Tilemap>().SetTile(new Vector3Int(x - _gridSize.x / 2,
			_gridSize.y / 2 - y, 
			z), 
			null);
	}
	
	private void rotateObject(ObjectDirection newOrientation, int x, int y)
	{
		var newpos = new Vector3Int(x - _gridSize.x / 2,
			_gridSize.y / 2 - y, -1);
		var quat = Quaternion.Euler(0, 0, orientationToInt(newOrientation));
		
		getTilemap().GetComponent<Tilemap>().SetTransformMatrix(newpos, Matrix4x4.Rotate(quat));
	}

	private int orientationToInt(ObjectDirection orientation)
	{
		return orientation switch
		{
			ObjectDirection.Up => 0,
			ObjectDirection.Right => 270,
			ObjectDirection.Down => 180,
			ObjectDirection.Left => 90,
			_ => orientationToInt((ObjectDirection) ((int) orientation % 4))
		};
	}
	
	private Tile cellToTile(Cell cell)
	{
		return cell switch
		{
			Cell.Void => voidTile,
			Cell.Ground => floorTile,
			Cell.Wall => wallTile,
			Cell.Spawn => spawnTile,
			Cell.Teleport => teleportTile,
			Cell.Player => playerTile,
			Cell.Enemy => enemyTile,
			Cell.Decoration => decoTile,
			Cell.Door => doorTile,
			Cell.Console => consoleTile,
			Cell.Coin => coinTile,
			_ => null
		};
	}

	// Calls to this could be reduced
	private GameObject getTilemap()
	{
		return f_paintables.First();
	}
	
	private Cell getActiveBrush()
	{
		return getTilemap().GetComponent<PaintableGrid>().activeBrush;
	}

	private bool canBePlaced(Cell cell, int x, int y)
	{
		var curCell = getTilemap().GetComponent<PaintableGrid>().grid[x, y];
		return (int) cell < 10000 || curCell == Cell.Ground || (curCell == Cell.Spawn && cell == Cell.Player);
	}
	
	private class XmlScriptGenerator
	{ 
		private Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));
		private GameObject canvas;
		
		public XmlScriptGenerator(GameObject canvas)
		{
			this.canvas = canvas;
		}
		
		// Lit le XML d'un script est génère les game objects des instructions
		public void readXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
		{
			if(scriptNode != null){
				List<GameObject> script = new List<GameObject>();
				foreach(XmlNode actionNode in scriptNode.ChildNodes){
					script.Add(readXMLInstruction(actionNode));
				}
				GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = name, editState = editMode, typeState = type, script = script });
			}
		}

		private GameObject getLibraryItemByName(string itemName)
		{
			foreach (GameObject item in f_draggableElement)
				if (item.name == itemName)
					return item;
			return null;
		}

		// Transforme le noeud d'action XML en gameObject
		private GameObject readXMLInstruction(XmlNode actionNode){
			GameObject obj = null;
			Transform conditionContainer = null;
			Transform firstContainerBloc = null;
			Transform secondContainerBloc = null;
			switch (actionNode.Name)
			{
				case "if":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("IfThen"), canvas);

					conditionContainer = obj.transform.Find("ConditionContainer");
					firstContainerBloc = obj.transform.Find("Container");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a If action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								EditingUtility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "container")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
					}
					break;

				case "ifElse":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("IfElse"), canvas);
					conditionContainer = obj.transform.Find("ConditionContainer");
					firstContainerBloc = obj.transform.Find("Container");
					secondContainerBloc = obj.transform.Find("ElseContainer");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a IfElse action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								EditingUtility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "thenContainer")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
						else if (containerNode.Name == "elseContainer")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(secondContainerBloc, containerNode);
						}
					}
					break;

				case "for":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("ForLoop"), canvas);
					firstContainerBloc = obj.transform.Find("Container");
					BaseElement action = obj.GetComponent<ForControl>();

					((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
					obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

					if (actionNode.HasChildNodes)
						processXMLInstruction(firstContainerBloc, actionNode);
					break;

				case "while":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("While"), canvas);
					firstContainerBloc = obj.transform.Find("Container");
					conditionContainer = obj.transform.Find("ConditionContainer");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a While action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								EditingUtility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "container")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
					}
					break;

				case "forever":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("Forever"), canvas);
					firstContainerBloc = obj.transform.Find("Container");

					if (actionNode.HasChildNodes)
						processXMLInstruction(firstContainerBloc, actionNode);
					break;
				case "action":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName(actionNode.Attributes.GetNamedItem("type").Value), canvas);
					break;
			}

			return obj;
		}

		private void processXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
		{
			// The first child of a control container is an emptySolt
			GameObject emptySlot = gameContainer.GetChild(0).gameObject;
			foreach (XmlNode eleNode in xmlContainer.ChildNodes)
				EditingUtility.addItemOnDropArea(readXMLInstruction(eleNode), emptySlot);
		}

		// Transforme le noeud d'action XML en gameObject élément/opérator
		private GameObject readXMLCondition(XmlNode conditionNode) {
			GameObject obj = null;
			ReplacementSlot[] slots = null;
			switch (conditionNode.Name)
			{
				case "and":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("AndOperator"), canvas);
					slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = null;
						foreach (XmlNode andNode in conditionNode.ChildNodes)
						{
							if (andNode.Name == "conditionLeft")
								// The Left slot is the second ReplacementSlot (first is the And operator)
								emptyZone = slots[1].gameObject;
							if (andNode.Name == "conditionRight")
								// The Right slot is the third ReplacementSlot
								emptyZone = slots[2].gameObject;
							if (emptyZone != null && andNode.HasChildNodes)
							{
								// Parse xml condition
								GameObject child = readXMLCondition(andNode.FirstChild);
								// Add child to empty zone
								EditingUtility.addItemOnDropArea(child, emptyZone);
							}
							emptyZone = null;
						}
					}
					break;

				case "or":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("OrOperator"), canvas);
					slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = null;
						foreach (XmlNode orNode in conditionNode.ChildNodes)
						{
							if (orNode.Name == "conditionLeft")
								// The Left slot is the second ReplacementSlot (first is the And operator)
								emptyZone = slots[1].gameObject;
							if (orNode.Name == "conditionRight")
								// The Right slot is the third ReplacementSlot
								emptyZone = slots[2].gameObject;
							if (emptyZone != null && orNode.HasChildNodes)
							{
								// Parse xml condition
								GameObject child = readXMLCondition(orNode.FirstChild);
								// Add child to empty zone
								EditingUtility.addItemOnDropArea(child, emptyZone);
							}
							emptyZone = null;
						}
					}
					break;

				case "not":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("NotOperator"), canvas);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
						GameObject child = readXMLCondition(conditionNode.FirstChild);
						// Add child to empty zone
						EditingUtility.addItemOnDropArea(child, emptyZone);
					}
					break;
				case "captor":
					obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName(conditionNode.Attributes.GetNamedItem("type").Value), canvas);
					break;
			}

			return obj;
		}
	}
}

public enum Cell
{ 
	Void = -1,
	Ground = 0, 
	Wall = 1, 
	Spawn = 2, 
	Teleport = 3,
	Select = -10000,
	Player = 10000,
	Enemy = 10001,
	Decoration = 10002,
	Door = 10003,
	Console = 10004,
	Coin = 10005
}

public enum ObjectDirection
{
	Up = 0,
	Down = 1,
	Right = 2,
	Left = 3
}

public enum EnemyTypeRange
{
	LineView = 0,
	//The following two are unimplemented
	ConeView = 1,
	AroundView = 2
}

public enum ScriptType
{
	Optimal = 0,
	NonOptimal = 1,
	Bugged = 2,
	Undefined = 3
}

public enum ScriptEditMode
{
	Locked = 0,
	Sync = 1,
	Editable = 2
}

public class FloorObject
{
	public Cell type;
	public ObjectDirection orientation;
	public bool orientable;
	public bool selectable;
	public int x;
	public int y;

	public FloorObject(Cell type, ObjectDirection orientation, int x, int y, bool orientable = true, bool selectable = true)
	{
		this.type = type;
		this.orientation = orientation;
		this.x = x;
		this.y = y;
		this.orientable = orientable;
		this.selectable = selectable;
	}
}

public class DecorationObject : FloorObject
{
	public string path;

	public DecorationObject(string path, ObjectDirection orientation, int x, int y) : base(Cell.Decoration, orientation, x, y)
	{
		this.path = path;
	}
}

public class Console : FloorObject
{
	public int slot;
	public bool state;

	public Console(ObjectDirection orientation, int x, int y, int slot, bool state) : base(Cell.Console, orientation, x, y)
	{
		this.slot = slot;
		this.state = state;
	}
}

public class Door : FloorObject
{
	public int slot;

	public Door(ObjectDirection orientation, int x, int y, int slot) : base(Cell.Door, orientation, x, y)
	{
		this.slot = slot;
	}
}

public class Robot : FloorObject
{
	public string associatedScriptName;
	public ScriptType scriptType;
	public ScriptEditMode scriptEditMode;

	protected Robot(Cell cellType, string associatedScriptName, ObjectDirection orientation, int x, int y
		, bool orientable = true, ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable) : base(cellType, orientation, x, y, orientable)
	{
		this.associatedScriptName = associatedScriptName;
		this.scriptType = scriptType;
		scriptEditMode = editMode;
	}

	public void editName(string newName)
	{
		associatedScriptName = newName;
	}
}

public class PlayerRobot : Robot
{
	public PlayerRobot(string associatedScriptName, ObjectDirection orientation, int x, int y,
		bool orientable = true, ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable) : 
		base(Cell.Player, associatedScriptName, orientation, x, y, orientable, scriptType, editMode)
	{
	}
}

public class EnemyRobot : Robot
{
	public EnemyTypeRange typeRange;
	public bool selfRange;
	public int range;

	public EnemyRobot(string associatedScriptName, ObjectDirection orientation, int x, int y, 
		ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable,
		bool selfRange = false, EnemyTypeRange typeRange = EnemyTypeRange.LineView, bool orientable = true, bool selectable = true, int range = 3)
		: base(Cell.Enemy, associatedScriptName,orientation, x, y)
	{
		this.typeRange = typeRange;
		this.selfRange = selfRange;
		this.range = range;
		this.associatedScriptName = associatedScriptName;
		this.scriptType = scriptType;
		scriptEditMode = editMode;
	}
}