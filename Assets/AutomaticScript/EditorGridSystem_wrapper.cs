using UnityEngine;
using FYFY;

public class EditorGridSystem_wrapper : BaseWrapper
{
	public UnityEngine.Tilemaps.Tile voidTile;
	public UnityEngine.Tilemaps.Tile floorTile;
	public UnityEngine.Tilemaps.Tile wallTile;
	public UnityEngine.Tilemaps.Tile spawnTile;
	public UnityEngine.Tilemaps.Tile teleportTile;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "voidTile", voidTile);
		MainLoop.initAppropriateSystemField (system, "floorTile", floorTile);
		MainLoop.initAppropriateSystemField (system, "wallTile", wallTile);
		MainLoop.initAppropriateSystemField (system, "spawnTile", spawnTile);
		MainLoop.initAppropriateSystemField (system, "teleportTile", teleportTile);
	}

}
