using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenSelector : MonoBehaviour
{
    public void playGame()
    {
        SceneManager.LoadScene("Subwoofer"); 
    }
}
