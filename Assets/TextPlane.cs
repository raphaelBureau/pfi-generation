using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPlane : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform); // Le text affichera toujours vers la caméra
        transform.Rotate(0, 180, 0);
    }
}
