using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTowerShootingRadius : MonoBehaviour
{
    private BombTower bombTowerScript;

    // Start is called before the first frame update
    void Start()
    {
        bombTowerScript = transform.parent.gameObject.GetComponent<BombTower>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bombTowerScript.targetedPlayers.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bombTowerScript.targetedPlayers.Remove(other.gameObject);
        }
    }
}
