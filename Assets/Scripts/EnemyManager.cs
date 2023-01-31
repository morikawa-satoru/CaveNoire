using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Common;


public class EnemyManager : MonoBehaviour
{

    public  Enemy[] work = new Enemy[Global.Map.size];
    private MapManager map;

    // ワーク作成
    public void CreateWork()
    {
        GameObject prefab = (GameObject)Resources.Load("Prefab/Enemy");
        map = GameObject.Find("Tilemap").GetComponent<MapManager>();
        for (int i = 0; i < work.Length; i++)
        {
            var e = GameObject.Instantiate(prefab, new Vector3(0, i * -100.0f, 0.0f), Quaternion.identity) as GameObject;
            e.name = "Ene" + i.ToString("00");
            work[i] = e.GetComponent<Enemy>();
            work[i].Setup(i);
        }
    }

    // リセット
    public void ResetEnemy()
    {
        for (int i = 0; i < work.Length; i++)
        {
            work[i].SetUnuse();
        }
    }

    // 行動
    public void Action()
    {
        for (int i = 0; i < work.Length; i++)
        {
            work[i].Action();
        }
        bool    bAction = false;
        for (int i = 0; i < work.Length; i++)
        {
            if ( work[i] == null) {    continue; }
            if (!work[i].CheckUse()) { continue; }
            if ( work[i].CheckEnd()) { continue; }
            bAction = true;
        }
        if (!bAction && Global.nextMode == Global.Mode.None)
        {
            Global.SetMode(Global.Mode.PlayerTurn);
            for (int i = 0; i < work.Length; i++)
            {
                work[i].ClearEnd();
            }
        }
    }

    // タイプを得る
    public Global.EnemyType GetType(int num)
    {
        return work[num].type;
    }

    // マップ座標よりエネミーを探し ID 番号を得る
    public int GetNumFromPos(int x, int y)
    {
        for (int i = 0; i < work.Length; i++)
        {
            if(work[i].mapX == x && work[i].mapY == y)
            {
                return i;
            }
        }
        return -1;
    }

	// マップ座標を得る
	public Vector2Int GetMapPos(int num)
	{
		return new Vector2Int(work[num].mapX, work[num].mapY);
	}

	// 部屋座標を得る
	public Vector2Int GetRoomPos(int num)
	{
		return new Vector2Int(work[num].mapX % Global.Room.szX, work[num].mapY % Global.Room.szY);
	}

	// スプライト座標を得る
	public Vector3 GetSpritePos(int num)
	{
		return work[num].MapToSprite(work[num].mapX, work[num].mapY);
	}

	// アルファ値設定
	public void SetAlpha(int num, float alp)
	{
		work[num].SetAlpha(alp);
	}

	// アニメーション設定
	public void SetAnim(int num, Global.Anim animName)
	{
		work[num].SetAnim(animName);
	}

	// アニメーション再生中か調べる
	public bool CheckAnimPlay(int num)
	{
        if(work[num].an == null) { return false;}
		AnimatorStateInfo stateInfo = work[num].an.GetCurrentAnimatorStateInfo(0);
		return (stateInfo.normalizedTime < 1.0f);
	}

	// マップ座標での衝突判定
	public bool Collision(int mx, int my)
    {
        for (int i = 0; i < work.Length; i++)
        {
            if ( work[i] == null) {            continue; }
            if (!work[i].CheckUse()) {         continue; }
            if ( work[i].CheckMapPos(mx, my)){ return true;}
        }
        return false;
    }

    // 消去
    public void Erase(int num)
    {
        work[num].Erase();
    }
}
