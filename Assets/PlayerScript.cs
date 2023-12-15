using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] TerrainGeneration TG;
    [SerializeField] float speed = 1f;
    [SerializeField] float lookSens = 3f;
    Vector3 mPos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        mPos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        var mDelta = Input.mousePosition - mPos;
        mPos = Input.mousePosition;
        var rot = transform.rotation.eulerAngles;
        transform.rotation.eulerAngles.Set(rot.x + mDelta.y * lookSens, rot.y + mDelta.x * lookSens, 0);
        print(new Vector3(rot.x + mDelta.y * lookSens, rot.y + mDelta.x * lookSens, 0));

        TG.perlinXOffset += transform.forward.x * speed * Time.deltaTime;
        TG.perlinYOffset += transform.forward.z * speed * Time.deltaTime;
    }
    
}
