using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using FYFY_plugins.TriggerManager;

/// <summary>
/// Manage collision between player agents and Coins
/// </summary>
public class CoinManager : FSystem {
    private Family f_robotcollision = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private GameData gameData;
    private bool activeCoin;

	protected override void onStart()
    {
		activeCoin = false;
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		f_robotcollision.addEntryCallback(onNewCollision);

		f_playingMode.addEntryCallback(delegate { activeCoin = true; });
		f_editingMode.addEntryCallback(delegate { activeCoin = false; });
	}

	private void onNewCollision(GameObject robot){
		if(activeCoin){
			Triggered3D trigger = robot.GetComponent<Triggered3D>();
			foreach(GameObject target in trigger.Targets){
				//Check if the player collide with a coin
                if(target.CompareTag("Coin")){
                    gameData.totalCoin++;
                    target.GetComponent<AudioSource>().Play();
                    MainLoop.instance.StartCoroutine(coinDestroy(target));					
                    CollectedObjetStatement($"coin_{gameData.totalCoin}");
                }
			}			
		}
    }
	
    /*A mettre quand on collecte un objet*/
    public void CollectedObjetStatement(string ObjectName)
    {
        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new 
        {
            verb = "collected",
            objectType = "key",
            activityExtensions = new Dictionary<string, string>() {
                { "value", ObjectName },
            }
        });
    }

	private IEnumerator coinDestroy(GameObject go){
		go.GetComponent<ParticleSystem>().Play();
		go.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(1f); // let time for animation
		GameObjectManager.setGameObjectState(go, false); // then disabling GameObject
	}
}