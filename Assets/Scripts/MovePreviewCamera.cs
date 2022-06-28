using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePreviewCamera : MonoBehaviour
{
    private float speed = 0.005f;
    private Vector3 startPos = new Vector3(70, 80, 20);
    private Vector3 direction = new Vector3(1600, 4000, 3600);
    private Vector3 endPos = new Vector3(450, 0, 410);

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(new Vector3(30, 27, 0));
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
    }

    public void MoveCamera()
    {

        if(transform.position.x < endPos.x && transform.position.z < endPos.z)
        {
            transform.Translate(direction * (speed * Time.deltaTime));
        }
        else
        {
            transform.position = startPos;
        }
    }
}
