using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;


public class CursorManager : BaseChara
{

	private int EnemyId;								// 攻撃対象のエネミー番号

	private class Define
	{
		public static readonly float yOffset = -0.38f;
		public static readonly float z       = -1;
	}

	void Start()
    {
        sp  = GetComponent<SpriteRenderer>();
        ply = GameObject.Find("Player").GetComponent<PlayerManager>();
        ene = GameObject.Find("Main").GetComponent<EnemyManager>();
		Hide();
    }

	// ターゲット番号
	public void SetTarget(int id)
    {
        sp.enabled = true;
		EnemyId    = id;

		SetPlayerPos();
	}

	public void Hide()
    {
        sp.enabled = false;
    }

	public void SetPlayerPos()
	{
		Vector2Int a = ply.GetRoomPos();
		Vector3    v = RoomToSprite(a.x, a.y);
		v.y += Define.yOffset;
		v.z  = Define.z;
		transform.position = v;
	}

	public void SetEnemyPos()
	{
		Vector2Int a = ene.GetRoomPos(EnemyId);
		Vector3    v = RoomToSprite(a.x, a.y);
		v.y += Define.yOffset;
		v.z = Define.z;
		transform.position = v;
	}
}
