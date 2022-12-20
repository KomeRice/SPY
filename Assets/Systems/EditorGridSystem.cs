using System;
using System.Collections.Generic;
using UnityEngine;
using FYFY;
using UnityEngine.Tilemaps;

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
	
	private Vector2Int _gridSize;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));

	public EditorGridSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		var tilemapGo = getTilemap();
		getTilemap().GetComponent<PaintableGrid>().floorObjects = new Dictionary<Tuple<int, int>, FloorObject>();
		getTilemap().GetComponent<PaintableGrid>().gridActive = true;
		initGrid();
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
		var pos = mousePosToGridPos();
		if (0 > pos.x || pos.x >= _gridSize.x || 0 > pos.y || pos.y >= _gridSize.y || !canBePlaced(getActiveBrush(), pos.x, pos.y)
		    || !getTilemap().GetComponent<PaintableGrid>().gridActive)
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

	private void initGrid(int width = 16, int height = 10)
	{
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

	public void setTile(int x, int y, Cell cell)
	{
		var tilemapGo = getTilemap();
		if ((int)cell < 10000)
		{
			tilemapGo.GetComponent<PaintableGrid>().grid[x, y] = cell;
			if (cell != Cell.Ground)
			{
				resetTile(x, y, -1);
			}
		}
		else
		{
			tilemapGo.GetComponent<PaintableGrid>().floorObjects[new Tuple<int, int>(x, y)] = 
				cell switch
				{
					Cell.Player => new PlayerRobot("Bob", ObjectDirection.Up, x, y),
					Cell.Enemy => new EnemyRobot("Eve", ObjectDirection.Up, x, y),
					Cell.Decoration => new DecorationObject(defaultDecoration, ObjectDirection.Up, x, y),
					Cell.Door => new Door(ObjectDirection.Up, x, y, 0),
					Cell.Console => new Console(ObjectDirection.Up, x, y, 0, true),
					Cell.Coin => new FloorObject(Cell.Coin, ObjectDirection.Up, x, y, orientable:false, selectable: false),
					_ => null
				};
			rotateObject(ObjectDirection.Up, x, y);
		}
		
		tilemapGo.GetComponent<Tilemap>().SetTile(new Vector3Int(x - _gridSize.x / 2,
			_gridSize.y / 2 - y, 
			(int) cell < 10000 ? 0 : -1), 
			cellToTile(cell));
	}

	public void resetTile(int x, int y, int z)
	{
		var tilemapGo = getTilemap();
		tilemapGo.GetComponent<PaintableGrid>().floorObjects.Remove(new Tuple<int, int>(x, y));
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
			_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation,"Impossible orientation")
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

public enum scriptType
{
	Optimal = 0,
	NonOptimal = 1,
	Bugged = 2,
	Undefined = 3
}

public enum scriptEditMode
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

public class PlayerRobot : FloorObject
{
	public string associatedScriptName;

	public PlayerRobot(string associatedScriptName, ObjectDirection orientation, int x, int y, bool orientable = true) : base(Cell.Player, orientation, x, y, orientable)
	{
		this.associatedScriptName = associatedScriptName;
	}
}

public class EnemyRobot : FloorObject
{
	public string associatedScriptName;

	public EnemyRobot(string associatedScriptName, ObjectDirection orientation, int x, int y,
		bool selfRange = false, EnemyTypeRange typeRange = EnemyTypeRange.LineView, bool orientable = true, bool selectable = true)
		: base(Cell.Enemy, orientation, x, y, orientable, selectable)
	{
		this.associatedScriptName = associatedScriptName;
	}
}