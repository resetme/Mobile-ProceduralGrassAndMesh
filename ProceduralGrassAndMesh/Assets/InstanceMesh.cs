using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceMesh : MonoBehaviour
{
    public GameObject go;
    public int copyAmout = 100;
    
    private bool _instanceMesh = false;
    private Material _material;
    private Mesh _mesh;
    private Matrix4x4[] _matrix4x4;
    
    void Start()
    {
        if (go.gameObject != null)
        {
            _material = go.GetComponent<MeshRenderer>().sharedMaterial;
            _mesh = go.GetComponent<MeshFilter>().sharedMesh;

            FillMatrix(); 

            _instanceMesh = true;
        }
    }

    void FillMatrix()
    {
        _matrix4x4 = new Matrix4x4[copyAmout];

        for (int i = 0; i < copyAmout; i++)
        {
            Vector3 position = new Vector3(i * 20, 0, 0);
            _matrix4x4[i].SetTRS(position, Quaternion.identity, Vector3.one);
        }
        
    }
    void Update()
    {
        if (!_instanceMesh)
            return;


        Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrix4x4, copyAmout);
    }
}
