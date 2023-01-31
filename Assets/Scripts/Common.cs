using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using Rewired;


namespace Common
{

	public class Global
	{
		public static class Debug
		{
			public static readonly int startX = 3;          // スタート位置(-1 ならランダム)
			public static readonly int startY = 5;
			public static readonly int startRoom = 0;       // スタートする部屋番号(-1 ならランダム)
		}

		public  enum Dir
        {
            None = -1,
            Up, Right, Down, Left
        };

        public enum Key { up, down, left, right, ok, cancel, any };

        public enum Mode { None, Start, PlayerTurn, EnemyTurn, StairsDown, TBox, Battle, Message, MessageWait, Item};

        public enum Anim { IDLE, WALK, HIT, DEAD, ATTACK };

		public enum EnemyType
		{
			TBox,
			Police,
		};

		// #define
		public static class Define
        {
            public static readonly int   offsetX  = -5;     // タイルマップの左上を(0,0)とする補正値
            public static readonly int   offsetY  = +4;
            public static readonly float szTile   = 0.64f;  // タイルサイズ
            public static readonly float homeX    = -2.88f;
            public static readonly float homeY    =  2.94f;
            public static readonly float FadeTime = 1.0f;   // フェード時間
        }
        // 部屋
        public static class Room
        {
            public static readonly int szX  = 10;
            public static readonly int szY  = 8;
            public static readonly int size = szX * szY;
        }
        // マップ
        public static class Map
        {
            // 部屋が縦と横に並んでいる数
            public static readonly int szX  = 8;
            public static readonly int szY  = 6;
            public static readonly int size = szX * szY;
        }
        // 全体マップのサイズ
        public static class All
        {
            public static readonly int szX  = Room.szX  * Map.szX;
            public static readonly int szY  = Room.szY  * Map.szY;
            public static readonly int size = Room.size * Map.size;
        }
		// アイテム
		public static class Item
		{
			public static readonly int size = 10;
			public static readonly int szX  = 5;
			public static readonly int szY  = 2;
		}

		// ここからグローバル変数
		public static bool   bFade = false;             // true : フェード中
        public static Mode   mode, nextMode;            // 動作モード
        public static float  waitTime;                  // ウエイト
        public static int    floor;                     // フロア
        public static bool   bStairs;                   // 階段で移動
		public static bool   bBgm = true;				// BGM スイッチ
        public static string message = "";              // 処理を止めて表示するメッセージ
        public static bool   bChangeMode = false;       // true : モードを変えた瞬間



        public static GameObject debug { get; set; }           // デバッグ用




        #if UNITY_EDITOR
            public static bool bDebug = false;
        #else
            public static   bool    bDebug  = false;
        #endif

        public static void SetMessage(string str)
        {
            message = str;
            Global.SetMode(Global.Mode.Message);
        }

        // 点滅
        public static bool GetBlink(int counter)
        {
            return (counter % 240) <= 80;
        }

        // キーの状態を調べる
        public static bool CheckKey(int id, Key key)
        {
            const float threshold = 0.1f;               // アナログスティックの閾値

            bool bRet = false;
            string a = "Player" + id.ToString();

            switch (key)
            {
                case Key.up:
                    bRet = (ReInput.players.GetPlayer(a).GetAxis("Move_Vertical") > threshold);
                    break;
                case Key.down:
                    bRet = (ReInput.players.GetPlayer(a).GetAxis("Move_Vertical") <= -threshold);
                    break;
                case Key.left:
                    bRet = (ReInput.players.GetPlayer(a).GetAxis("Move_Horizontal") <= -threshold);
                    break;
                case Key.right:
                    bRet = (ReInput.players.GetPlayer(a).GetAxis("Move_Horizontal") > threshold);
                    break;
                case Key.ok:
                    bRet = ReInput.players.GetPlayer(a).GetButtonDown("Ok");
                    break;
                case Key.cancel:
                    bRet = ReInput.players.GetPlayer(a).GetButtonDown("Cancel");
                    break;
                case Key.any:
					if(ReInput.players.GetPlayer(a).GetButtonDown("Ok")) { bRet = true;}
					if (ReInput.players.GetPlayer(a).GetButtonDown("Cancel")){  bRet = true; }
                    break;
            }
            return bRet;
        }

        // キーの押した瞬間を調べる
        public static bool CheckPressKey(int id, Key key)
        {

            bool bRet = false;
            string a = "Player" + id.ToString();

            switch (key)
            {
                case Key.up:
                    bRet = (ReInput.players.GetPlayer(a).GetButtonDown("MoveVertical"));
                    break;
                case Key.down:
                    bRet = (ReInput.players.GetPlayer(a).GetNegativeButtonDown("MoveVertical"));
                    break;
                case Key.left:
                    bRet = (ReInput.players.GetPlayer(a).GetNegativeButtonDown("MoveHorizontal"));
                    break;
                case Key.right:
                    bRet = (ReInput.players.GetPlayer(a).GetButtonDown("MoveHorizontal"));
                    break;
                case Key.ok:
                    bRet = (ReInput.players.GetPlayer(a).GetButtonDown("Ok"));
                    break;
                case Key.cancel:
                    bRet = (ReInput.players.GetPlayer(a).GetButtonDown("Cancel"));
					break;
				case Key.any:
					if (ReInput.players.GetPlayer(a).GetButtonDown("Ok")){ bRet = true; }
					if (ReInput.players.GetPlayer(a).GetButtonDown("Cancel")){ bRet = true; }
					break;
            }
            return bRet;
        }

        // モード変更
        public static void SetMode(Mode m, float wait = 0)
        {
            nextMode = m;
            waitTime = wait;
        }

        public static void SetWait(float wait)
        {
            waitTime = wait;
        }


		///【機能】マルチプルスプライトからスライスしたスプライトを取得する
		///【第1引数】スプライトファイル名（正確にはResources フォルダからのスプライトファイルまでのパス）
		///【第2引数】取得したいスライスされたスプライト名
		///【戻り値】取得したスプライト
		public static Sprite GetSprite(string fileName, string spriteName)
		{
			Sprite[] sprites = Resources.LoadAll<Sprite>(fileName);
			return System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(spriteName));
		}
	}
}
