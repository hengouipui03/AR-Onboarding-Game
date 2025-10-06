using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperBall : MonoBehaviour
{
    Camera cam;
    Rigidbody rb;
    PaperToss paperToss;
    float colliderRadius;
    float colliderHeight;
    CapsuleCollider capsuleCollider;

    Vector2 startPos, endPos, direction;
    float touchTimeStart, touchTimeFinish, timeInterval;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        paperToss = FindObjectOfType<PaperToss>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        colliderRadius = capsuleCollider.radius;
        colliderHeight = capsuleCollider.height;
        capsuleCollider.radius = 0.7f;
        capsuleCollider.height = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Quaternion.LookRotation(rb.velocity);
        //if you touch the screen
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            //getting touch position and marking time when you touch the screen
            touchTimeStart = Time.time;
            startPos = Input.GetTouch(0).position;
        }

        //if you release your finger
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            capsuleCollider.radius = colliderRadius;
            capsuleCollider.height = colliderHeight;

            //marking time when you release it
            touchTimeFinish = Time.time;

            //calculate time between start and finish time
            timeInterval = touchTimeFinish - touchTimeStart;

            //getting release finger position
            endPos = Input.GetTouch(0).position;

            //calculating swipe direction in 2D space
            direction = endPos - startPos;

            //add force to balls rigidbody in 3D space based of swipe time, direction and force used
            rb.isKinematic = false;
            rb.AddForce(new Vector3(0, direction.y * 1.8f, 0) + (cam.transform.forward * 180f / timeInterval));

            //destroy ball in 4 seconds if it doesn't hit goal
            StartCoroutine(paperToss.SpawnBall());
            paperToss.spawned = false;
            Destroy(gameObject, 3f);
        }

        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision != null)
        {
            if (collision.collider.CompareTag("Floor"))
            {
                if(gameObject.tag == "cans")
                {
                    FindObjectOfType<AudioManager>().Play("canSound");
                }
                if (gameObject.tag == "rubbish")
                {
                    FindObjectOfType<AudioManager>().Play("rubbishSound");
                }
                if (gameObject.tag == "plasticBottle")
                {
                    FindObjectOfType<AudioManager>().Play("plasticSound");
                }
            }
        }
    }
}