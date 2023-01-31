using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using DG.Tweening;


public class BattleManager : MonoBehaviour
{

    public enum BtlMode { Select,           PlayerAttack, PlayerAttackWait,
						  EnemyAttackStart, EnemyAttack,  EnemyAttackWait};

	private CursorManager  cur;
	private BtlMode        btlMode;
	private CameraShake    shake;
	private PlayerManager  ply;
	private EnemyManager   ene;
	private int            enemyID = -1;
	private GameObject	   dam;
	private Text		   damText;
	private GameObject     exp;
	private Animator       expAnim;
	private ItemmapManager itemMap;

	void Start()
	{
		cur     = GameObject.Find("Cursor").GetComponent<CursorManager>();
		shake   = GameObject.Find("Main Camera").GetComponent<CameraShake>();
		ene     = GameObject.Find("Main").GetComponent<EnemyManager>();
		ply	    = GameObject.Find("Player").GetComponent<PlayerManager>();
		dam     = GameObject.Find("Damage");
		itemMap = GameObject.Find("TilemapItem").GetComponent<ItemmapManager>();
		damText = dam.GetComponent<Text>();
		OffDamageText();
		exp     = GameObject.Find("Explosion");
		expAnim = exp.GetComponent<Animator>();
		StopExp();
	}

	void SetMode(BtlMode m, float wait = 0)
	{
		btlMode = m;
		Global.SetWait(wait);
	}

	// 準備
	public void SetupBattle(int x, int y, bool bEnemyAttack = false)
    {
		enemyID = ene.GetNumFromPos(x, y);
		cur.SetTarget(enemyID);
		Global.SetMode(Global.Mode.Battle);
		if (!bEnemyAttack)
		{
			cur.SetEnemyPos();
			SetMode(BtlMode.Select);
		}
		else
		{
			cur.SetPlayerPos();
			SetMode(BtlMode.EnemyAttack);
			Global.SetWait(1.0f);
		}
		if (ply.GetRoomPos().x > ene.work[enemyID].GetRoomPos().x)
		{
			ply.SetDir(Global.Dir.Left);
			ene.work[enemyID].SetDir(Global.Dir.Right);
		}
		else if (ply.GetRoomPos().x < ene.work[enemyID].GetRoomPos().x)
		{
			ply.SetDir(Global.Dir.Right);
			ene.work[enemyID].SetDir(Global.Dir.Left);
		}
	}

	// バトル
	public void UpdateBattle()
    {
		switch (btlMode)
		{
			//  選択
			case BtlMode.Select:
				Select();
				break;
			// プレイヤー攻撃
			case BtlMode.PlayerAttack:
				PlayerAttack();
				break;
			// プレイヤー攻撃モーション待ち
			case BtlMode.PlayerAttackWait:
				PlayerAttackWait();
				break;
			
			// エネミー攻撃準備
			case BtlMode.EnemyAttackStart:
				EnemyAttackStart();
				break;
			// エネミー攻撃
			case BtlMode.EnemyAttack:
				EnemyAttack();
				break;
			// エネミー攻撃モーション待ち
			case BtlMode.EnemyAttackWait:
				EnemyAttackWait();
				break;
		}
	}

	// ダメージ表示
	void SetDamageText(int damage, bool bPlayer)
	{
		if(bPlayer)
		{
			dam.transform.position = ply.transform.position;
		}
		else
		{
			dam.transform.position = ene.GetSpritePos(enemyID);
		}
		damText.text = damage.ToString();
		dam.GetComponent<BlinkManager>().Reset();
	}
	void OffDamageText()
	{
		damText.transform.position = new Vector2(-1000, 0);
	}
	void SetDamageTextAlpha(float alp)
	{
		damText.color = new Vector4(damText.color.r, damText.color.g, damText.color.b, alp);
	}

	// 爆発
	void StopExp()
	{
		expAnim.enabled = false;
		exp.SetActive(false);
	}
	void PlayExp(Vector2 pos)
	{
		expAnim.enabled = true;
		exp.SetActive(true);
		expAnim.Play("exp");
		exp.transform.position = new Vector2(pos.x, pos.y - 0.16f);
	}

	// Player Step1
	void Select()
	{
		if (Global.CheckPressKey(0, Global.Key.ok))
		{
			if(ene.GetType(enemyID) == Global.EnemyType.TBox)
			{	// 宝箱ならここで終了
				ene.Erase(enemyID);
				cur.Hide();
				itemMap.SetTile(ene.GetMapPos(enemyID).x, ene.GetMapPos(enemyID).y, 1);
				AudioManager.Instance.PlaySE(AUDIO.SE_TBOX);
				Global.SetMode(Global.Mode.PlayerTurn);
			}
			else
			{	// エネミー選択後は攻撃アニメ再生
				ply.SetAnim(Global.Anim.ATTACK);
				AudioManager.Instance.PlaySE(AUDIO.SE_ATTACK);
				SetMode(BtlMode.PlayerAttack);
			}
		}
		else if (Global.CheckPressKey(0, Global.Key.cancel))
		{	// キャンセル
			AudioManager.Instance.PlaySE(AUDIO.SE_CANCEL);
			cur.Hide();
			OffDamageText();
			Global.SetMode(Global.Mode.PlayerTurn);
		}
	}

	// Player Step2
	void PlayerAttack()
	{
		// プレイヤーアニメーション再生チェック
		AnimatorStateInfo stateInfo = ply.an.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.normalizedTime < 1.0f){ return;}
		// エネミーダメージアニメ再生
		shake.Shake(0.50f, 0.1f);
		AudioManager.Instance.PlaySE(AUDIO.SE_DAMAGE);
		SetDamageText(7, false);
		ene.SetAnim(enemyID, Global.Anim.HIT);
		PlayExp(ene.GetSpritePos(enemyID));
		SetMode(BtlMode.PlayerAttackWait);
	}

	// Player Step3
	void PlayerAttackWait()
	{
		// エネミーアニメーション再生チェック
		if (ene.CheckAnimPlay(enemyID)) { return;}

		StopExp();
		ene.SetAnim(enemyID, Global.Anim.IDLE);
		OffDamageText();
		SetMode(BtlMode.EnemyAttackStart);
	}

	// Enemy Step1
	void EnemyAttackStart()
	{
		cur.SetPlayerPos();
		ene.SetAnim(enemyID, Global.Anim.ATTACK);
		AudioManager.Instance.PlaySE(AUDIO.SE_ATTACK);
		SetMode(BtlMode.EnemyAttack);
	}

	// Enemy Step2
	void EnemyAttack()
	{
		// アニメーション再生チェック
		if (ene.CheckAnimPlay(enemyID)) { return;}

		// プレイヤーダメージアニメ再生
		shake.Shake(0.50f, 0.1f);
		AudioManager.Instance.PlaySE(AUDIO.SE_DAMAGE);
		SetDamageText(8, true);
		PlayExp(ply.GetSpritePos());
		ply.SetAnim(Global.Anim.HIT);
		SetMode(BtlMode.EnemyAttackWait);
	}

	// Enemy Step3
	void EnemyAttackWait()
	{
		// プレイヤーアニメーション再生チェック
		AnimatorStateInfo stateInfo = ply.an.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.normalizedTime < 1.0f) { return; }

		ply.SetAnim(Global.Anim.IDLE);
		OffDamageText();
		StopExp();
		cur.Hide();
		Global.SetMode(Global.Mode.PlayerTurn);
	}
}

