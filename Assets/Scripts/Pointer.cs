using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pointer : MonoBehaviour
{
    Vector3 dir;
    float angle;
    public GameObject target;
    public Camera cam;
    public float rotationSpeed = 11f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mascotPos = new Vector3();
        try
        {
            mascotPos = cam.WorldToScreenPoint(target.transform.position);
        }

        catch(Exception e)
        {
            mascotPos = Camera.main.WorldToScreenPoint(target.transform.position);
        }

        dir = mascotPos - transform.position;
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
        transform.eulerAngles = new Vector3(0, 0, angle - 90);
    }
}
