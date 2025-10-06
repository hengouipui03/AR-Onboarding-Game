using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbishBin : MonoBehaviour
{
    string binTag;

    private void Start()
    {
        binTag = gameObject.tag;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag(binTag))
            {
                FindObjectOfType<PaperToss>().AddScore();
            }
            else
            {
                FindObjectOfType<PaperToss>().DecreaseScore();
            }
            other.gameObject.tag = null;
            Destroy(other.gameObject, 0.5f);
        }
    }
}

//cld add reset btn
