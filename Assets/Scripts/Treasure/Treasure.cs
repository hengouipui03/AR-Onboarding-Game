using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Treasure : MonoBehaviour, IPointerDownHandler
{
    public int treasureId;
    public GameObject prompt;
    Camera cam;

    //Unity new input system, need use iPointerDownHandler, cannot use onMouseDown :(
    public void OnPointerDown(PointerEventData pointer)
    {
        //check if the object is too far away
        float distance = Vector3.Distance(cam.transform.position, gameObject.transform.position);
        if (distance < 12f)
        {
            //Debug.Log("Added treasure: " + treasureId);
            MainManager.Instance.FoundTreasure(treasureId);
            ShowPrompt();                      
        }
        else
        {
            //StartCoroutine(GameObject.Find("GameManager").GetComponent<Station>().MoveCloserPrompt());
        }
    }

    //Need add sound effect

    private void Start()
    {
        cam = Camera.main;
    }

    //This prompt is to show the user that they have obtained the treasure
    void ShowPrompt()
    {
        GameObject obtainedPrompt = Instantiate(prompt,Vector3.zero,Quaternion.identity, GameObject.Find("Canvas").transform);
        obtainedPrompt.transform.localPosition = Vector2.zero;
        Destroy(gameObject); 
    }
}
