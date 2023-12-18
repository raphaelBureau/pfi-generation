using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGeneration : MonoBehaviour
{
    MeshFilter mf;
    MeshRenderer mr;
    [Range(5, 800)]
    [SerializeField] int segments = 440; //number of verticies in a row, total vertex count = segments * segments
    [Range(0.1f,90)]
    [SerializeField] float size = 20;
    [Range(-20f, 30)]
    [SerializeField] float amplitude = 2.5f;
    [Range(0.01f, 200)]
    [SerializeField] float perlinScale = 150;
    [SerializeField] public float perlinXOffset = 0.01f;
    [SerializeField] public float perlinYOffset = 0.01f;
    [Range(-0.1f, 0.1f)]
    [SerializeField] float waterY = 0;
    [Range(0, 0.02f)]
    [SerializeField] float waterAmplitude = 0.002f;
    [Range(0, 200)]
    [SerializeField] float waveLength = 90;
    [Range(-5, 5)]
    [SerializeField] float waveSpeed = 5f;
    [Range(0.00001f, 5)]
    [SerializeField] float waterToTerrainMargin = -0.00001f;
    [Range(-1,1)]
    [SerializeField] float snowY = 0.4f;
    [Range(-1, 1)]
    [SerializeField] float stoneY = 0.1f;
    [Range(-1, 5)]
    [SerializeField] float lavaY = 5f;
    [Range(0.1f, 5)]
    [SerializeField] float lavaDepth = 2f;
    [Range(1, 10)]
    [SerializeField] int FractalBrownianOctaves = 5;
    [Range(0, 100)]
    [SerializeField] int cullingSkips = 15;
    [Range(0, 1)]
    [SerializeField] float cullingMargin = 0.5f;
    [SerializeField] Camera Camera;

    Mesh mesh;
    float waterOffset = 0;
    // Start is called before the first frame update
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mesh = mf.mesh;
    }
    float WaterSine(float x)
    {
        return Mathf.Sin(x * waveLength/size + waterOffset) * waterAmplitude * size;
    }
    bool AboveStone(float y)
    {
        return y > stoneY * 3;
    }
    bool AboveSnow(float y)
    {
        return y > snowY * 3;
    }
    bool AboveLava(float y)
    {
        return y >= lavaY * 3;
    }
    bool InScreen(Vector3 point)
    {
        var vpPos = Camera.WorldToViewportPoint(point + transform.position);
        return vpPos.x >= 0 - cullingMargin && vpPos.x <= 1 + cullingMargin && vpPos.y >= 0 - cullingMargin && vpPos.y <= 1 + cullingMargin && vpPos.z >= 0 - cullingMargin;
    }
    float FBMotion(float x, float y)
    {
        float res = 0f;
        float amp = 1f;
        float frequency = 0.05f;
        for(int o = 0; o< FractalBrownianOctaves; o++)
        {
            res += amp * (Mathf.PerlinNoise((float)(x / segments * perlinScale + perlinXOffset) * frequency,(float)(y / segments * perlinScale + perlinYOffset) * frequency) - 0.5f);
            amp = amp * 0.5f;
            frequency = frequency * 2;
        }
        return res;
    }
    void AddPoly(List<int> mesh, int v1, int v2, int v3)
    {
        mesh.Add(v1);
        mesh.Add(v2);
        mesh.Add(v3);
    }
    Vector3 GenY(Vector3 vert, float x, float y)
    {
        vert.y = FBMotion(x, y) * amplitude;
        float wY = waterY * size + WaterSine(vert.x);
        if (vert.y < wY) //check if terrain is above water level
        { //terrain generation
            vert.y = wY;
        }
        return vert;
    }

    // Update is called once per frame
    void Update()
    {
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.subMeshCount = 5;
        waterOffset += Time.deltaTime * waveSpeed;
        transform.position = new Vector3(-size / 2, 0, -size / 2); //center at 0,0,0

        Vector3[] verts = new Vector3[segments * segments];
        int index = 0;
        for (int x = 0; x < segments; x++)
        {
            for (int z = 0; z < segments; z++)
            {
                float y = 0; //perlin noise calculated only when not culled
                float xPos = x * size / segments;
                float zPos = z * size / segments;
                verts[index] = new Vector3(xPos, y, zPos);
                index++;
            }
        }
        List<int> waterMesh = new List<int>();//submesh instanciation, possible memory optimisations to be made
        List<int> terrainMesh = new List<int>();
        List<int> snowMesh = new List<int>();
        List<int> stoneMesh = new List<int>();
        List<int> lavaMesh = new List<int>();

        int CheckSkip;
        bool culled = false;

        for (int x = 0; x < segments - 1; x++) //-1 to skip end tiles
        {
            CheckSkip = 0;
            for (int z = 0; z < segments - 1; z++)
            {
            
                int v1 = x * segments + z; //shared
                if (verts[v1].y == 0) {
                    verts[v1] = GenY(verts[v1], x, z);
                }
                if (CheckSkip <= 0)//culling
                {
                    culled = !InScreen(verts[v1]);
                    CheckSkip = cullingSkips;
                }
                CheckSkip--;
                if (!culled) 
                {

                    int v2 = x * segments + z + segments + 1; //shared
                    if (verts[v2].y == 0)
                    {
                        verts[v2] = GenY(verts[v2], x, z);
                    }
                    int v3 = x * segments + z + segments;//unique
                    if (verts[v3].y == 0)
                    {
                        verts[v3] = GenY(verts[v3], x, z);
                    }
                    int v4 = x * segments + z + 1;//unique
                    if (verts[v4].y == 0)
                    {
                        verts[v4] = GenY(verts[v4], x, z);
                    }
                    //first polygon of square face
                    //compare polygon height and add to either water or terrain submesh
                    //mesh neighboor is equal to (index + segments)
                    float wSS = WaterSine(verts[v1].x) + WaterSine(verts[v2].x); //water shared sine
                    float f1Y = verts[v1].y + verts[v2].y + verts[v3].y;
                    if (f1Y
                        <= waterY * size * 3 + wSS + WaterSine(verts[v3].x) + waterToTerrainMargin)
                    {
                        AddPoly(waterMesh, v1, v2, v3);
                    }
                    else
                    {
                        if (AboveStone(f1Y))
                        {
                            if (AboveSnow(f1Y))
                            {
                                if (AboveLava(f1Y))
                                {
                                    verts[v1].y += (lavaY - verts[v1].y) * lavaDepth ;
                                    AddPoly(lavaMesh, v1, v2, v3);
                                }
                                else
                                {
                                    AddPoly(snowMesh, v1, v2, v3);
                                }
                            }
                            else
                            {
                                AddPoly(stoneMesh, v1, v2, v3);
                            }
                        }
                        else
                        {
                            AddPoly(terrainMesh, v1, v2, v3);
                        }
                    }
                    //2nd polygon of square face
                    float f2Y = verts[v1].y + verts[v4].y + verts[v2].y;
                    if (f2Y
                        <= waterY * size * 3 + wSS + WaterSine(verts[v4].x) + waterToTerrainMargin)
                    {
                        AddPoly(waterMesh, v1, v4, v2);
                    }
                    else
                    {
                        if (AboveStone(f2Y))
                        {
                            if (AboveSnow(f2Y))
                            {
                                if (AboveLava(f2Y))
                                {
                                   
                                    AddPoly(lavaMesh, v1, v4, v2);
                                }
                                else
                                {
                                    AddPoly(snowMesh, v1, v4, v2);
                                }
                            }
                            else
                            {
                                AddPoly(stoneMesh, v1, v4, v2);
                            }
                        }
                        else
                        {
                            AddPoly(terrainMesh, v1, v4, v2);
                        }
                    }
                }
            }
        }
        //mesh.triangles = faces;
        mesh.vertices = verts;
        mesh.SetTriangles(terrainMesh, 0);
        mesh.SetTriangles(waterMesh, 1);
        mesh.SetTriangles(snowMesh, 2);
        mesh.SetTriangles(stoneMesh, 3);
        mesh.SetTriangles(lavaMesh, 4);
        mesh.RecalculateNormals();
        mf.mesh = mesh;

    }

}
