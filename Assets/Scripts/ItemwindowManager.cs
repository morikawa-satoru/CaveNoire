using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;


public class ItemwindowManager : MonoBehaviour
{

	public enum SubMode { Setup, Select, WhatToDo };

	private class Define
	{
		public static readonly int	maxWhatToDo = 3;
		public static readonly string[] strWhatToDo = {"つかう", "すてる", "やめる"};
		public static readonly string[] strCol = { "<color=#ffffff>", "<color=#ff0000>" , "</color>" };
	}

	private SubMode		   subMode = SubMode.Select;
	private SpriteRenderer sp;
	private Text		   mes, whatToDoText;
	private GameObject	   cur, whatToDo;
	private SpriteRenderer curSp;
	private PlayerManager  ply;
	private int			   sel = 0, selWhatToDo = 0;
	private SpriteRenderer[] ItemSp = new SpriteRenderer[Global.Item.size];
	private float	whatToDoTextScale;

	private string[] Description =
	{
		"",
		"",
		"鍵",
		"地図",
		"回復",
		"解毒",
		"ロウソク",
		"ダミー1",
		"ダミー2",
		"ダミー3",
		"ダミー4",
		"ダミー5",
	};

	public void Setup()
    {
		sp     = GetComponent<SpriteRenderer>();
		mes    = GameObject.Find("ItemMessage").GetComponent<Text>();
		cur    = GameObject.Find("ItemCursor");
		curSp  = cur.GetComponent<SpriteRenderer>();
		ply      = GameObject.Find("Player").GetComponent<PlayerManager>();
		whatToDo = GameObject.Find("WhatToDoWindow");
		whatToDoText = GameObject.Find("WhatToDoText").GetComponent<Text>();
		whatToDoText.text = Define.strCol[1] + Define.strWhatToDo[0] + Define.strCol[2] + " "
						  +	Define.strCol[0] + Define.strWhatToDo[1] + Define.strCol[2] + " "
						  + Define.strCol[0] + Define.strWhatToDo[2] + Define.strCol[2];
		whatToDoTextScale = GameObject.Find("Canvas").transform.localScale.x;
		mes.text = "";
		for (int i = 0; i < Global.Item.size; i++)
		{
			ItemSp[i] = GameObject.Find("Item_" + i.ToString()).GetComponent<SpriteRenderer>();
			Sprite sp = Global.GetSprite("Image/Item/", "Item_" + ply.item[i]);
			ItemSp[i].sprite = sp;
			ItemSp[i].enabled = false;
		}
		Hide();
		subMode = SubMode.Setup;
	}

	public void View()
	{
		sp.color    = new Color(1, 1, 1, 1);
		mes.color   = new Color(1, 1, 1, 1);
		curSp.color = new Color(1, 1, 1, 1);
		for (int i = 0; i < Global.Item.size; i++){ ItemSp[i].enabled = true;}
		mes.text = Description[ply.item[sel]];
	}

	public void Hide()
	{
		sp.color    = new Color(1, 1, 1, 0);
		mes.color   = new Color(1, 1, 1, 0);
		curSp.color = new Color(1, 1, 1, 0);
		for (int i = 0; i < Global.Item.size; i++) { ItemSp[i].enabled = false; }
		mes.text = "";
		HideWhatToDo();
	}

	void ViewWhatToDo()
	{
		whatToDo.SetActive(true);
	}

	void HideWhatToDo()
	{
		whatToDo.SetActive(false);
	}

	// メイン
	public void UpdateSelect()
	{
		switch(subMode)
		{
			case SubMode.Setup:
				sel = 0;
				selWhatToDo = 0;
				SetCursorPos(sel);
				subMode = SubMode.Select;
				SetWhatToDoCursorPos(-1);
				break;
			case SubMode.Select:
				Select();
				break;
			case SubMode.WhatToDo:
				WhatToDo();
				break;
		}
	}

