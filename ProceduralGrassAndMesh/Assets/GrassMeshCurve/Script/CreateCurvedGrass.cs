using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateCurvedGrass : MonoBehaviour
{
    private readonly int _maxAllowedVerticesPerMesh = 900; //Better not to change
    private bool _componentReady = false;
    
    public Material material;
    public Material debugMaterial;
    public List<MeshCluster> meshClusterList = new List<MeshCluster>();

    public MeshFilter mf;
    public MeshRenderer mr;

    public enum CLUSTER_TYPE
    {
        grass, particle, particleFall, fixedObject
    }
    
    [System.Serializable]
    public struct MeshCluster
    {
        //public string clusterName;
        public bool clusterShowProperties;
        public List<NodeInfo> clusterLine;
        public Color clusterColor;
        public Mesh clusterMesh;
        public float clusterArea;
        public float clusterScale;
        public float clusterRandomScale;
        public float clusterHeigth;
        public CLUSTER_TYPE clusterType;
        public bool clusterRandomRotate;
        public float clusterRandomHeightVariation;
    }
    
    [System.Serializable]
    public  struct NodeInfo
    {
        public Vector3 nodePosition;
        public int scatterAmount;
    }

    private void Awake()
    {
        SetupComponent();
    }

    private void SetupComponent()
    {
        mf = GetComponent<MeshFilter>();
        if (!mf)
        {
            mf = gameObject.AddComponent<MeshFilter>();
        }

        mr = GetComponent<MeshRenderer>();
        if (!mr)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }

        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.lightProbeUsage = LightProbeUsage.Off;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        
        _componentReady = true;
    }

    public bool GetComponentReadyState()
    {
        return _componentReady;
    }
    
    public void UpdateBake()
    {
        if (!_componentReady)
        {
            SetupComponent(); 
        }
        else
        {
            CreateMesh();
        }
    }

    public void CreateMesh()
    {
        if (meshClusterList.Count > 0)
        {
            //Combine Clusters to one Area
            CombineInstance[] combine = new CombineInstance[meshClusterList.Count];

            Transform cache = gameObject.transform;
            Vector3 pos = cache.position;
            Quaternion rot = cache.rotation;
            
            for (int i = 0; i < meshClusterList.Count; i++)
            {
                int type = (int)meshClusterList[i].clusterType;
                combine[i].mesh = CreateCurvedMeshBake.BakedMesh(meshClusterList[i], _maxAllowedVerticesPerMesh);
                
                cache.transform.position = Vector3.zero;;
                combine[i].transform = cache.localToWorldMatrix;
            }

            gameObject.transform.SetPositionAndRotation(pos, rot);

            //Assign and Combine
            Mesh bakedMesh = new Mesh();;
            bakedMesh.name = "Baked Mesh";
            bakedMesh.CombineMeshes(combine);
            
            mf.sharedMesh = bakedMesh;
            mr.sharedMaterial = material;
        }
    }
}
