using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public GameObject goPad;
    float time;
    private AudioSource audioSource;
    public AudioClip jumpPadSound;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        var test = GameObject.FindGameObjectWithTag("Finish");
        if (test == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - test.transform.position, Vector3.up);
        transform.rotation = Quaternion.Euler(45, transform.rotation.eulerAngles.y + 180, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float dTime = Time.realtimeSinceStartup - time;
        if (dTime < 1f / 10f)
            goPad.transform.localPosition = new Vector3(0, dTime * 10f, 0);
        else if (dTime < 1f)
            goPad.transform.localPosition = new Vector3(0, 1, 0);
        else if (dTime < 3)
            goPad.transform.localPosition = new Vector3(0, 1f - Easings.easeInOutCubic((dTime - 1f) / 2f), 0);
        else if (dTime < 4)
            goPad.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Player") return;
        if (Time.realtimeSinceStartup - time < 4) return;
        audioSource.PlayOneShot(jumpPadSound, 1);
        other.gameObject.GetComponent<ThirdPersonMovement>().AddImpact(transform.forward, 50);
        time = Time.realtimeSinceStartup;
    }
}
