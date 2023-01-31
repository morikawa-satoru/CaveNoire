using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using DG.Tweening;


public class GameManager : MonoBehaviour
{

    private MapManager        map;
    private PlayerManager     ply;
    private BattleManager     btl;
    private SpriteRenderer    mask;
    private Text              floorText;
    //private GameObject        debug;
    private EnemyManager      ene;
    private GameObject        message;
    private Text              messageText;
	private ItemwindowManager itemWindow;


	void Start()
    {
        Global.debug = GameObject.Find("Debug");
        map   = GameObject.Find("Tilemap").GetComponent<MapManager>();
        map.Init();
        ply = GameObject.Find("Player").GetComponent<PlayerManager>();
        ply.Init();
        SpriteRenderer a = GameObject.Find("Frame").GetComponent<SpriteRenderer>();
        a.color          = new Color(a.color.r, a.color.g, a.color.b, 1);
        Global.SetMode(Global.Mode.Start, Global.Define.FadeTime);

        mask = GameObject.Find("Mask").GetComponent<SpriteRenderer>();
        mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 1);
        Global.floor = 0;
        floorText = GameObject.Find("Floor").GetComponent<Text>();
        floorText.text = "B" + (Global.floor + 1).ToString() + "F";
        Global.SetMode(Global.Mode.Start, Global.Define.FadeTime);
        AudioManager.Instance.PlaySE(Global.floor == 0 ? AUDIO.SE_START : AUDIO.SE_STAIRS);
        ply.HideStatus();

        message     = GameObject.Find("Message");
        messageText = GameObject.Find("Message").GetComponent<Text>();
        GameObject.Find("MessageFrame").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        message.SetActive(false);

		ene = GameObject.Find("Main").GetComponent<EnemyManager>();
        btl = GameObject.Find("Main").GetComponent<BattleManager>();
		itemWindow = GameObject.Find("ItemwindowManager").GetComponent<ItemwindowManager>();

		Global.bBgm     = false;
        Global.mode     = Global.Mode.Start;
        Global.nextMode = Global.Mode.None;
		debug.SetActive(Global.bDebug);
	}

    void Update()
    {
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1)){ Global.bDebug ^= true; debug.SetActive(Global.bDebug); }
        #endif

        Global.waitTime -= Time.deltaTime;
        if(Global.waitTime > 0.0f){ return;}

        switch (Global.mode)
        {
            case Global.Mode.Start:
				SetupRoom();
				Invoke("PlayBgm", 1.0f);                        // 一秒後に音楽再生
                floorText.text = "";
                Global.SetMode(Global.Mode.PlayerTurn);
                break;
            case Global.Mode.PlayerTurn:
                ply.Action();
                break;
            case Global.Mode.EnemyTurn:
                ene.Action();
                break;
            case Global.Mode.StairsDown:
                StairsDown();
                break;
            case Global.Mode.Battle:
                btl.UpdateBattle();
                break;
            case Global.Mode.Message:
                SetMessage();
                break;
            case Global.Mode.MessageWait:
                Message();
                break;
			case Global.Mode.Item:
				itemWindow.UpdateSelect();
				break;
			//case Global.Mode.ItemWhatToDo:
			//	ItemWhatToDo();
			//	break;
		}

		if (Global.nextMode != Global.Mode.None && Global.nextMode != Global.mode)
        {
            Global.mode        = Global.nextMode;
            Global.nextMode    = Global.Mode.None;
            Global.bChangeMode = true;
        }
        else
        {
            Global.bChangeMode = false;
        }
    }

    void PlayBgm()
    {
		if (!Global.bBgm) { return;}
        AudioManager.Instance.PlayBGM(AUDIO.BGM_DUNGEON01_LOOP, AudioManager.BGM_FADE_SPEED_RATE_HIGH);
    }

	// マップ作成
	void SetupRoom()
	{
		for (; ; )
		{
			if (ply.SetStartPosition() > 0) { break; }
		}
	}

	void StairsDown()
    {
        if (Global.bStairs) { AudioManager.Instance.PlaySE(AUDIO.SE_STAIRS); }
        Global.floor++;
        floorText.text = "";
        floorText.text = "B" + (Global.floor + 1).ToString() + "F";
        ply.HideStatus();
        Global.SetMode(Global.Mode.Start, Global.Define.FadeTime);
    }

    void SetMessage()
    {
        message.SetActive(true);
        Global.SetMode(Global.Mode.MessageWait);
    }
    void Message()
    {
        if (Global.CheckPressKey(0, Global.Key.any))
        {
            message.SetActive(false);
            Global.SetMode(Global.Mode.PlayerTurn);
            //AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
        }
    }
}

