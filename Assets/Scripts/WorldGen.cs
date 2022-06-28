using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WorldGen : MonoBehaviour
{
    public Texture2D _map;
    public Material matTerrain;
    public GameObject preCheckpoint;
    [Range(0, 1_000_000)]
    public int seed;

    [Header("Cache")]
    public Texture2D cacheMap;
    public GameObject cacheTerrain;

    // Start is called before the first frame update
    void Start()
    {
        var map = GenerateMap();
        
        matTerrain.SetTexture("Texture2D_a1b76558a8f644b79097b0834bf16d27", map);

        if (cacheTerrain != null)
        {
            Instantiate(cacheTerrain);
            GetComponent<ObjectManager>().Init(_map, map);
            return;
        }

        float time = Time.realtimeSinceStartup;

        var _parent = new GameObject("Env");
        int chunkSize = 8;


        for (int yy = 0; yy < chunkSize; yy++)
            for (int xx = 0; xx < chunkSize; xx++)
            {
                var parent = new GameObject($"Chunk{xx}{yy}");
                parent.layer = 6;
                parent.transform.parent = _parent.transform;
                var mesh = new Mesh();

                int chunkSizeX = 512 / chunkSize, chunkSizeY = 512 / chunkSize;

                List<Vector3> vert = new List<Vector3>();
                List<int> tri = new List<int>();

                int _x(int x, int y) => (x + xx * chunkSizeX) * 6 + y % 2 * 3;
                int _y(int y) => (y + yy * chunkSizeY) * 5;
                float height(int x, int y) => (1 - map.GetPixel(_x(x, y), _y(y)).a) * 1000;
                int index = 0;

                for (int y = 0; y < chunkSizeY; y++)
                    for (int x = 0; x < chunkSizeX; x++)
                    {
                        vert.AddRange(new[] { new Vector3(_x(x, y + 1), height(x, y + 1), _y(y + 1)), new Vector3(_x(x + 1, y), height(x + 1, y), _y(y)), new Vector3(_x(x, y), height(x, y), _y(y)) });
                        tri.AddRange(new[] { index++, index++, index++ });
                    }

                for (int y = 1; y <= chunkSizeY; y++)
                    for (int x = 0; x < chunkSizeX; x++)
                    {
                        vert.AddRange(new[] { new Vector3(_x(x + 1, y - 1), height(x + 1, y - 1), _y(y - 1)), new Vector3(_x(x, y), height(x, y), _y(y)), new Vector3(_x(x + 1, y), height(x + 1, y), _y(y)) });
                        tri.AddRange(new[] { index++, index++, index++ });
                }

                mesh.vertices = vert.ToArray();
                mesh.triangles = tri.ToArray();
                //mesh.Optimize();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                //mesh.ClearBlendShapes();
                mesh.RecalculateTangents();
                parent.AddComponent<MeshFilter>().mesh = mesh;
                parent.AddComponent<MeshRenderer>().material = matTerrain;
                parent.AddComponent<MeshCollider>().sharedMesh = mesh;
                Directory.CreateDirectory(Path.Combine("Assets", "Cache", "Mesh", seed.ToString()));
                UnityEditor.AssetDatabase.CreateAsset(mesh, Path.Combine("Assets", "Cache", "Mesh", seed.ToString(), mesh.GetInstanceID() + ".asset"));

                if (xx % 2 != yy % 2) continue;

                float offsetCenter = 1f / chunkSize / 2f;
                float offsetChunkX = 1f / chunkSize * xx, offsetChunkY = 1f / chunkSize * yy;
                int c = 50, b = 10;
                for (int i = 0; i < 100; i++)
                {
                    float ux = (c + b * i) * Mathf.Cos(i) / 2000f / chunkSize + offsetCenter + offsetChunkX, uy = (c + b * i) * Mathf.Sin(i) / 2000f / chunkSize + offsetCenter + offsetChunkY;
                    Color color = _map.GetPixelBilinear(ux, uy);
                    if (color.a != 1) continue;
                    var go = Instantiate(preCheckpoint, parent.transform, false);
                    Debug.Log($"{ux}/{uy} = {ux * chunkSizeX}/{uy * chunkSizeY}");
                    go.transform.localPosition = new Vector3(ux * _map.width, (1 - map.GetPixelBilinear(ux, uy).a) * 1000, uy * _map.height);
                    break;
                }
            }
        _parent.transform.localScale = Vector3.one * 0.2f;
        Directory.CreateDirectory(Path.Combine("Assets", "Cache", "Prefab"));
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(_parent, $"Assets/Cache/Prefab/{seed}.prefab");
        Debug.LogWarning($"No cache provided for: Terrain. Generation took {Time.realtimeSinceStartup - time} seconds. Please consider supplying cached terrain from the Cache directory.");
        GetComponent<ObjectManager>().Init(_map, map);
    }

    ///https://easings.net/
    float EaseInCirc(float x) => 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));

    float EaseOutCirc(float x)
    {
        x = Mathf.Clamp(x, 0, 1);
        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
    }

    float EaseInBounce(float x) => 1 - EaseOutBounce(1 - x);

    float EaseOutBounce(float x)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }

    }

    float EaseInOutBounce(float x) => x < 0.5 ? (1 - EaseOutBounce(1 - 2 * x)) / 2f : (1 + EaseOutBounce(2 * x - 1)) / 2f;

    Texture2D GenerateMap()
    {
        if (cacheMap != null) return cacheMap;
        float time = Time.realtimeSinceStartup;
        Texture2D map = new Texture2D(_map.width, _map.height);
        float xx = map.width, yy = map.height;
        for (int x = 0; x < xx; x++)
            for (int y = 0; y < yy; y++)
            {
                float height = (x / xx + y / yy) / 2f;
                Color c = _map.GetPixel(x, y);
                c.a *= 1 - height;
                map.SetPixel(x, y, c);
            }
        map.Apply();
        map = Blur.FasterBlur(map, 64, 4);
        File.WriteAllBytes("test0.png", map.EncodeToPNG());

        for (int x = 0; x < xx; x++)
            for (int y = 0; y < yy; y++)
            {
                float height = 1 - (x / xx + y / yy) / 2f;
                Color c = map.GetPixel(x, y);
                c.a = Mathf.Lerp((EaseInBounce(Mathf.PerlinNoise(x / xx * 8f + seed, y / yy * 8f + seed)) * 4 + EaseOutCirc(Mathf.PerlinNoise(x / xx * 25f, y / yy * 25f)) + height * 20) / 25f, 1, c.a);
                map.SetPixel(x, y, c);
            }
        map.Apply();

        Directory.CreateDirectory(Path.Combine("Assets", "Cache", "Sprite"));
        File.WriteAllBytes($"Assets/Cache/Sprite/{seed}.png", map.EncodeToPNG());
        Debug.LogWarning($"No cache provided for: Map. Generation took {Time.realtimeSinceStartup - time} seconds. Please consider supplying cached map from the Cache directory.");
        return map;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*private void OnDrawGizmos()
    {
        int chunkSize = 8;
        for (int yy = 0; yy < chunkSize; yy++)
            for (int xx = 0; xx < chunkSize; xx++)
            {
                int chunkSizeX = 512 / chunkSize, chunkSizeY = 512 / chunkSize;
                float offsetCenter = 1f / chunkSize / 2f;
                float offsetChunkX = 1f / chunkSize * xx, offsetChunkY = 1f / chunkSize * yy;
                int c = 50, b = 10;
                for (int i = 0; i < 100; i++)
                {
                    float ux = (c + b * i) * Mathf.Cos(i) / 2000f / chunkSize + offsetCenter + offsetChunkX, uy = (c + b * i) * Mathf.Sin(i) / 2000f / chunkSize + offsetCenter + offsetChunkY;
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(new Vector3(ux * _map.width, 0, uy * _map.height), 1);
                    //Color color = _map.GetPixelBilinear(ux, uy);
                    //if (color.a != 1) continue;
                    //var go = Instantiate(preCheckpoint, parent.transform, false);
                    //Debug.Log($"{ux}/{uy} = {ux * chunkSizeX}/{uy * chunkSizeY}");
                    //go.transform.localPosition = new Vector3(ux * _map.width, (1 - map.GetPixelBilinear(ux, uy).a) * 1000, uy * _map.height);
                }
            }
    }*/
}
