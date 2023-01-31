using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using DG.Tweening;


public class BaseEnemy : BaseChara
{
    public class Define : BaseDefine
    {
        public static readonly float homeX = -2.88f;
        public static readonly float homeY =  2.94f;
    }

	public Vector2  mapHome;

    public int				id = -1;
    public bool				bEnd, bUse;
    public Global.EnemyType type;


    public void Setup(int id)
    {
		ene = GameObject.Find("Main").GetComponent<EnemyManager>();
		btl = GameObject.Find("Main").GetComponent<BattleManager>();
		map = GameObject.Find("Tilemap").GetComponent<MapManager>();
        mapHome   = map.transform.position;
        sp        = GetComponent<SpriteRenderer>();
        ply       = GameObject.Find("Player").GetComponent<PlayerManager>();
        itemmap   = GameObject.Find("TilemapItem").GetComponent<ItemmapManager>();
        //an        = GetComponent<Animator>();
        subMode = SubMode.Move;
        dir  = Global.Dir.Right;
        bEnd = false;
    }

    void FixedUpdate()
    {
        if (!bUse) { return;}
        float xx = map.transform.position.x - mapHome.x;
        float yy = map.transform.position.y - mapHome.y;
        float zz = (float)((int)(mapY % Global.Room.szY));
        transform.position = new Vector3(pos.x + xx, pos.y + yy, -zz);
    }

    public void MoveEnd()
    {
        subMode = SubMode.Move;
        bEnd    = true;
    }

    public bool CheckEnd()
    {
        return bEnd;
    }

    public void ClearEnd()
    {
        bEnd = false;
    }

    public void SetUnuse()
    {
        bUse = false;
        //sp.enabled = false;
        //shadow.GetComponent<SpriteRenderer>().enabled = false;
        //shadow.SetActive(false);
		transform.position = new Vector2(-100, 100);
    }

    public bool CheckUse()
    {
        return bUse;
    }

    public void HitEnd()
    {
        bEnd = true;
        subMode = SubMode.MoveWait;
    }

    public void Erase()
    {
		SetUnuse();
		subMode = SubMode.MoveWait;
    }
}
