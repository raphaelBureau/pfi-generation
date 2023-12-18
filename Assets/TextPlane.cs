using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextPlane : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    bool inside = false;
    void Update()
    {
        if (inside)
        {
            tmp.transform.LookAt(Camera.main.transform); // Le text affichera toujours vers la caméra
            tmp.transform.Rotate(0, 180, 0);
            if (Input.GetKeyDown(KeyCode.E))
            {
                SceneManager.LoadScene("Fly");
                print("loading scene");
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        tmp.gameObject.SetActive(true);
        inside = true;
    }
    private void OnTriggerExit(Collider other)
    {
        tmp.gameObject.SetActive(false);
        inside = false;
    }
}
