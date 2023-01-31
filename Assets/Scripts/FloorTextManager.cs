using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorTextManager : MonoBehaviour

{
    private SpriteRenderer mask;
    private Text text;

    void Start()
    {
        mask = GameObject.Find("Mask").GetComponent<SpriteRenderer>();
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, mask.color.a);
    }
}
