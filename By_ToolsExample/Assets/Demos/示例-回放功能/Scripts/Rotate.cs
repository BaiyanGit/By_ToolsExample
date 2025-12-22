using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    private Quaternion q = new Quaternion();
    public float yDelta = 0.1f;
    public float speed = 0.5f;

    void Start()
    {
        // q.eulerAngles = new(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        q.eulerAngles = new Vector3(0, q.eulerAngles.y + yDelta * speed, 0);
        transform.rotation = q;
    }
}