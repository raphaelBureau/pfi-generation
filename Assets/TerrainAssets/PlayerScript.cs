using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] TerrainGeneration TG;
    [SerializeField] float speed = 1f;
    [SerializeField] float lookSens = 3f;
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //clamp rotation min 0 - 40, max 360 - 355
        Quaternion oldRot = camera.transform.rotation;
        camera.transform.Rotate(new Vector3 (-Input.GetAxis("Mouse Y") * lookSens, 0, 0));
        Vector3 newRot = camera.transform.rotation.eulerAngles;
        if(!(newRot.x > 0 && newRot.x < 40 || newRot.x < 360 && newRot.x > 355)) {
            camera.transform.rotation = oldRot;
        }

        speed += Input.mouseScrollDelta.y / 3;

        transform.Rotate(new Vector3 (0, Input.GetAxis("Mouse X") * lookSens,0));

        TG.perlinXOffset += transform.forward.x * speed * Time.deltaTime;
        TG.perlinYOffset += transform.forward.z * speed * Time.deltaTime;
    }
    
}
