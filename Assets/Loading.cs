using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("LoadScene", 2f); 
    }
    void LoadScene()
    {
        SceneManager.LoadScene("Fly");
    }

}
