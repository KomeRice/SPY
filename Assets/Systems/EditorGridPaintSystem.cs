using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EditorGridPaintSystem : FSystem {

	public static EditorGridPaintSystem instance;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_brushes = FamilyManager.getFamily(new AllOfComponents(typeof(CellBrush)));


	public EditorGridPaintSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
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