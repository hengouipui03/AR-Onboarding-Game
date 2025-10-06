using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasurePrompt : MonoBehaviour
{
    //This script is attached to the obtained treasure prompt prefab
    CanvasGroup cg;
    // Start is called before the first frame update
    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        StartCoroutine(FadeInOut());
    }

    //Fades in and out then destroys itself
    IEnumerator FadeInOut()
    {
        cg.LeanAlpha(1, 0.5f);
        yield return new WaitForSeconds(1.5f);
        cg.LeanAlpha(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
