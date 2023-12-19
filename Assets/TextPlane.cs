using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextPlane : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] Camera cam;
    bool inside = false;
    bool move = false;
    public static bool isMoving = false;
    float speed = 10;

    void Start()
    {
        Camera.main.enabled = true;
        cam.enabled = false;
    }

    void Update()
    {
        if (inside)
        {
            tmp.transform.LookAt(Camera.main.transform);
            tmp.transform.Rotate(0, 180, 0);
            if (Input.GetKeyDown(KeyCode.E))
            {
                move = true;
                isMoving = true;
                cam.enabled = true;
                Invoke("LoadScene", 5f);
            }
        }

        if (move)
        {
            transform.Translate(transform.forward * Time.deltaTime * speed);
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

    void LoadScene()
    {
        SceneManager.LoadScene("Load");
    }
}
