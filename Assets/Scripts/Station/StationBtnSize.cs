using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationBtnSize : MonoBehaviour
{
    //public int col, row, distance;
    public Sprite completed, incomplete;
    public List<GameObject> stnBtns;

    // Start is called before the first frame update
    void Start()
    {
        //RectTransform parent = GetComponent<RectTransform>();
        //GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

        //float newWidth = (parent.rect.width / col) - grid.spacing.x;
        //float newHeight = (parent.rect.height / row) - grid.spacing.y;
        //grid.spacing = new Vector2(parent.rect.width / 15, (parent.rect.width / 15) + 100f);
        //if (newHeight > newWidth)
        //{
        //    grid.cellSize = new Vector2((parent.rect.width / col) - grid.spacing.x / 2, (parent.rect.width / col) - grid.spacing.x / 2);
        //}
        //else
        //{
        //    grid.cellSize = new Vector2((parent.rect.height / row) - grid.spacing.y/2, (parent.rect.height / row) - grid.spacing.y/2);
        //}
        UpdateStnSprite();
    }

    void UpdateStnSprite()
    {
        for(int i = 0; i < stnBtns.Count; i++)
        {
            //if player completed station
            if(PlayerPrefs.HasKey("Station" + (i+2) + "ObjectFound"))
            {
                stnBtns[i].GetComponent<Image>().sprite = completed;
            }
            else
            {
                stnBtns[i].GetComponent<Image>().sprite = incomplete;
            }
        }
    }
}
