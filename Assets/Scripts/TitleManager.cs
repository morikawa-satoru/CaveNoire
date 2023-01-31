using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Common;


public class TitleManager : MonoBehaviour
{
    public static readonly string nextScene = "GameScene";

    private SpriteRenderer mask;
    private GameObject     pushkey;

    private int            interval;
    private bool           bFade;


    void Start()
    {
        mask = GameObject.Find("Mask").GetComponent<SpriteRenderer>();
        pushkey = GameObject.Find("PushKey");
        interval = 0;
        mask.DOFade(0.0f, 0);
    }

    void Update()
    {
        if (bFade) { return; }

        // キーチェック
        if(Global.CheckPressKey(0, Global.Key.ok) || Global.CheckPressKey(0, Global.Key.cancel))
        {
            bFade = true;
            mask.DOFade(1.0f, Global.Define.FadeTime).OnComplete(() => FadeEnd());
            AudioManager.Instance.PlaySE(AUDIO.SE_PUSHKEY);
        }

        // Push Key 点滅
        interval++;
        pushkey.SetActive(Global.GetBlink(interval));
    }

    // 終了関数
    void FadeEnd()
    {
        SceneManager.LoadScene(nextScene);
    }
}



