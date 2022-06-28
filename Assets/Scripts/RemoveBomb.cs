using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBomb : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Detonate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Detonate()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
