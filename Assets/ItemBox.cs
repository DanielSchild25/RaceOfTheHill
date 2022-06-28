using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public GameObject goTop;
    public GameObject goBottom;
    public GameObject goGlass;
    public GameObject goItem;

    Vector3 ground;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        ground = transform.position;
        time = Time.realtimeSinceStartup;
        var scalexy = new Vector3(0, 0, 100);
        goTop.transform.localPosition = new Vector3(0, 0.4f, 0);
        goTop.transform.localScale = scalexy;
        goBottom.transform.localScale = scalexy;
        goGlass.transform.localScale = Vector3.zero;
        goItem.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = ground + 0.2f * Mathf.Sin(Time.realtimeSinceStartup * 2) * Vector3.up;
        goItem.transform.localPosition = 0.1f * Mathf.Sin((Time.realtimeSinceStartup - 0.4f) * 2) * Vector3.up;
        goItem.transform.localRotation = Quaternion.Euler(0, Time.realtimeSinceStartup * 45, 0);
        float dTime = Time.realtimeSinceStartup - time;
        if (dTime > 3) return;
        if (dTime < 1)
        {
            var scale = new Vector3(Easings.easeInOutCubic(dTime) * 100, Easings.easeInOutCubic(dTime) * 100, 100);
            goTop.transform.localScale = scale;
            goBottom.transform.localScale = scale;
        } else if (dTime < 2)
        {
            goTop.transform.localPosition = new Vector3(0, 0.4f + Easings.easeInOutCubic(dTime - 1) * 1.6f, 0);
            goGlass.transform.localScale = new Vector3(100, 100, Easings.easeInOutCubic(dTime - 1) * 100);
            goGlass.transform.localPosition = new Vector3(0, 0.2f + Easings.easeInOutCubic(dTime - 1) * 0.8f, 0);
        }
        if (dTime > 1.5f && dTime < 2.5f)
        {
            goItem.transform.localScale = Easings.easeInOutCubic(dTime - 1.5f) * Vector3.one;
        }
    }
}
