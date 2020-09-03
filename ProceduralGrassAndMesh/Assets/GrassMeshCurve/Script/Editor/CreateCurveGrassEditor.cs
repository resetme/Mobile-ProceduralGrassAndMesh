using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[CustomEditor(typeof(CreateCurvedGrass))]
public class CreateCurveGrassEditor : Editor
{
    private CreateCurvedGrass ccg;
    private object _mesh;
    private int _amount; 
    private float _width;
    private float _height;
    private const int MAXAMOUNTVERTICES =  65535; // 16 bits
    private Object material;
    private Object debugMaterial;
    
    public override void OnInspectorGUI()
    {
        ccg = target as CreateCurvedGrass;

       
        material = EditorGUILayout.ObjectField("Default Material: ", ccg.material, typeof(Material), true);
        ccg.material = (Material)material;
        
        Object debugMaterial = EditorGUILayout.ObjectField("Debug Material: ", ccg.debugMaterial, typeof(Material), true);
        ccg.debugMaterial = (Material)debugMaterial;
        
        if (ccg.GetComponentReadyState())
        {
            if (ccg.mf.sharedMesh != null)
            {
                //Memory Size
                GUILayout.BeginVertical("Box"); 
                GUILayout.Label("Mesh Memory Size:" + GetMeshMemorySizeMB(ccg.mf.sharedMesh).ToString() + " MB");
                GUILayout.Label("Vertex Count:" + ccg.mf.sharedMesh.vertexCount);
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, ((ccg.mf.sharedMesh.vertexCount*100f)/MAXAMOUNTVERTICES)/100f, MAXAMOUNTVERTICES/100f + "%");
                EditorGUILayout.EndVertical();
                GUILayout.EndVertical();  
            }
        }

        GUILayout.Space(10);

