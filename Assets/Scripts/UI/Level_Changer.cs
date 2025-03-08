using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Level_Changer : MonoBehaviour
{

    [SerializeField]
    private string sceneName; // Scene to load, assign in the Inspector
    public bool lastHitToBoss;



    private void Update()
    {
        
    if (lastHitToBoss == true)
        {

            LoadScene(sceneName);

        }


    }


    public void LoadScene(string scene)
    {
        if (!string.IsNullOrEmpty(scene))
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.LogError("Scene name is not assigned!");
        }
    }



    

}
