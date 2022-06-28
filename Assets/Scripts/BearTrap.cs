using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : MonoBehaviour
{
    public GameObject goClawLeft;
    public GameObject goClawRight;
    public GameObject trapped;
    float time;
    private AudioSource audioSource;
    public AudioClip bearTrapSound;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float dTime = Time.realtimeSinceStartup - time;
        if (dTime < 1f / 10f)
        {
            goClawLeft.transform.localRotation = Quaternion.Euler(-90f + dTime * 10f * 60f, 0, 0);
            goClawRight.transform.localRotation = Quaternion.Euler(-90f - dTime * 10f * 60f, 0, 0);
        }
        else if (dTime < 1f)
        {
            goClawLeft.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            goClawRight.transform.localRotation = Quaternion.Euler(-150f, 0, 0);
        }
        else if (dTime < 3f)
        {
            goClawLeft.transform.localRotation = Quaternion.Euler(-30f - 60f * Easings.easeInOutCubic((dTime - 1f) / 2f), 0, 0);
            goClawRight.transform.localRotation = Quaternion.Euler(-150f + 60f * Easings.easeInOutCubic((dTime - 1f) / 2f), 0, 0);
        }
        else if (dTime < 4f)
        {
            goClawLeft.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            goClawRight.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Player") return;
        if (Time.realtimeSinceStartup - time < 4) return;
        audioSource.PlayOneShot(bearTrapSound, 1);
        trapped = other.gameObject;
        time = Time.realtimeSinceStartup;
    }

    private void LateUpdate()
    {
        if (Time.realtimeSinceStartup - time > 2) return;
        trapped.transform.position = transform.position + Vector3.up;
    }
}
