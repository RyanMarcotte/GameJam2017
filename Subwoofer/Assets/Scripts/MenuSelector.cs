using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSelector : MonoBehaviour
{
    public GameObject player;
    public GameObject map;

    public void playAgain()
    {
	    var currentScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene("Subwoofer");
	    SceneManager.UnloadSceneAsync(currentScene);

	    //Restart the game!
	    //player.GetComponent<PlayerController>().Start();
	    //map.GetComponent<MapGenerator>().GenerateMap();
    }

    public void exitGame()
    {
        Debug.Log("EXIT");
        Application.Quit();
    }
}
