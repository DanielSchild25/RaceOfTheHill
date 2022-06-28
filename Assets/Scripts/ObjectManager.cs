using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T GetRandom<T>(this T[] array)
    {
        return array[Mathf.Clamp((int)(Random.value * array.Length), 0, array.Length - 1)];
    }

    public static T GetRandom<T>(this List<T> list)
    {
        return list[Mathf.Clamp((int)(Random.value * list.Count), 0, list.Count - 1)];
    }
}

public class ObjectManager : MonoBehaviour
{

    List<Vector2Int> spawnPoints = new List<Vector2Int>();
    Texture2D map;
    public GameObject[] objects;

    public void Init(Texture2D map, Texture2D map_)
    {
        this.map = map;
        for (int x = 0; x < map.width; x += 10)
            for (int y = 0; y < map.height; y += 10)
            {
                if (map.GetPixel(x, y).a < 1) continue;
                if (x > 10 && map.GetPixel(x - 10, y).a < 1) continue;
                if (x < map.width - 11 && map.GetPixel(x + 10, y).a < 1) continue;
                if (y > 10 && map.GetPixel(x, y - 10).a < 1) continue;
                if (y < map.height - 11 && map.GetPixel(x, y + 10).a < 1) continue;
                spawnPoints.Add(new Vector2Int(x, y));
            }
        GetComponent<ItemManager>().StartRoutine();
        SpawnObjects();
    }

    void SpawnObjects()
    {
        List<Vector2Int> spawnPoints = new List<Vector2Int>(this.spawnPoints);
        List<Vector3> spawnPointsUsed = new List<Vector3>();
        for (int i = 0; i < 35; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                var spawnPoint = spawnPoints.GetRandom();
                if (GetSpawnPointProgress(spawnPoint) * 3 > j) continue;
                var position = Vector3.zero;
                if ((position = GetGroundPosition(spawnPoint)) == Vector3.zero) continue;
                spawnPoints.Remove(spawnPoint);
                bool tooClose = false;
                foreach (Vector3 point in spawnPointsUsed)
                    if (tooClose = Vector3.Distance(point, position) < 35) break;
                if (tooClose) continue;
                spawnPointsUsed.Add(position);
                Instantiate(objects.GetRandom(), position + Vector3.up * 0.5f, Quaternion.identity);
                break;
            }
        }
    }

    public Vector3 GetGroundPosition(Vector2Int spawnPoint)
    {
        var position = new Vector3(spawnPoint.x / 5, 200, spawnPoint.y / 5);
        if (Physics.Raycast(position, Vector3.down, out var hit, 200, 1 << 6))
            return hit.point;
        return Vector3.zero;
    }

    public Vector2Int GetSpawnPoint()
    {
        return spawnPoints.GetRandom();
    }

    public float GetSpawnPointProgress(Vector2Int spawnPoint)
    {
        return (spawnPoint.x / (map.width - 1f) + spawnPoint.y / (map.height - 1f)) / 2f;
    }
}
