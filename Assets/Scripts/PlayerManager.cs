using Common;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerManager : BaseChara
{
    public static readonly string nextScene = "OverScene";

    private class Define : BaseDefine
    {
        public static readonly float blockTime    = 0.4f;   // 障害物に当たった時のウエイト
        public static readonly float fallTime     = 0.5f;   // 落下時間
        public static readonly float smallTime    = 1.0f;   // 落下時の小さくなるまでの待ち時間
        public static readonly float passOpenTime = 0.3f;   // 通路が貫通する間隔
        public static readonly int   damageFall   = 10;     // 落下時のダメージ値
    }

    private GameObject        info, qmark, emark;
    private SpriteRenderer    mask;
    private Text              infoText, status;
	private ItemwindowManager itemWindow;

	// 変数
	private GameObject shadow;
	private Vector2 scaleBak;
    private int     passX, passY;

	public	int[] item = new int[Global.Item.size];


	public void Init()
    {
        map     = GameObject.Find("Tilemap").GetComponent<MapManager>();
        info    = GameObject.Find("DebugPlayerPos");
        qmark   = GameObject.Find("QMark");
        emark   = GameObject.Find("EMark");
        mask    = GameObject.Find("Mask").GetComponent<SpriteRenderer>();
        status  = GameObject.Find("Status").GetComponent<Text>();
        shake   = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        sp      = GetComponent<SpriteRenderer>();
        shadow  = GameObject.Find("PlShadow");
        cursor  = GameObject.Find("Cursor").GetComponent<CursorManager>();
        an      = GetComponent<Animator>();
        an.Play(Define.AnName[(int)Global.Anim.IDLE]);
        ene     = GameObject.Find("Main").GetComponent<EnemyManager>();
        btl     = GameObject.Find("Main").GetComponent<BattleManager>();
		itemmap = GameObject.Find("TilemapItem").GetComponent<ItemmapManager>();
		itemWindow = GameObject.Find("ItemwindowManager").GetComponent<ItemwindowManager>();

		scaleBak = transform.localScale;

        #if UNITY_EDITOR
            infoText = info.GetComponent<Text>();
            infoText.text = "(" + mapX.ToString() + "," + mapY.ToString() + ")";
        #else
            info.SetActive(false);
        #endif

        Setup();
    }

    // ステイタス設定
    void Setup()
    {
        maxHp    = 10;
        atk      = 4;
        def      = 4;
        gold     = 10;
        waitTime = 0;
        dir      = Global.Dir.Down;
        passX    = 0;
        passY    = 0;
        moveX    = 0;
        moveY    = 0;
        bPoison  = false;
        transform.localScale = scaleBak;
        subMode  = SubMode.InputMove;
 
        hp       = maxHp;
        bakHp    = hp;

		for(int i = 0; i < item.Length; i++){ item[i] = 0;}
		item[0] = 4;        // 回復
		item[1] = 5;        // 解毒
		item[2] = 6;		// ロウソク
        item[3] = 7;
        item[4] = 8;
        item[5] = 9;
        item[6] = 10;
        itemWindow.Setup();
	}

	public void Action()
    {
        DrawStatus();
 
        // デバッグ情報
        info.transform.position = new Vector2(transform.position.x + 0.48f, transform.position.y + 0.32f);

        // ウエイト
        if (waitTime > 0) { waitTime -= Time.deltaTime; return; }

        // 動作
        switch(subMode)
        {
            case SubMode.InputMove:
                KeyInput();
                break;
            case SubMode.MoveWait:
                break;
            case SubMode.Passage:
                Passage();
                break;
            case SubMode.Dead:
                break;
            case SubMode.Fall:
                Fall();
                break;
            case SubMode.Damage:
                break;
            case SubMode.Over:
                Over();
                break;
        }
        // デバッグ表示
        #if UNITY_EDITOR
            infoText.text = "(R:" + map.GetStayRoom().ToString() + ", " + mapX.ToString() + "," + mapY.ToString() + ")";
        #endif
    }

    // キー入力
    void KeyInput()
    {
        bool bMove = false;
        if (     Global.CheckPressKey(0, Global.Key.left)) {  moveX = -1; bMove = true; dir = Global.Dir.Left;  sp.flipX = true;}
        else if (Global.CheckPressKey(0, Global.Key.right)) { moveX =  1; bMove = true; dir = Global.Dir.Right; sp.flipX = false; }
        else if (Global.CheckPressKey(0, Global.Key.up)) {    moveY = -1; bMove = true; dir = Global.Dir.Up; }
        else if (Global.CheckPressKey(0, Global.Key.down)) {  moveY =  1; bMove = true; dir = Global.Dir.Down; }
        else if (Global.CheckPressKey(0, Global.Key.cancel))
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_CANCEL);
            Global.SetMode(Global.Mode.EnemyTurn);
            return;
        }
		else if (Global.CheckPressKey(0, Global.Key.ok))
		{
			if (itemmap.Check(mapX, mapY))
			{
	            Global.SetMessage("アイテムを得た!");
		        AudioManager.Instance.PlaySE(AUDIO.SE_GETITEM);
				itemmap.Erase(mapX, mapY);
			}
			else if(map.CheckStiars(mapX, mapY))
			{
				mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 1);
				Global.SetMode(Global.Mode.StairsDown);
				Global.bStairs = true;
				return;
			}
			else
			{
				Global.SetMode(Global.Mode.Item);
				itemWindow.View();
				AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
			}
		}
        if (bMove)
        {
            subMode = SubMode.MoveWait;
            MoveStart();
        }
    }

    // 移動
    void MoveStart()
    {
        // エネミーチェック
        if (ene.Collision(mapX + moveX, mapY + moveY))
        {
            SetupSelect();
        }
        else 
        if (map.CheckBlock(mapX + moveX, mapY + moveY))
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_BLOCK);
            waitTime = Define.blockTime;
            subMode = SubMode.InputMove;

            // 隠し通路
            if (map.CheckPassage(mapX + moveX, mapY + moveY))
            {
                subMode = SubMode.Passage;
                Passage();
            }
            else
            {
                BlockHitStart();
                an.Play(Define.AnName[(int)Global.Anim.HIT]);
            }
        }
        else
        {
            an.Play(Define.AnName[(int)Global.Anim.WALK]);
            // 画面端でスクロール
            if (moveY == -1 && (mapY % Global.Room.szY) == 0)
            {   // 上
                mapY += moveY;
                map.Scroll(Global.Dir.Up);
                transform.DOLocalMove(new Vector3(this.transform.position.x, Define.screenButton, this.transform.position.z), Define.ScrollTime).OnComplete(() => MoveEnd());
            }
            else if (moveX == 1 && (mapX % Global.Room.szX) == Global.Room.szX - 1)
            {   // 右
                mapX += moveX;
                map.Scroll(Global.Dir.Right);
                transform.DOLocalMove(new Vector3(Define.screenLeft, this.transform.position.y, this.transform.position.z), Define.ScrollTime).OnComplete(() => MoveEnd());
            }
            else if (moveY == 1 && (mapY % Global.Room.szY) == Global.Room.szY - 1)
            {   // 下
                mapY += moveY;
                map.Scroll(Global.Dir.Down);
                transform.DOLocalMove(new Vector3(this.transform.position.x, Define.screenTop, this.transform.position.z), Define.ScrollTime).OnComplete(() => MoveEnd());
            }
            else if (moveX == -1 && (mapX % Global.Room.szX) == 0)
            {   // 左
                mapX += moveX;
                map.Scroll(Global.Dir.Left);
                transform.DOLocalMove(new Vector3(Define.screenRight, this.transform.position.y, this.transform.position.z), Define.ScrollTime).OnComplete(() => MoveEnd());
            }
            else
            {   // スクロールしないでキャラだけ移動
                mapX += moveX;
                mapY += moveY;
                transform.DOMove(RoomToSprite(mapX, mapY), Define.MoveTime).OnComplete(() => MoveEnd());
            }
            // 落下
            if (map.CheckFall(mapX, mapY))
            {
                bFall = true;
                shadow.SetActive(false);
            }
            // 足音
            else if (map.CheckWater(mapX, mapY))
            {
                AudioManager.Instance.PlaySE(AUDIO.SE_WATER);
                shadow.SetActive(false);
            }
            else
            {
                AudioManager.Instance.PlaySE(AUDIO.SE_WALK);
                shadow.SetActive(true);
            }
        }
        moveX = 0;
        moveY = 0;
    }

    // 移動終了
    void MoveEnd()
    {
        subMode = SubMode.InputMove;
        an.Play(Define.AnName[(int)Global.Anim.IDLE]);
        map.SetBrighten(mapX, mapY);
        // 落下
        if (bFall)
        {
            Invoke("FallScale", Define.smallTime);
            subMode = SubMode.Fall;
            waitTime = Define.fallTime + Define.smallTime;
            AudioManager.Instance.PlaySE(AUDIO.SE_EXMARK);
        }
        else
        {
            //Global.mode = Global.Mode.EnemyTurn;
            //map.ChangeEnemyTurn();
            Global.SetMode(Global.Mode.EnemyTurn);
        }
        QMark();
        EMark();
    }

    // ? マーク
    void QMark()
    {
        qmark.SetActive(map.CheckArround(mapX, mapY));
    }
    // ! マーク
    void EMark()
    {
        emark.SetActive(map.CheckFall(mapX, mapY));
    }

    // 敵に当たった
    public void EnemyHitStart(int ninusHp, bool bEnemy = false)
    {
        if(!bEnemy)
        {
            Invoke("EnemyHitEnd", 0.5f + ninusHp / 4.0f + 1);
        }
        shake.Shake(0.50f, 0.1f);
        DOTween.To(
           () => hp,                       // 何を対象にするのか
           num => hp = num,                // 値の更新
           hp - ninusHp,                   // 最終的な値
           0.5f + ninusHp / 4.0f           // アニメーション時間
       ).SetDelay(1);
    }
    void EnemyHitEnd()
    {
        //map.ChangeEnemyTurn();
        Global.SetMode(Global.Mode.EnemyTurn);
        subMode = SubMode.InputMove;
        an.Play(Define.AnName[(int)Global.Anim.IDLE]);
    }

    // 壁に当たった
    void BlockHitStart()
    {
        Invoke("BlockHitEnd", 0.5f);
    }
    void BlockHitEnd()
    {
        //Global.mode = Global.Mode.EnemyTurn;
        //Wmap.ChangeEnemyTurn();
        Global.SetMode(Global.Mode.EnemyTurn);
        subMode = SubMode.InputMove;
        an.Play(Define.AnName[(int)Global.Anim.IDLE]);
    }

    // 落下
    void Fall()
    {
        mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 1);
        Global.SetMode(Global.Mode.StairsDown);
        bFall       = false;
        bFallDamage = true;
        subMode     = SubMode.Damage;
    }

    // ダメージ
    void FallDamageStart(int ninusHp)
    {
        Invoke("FallDamageEnd", 1.5f + (float)ninusHp / 4.0f);
        AudioManager.Instance.PlaySE(AUDIO.SE_FALLDAMAGE);
        shake.Shake(0.50f, 0.1f);
        bFallDamage = false;
        DOTween.To(
            () => hp,                       // 何を対象にするのか
            num => hp = num,                // 値の更新
            hp - ninusHp,                   // 最終的な値
            0.5f + ninusHp / 4.0f           // アニメーション時間
        ).SetDelay(1);
    }
    void FallDamageEnd()
    {
        an.Play(Define.AnName[(int)Global.Anim.IDLE]);
        if (hp <= 0)
        {
            subMode = SubMode.Over;
            AudioManager.Instance.StopBGM();
            waitTime = 1;
            an.Play(Define.AnName[(int)Global.Anim.DEAD]);
        }
        else 
        {
            //Global.mode = Global.Mode.EnemyTurn;
            //map.ChangeEnemyTurn();
            Global.SetMode(Global.Mode.EnemyTurn);
            subMode = SubMode.InputMove;
        }
    }

    // ゲームオーバー
    void Over()
    {
        SceneManager.LoadScene(nextScene);
    }

    // 隠し通路
    void Passage()
    {
        // 一定時間を掛けて通路を形成していく
        switch(dir)
        {
            case Global.Dir.Up:
                passY--;
                break;
            case Global.Dir.Right:
                passX++;
                break;
            case Global.Dir.Down:
                passY++;
                break;
            case Global.Dir.Left:
                passX--;
                break;
        }
        if (map.CheckPassage(mapX + passX, mapY + passY))
        {
            waitTime = Define.passOpenTime;
            map.OpenPassage(mapX + passX, mapY + passY);
            AudioManager.Instance.PlaySE(AUDIO.SE_PASSAGE);
        }
        else
        {
            passX = 0;
            passY = 0;
            subMode = SubMode.InputMove;
        }
    }

    // 落下中
    void FallScale()
    {
        an.Play(Define.AnName[(int)Global.Anim.HIT]);
        emark.SetActive(false);
        AudioManager.Instance.PlaySE(AUDIO.SE_FALL);
        transform.DOScale(0.25f, Define.fallTime);
        sp.DOFade(0.5f, Define.fallTime);
        transform.DOLocalMove(new Vector3(this.transform.position.x, this.transform.position.y - 0.5f), Define.fallTime);
    }

    // ステイタス
    public void DrawStatus()
    {   // フェード中はステイタス表示を行わない
        if(mask.color.a == 0)
        {
            if (bakHp != hp)
            {
                AudioManager.Instance.PlaySE(AUDIO.SE_PICO);
                bakHp = hp;
            }
            int a = hp;
            if (a < 0){ a = 0;}
            status.text = "HP " + a.ToString() + "/" + maxHp.ToString() + "\nゴールド 0$";
        }
        else
        {
            status.text = "";
        }
    }

    // ステイタス
    public void HideStatus()
    {
        status.text = "";
    }

    // 初期位置設定
    public int SetStartPosition()
    {
        if (bFallDamage) { FallDamageStart(Define.damageFall); }

        Global.bStairs = false;
        sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 1);
        shadow.SetActive(true);

        // スケールを元に戻す
        transform.localScale = scaleBak;

        // 部屋作成
        int numStartRoom = map.SetCreateRoom(false);

        // 初期位置設定
        for (int x = 0; x < Define.aryHomeX.Length; x++)
        {
            for (int y = 0; y < Define.aryHomeY.Length; y++)
            {
                if (!map.CheckBlock(numStartRoom, Define.aryHomeX[x], Define.aryHomeY[y]))
                {
                    int startX = Define.aryHomeX[x];
                    int startY = Define.aryHomeY[y];
                    if (Global.Debug.startX != -1) { startX = Global.Debug.startX; }
                    if (Global.Debug.startY != -1) { startY = Global.Debug.startY; }
                    mapX = (numStartRoom % Global.Map.szX) * Global.Room.szX + startX;
                    mapY = (numStartRoom / Global.Map.szX) * Global.Room.szY + startY;
                    transform.position = RoomToSprite(startX, startY);
                    x = Define.aryHomeX.Length;
                    break;
                }
            }
        }
        map.SetBrighten(mapX, mapY);
        qmark.SetActive(map.CheckArround(mapX, mapY));

        #if UNITY_EDITOR
            infoText.text = "(R:" + map.GetStayRoom().ToString() + ", " + mapX.ToString() + "," + mapY.ToString() + ")";
        #endif

        // 階段を設置(プレイヤーがいる部屋は設置しない)
        for (;;)
        {
            int a = map.GetAnyRoom();
            if (numStartRoom != a)
            {
                Debug.Log("StartRoom:" + numStartRoom.ToString() + ",Stairs:" + a.ToString());
                map.SetStairs(a);
                break;
            }
        }
        // フェードイン
        mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 0);

        // 出口までのルートが確保されているか調べる
        int b = map.CheckRoute(1000, mapX, mapY, map.GetXStiars(), map.GetYStiars());
        #if UNITY_EDITOR
            Debug.Log("Cost:" + b.ToString());
            if(b > 0)
            {   // 出口までの経路を表示する
                map.DrawRoute(mapX, mapY, map.GetXStiars(), map.GetYStiars());
            }
        #endif

        // マークを出す必要がある場合は出す
        QMark();
        EMark();

        return b;
    }

    // 選択開始
    void SetupSelect()
    {
        AudioManager.Instance.PlaySE(AUDIO.SE_SELECT);
        btl.SetupBattle(mapX + moveX, mapY + moveY);
		subMode = SubMode.InputMove;
	}
}

