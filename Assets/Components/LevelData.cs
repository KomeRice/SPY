using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool dragdropDisabled;
	public bool executionLimitEnabled;
	public int executionLimit;
	public bool fogEnabled;
	public bool scoreEnabled;
	public int scoreTwoStars;
	public int scoreThreeStars;
}