using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;


public class BaseChara : MonoBehaviour
{

    public enum SubMode { InputMove, MoveWait, Passage, Fall, Damage, Dead, Over, Move, Select};

    public class BaseDefine
    {
        public static readonly float MoveTime = 0.15f;      // 移動時のウエイト
        public static readonly float ScrollTime = 0.75f;    // 画面切り替え時間

        //public static readonly float homeX = -2.88f;
        //public static readonly float homeY =  2.94f;

        public static readonly float screenTop    = 2.94f; // スクリーンの端
        public static readonly float screenButton = -1.54f;
        public static readonly float screenLeft   = -2.88f;
        public static readonly float screenRight  = 2.88f;
        public static readonly string[] AnName    = { "idle", "walk", "hit", "dead", "attack" };

        // 配置位置
        public static readonly int[] aryHomeX = { 4, 5, 3, 6, 2, 7, 1, 8 };
        public static readonly int[] aryHomeY = { 5, 6, 4, 3, 2, 1 };
    }

    public SpriteRenderer sp;
    public MapManager     map;
	public Animator		  an;
	public EnemyManager   ene;
    public PlayerManager  ply;
    public BattleManager  btl;
	public ItemmapManager itemmap;
	public CameraShake    shake;
	public CursorManager  cursor;


	public SubMode    subMode;
    public Vector2    pos;
    public int        mapX, mapY;
    public int        moveX, moveY;
    public float      waitTime;
	public bool		  animEnd;
    public Global.Dir dir = Global.Dir.None;

    public int  hp, maxHp, bakHp, atk, def, gold;
    public bool bFall, bFallDamage, bPoison;

    // マップ座標を得る
    public Vector2Int GetMapPos()
    {
        return new Vector2Int(mapX, mapY);
    }

	// 部屋座標を得る
	public Vector2Int GetRoomPos()
	{
		return new Vector2Int(mapX % Global.Room.szX, mapY % Global.Room.szY);
	}

	// スブライト座標を得る
	public Vector3 GetSpritePos()
	{
		return transform.position;
	}

	// マップ座標をスプライト座標にする
	public Vector3 RoomToSprite(int mx, int my)
	{
		return new Vector3((mx % Global.Room.szX) * Global.Define.szTile + Global.Define.homeX, -(my % Global.Room.szY) * Global.Define.szTile + Global.Define.homeY, -(float)(my % Global.Room.szY));
	}

	// マップ座標をスプライト座標にする
	public Vector3 MapToSprite(int mx, int my)
	{
		return new Vector3(mx * Global.Define.szTile + Global.Define.homeX, -my * Global.Define.szTile + Global.Define.homeY, -(float)(my % Global.Room.szY));
	}

	// 方向を得る
	public Global.Dir GetDir()
	{
		return dir;
	}

	// 方向を設定する
	public void SetDir(Global.Dir d)
	{
		dir = d;
		if((int)d <= (int)Global.Mode.TBox) { return;}
		sp.flipX = d == Global.Dir.Left;
	}

	// マップ座標をチェックする
	public bool CheckMapPos(int mx, int my)
    {
        if (mapX == mx && mapY == my) { return true; }
        else { return false; }
    }

    // スクロール速度を得る
    public float GetScrollTime()
    {
        return BaseDefine.ScrollTime;
    }

	// アルファ値
	public void SetAlpha(float alp)
	{
		sp.color = new Vector4(sp.color.r, sp.color.g, sp.color.b, alp);
	}

	// アニメーション
	public void SetAnim(Global.Anim animName)
	{
		animEnd = false;
		if (an != null)
		{
			an.Play(BaseDefine.AnName[(int)animName]);
		}
	}
	public void SetAnimEnd()
	{
		animEnd = true;
	}
	public bool CheckAnimEnd()
	{
		return animEnd;
	}
}


