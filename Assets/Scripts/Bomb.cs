using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    bool detonated = false;
    public GameObject preExplosion;
    private AudioSource audioSource;
    public AudioClip bombSound;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke(nameof(StartDetonate), 5);
    }

    void StartDetonate()
    {
        audioSource.PlayOneShot(bombSound, 1);
        StartCoroutine(Detonate());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            audioSource.PlayOneShot(bombSound, 1);
            StartCoroutine(Detonate());
        }
    }

    IEnumerator Detonate()
    {
        if (detonated) yield break;
        detonated = true;
        Instantiate(preExplosion, transform.position, Quaternion.identity);
        var colliders = Physics.OverlapSphere(transform.position, 30);
        foreach (Collider collider in colliders)
        {
            if (!collider.gameObject.CompareTag("Player")) continue;
            var distance = Vector3.Distance(collider.transform.position, transform.position);
            collider.GetComponent<ThirdPersonMovement>().AddImpact(collider.transform.position - (transform.position + Vector3.down), 90 - 3 * distance);
        }
        for (int i = 0; i < 10; i++)
        {
            transform.localScale = (10f - i) / 10f * Vector3.one;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        gameObject.transform.localScale = new Vector3(0, 0, 0);
    }
}
