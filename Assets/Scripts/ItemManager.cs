using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    ObjectManager objManager;
    List<Vector2Int> spawnPoints = new List<Vector2Int>();
    public GameObject[] items;
    int i = 1;    

    public void StartRoutine()
    {
        objManager = GetComponent<ObjectManager>();
        InvokeRepeating(nameof(SpawnItem), 5, 5);
    }


    void SpawnItem()
    {
        Debug.Log("spawn new item");
        while (true)
        {
            var spawnPoint = objManager.GetSpawnPoint();
            if (objManager.GetSpawnPointProgress(spawnPoint) * 20f > i++) continue;
            var position = Vector3.zero;
            if ((position = objManager.GetGroundPosition(spawnPoint)) != Vector3.zero)
                Instantiate(items.GetRandom(), position + Vector3.up * 0.5f, Quaternion.identity);
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
