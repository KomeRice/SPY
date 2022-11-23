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
	
	private Vector3Int _gridSize;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));

	public EditorGridSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		var tilemapGo = getTilemap();
		_gridSize = tilemapGo.GetComponent<Tilemap>().size;
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
		if (!Input.GetMouseButtonDown(0)) 
			return;
		
		var pos = mousePosToGridPos();
		Debug.Log($"Mouse on tile {pos.x}, {pos.y}");
	}

	private void initGrid(int width = 16, int height = 10)
	{
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
		return new Vector2Int(vec.x + _gridSize.x, _gridSize.y - vec.y);
	}

	private void setTile(int x, int y, Cell cell)
	{
		var tilemapGo = getTilemap();
		tilemapGo.GetComponent<PaintableGrid>().grid[x, y] = cell;
		tilemapGo.GetComponent<Tilemap>().SetTile(new Vector3Int(x - _gridSize.x, _gridSize.y - y, 0), cellToTile(cell));
		Debug.Log($"Set tile: {x - _gridSize.x}, {_gridSize.y - y} to tile");
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
			_ => voidTile
		};
	}

	// Calls to this could be reduced
	private GameObject getTilemap()
	{
		return f_paintables.First();
	}

}
public enum Cell
{ 
	Void = -1,
	Ground = 0, 
	Wall = 1, 
	Spawn = 2, 
	Teleport = 3
}
