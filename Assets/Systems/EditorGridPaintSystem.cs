using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EditorGridPaintSystem : FSystem {

	public static EditorGridPaintSystem instance;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_brushes = FamilyManager.getFamily(new AllOfComponents(typeof(CellBrush)));

	private Vector2Int _gridSize;

	public EditorGridPaintSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		var grid = getTilemap().GetComponent<PaintableGrid>().grid;
		_gridSize = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
		
		foreach (var brush in f_brushes)
		{
			if (brush.GetComponent<CellBrush>().brush != Cell.Select) 
				continue;
			setBrush(brush);
			return;
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
		var pos = mousePosToGridPos();
		if(0 < pos.x || pos.x >= _gridSize.x || 0 < pos.y || _gridSize.y >= pos.y || getActiveBrush() == Cell.Select)
			return;
		
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

	public void setBrush(GameObject go)
	{
		getTilemap().GetComponent<PaintableGrid>().activeBrush = go.GetComponent<CellBrush>().brush;
		foreach (var fBrush in f_brushes)
		{
			fBrush.GetComponent<Button>().interactable = true;
		}

		go.GetComponent<Button>().interactable = false;
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
}