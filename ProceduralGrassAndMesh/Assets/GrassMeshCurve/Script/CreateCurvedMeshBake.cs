using System.Collections.Generic;
using UnityEngine;

public static class CreateCurvedMeshBake
{
    public static Mesh BakedMesh(CreateCurvedGrass.MeshCluster cluster, int maxAllowedVertices)
    {
        Mesh baseMesh = cluster.clusterMesh;
        float clusterArea = cluster.clusterArea;
        float clusterHeight = cluster.clusterHeigth;
        float clusterRandomHeightVariation = cluster.clusterRandomHeightVariation;
        float clusterScale = cluster.clusterScale;
        float clusterRandomScale = cluster.clusterRandomScale;
        Color clusterColor = cluster.clusterColor;
       
        CreateCurvedGrass.CLUSTER_TYPE clusterType = cluster.clusterType;
        List<CreateCurvedGrass.NodeInfo> nodes = cluster.clusterLine;

        if (baseMesh.vertexCount > maxAllowedVertices)
        {
            Debug.LogError("Select a smaller mesh!");
            return null;
        }

        //Create Mesh
        Mesh mesh = new Mesh();
        //Amount
        int nodesCount = nodes.Count;
        int scatterMaxAmount = 0;

        for (int i = 0; i < nodesCount - 1; i++)
        {
            scatterMaxAmount += nodes[i].scatterAmount;
        }

        int amount = scatterMaxAmount;

        //Line Information Position
        List<Vector3> linePositionScatter = new List<Vector3>();

        int arrayIndex = 0;
        int nodeIndex = 0;

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = Vector3.Lerp(nodes[arrayIndex].nodePosition, nodes[arrayIndex + 1].nodePosition,
                (float) nodeIndex / nodes[arrayIndex].scatterAmount);
            linePositionScatter.Add(pos);
            nodeIndex++;

            if (nodes[arrayIndex].scatterAmount == 0)
            {
                nodeIndex = 0;

                if (arrayIndex < nodesCount - 2)
                {
                    arrayIndex++;
                }
            }
            else if (nodeIndex % nodes[arrayIndex].scatterAmount == 0)
            {
                nodeIndex = 0;

                if (arrayIndex < nodesCount - 2)
                {
                    arrayIndex++;
                }
            }
        }

        //Last one should be in last node
        if (linePositionScatter.Count > 2)
        {
            linePositionScatter[linePositionScatter.Count - 1] = nodes[nodes.Count - 1].nodePosition; 
        }

        //Vertices
        Vector3[] vertices = new Vector3[baseMesh.vertices.Length * amount];
        Vector3[] baseVertices = baseMesh.vertices;
        int baseVerticesIndex = 0;

        //Random Height
        float randomHeight = Random.Range(1f, clusterRandomHeightVariation);
        
        //Random Scale
        float randomScale = Random.Range(1f, clusterRandomScale);

        //Vertex ID and Pivot Point
        Vector4[] tangents = new Vector4[baseMesh.vertices.Length * amount];

        //Area Position
        float randonXPosision = 0;
        float randonZPosision = 0;
        float width = clusterArea;
       
        //Rotation
        float rotYRandom = Random.Range(-90f, 90f);
        float rotXRandom = Random.Range(-90f, 90f);
        float rotZRandom = Random.Range(-90f, 90f);
        
        arrayIndex = 0;
        

