using System;
using System.Collections.Generic;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePopupSystem : FSystem {
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_orienters = FamilyManager.getFamily(new AllOfComponents(typeof(OrienterButton)));

	public static TilePopupSystem instance;
	public GameObject orientationPopup;
    private TMP_Text orientationText;
    private RectTransform orientationRectTransform;
    private Image orientationBgImage;
    private Vector2Int _gridsize;
    private List<GameObject> activePopups = new List<GameObject>();

	public TilePopupSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		orientationText = orientationPopup.transform.GetChild(0).GetComponent<TMP_Text>();
		orientationRectTransform = orientationPopup.GetComponent<RectTransform>();
		orientationBgImage = orientationPopup.GetComponent<Image>();
		setOrientationPopupState(false);
		_gridsize = new Vector2Int(getTilemap().GetComponent<PaintableGrid>().grid.GetLength(0),
			getTilemap().GetComponent<PaintableGrid>().grid.GetLength(1));
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
		if (getSelected() == null)
			return;
		
		if (activePopups.Count > 0)
		{
			if (Input.GetMouseButtonDown(0))
			{
				foreach (var go in activePopups)
				{
					if (go.GetComponent<PopupMouseSensitive>().isOver)
						return;
				}
				
				hideAllPopups();
			}
		}
		else if(Input.GetMouseButtonDown(0))
		{
			switch (getSelected())
			{
				case Door d:
					break;
				case Console c:
					break;
				case DecorationObject deco:
					break;
			}

			if (getSelected().orientable)
			{
				setOrientationPopupState(true);
				activePopups.Add(orientationPopup);
			}
		}
	}
	
	private void setOrientationPopupState(bool enabled)
	{
		orientationText.enabled = enabled;
		orientationBgImage.enabled = enabled;

		for (var i = 1; i < orientationPopup.transform.childCount; i++)
		{
			orientationPopup.transform.GetChild(i).gameObject.SetActive(enabled);
		}
		
	}

	private void hideAllPopups()
	{
		setOrientationPopupState(false);
		activePopups.Clear();
		getTilemap().GetComponent<PaintableGrid>().selectedObject = null;
	}

	private void rotateObject(ObjectDirection newOrientation, int x, int y)
	{
		var newpos = coordsToGridCoords(x, y);
		var quat = Quaternion.Euler(0, 0, orientationToInt(newOrientation));
		
		getTilemap().GetComponent<Tilemap>().SetTransformMatrix(newpos, Matrix4x4.Rotate(quat));
		getSelected().orientation = newOrientation;
	}

	public void rotateSelectionUp()
	{
		rotateObject(ObjectDirection.Up, getSelected().x, getSelected().y);
	}
	
	public void rotateSelectionRight()
	{
		rotateObject(ObjectDirection.Right, getSelected().x, getSelected().y);
	}
	
	public void rotateSelectionLeft()
	{
		rotateObject(ObjectDirection.Left, getSelected().x, getSelected().y);
	}
	
	public void rotateSelectionDown()
	{
		rotateObject(ObjectDirection.Down, getSelected().x, getSelected().y);
	}

	private FloorObject getSelected()
	{
		return getTilemap().GetComponent<PaintableGrid>().selectedObject;
	}

	private GameObject getTilemap()
	{
		return f_paintables.First();
	}

	private Vector3Int coordsToGridCoords(int x, int y)
	{
		return new Vector3Int(x - _gridsize.x / 2,
			_gridsize.y / 2 - y, -1);
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
}