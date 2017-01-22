using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelector : MonoBehaviour
{
    public GameObject player;
    public GameObject map;

    public void playAgain()
    {
        //Restart the game!
        player.GetComponent<PlayerController>().Start();
        map.GetComponent<MapGenerator>().GenerateMap();
    }

    public void exitGame()
    {
        Debug.Log("EXIT");
        Application.Quit();
    }
}
