using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class startbutton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gameStart()
    {
        // Only specifying the sceneName 
        SceneManager.LoadScene("Level1");
    }
    
    public void exit()
    {
        // Only specifying the sceneName 
        Debug.Log("Exit");
        Application.Quit();
    }
}