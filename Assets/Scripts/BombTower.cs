using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTower : MonoBehaviour
{
    public GameObject shootingRadius;
    public GameObject canon;
    public GameObject prefabBomb;

    public ICollection<GameObject> targetedPlayers;

    // Start is called before the first frame update
    void Start()
    {
        targetedPlayers = new List<GameObject>();
        StartCoroutine(SearchTargets());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SearchTargets()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3));

            if(targetedPlayers.Count > 0)
            {
                foreach(var player in targetedPlayers)
                {
                    //canon.transform.LookAt(new Vector3(player.transform.position.x, canon.transform.position.y, player.transform.position.z));
                    
                    var shootDirection = (player.transform.position - canon.transform.position).normalized;

                    Quaternion spread = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), -8f));

                    var bomb = Instantiate(prefabBomb, canon.transform.position, spread);

                    bomb.GetComponent<Rigidbody>().AddRelativeForce(shootDirection * 50, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bomb"))
        {
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
        }
    }
}
