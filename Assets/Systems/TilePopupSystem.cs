using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FYFY;
using Newtonsoft.Json.Utilities;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePopupSystem : FSystem {
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	private Family f_orienters = FamilyManager.getFamily(new AllOfComponents(typeof(OrienterButton)));

	public static TilePopupSystem instance;
	public GameObject orientationPopup;
	public GameObject slotPopup;
	public GameObject scriptNamePopup;
	public GameObject furniturePopup;
	
    private Vector2Int _gridsize;
    private List<GameObject> activePopups = new List<GameObject>();
    private Dictionary<string, string> furnitureNameToPath = new Dictionary<string, string>();

    public TilePopupSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		hideAllPopups();
		_gridsize = new Vector2Int(getTilemap().GetComponent<PaintableGrid>().grid.GetLength(0),
			getTilemap().GetComponent<PaintableGrid>().grid.GetLength(1));
		initFurniturePopup();
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
		{
			if(activePopups.Count > 0)
				hideAllPopups();
			return;
		}

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
					setSlotPopupState(true);
					break;
				case Console c:
					setSlotPopupState(true, true);
					break;
				case PlayerRobot pr:
					setScriptNamePopupState(true);
					break;
				case EnemyRobot er:
					setScriptNamePopupState(true);
					break;
				case DecorationObject deco:
					setFurniturePopupState(true);
					break;
			}

			if (getSelected().orientable)
			{
				setOrientationPopupState(true);
			}
		}
	}

	private void initFurniturePopup()
	{
		var prefabsGuid = AssetDatabase.FindAssets("t:prefab", new [] {"Assets/Resources/Prefabs/Modern Furniture/Prefabs"});
		var prefabPaths = prefabsGuid.Select(AssetDatabase.GUIDToAssetPath).Select(s => s.Replace("Assets/Resources/Prefabs/", "").Replace(".prefab", ""));
		foreach (var path in prefabPaths)
		{
			var name = path.Split('/').Last();
			furnitureNameToPath[name] = path;
		}
		furniturePopup.GetComponentInChildren<Dropdown>().AddOptions(furnitureNameToPath.Keys.ToList());
	}

	// Can be factorised
	private void setOrientationPopupState(bool enabled)
	{
		if(enabled)
			activePopups.Add(orientationPopup);
		orientationPopup.SetActive(enabled);
	}

	private void setSlotPopupState(bool enabled, bool showToggle = false)
	{
		if (enabled)
		{
			activePopups.Add(slotPopup);
			slotPopup.GetComponentInChildren<InputField>().text = getSelectedSlot().ToString();
			slotPopup.GetComponentInChildren<Toggle>().interactable = showToggle;
			slotPopup.GetComponentInChildren<Toggle>().isOn = !showToggle || ((Console)getSelected()).state;
		}
		else if(getSelected() != null)
		{
			var text = slotPopup.GetComponentInChildren<InputField>().text;
			var slotId = text.Length > 0 ? int.Parse(text) : 0;
			switch (getSelected())
			{
				case Door d:
					d.slot = slotId;
					break;
				case Console c:
					c.slot = slotId;
					c.state = slotPopup.GetComponentInChildren<Toggle>().isOn;
					break;
			}
			slotPopup.GetComponentInChildren<InputField>().text = "";
		}
		slotPopup.SetActive(enabled);
	}
	
	private void setScriptNamePopupState(bool enabled)
	{
		if (enabled)
		{
			activePopups.Add(scriptNamePopup);
			scriptNamePopup.GetComponentInChildren<InputField>().text = getSelected() switch
			{
				PlayerRobot pr => pr.associatedScriptName,
				EnemyRobot er => er.associatedScriptName,
				_ => scriptNamePopup.GetComponentInChildren<InputField>().text
			};
		}
		else if(getSelected() != null)
		{
			switch (getSelected())
			{
				case PlayerRobot pr:
					pr.associatedScriptName = scriptNamePopup.GetComponentInChildren<InputField>().text;
					break;
				case EnemyRobot er:
					er.associatedScriptName = scriptNamePopup.GetComponentInChildren<InputField>().text;
					break;
			}
			scriptNamePopup.GetComponentInChildren<InputField>().text = "";
		}
		scriptNamePopup.SetActive(enabled);
	}

	private void setFurniturePopupState(bool enabled)
	{
		if (enabled)
		{
			activePopups.Add(furniturePopup);
			furniturePopup.GetComponentInChildren<Dropdown>().value = furnitureNameToPath.Keys.IndexOf(s => furnitureNameToPath[s] == ((DecorationObject)getSelected()).path);
		}
		else if (getSelected() != null && getSelected() is DecorationObject)
		{
			var value = furniturePopup.GetComponentInChildren<Dropdown>()
				.options[furniturePopup.GetComponentInChildren<Dropdown>().value].text;

			((DecorationObject)getSelected()).path = furnitureNameToPath[value];
		}
		
		furniturePopup.SetActive(enabled);
	}
	
	private void hideAllPopups()
	{
		setOrientationPopupState(false);
		setSlotPopupState(false);
		setScriptNamePopupState(false);
		setFurniturePopupState(false);
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

	private int getSelectedSlot()
	{
		return getSelected() switch
		{
			Door d => d.slot,
			Console c => c.slot,
			_ => throw new ArgumentException("Unexpected slot for non slot object")
		};
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