        if (ccg.meshClusterList.Count > 0)
        {
            for (int i = 0; i < ccg.meshClusterList.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                
                GUILayout.BeginVertical("Box");
                GUILayout.Label("Cluster - " + ccg.meshClusterList[i].clusterType.ToString().ToUpper());
                GUILayout.BeginVertical("Box");

                GUI.backgroundColor = ccg.meshClusterList[i].clusterColor;
                if (GUILayout.Button(ccg.meshClusterList[i].clusterShowProperties ? "Hide Cluster" : "Show Cluster"))
                {
                    CreateCurvedGrass.MeshCluster setcm;
                    setcm = ccg.meshClusterList[i];

                    setcm.clusterShowProperties = !setcm.clusterShowProperties;
                    
                    ccg.meshClusterList[i] = setcm;
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();

               
                if (ccg.meshClusterList[i].clusterShowProperties)
                {
                    GUILayout.BeginVertical("Box");
                    
                    //Delete Cluster
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete Mesh Cluster"))
                    {
                        ccg.meshClusterList.RemoveAt(i);
                        if (ccg.meshClusterList.Count == 0)
                        {
                            DestroyImmediate(ccg.mf.sharedMesh);
                            ccg.mf.sharedMesh = null;
                        }
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                    
                    //Set Values
                    Object mesh = EditorGUILayout.ObjectField("Mesh to Spawn: ", ccg.meshClusterList[i].clusterMesh, typeof(Mesh), true);
                    bool randomRotate =
                        EditorGUILayout.Toggle("Random Rotate:", ccg.meshClusterList[i].clusterRandomRotate);
                    float width = EditorGUILayout.Slider("Area Spawn", ccg.meshClusterList[i].clusterArea, 0, 100);
                    float height = EditorGUILayout.Slider("Height", ccg.meshClusterList[i].clusterHeigth, 0.0f, 25.0f);
                    float randomHeight = EditorGUILayout.Slider("Random Height", ccg.meshClusterList[i].clusterRandomHeightVariation, 0.0f, 25.0f);
                    float scale = EditorGUILayout.Slider("Uniform Scale", ccg.meshClusterList[i].clusterScale, 0.0f, 25.0f);
                    float randomScale = EditorGUILayout.Slider("Uniform Random Scale", ccg.meshClusterList[i].clusterRandomScale, 0.0f, 25.0f);
                    Color col = EditorGUILayout.ColorField("Color", ccg.meshClusterList[i].clusterColor);
                    CreateCurvedGrass.CLUSTER_TYPE clusterType =
                        (CreateCurvedGrass.CLUSTER_TYPE)EditorGUILayout.EnumPopup("Type", ccg.meshClusterList[i].clusterType);
                    
                    //Assign Back
                    CreateCurvedGrass.MeshCluster setcm;
                    setcm.clusterMesh = (Mesh)mesh;
                    setcm.clusterRandomRotate = randomRotate;
                    setcm.clusterArea = width;
                    setcm.clusterHeigth = height;
                    setcm.clusterColor = col;
                    setcm.clusterType = clusterType;
                    setcm.clusterRandomHeightVariation = randomHeight;
                    setcm.clusterScale = scale;
                    setcm.clusterRandomScale = randomScale;
                    setcm.clusterLine = ccg.meshClusterList[i].clusterLine;
                    setcm.clusterShowProperties = ccg.meshClusterList[i].clusterShowProperties;;
                    ccg.meshClusterList[i] = setcm;
                    
                    GUILayout.Space(10);
                    
                    GUILayout.Label("Nodes");
                    for (int j = 0; j < ccg.meshClusterList[i].clusterLine.Count; j++)
                    {
                        CreateCurvedGrass.NodeInfo nf;
        
                        GUILayout.BeginVertical("Box");
                        if (j < ccg.meshClusterList[i].clusterLine.Count - 1)
                        { 
                            Vector3 nodePos = EditorGUILayout.Vector3Field("Node " + j, ccg.meshClusterList[i].clusterLine[j].nodePosition);
                            int nodeScatterAmount = EditorGUILayout.IntSlider("Amount",  ccg.meshClusterList[i].clusterLine[j].scatterAmount, 0, 1000);
                            nf.nodePosition = nodePos;
                            nf.scatterAmount = nodeScatterAmount;
                            ccg.meshClusterList[i].clusterLine[j] = nf;
                        }
                        else
                        {
                            //Last Node
                            Vector3 nodePos = EditorGUILayout.Vector3Field("Close Node " + i, ccg.meshClusterList[i].clusterLine[j].nodePosition);
                            nf.nodePosition = nodePos;
                            nf.scatterAmount = 0;
                            ccg.meshClusterList[i].clusterLine[j] = nf;
                        }
                
                        //Delete node
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Delete Node"))
                        {
                            CreateCurvedGrass.MeshCluster mc;
                            mc = ccg.meshClusterList[i];
                            mc.clusterLine.RemoveAt(j);
                        
                            ccg.meshClusterList[i] = mc;
                        } 
                        GUI.backgroundColor = Color.white;
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
        
                    }
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Insert Node"))
                    {
                        Vector3 nextPosition;
                        //Get next node position from previous direction
                        if (ccg.meshClusterList[i].clusterLine.Count > 2)
                        {
                            Vector3 targetPosition = ccg.meshClusterList[i]
                                .clusterLine[ccg.meshClusterList[i].clusterLine.Count - 1].nodePosition;
                            Vector3 currPosition = ccg.meshClusterList[i]
                                .clusterLine[ccg.meshClusterList[i].clusterLine.Count - 2]
                                .nodePosition;
                            Vector3 direction = targetPosition - currPosition;

                            nextPosition =
                                ccg.meshClusterList[i].clusterLine[ccg.meshClusterList[i].clusterLine.Count - 1]
                                    .nodePosition + (direction.normalized * 2);
                        }
                        else
                        {
                            nextPosition = Vector3.zero;
                        }
         
                        
                        CreateCurvedGrass.MeshCluster mc;
                        mc = ccg.meshClusterList[i];
                        
                        CreateCurvedGrass.NodeInfo ni;
                        ni.nodePosition = nextPosition;
                        ni.scatterAmount = 0;
                        mc.clusterLine.Add(ni);
                        ccg.meshClusterList[i] = mc;
                    }
                    GUI.backgroundColor = Color.white;
                    
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical(); 
                
                
                GUILayout.Space(10);
                
                //
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(ccg);
                    UpdateMesh(i);
                }
            }
        }

        
        if (GUILayout.Button("Create Mesh Cluster"))
        {
            CreateCurvedGrass.MeshCluster mc;
            mc.clusterMesh = null;
            mc.clusterHeigth = 1;
            mc.clusterArea = 1;
            mc.clusterColor = Color.white;;
            mc.clusterType = CreateCurvedGrass.CLUSTER_TYPE.grass;
            mc.clusterLine = new List<CreateCurvedGrass.NodeInfo>();
            mc.clusterRandomRotate = true;
            mc.clusterShowProperties = true;
            mc.clusterRandomHeightVariation = 1f;
            mc.clusterScale = 1f;
            mc.clusterRandomScale = 1f;
            ccg.meshClusterList.Add(mc);
        }
        
        //
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("DEBUG");
        if (GUILayout.Button("Toggle Debug Vertex"))
        {
            showVertexInfo = !showVertexInfo;
            ToggleDebugMaterial();
        }
        
        if (GUILayout.Button("Toggle Vertex Color"))
        {
            toggleUvVertexColor = !toggleUvVertexColor;
            ToggleVertexColor();
        }
        EditorGUILayout.EndVertical();
    }
    
    private void OnSceneGUI()
    {
        ccg = target as CreateCurvedGrass;

        float infoHeightOffset = 2f;

        for (int i = 0; i < ccg.meshClusterList.Count; i++)
        {
            Handles.color = ccg.meshClusterList[i].clusterColor;
            Vector3[] linePosition = new Vector3[ccg.meshClusterList[i].clusterLine.Count]; 
            
            if (ccg.meshClusterList[i].clusterLine.Count >= 2)
            {
                for (int j = 0; j < ccg.meshClusterList[i].clusterLine.Count; j++)
                {
                    linePosition[j] = (ccg.meshClusterList[i].clusterLine[j].nodePosition + ccg.transform.position); 
                    Vector3 labelpos = linePosition[j] ;
                    labelpos.y += ccg.meshClusterList[i].clusterHeigth + infoHeightOffset;
                    Handles.Label(labelpos,  ccg.meshClusterList[i].clusterType + " Cluster: " + i + " / Node: " + j);
                    
                    EditorGUI.BeginChangeCheck();
                    Vector3 p = Handles.DoPositionHandle(linePosition[j], Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        CreateCurvedGrass.NodeInfo nf;
                        nf.nodePosition = (p - ccg.transform.position);
                        nf.scatterAmount = ccg.meshClusterList[i].clusterLine[j].scatterAmount;
                   
                        Undo.RecordObject(ccg, "Move Point");
                        EditorUtility.SetDirty(ccg);
                        ccg.meshClusterList[i].clusterLine[j] = nf;
                        UpdateMesh(i);  
                    }
                    
                    //Direction
                    if (j < ccg.meshClusterList[i].clusterLine.Count - 1)
                    {
                        Vector3 direction = ccg.meshClusterList[i].clusterLine[j+1].nodePosition - ccg.meshClusterList[i].clusterLine[j].nodePosition;
                        Vector3 startRayPos = ccg.meshClusterList[i].clusterLine[j].nodePosition;
                        startRayPos.y += ccg.meshClusterList[i].clusterHeigth + infoHeightOffset;
                        //Debug.DrawRay(startRayPos, direction * 0.1f, Color.green);  
                        Quaternion rot =
                            Quaternion.LookRotation(Vector3.RotateTowards(Vector3.forward, direction.normalized, Mathf.PI,
                                0));
                        Handles.ConeHandleCap(0, startRayPos + ccg.transform.position, rot, 0.5f, EventType.Repaint);
                    }
                }
            }
            
            Handles.DrawAAPolyLine(5f, linePosition);
        }

        if (!ccg.GetComponentReadyState())
            return;

        if (ccg.mf.sharedMesh != null)
        {
            Handles.color = new Color(1,1,1,0.2f);
            Handles.Label(ccg.mf.sharedMesh.bounds.center, "DrawCall: " + ccg.gameObject.name);
            Handles.DrawWireCube(ccg.mf.sharedMesh.bounds.center + ccg.transform.position, ccg.mf.sharedMesh.bounds.size);
        }
    }

    private void UpdateMesh(int id)
    {
        if(ccg.meshClusterList[id].clusterMesh != null)
            ccg.UpdateBake(); 
    }
    
    float GetMeshMemorySizeMB(Mesh mesh)
    {
        return (Profiler.GetRuntimeMemorySizeLong(mesh) / 1024f) / 1024f;
    }
    
    private bool showVertexInfo = false;
    private bool toggleUvVertexColor = false;

    public void ToggleDebugMaterial()
    {
        ccg.mr.sharedMaterial = showVertexInfo ? ccg.material : ccg.debugMaterial;
    }

    public void ToggleVertexColor()
    {
        ccg.debugMaterial.SetFloat("_DebugUVVertexColor", toggleUvVertexColor ? 0 : 1);
    }
}
