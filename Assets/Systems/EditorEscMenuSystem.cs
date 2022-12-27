using UnityEngine;
using FYFY;

public class EditorEscMenu : FSystem
{
	private Family f_levelData = FamilyManager.getFamily(new AllOfComponents(typeof(LevelData)));
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));
	public GameObject escCanvas;

	public static EditorEscMenu instance;

	public EditorEscMenu()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		escCanvas.SetActive(false);
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
	protected override void onProcess(int familiesUpdateCount) {
		if (!Input.GetKeyDown(KeyCode.Escape))
			return;
		if (!f_levelData.First().GetComponent<LevelData>().isReady)
		{
			closeEditor();
			return;
		}

		toggleMenu();
	}

	public void toggleMenu()
	{
		var newState = !escCanvas.activeSelf;
		f_paintables.First().GetComponent<PaintableGrid>().gridActive = !newState;
		escCanvas.SetActive(newState);
	}

	public void closeEditor()
	{
		GameObjectManager.loadScene("TitleScreen");
	}
}