	// Step2 : アイテム選択
	public void Select()
	{

		int bak = sel;
	
		// カーソル移動
		if      (Global.CheckPressKey(0, Global.Key.left) && (sel % Global.Item.szX) > 0)
		{
			sel--;
		}
		else if (Global.CheckPressKey(0, Global.Key.right) && (sel % Global.Item.szX) < Global.Item.szX - 1)
		{
			sel++;
		}
		else if (Global.CheckPressKey(0, Global.Key.up) && sel >= Global.Item.szX)
		{
			sel -= Global.Item.szX;
		}
		else if (Global.CheckPressKey(0, Global.Key.down) && sel < Global.Item.szX)
		{
			sel += Global.Item.szX;
		}
		else if (Global.CheckPressKey(0, Global.Key.ok) && ply.item[sel] > 0)
		{
			ViewWhatToDo();
			AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
			SetCursorPos(-1);
			selWhatToDo = 0;
			SetWhatToDoCursorPos(selWhatToDo);
			subMode = SubMode.WhatToDo;
		}
		else if (Global.CheckPressKey(0, Global.Key.cancel))
		{
			Global.SetMode(Global.Mode.PlayerTurn);
			AudioManager.Instance.PlaySE(AUDIO.SE_CANCEL);
			Hide();
			subMode = SubMode.Setup;
			SetCursorPos(-1);
		}
		// 項目移動
		if (sel != bak) 
		{
			AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
			mes.text = Description[ply.item[sel]];
			SetCursorPos(sel);
		}
	}

	// カーソル表示座標
	void SetCursorPos(int a)
	{
		if (a >= 0)
		{
			cur.transform.position = new Vector2(-2.24f + (a % Global.Item.szX) * 1.12f, 0.96f - (a / Global.Item.szX) * 1.12f);
		}
		else
		{
			cur.transform.position = new Vector2(1000, 1000);
		}
	}

	// Step3 : アイテムをどうするか?
	void WhatToDo()
	{
		int bak = selWhatToDo;

		if (Global.CheckPressKey(0, Global.Key.left) && selWhatToDo > 0)
		{
			selWhatToDo--;
		}
		else if (Global.CheckPressKey(0, Global.Key.right) && selWhatToDo < Define.strWhatToDo.Length)
		{
			selWhatToDo++;
		}
		else if (Global.CheckPressKey(0, Global.Key.ok))
		{
			// つかう
			if(selWhatToDo == 0)
			{
				ply.item[sel] = 0;
				ItemSp[sel] = GameObject.Find("Item_" + sel.ToString()).GetComponent<SpriteRenderer>();
				Sprite sp = Global.GetSprite("Image/Item/", "Item_" + ply.item[sel]);
				ItemSp[sel].sprite = sp;
				ItemSp[sel].enabled = false;
				Hide();
				AudioManager.Instance.PlaySE(AUDIO.SE_USEITEM);
			}
			else
			{
				AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
			}
			subMode = SubMode.Setup;
			SetWhatToDoCursorPos(-1);
			HideWhatToDo();
			Global.SetMode(Global.Mode.PlayerTurn);
		}
		else if (Global.CheckPressKey(0, Global.Key.cancel))
		{
			HideWhatToDo();
			AudioManager.Instance.PlaySE(AUDIO.SE_CANCEL);
			SetWhatToDoCursorPos(-1);
			int selBak = sel;
			subMode = SubMode.Setup;
			sel = selBak;
		}

		if (bak != selWhatToDo)
		{
			AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
			SetWhatToDoCursorPos(selWhatToDo);
		}
	}

	// どうするウィンドウの項目座標
	void SetWhatToDoCursorPos(int a)
	{
		// テキスト
		whatToDoText.text = "";
		if (a >= 0)
		{
			for (int i = 0; i < Define.strWhatToDo.Length; i++)
			{
				whatToDoText.text += Define.strCol[(selWhatToDo == i) ? 1 : 0] + Define.strWhatToDo[i] + Define.strCol[2] + " ";
			}
		}
		// ウィンドウ位置
		float x = -32 + 15.6f * (sel % 5);
		float y =   8 - 28.0f * (float)((int)(sel / 5));
		whatToDo.transform.position = new Vector3(x, y, 0) * whatToDoTextScale;
	}
}
