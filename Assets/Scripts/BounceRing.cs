using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BounceRing : MonoBehaviour
{
    public GameObject[] rings;
    float time;
    private AudioSource audioSource;
    public AudioClip bounceRingSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rings = rings.OrderBy(x => Random.value).ToArray();
    }

    private void Update()
    {
        float dTime = Time.realtimeSinceStartup - time;
        if (dTime > 2) return;
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i].transform.localScale = 100 * Mathf.Clamp(-Mathf.Pow(5 * dTime - (i / 5f) - 1f, 2f) + 1.3f, 1, 1.3f) * Vector3.one;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        audioSource.PlayOneShot(bounceRingSound, 1);
        Vector3 source = new Vector3(transform.position.x, other.transform.position.y - 2, transform.position.z);
        other.GetComponent<ThirdPersonMovement>().AddImpact(other.transform.position - source, 30);
        time = Time.realtimeSinceStartup;
    }
}
