using UnityEngine;
using FYFY;

public class ViewToggleSystem : FSystem
{
	public Canvas mainCanvas;
	public Canvas metadataCanvas;

	private bool mainCanvasActive = true;
	private Family f_paintables = FamilyManager.getFamily(new AllOfComponents(typeof(PaintableGrid)));

	public static ViewToggleSystem instance;

	public ViewToggleSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart(){
		metadataCanvas.gameObject.SetActive(false);
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
	}

	public void toggleCanvas()
	{
		mainCanvasActive = !mainCanvasActive;
		mainCanvas.gameObject.SetActive(mainCanvasActive);
		f_paintables.First().GetComponent<PaintableGrid>().gridActive = mainCanvasActive;
		f_paintables.First().SetActive(mainCanvasActive);
		metadataCanvas.gameObject.SetActive(!mainCanvasActive);
	}
}