using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainCameraMovemtn : MonoBehaviour
{
    private Transform _cameraTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        
        _cameraTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        float xPos = _cameraTransform.position.x + 2 + Time.deltaTime;
        Vector3 pos = new Vector3(xPos, _cameraTransform.position.y ,_cameraTransform.position.z);
        _cameraTransform.position = pos;
    }
}
