using UnityEngine;

public class PaintableGrid : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Cell brush;
	public Cell[,] grid;
}