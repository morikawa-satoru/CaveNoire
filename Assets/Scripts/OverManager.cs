using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using UnityEngine.SceneManagement;


public class OverManager : MonoBehaviour
{
    public static readonly string nextScene = "TitleScene";

    void Start()
    {
    }

    void Update()
    {
        if(Global.CheckPressKey(0, Global.Key.ok))
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
