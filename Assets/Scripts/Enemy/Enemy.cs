using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Common;
using DG.Tweening;


public class Enemy : BaseEnemy
{

    private static readonly string[] AnimationFile = { "TBox", "policeman" };

    public void Action()
    {
        if (bEnd || !bUse) { return;}
        switch (subMode)
        {
            case SubMode.Move:
                Move();
                break;
            case SubMode.MoveWait:
                break;
        }
    }

    // 移動
    void Move()
    {
        switch (type)
        {
            case Global.EnemyType.TBox:
                bEnd = true;
				break;
            case Global.EnemyType.Police:
                XSlide();
                break;
            //case 1:
            //    YSlide();
            //    break;
        }
    }

    void XSlide()
    {
        int moveX = 0, moveY = 0;
        int roomX = mapX % Global.Room.szX;
        //int roomY = mapY % Global.Room.szY;
        switch (dir)
        {
            case Global.Dir.Right:
                moveX = 1;
                if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomX + moveX) > (Global.Room.szX - 2))
                {
                    dir = Global.Dir.Left;
                    moveX = -1;
                    if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomX + moveX) < 1)
                    {
                        moveX = 0;
                    }
                }
                break;
            case Global.Dir.Left:
                moveX = -1;
                if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomX + moveX) < 1)
                {
                    dir = Global.Dir.Right;
                    moveX = 1;
                    if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomX + moveX) > (Global.Room.szX - 2))
                    {
                        moveX = 0;
                    }
                }
                break;
        }
        // 攻撃
        if (ply.CheckMapPos(mapX + moveX, mapY))
        {
            btl.SetupBattle(mapX, mapY, true);
            subMode = SubMode.Move;
        }
        else
        {   // 移動
            subMode = SubMode.MoveWait;
            mapX += moveX;
            if (moveX < 0) { sp.flipX = true; }
            else { sp.flipX = false; }
            Vector2 v = MapToSprite(mapX, mapY);
            DOTween.To(() => pos.x, num => pos.x = num, v.x, Define.MoveTime).OnComplete(() => MoveEnd());
            DOTween.To(() => pos.y, num => pos.y = num, v.y, Define.MoveTime);
        }
    }

    void YSlide()
    {
        int moveX = 0, moveY = 0;
        int roomY = mapY % Global.Room.szY;
        switch (dir)
        {
            case Global.Dir.Down:
                moveY = 1;
                if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomY + moveY) > (Global.Room.szY - 2))
                {
                    dir = Global.Dir.Up;
                    moveY = -1;
                    if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomY + moveY) < 1)
                    {
                        moveY = 0;
                    }
                }
                break;
            case Global.Dir.Up:
                moveY = -1;
                if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomY + moveY) < 1)
                {
                    dir = Global.Dir.Down;
                    moveY = 1;
                    if (map.CheckBlock(mapX + moveX, mapY + moveY) || (roomY + moveY) > (Global.Room.szY - 2))
                    {
                        moveY = 0;
                    }
                }
                break;
        }
        subMode   = SubMode.MoveWait;
        mapY     += moveY;
        Vector2 v = MapToSprite(mapX, mapY);
        DOTween.To(() => pos.x, num => pos.x = num, v.x, Define.MoveTime);
        DOTween.To(() => pos.y, num => pos.y = num, v.y, Define.MoveTime).OnComplete(() => MoveEnd());
    }

    // 開始位置
    public void SetUp(int numStartRoom, Global.EnemyType Etype)
    {
        //shadow.SetActive(true);

        string   animFile = AnimationFile[(int)Etype];
        if(animFile == null)
        {
		}
        else
        {
            an = GetComponent<Animator>();
            an.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Anim/" + animFile);
        }

        // 初期位置設定
        for (int x = 0; x < Define.aryHomeX.Length; x++)
        {
            for (int y = 0; y < Define.aryHomeY.Length; y++)
            {
                if (!map.CheckBlock(numStartRoom, Define.aryHomeX[x], Define.aryHomeY[y]))
                {
                    mapX = (numStartRoom % Global.Map.szX) * Global.Room.szX + Define.aryHomeX[x];
                    mapY = (numStartRoom / Global.Map.szX) * Global.Room.szY + Define.aryHomeY[y];
                    type = Etype;
                    pos  = MapToSprite(mapX, mapY);
                    //x    = Define.aryHomeX.Length;
                    bUse = true;
                    sp.enabled = true;
                    //shadow.GetComponent<SpriteRenderer>().enabled = true;

                    subMode = SubMode.Move;
                    bEnd = false;

                    switch(Etype)
                    {
						case Global.EnemyType.TBox:
							break;
                        case Global.EnemyType.Police:
                            dir = Global.Dir.Right;
                            break;
                        //case 1:
                        //    dir = Global.Dir.Down;
                        //    break;
                    }
                    return;
                }
            }
        }
        bUse = false;
    }
}
