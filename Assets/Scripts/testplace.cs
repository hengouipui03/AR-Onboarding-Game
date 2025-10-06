using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Vuforia;

public class testplace : MonoBehaviour
{
    public GameObject objPrefab;
    public GameObject ground;
    [HideInInspector]
    public GameObject instantiatedObj;
    public bool spawned;

    private void Start()
    {
        spawned = false;
    }
    
    public void spawnObject()
    {
        if (!spawned)
        {
            Vector3 spawnPos = new Vector3(0, -0.75f, 1f);
           
            instantiatedObj = Instantiate(objPrefab, spawnPos, transform.rotation * Quaternion.Euler(0f,180f,0f), ground.transform);
            try
            {
                FindObjectOfType<DialogueManager>().MascotAnim = instantiatedObj.GetComponent<Animator>();
            }
            catch (Exception e)
            {

            }
            spawned = true;
        }
    }
}