        for (int i = 0; i < vertices.Length; i++)
        {
            //Copy base vertices position
            vertices[i] = baseVertices[baseVerticesIndex];
            Vector3 vertexPosition = vertices[i];

            //Rotation
            if (cluster.clusterRandomRotate)
            {
                switch (cluster.clusterType)
                {
                    case CreateCurvedGrass.CLUSTER_TYPE.grass:
                        //Rotate Y only
                        vertexPosition = Quaternion.Euler(0, rotYRandom, 0) * vertexPosition;
                        break;
                    case CreateCurvedGrass.CLUSTER_TYPE.particle:
                        //Rotate All
                        vertexPosition = Quaternion.Euler(rotXRandom, rotYRandom, rotZRandom) * vertexPosition;
                        break;
                    case CreateCurvedGrass.CLUSTER_TYPE.fixedObject:
                        vertexPosition = Quaternion.Euler(0, rotYRandom, 0) * vertexPosition;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Try to Follow Line, im bad with quaternions
                if (arrayIndex < linePositionScatter.Count - 1)
                {
                    Vector3 direction = (linePositionScatter[arrayIndex + 1] - linePositionScatter[arrayIndex]);
                    Quaternion rot =
                        Quaternion.LookRotation(Vector3.RotateTowards(Vector3.forward, direction.normalized, Mathf.PI,
                            0));
                    vertexPosition = rot * vertexPosition;
                }
                else if (arrayIndex >= linePositionScatter.Count - 1)
                {
                    //Fix Last object
                    Vector3 direction = (linePositionScatter[arrayIndex] - linePositionScatter[arrayIndex - 1]);
                    Quaternion rot =
                        Quaternion.LookRotation(Vector3.RotateTowards(Vector3.forward, direction.normalized, Mathf.PI,
                            0));
                    vertexPosition = rot * vertexPosition;
                }
            }

            //Scale
            switch (cluster.clusterType)
            {
                case CreateCurvedGrass.CLUSTER_TYPE.grass:
                    //Add Random height
                    vertexPosition *= clusterScale;
                    vertexPosition *= randomScale;
                    vertexPosition.y *= clusterHeight;
                    vertexPosition.y *= randomHeight;
                    break;
                case CreateCurvedGrass.CLUSTER_TYPE.particle:
                    //Fall
                   
                case CreateCurvedGrass.CLUSTER_TYPE.particleFall:
                    //Add random Scale All
                    vertexPosition *= clusterScale;
                    vertexPosition *= randomScale;
                    vertexPosition.y *= clusterHeight;
                    vertexPosition.y *= randomHeight;
                    break;
                case CreateCurvedGrass.CLUSTER_TYPE.fixedObject:
                    //Add Fixed Value
                    vertexPosition *= clusterScale;
                    vertexPosition *= randomScale;
                    vertexPosition.y *= clusterHeight;
                    vertexPosition.y *= randomHeight;
                    break;
                default:
                    break;
            }

            //Position
            vertexPosition.x += (linePositionScatter[arrayIndex].x + randonXPosision);
            vertexPosition.y += (linePositionScatter[arrayIndex].y);
            vertexPosition.z += (linePositionScatter[arrayIndex].z + randonZPosision);

            //Set
            vertices[i].x = vertexPosition.x;
            vertices[i].y = vertexPosition.y;
            vertices[i].z = vertexPosition.z;

            //Pivot ID
            tangents[i] = new Vector4((int) clusterType, (linePositionScatter[arrayIndex].x + randonXPosision),
                (linePositionScatter[arrayIndex].y), (linePositionScatter[arrayIndex].z + randonZPosision));

            baseVerticesIndex++;

            //Set Next batch
            if (baseVerticesIndex % baseMesh.vertices.Length == 0)
            {
                //Height Variation
                randomHeight = Random.Range(1f, clusterRandomHeightVariation);
                
                //Scale Variation
                randomScale = Random.Range(1f, clusterRandomScale);

                //Set new random position and rotation for next batch
                randonXPosision = Random.Range(-width / 2f, width / 2f);
                randonZPosision = Random.Range(-width / 2f, width / 2f);

                //Rotation for next Batch
                if (cluster.clusterRandomRotate)
                {
                    rotXRandom = Random.Range(-90f, 90f);
                    rotYRandom = Random.Range(-90f, 90f);
                    rotZRandom = Random.Range(-90f, 90f);
                }

                baseVerticesIndex = 0;
                arrayIndex++;
            }
        }

        mesh.vertices = vertices;

        // 4 triangles * 3 vertices (Make a triangles) = 12
        int[] triangles = new int[baseMesh.triangles.Length * amount];
        int[] baseTriangles = baseMesh.triangles;
        int baseTriangleIndex = 0;

        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = baseTriangles[baseTriangleIndex];

            baseTriangleIndex++;
            if (baseTriangleIndex % baseTriangles.Length == 0)
            {
                baseTriangleIndex = 0;
                for (int j = 0; j < baseTriangles.Length; j++)
                {
                    baseTriangles[j] += baseMesh.vertexCount;
                }
            }
        }

        mesh.triangles = triangles;

        //UV
        Vector2[] uvs = new Vector2[baseMesh.vertices.Length * amount];
        Vector2[] baseUV = baseMesh.uv;
        int baseUvsIndex = 0;
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = baseUV[baseUvsIndex];

            baseUvsIndex++;

            if (baseUvsIndex % baseMesh.vertices.Length == 0)
            {
                baseUvsIndex = 0;
            }
        }

        mesh.uv = uvs;

        //Normals
        Vector3[] normals = new Vector3[baseMesh.vertices.Length * amount];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }

        mesh.normals = normals;

        //Vertex Color
        float blueRandom = Random.Range(0f, 1f);
        Color[] vertexFinalColor;

        //Particle
        Color[] vertexColor = new Color[baseMesh.vertexCount * amount];
        int baseVertexColorIndex = 0;
        for (int i = 0; i < vertexColor.Length; i++)
        {
            vertexColor[i].r = clusterColor.r;
            vertexColor[i].g = clusterColor.g;
            vertexColor[i].b = clusterColor.b;
            vertexColor[i].a = blueRandom; // A is random offset

            baseVertexColorIndex++;

            if (baseVertexColorIndex % baseMesh.vertexCount == 0)
            {
                blueRandom = Random.Range(0f, 1f);
                baseVertexColorIndex = 0;
            }
        }

        vertexFinalColor = vertexColor;

        mesh.colors = vertexFinalColor;

        //Use Tangents for ID and Pivot
        mesh.tangents = tangents;

        return mesh;
    }
}
