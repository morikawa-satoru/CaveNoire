using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Common;
using DG.Tweening;
using System;
using System.Linq;


public class MapManager : MonoBehaviour
{
    private class Define
    {
        public static readonly float  homeX        = 0;                 // ホームポジション
        public static readonly float  homeY        = -0.32f;
        public static readonly float  screenX      = 6.4f;
        public static readonly float  screenY      = 5.12f;
        public static readonly float  speedWater   = 0.05f;             // 水の流れる速度
        public static readonly float  speedDark    = 0.025f;            // 闇の流れる速度
        public static readonly float  speedFlare   = 0.010f;            // 炎の速度
        public static readonly int    RateSecretPassage = 40;           // 通路が隠しになる確率

        public static readonly int    maxTile      = 100;               // 読み込みタイル総数
        public static readonly int    numDokan     = 3;                 // 土管
        public static readonly int    maxDokan     = 7;                 // 土管の数
        public static readonly int    numCanWalk   = 11;                // 歩ける床番号
        public static readonly int    numPassage   = 44;                // 通路番号
        public static readonly int    numStiars    = 21;                // 階段番号
        public static readonly int    numWater     = 4;                 // 水(床)
        public static readonly int    numWaterPat  = 50;                // 水のパターン
        public static readonly int    maxWaterPat  = 8;                 // 水のパターン数
        public static readonly int    numDark      = 60;                // 闇
        public static readonly int    maxDarkPat   = 8;                 // 闇のパターン数
        public static readonly int    numFlare     = 15;                // 炎
        public static readonly int    maxFlare     = 2;                 // 炎のタイル数
        public static readonly int    numFlarePat  = 70;                // 炎のパターン
        public static readonly int    maxFlarePat  = 4;                 // 炎のパターン数
        public static readonly int    numPath      = 97;                // 通れるところ
        public static readonly string nameHead     = "tile0_";          //
        public static readonly string nameTileHead = "MapTile/tile0_";  // タイルの読み込み先と冒頭のファイル名

        public static readonly string nameWater = "TilemapWater";
        public static readonly string nameDark  = "TilemapDark";
        public static readonly string namePath  = "TilemapPass";
        public static readonly string nameRoute = "TilemapRoute";

        // 部屋を繋げるときの調べる順
        public static readonly int[] arySearchX = { 4, 5, 3, 6, 2, 7, 1, 8 };
        public static readonly int[] arySearchY = { 3, 4, 2, 5, 1, 6 };
        //public static readonly int[] arySearchY = { 5, 5, 5, 5, 5, 5 };
        // 床
        public static readonly int[] aryFloor = { 11, 21, 4, 13, 23, 24, 25, 44 };
        // 落ちる床も含めた床
        public static readonly int[] aryFloor2 = { 11, 21, 4, 13, 15, 16, 23, 24, 25, 31, 43, 44 };
        //public static readonly int[] aryFloor2 = { 11, 21, 4, 13, 23, 24, 25, 31, 43, 44 };
        // 壁
        public static readonly int[] aryWall  = { 0, 1, 2, 3, 5, 6, 7, 8, 9, 10, 12, 14, 20, 22, 30, 32, 40, 41, 42, 98 };
        // 壁
        public static readonly int[] aryFixWall = { 1, 22, 41, 20, 4, 6, 41, 20};
        // 壁
        public static readonly int[] aryFixCorner = { 10, 12, 30, 32,   6, 7, 8, 9};
        // 階段
        public static readonly int[] aryStairsX = { 4, 5, 3, 6, 2, 7, 1, 8 };
        public static readonly int[] aryStairsY = { 5, 6, 4, 3, 2, 1 };
        // 落下
        public static readonly int[] aryFall = { 31, 43};
        // 溶岩
        public static readonly int[] aryMagma = { 15, 16 };
        // 壁水路
        public static readonly int[] aryWater = {3, 4, 5 };
        // 部屋の大きさ                              B1 B2 B3 B4 B5 B6 B7 B8 B9 B10 B11 B12 B13 B14 B15 B16 B17 B18 B19 B20 B21 B22 B23～
        public static readonly int[] aryRoomXSize = { 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6,  6,  6,  6,  7,  7,  7,  7,  8,  8,  8};
        public static readonly int[] aryRoomYSize = { 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5,  5,  6,  6,  6,  6,  7,  7,  7,  7,  8};
    }

    // 部屋クラス
    private class CRoom
    {
        public bool[] block  = new bool[Global.Room.size];
        public int[]  buf    = new int[ Global.Room.size];
        public bool[] pass   = new bool[Global.Room.size];
        public int[]  secret = new int[ Global.Room.size];
        public bool[] dark   = new bool[Global.Room.size];
        public int[]  route  = new int[ Global.Room.size];
    }
    // 全体マップ
    private class CMap
    {
        public bool[] block   = new bool[Global.Room.size * Global.Map.size];
        public int[]  buf     = new int[ Global.Room.size * Global.Map.size];
        public bool[] pass    = new bool[Global.Room.size * Global.Map.size];
        public int[]  secret  = new int[ Global.Room.size * Global.Map.size];
        public bool[] dark    = new bool[Global.Room.size * Global.Map.size];
        public int[]  route   = new int[ Global.Room.size * Global.Map.size];
        public int[]  aryRoom = new int[ Global.Map.size];
        public int    cntRoom = 0;                              // 使用部屋数
        public int    stairsX, stairsY;                         // 階段の設置座標
        public float  WaterCount = 0;                           // 水の流れアニメーション用
        public float  DarkCount  = 0;                           // 暗闇アニメーション用
        public float  FlareCount = 0;                           // 炎アニメーション用
        public bool   bDark = false;                            // true : 暗闇モード
        public int    stayRoom = 0;                             // 表示中の部屋
    }

    // 変数
    private GameObject     path;
    private Tilemap        bg, pass, water, dark, route;
    private TileBase[]     tile    = new TileBase[Define.maxTile]; // マップのタイルが入る
    private CRoom[]        oriRoom = new CRoom[Global.Map.size];   // 部屋
    private CRoom[]        room    = new CRoom[Global.Map.size];   // 実際に移動する部屋
    private CMap           map     = new CMap();                   // 全体マップ
    private PlayerManager  ply;
    private EnemyManager   ene;
    //private ItemMapManager itemMap;

    public void Init()
    {
        bg      = GetComponent<Tilemap>();
        pass    = GameObject.Find(Define.namePath).GetComponent<Tilemap>();
        water   = GameObject.Find(Define.nameWater).GetComponent<Tilemap>();
        dark    = GameObject.Find(Define.nameDark).GetComponent<Tilemap>();
        path    = GameObject.Find(Define.namePath);
        route   = GameObject.Find(Define.nameRoute).GetComponent<Tilemap>();
        ply     = GameObject.Find("Player").GetComponent<PlayerManager>();
        ene     = GameObject.Find("Main").GetComponent<EnemyManager>();
        //itemMap = GameObject.Find("TilemapItem").GetComponent<ItemMapManager>();

        // タイル情報を得る
        for (int i = 0; i < Define.maxTile; i++)
        {
            tile[i] = Resources.Load<Tile>(Define.nameTileHead + i.ToString());
        }
        // 部屋データ取得
        for(int i = 0; i < Global.Map.size; i++)
        {
            oriRoom[i] = new CRoom();
            ReadRoom(i);
        }
        // とりあえず仮に部屋を置く
        for (int i = 0; i < Global.Map.size; i++)
        {
            room[i] = new CRoom();
            CopyRoom(i, i);
        }
        // エネミー生成
        ene.CreateWork();

    }

    // タイルマップより部屋データを作成する
    void ReadRoom(int numRoom)
    {
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
        for (int y = 0; y < Global.Room.szY; y++)
        {
            for (int x = 0; x < Global.Room.szX; x++)
            {
                int         adr = GetRoomAdr(x, y);
                TileBase    b   = GetTile(xx + x, yy + y);
                // タイル番号を得る
                oriRoom[numRoom].buf[adr] = int.Parse(b.name.Substring(Define.nameHead.Length));
                // 床判定
                oriRoom[numRoom].block[adr] = !CheckFloor(oriRoom[numRoom].buf[adr]);
                // 通路
                oriRoom[numRoom].pass[adr] = pass.GetTile(new Vector3Int((xx + x) + Global.Define.offsetX, -(yy + y) + Global.Define.offsetY, 0)) != null;
            }
        }
    }

    // 床判定
    bool CheckFloor(int numTile)
    {
        return Array.IndexOf(Define.aryFloor, numTile) > -1;    // こうすると配列の検索ができるらしい
    }
    bool CheckFloor(int numRoom, int x, int y)
    {
        return CheckFloor(GetTileNum(numRoom, x, y));
    }
    bool CheckFloor(int x, int y)
    {
        return CheckFloor(GetTileNum(x, y));
    }
    bool CheckFloor2(int numTile)
    {
        return Array.IndexOf(Define.aryFloor2, numTile) > -1;
    }
    bool CheckFloor2(int numRoom, int x, int y)
    {
        return CheckFloor2(GetTileNum(numRoom, x, y));
    }
    bool CheckFloor2(int x, int y)
    {
        return CheckFloor2(GetTileNum(x, y));
    }

    // 落下
    public bool CheckFall(int x, int y)
    {
        return Array.IndexOf(Define.aryFall, GetTileNum(x, y)) > -1;
    }
    public bool CheckFall(int numRoom, int x, int y)
    {
        return Array.IndexOf(Define.aryFall, GetTileNum(numRoom, x, y)) > -1;
    }

    // 水判定
    public bool CheckWater(int numRoom, int x, int y)
    {
        return GetTileNum(numRoom, x, y) == Define.numWater;
    }
    public bool CheckWater(int x, int y)
    {
        return GetTileNum(x, y) == Define.numWater;
    }

    // 壁判定
    bool CheckWall(int numTile)
    {
        return Array.IndexOf(Define.aryWall, numTile) > -1;
    }
    bool CheckWall(int numRoom, int x, int y)
    {
        return CheckWall(GetTileNum(numRoom, x, y));
    }
    bool CheckWall(int x, int y)
    {
        return CheckWall(GetTileNum(x, y));
    }
    // 一部壁を除いた判定
    bool CheckWall2(int numTile)
    {
        return Array.IndexOf(Define.aryWall, numTile) > -1;
    }
    bool CheckWall2(int x, int y)
    {
        return CheckWall2(GetTileNum(x, y));
    }

    // アドレスを得る
    int GetRoomAdr(int x, int y)
    {
        return y * Global.Room.szX + x;
    }
    int GetMapAdr(int x, int y)
    {
        return y * Global.All.szX + x;
    }

    int GetMapAdr(int numRoom, int x, int y)
    {
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
        return (yy + y) * Global.All.szX + (xx + x);
    }

    // タイルマップからタイルを得る
    TileBase GetTile(int x, int y)
    {
		return bg.GetTile(new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0));
    }

    // タイルマップにタイルを設定する
    void SetTile(int numRoom, int x, int y, int numTile)
    {
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
        Vector3Int pos = new Vector3Int(xx + x + Global.Define.offsetX, -yy - y + Global.Define.offsetY, 0);
        bg.SetTile(pos, tile[numTile]);
        room[numRoom].buf[GetRoomAdr(x, y)] = numTile;
        map.buf[GetMapAdr(xx + x, yy + y)]  = numTile;
    }
    void SetTile(int x, int y, int numTile)
    {
        int xx      = (x % Global.Room.szX);
        int yy      = (y % Global.Room.szY);
        int numRoom = (y / Global.Room.szY) * Global.Map.szX + (x / Global.Room.szX);
        Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
        bg.SetTile(pos, tile[numTile]);
        room[numRoom].buf[GetRoomAdr(xx, yy)] = numTile;
        map.buf[GetMapAdr(x, y)] = numTile;
    }

    // タイルマップにタイルを設定する
    void SetPath(int numRoom, int x, int y, bool flag)
    {
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
        Vector3Int pos = new Vector3Int(xx + x + Global.Define.offsetX, -yy - y + Global.Define.offsetY, 0);
        if(flag)
        {
            pass.SetTile(pos, tile[Define.numPath]);
        }
        else
        {
            pass.SetTile(pos, null);
        }
    }

    // 障害設定
    void SetBlock(int numRoom, int x, int y, bool flag)
    {
        room[numRoom].block[GetRoomAdr(x, y)] = flag;
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
        map.block[GetMapAdr(xx + x, yy + y)] = flag;
    }
    void SetBlock(int x, int y, bool flag)
    {
        int xx      = (x % Global.Room.szX);
        int yy      = (y % Global.Room.szY);
        int numRoom = (y / Global.Room.szY) * Global.Map.szX + (x / Global.Room.szX);
        room[numRoom].block[GetRoomAdr(xx, yy)] = flag;
        map.block[GetMapAdr(x, y)] = flag;
    }

    // タイルを調べる
    int GetTileNum(int numRoom, int x, int y)
    {
        return room[numRoom].buf[GetRoomAdr(x, y)];
    }
    int GetTileNum(int x, int y)
    {
        return map.buf[GetMapAdr(x, y)];
    }

    // 障害物があるか調べる
    public bool CheckBlock(int numRoom, int x, int y)
    {
        return (room[numRoom].block[GetRoomAdr(x, y)] && !CheckFall(numRoom, x, y));
    }
    public bool CheckBlock(int x, int y)
    {
        return (map.block[GetMapAdr(x, y)] && !CheckFall(x, y));
    }

    // パスを調べる
    public bool CheckPath(int numRoom, int x, int y)
    {
        return room[numRoom].pass[GetRoomAdr(x, y)];
    }

    // 階段か調べる
    public bool CheckStiars(int numRoom, int x, int y)
    {
        return room[numRoom].buf[GetRoomAdr(x, y)] == Define.numStiars;
    }
    public bool CheckStiars(int x, int y)
    {
        return map.buf[GetMapAdr(x, y)] == Define.numStiars;
    }
    public int GetXStiars()
    {
        return map.stairsX;
    }
    public int GetYStiars()
    {
        return map.stairsY;
    }

    // 表示中の部屋を得る
    public int GetStayRoom()
    {
        return map.stayRoom;
    }
    
    // 部屋をコピーする
    void CopyRoom(int dstRoom, int srcRoom)
    {
        int dstX = (dstRoom % Global.Map.szX) * Global.Room.szX;
        int dstY = (dstRoom / Global.Map.szX) * Global.Room.szY;
        for (int y = 0; y < Global.Room.szY; y++)
        {
            for (int x = 0; x < Global.Room.szX; x++)
            {
                int adr = GetRoomAdr(x, y);
                SetTile(dstRoom, x, y, oriRoom[srcRoom].buf[adr]);
                room[dstRoom].block[adr] = oriRoom[srcRoom].block[adr];
                room[dstRoom].buf[  adr] = oriRoom[srcRoom].buf[  adr];
                // 全体マップ
                int adr2 = GetMapAdr(dstX + x, dstY + y);
                map.block[ adr2] = oriRoom[srcRoom].block[adr];
                map.buf[   adr2] = oriRoom[srcRoom].buf[  adr];
                map.dark[  adr2] = map.bDark;
                map.secret[adr2] = 0;
                // 通路
                SetPath(dstRoom, x, y, oriRoom[srcRoom].pass[adr]);
                room[dstRoom].pass[adr] = oriRoom[srcRoom].pass[adr];
            }
        }
    }

    // 部屋つなげる
    public void ConnectRoom(int numRoom, Global.Dir direction, bool bSecretPassage)
    {
        // 配列をシャッフル
        int[] aryX = Define.arySearchX.OrderBy(i => Guid.NewGuid()).ToArray();  // この一行でシャッフルできるらしい
        int[] aryY = Define.arySearchY.OrderBy(i => Guid.NewGuid()).ToArray();

        // 通路を広げるようにして接続可能か調べる
        switch (direction)
        {
            case Global.Dir.Up:
                break;
            case Global.Dir.Right:
                // 縦に調べる
                for (int y = 0; y < aryY.Length; y++)
                {
                    int yyy = aryY[y];
                    // 通路は横に広げる
                    for (int x = 0; x < Global.Room.szX / 2 - 1; x++)
                    {
                        if (CheckPath(numRoom, Global.Room.szX - 1 - x, yyy) && CheckPath(numRoom + 1, x, yyy))
                        {
                            for(int xx = 0; xx < Global.Room.szX / 2; xx++)
                            {
                                // 左の部屋
                                if (CheckPath(numRoom, Global.Room.szX - 1 - x - xx, yyy))
                                {
                                    if(bSecretPassage)
                                    {
                                        map.secret[GetMapAdr(numRoom, Global.Room.szX - 1 - x - xx, yyy)] = Define.numPassage;
                                    }
                                    else
                                    {
                                        SetTile( numRoom, Global.Room.szX - 1 - x - xx, yyy, Define.numCanWalk);
                                        SetBlock(numRoom, Global.Room.szX - 1 - x - xx, yyy, false);
                                        FixMapUpDown(numRoom, Global.Room.szX - 1 - x - xx, yyy);
                                    }
                                }
                                // 右の部屋
                                if (CheckPath(numRoom + 1, x + xx, yyy))
                                {
                                    if (bSecretPassage)
                                    {
                                        map.secret[GetMapAdr(numRoom + 1, x + xx, yyy)] = Define.numPassage;
                                    }
                                    else
                                    {
                                        SetTile( numRoom + 1, x + xx, yyy, Define.numCanWalk);
                                        SetBlock(numRoom + 1, x + xx, yyy, false);
                                        FixMapUpDown(numRoom + 1, x + xx, yyy);
                                    }
                                }
                            }
                            return;
                        }
                        // 通路が無くて壁があるならばら諦める
                        else if (CheckWall(numRoom, Global.Room.szX - 1 - x, yyy) || CheckPath(numRoom + 1, x, yyy))
                        {
                            x = Global.Room.szX / 2 - 1;
                            break;
                        }
                    }
                }
                break;
            case Global.Dir.Down:
                // 横に調べる
                for (int x = 0; x < aryX.Length; x++)
                {
                    int xxx = aryX[x];
                    // 通路を上下に広げる
                    for (int y = 0; y < Global.Room.szY / 2 - 1; y++)
                    {
                        if (CheckPath(numRoom, xxx, Global.Room.szY - 1 - y) && CheckPath(numRoom + Global.Map.szX, xxx, y))
                        {
                            for (int yy = 0; yy < Global.Room.szY / 2; yy++)
                            {
                                // 上の部屋
                                if (CheckPath(numRoom, xxx, Global.Room.szY - 1 - y - yy))
                                {
                                    if (bSecretPassage)
                                    {
                                        map.secret[GetMapAdr(numRoom, xxx, Global.Room.szY - 1 - y - yy)] = Define.numPassage;
                                    }
                                    else
                                    {
                                        SetTile(     numRoom, xxx, Global.Room.szY - 1 - y - yy, Define.numCanWalk);
                                        SetBlock(    numRoom, xxx, Global.Room.szY - 1 - y - yy, false);
                                        FixMapUpDown(numRoom, xxx, Global.Room.szY - 1 - y - yy);
                                    }
                                }
                                // 下の部屋
                                if (CheckPath(numRoom + Global.Map.szX, xxx, yy))
                                {
                                    if (bSecretPassage)
                                    {
                                        map.secret[GetMapAdr(numRoom + Global.Map.szX, xxx, yy)] = Define.numPassage;
                                    }
                                    else
                                    {
                                        SetTile(     numRoom + Global.Map.szX, xxx, yy, Define.numCanWalk);
                                        SetBlock(    numRoom + Global.Map.szX, xxx, yy, false);
                                        FixMapUpDown(numRoom + Global.Map.szX, xxx, yy);
                                    }
                                }
                            }
                            return;
                        }
                        // 通路が無くて壁があるならばら諦める
                        else if (CheckWall(numRoom, xxx, Global.Room.szY - 1 - y) || CheckPath(numRoom + Global.Map.szX, xxx, y))
                        {
                            y = Global.Room.szY / 2 - 1;
                            break;
                        }
                    }
                }
                break;
            case Global.Dir.Left:
                break;
        }
    }

    // マップを成型する
    void FixMapUpDown(int numRoom, int x, int y)
    {   // 通路の上下に壁を作る
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX + x;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY + y;
        // 上の壁
        if(CheckWall(xx, yy - 1))
        {
            SetTile(xx, yy - 1, Define.aryFixWall[(int)Global.Dir.Up]);
        }
        // 下の壁
        if (CheckWall(xx, yy + 1))
        {
            SetTile(xx, yy + 1, Define.aryFixWall[(int)Global.Dir.Down]);
        }
    }
    void FixMapLeftRight(int numRoom, int x, int y)
    {   // 通路の左右に壁を作る
        int xx = (numRoom % Global.Map.szX) * Global.Room.szX + x;
        int yy = (numRoom / Global.Map.szX) * Global.Room.szY + y;
        // 左の壁
        if (CheckWall(xx - 1, yy))
        {
            SetTile(xx - 1, yy, Define.aryFixWall[(int)Global.Dir.Left]);
        }
        // 右の壁
        if (CheckWall(xx + 1, yy))
        {
            SetTile(xx + 1, yy, Define.aryFixWall[(int)Global.Dir.Right]);
        }
    }
    public void FixAllMap()
    {   // 通路ができた後に壁を滑らかにするa
        for (int y = 1; y < Global.All.szY - 1; y++)
        {
            for (int x = 0; x < Global.All.szX; x++)
            {
                bool bTop    = y == 0;
                bool bRight  = x == (Global.All.szX - 1);
                bool bBottom = y == (Global.All.szY - 1);
                bool bLeft   = x == 0;
                int bWater   = Array.IndexOf(Define.aryWater, GetTileNum(x, y)) == -1 ? 0 : 4;    // 水路ならば 4

                if (!bTop && !bRight && !bBottom && !bLeft)
                {
                    // 角を置く
                    if (CheckWall(x, y - 1) && CheckFloor2(x - 1, y) && CheckWall(x, y) && CheckWall(x + 1, y) && CheckFloor2(x, y + 1))
                    {
                        SetTile(x, y, Define.aryFixCorner[1 + bWater]);
                    }
                    else if (CheckWall(x, y - 1) && CheckWall(x - 1, y) && CheckWall(x, y) && CheckFloor2(x + 1, y) && CheckFloor2(x, y + 1))
                    {
                        SetTile(x, y, Define.aryFixCorner[0 + bWater]);
                    }
                    else if (CheckFloor2(x, y - 1) && CheckFloor2(x - 1, y) && CheckWall(x, y) && CheckWall(x + 1, y) && CheckWall(x, y + 1))
                    {
                        SetTile(x, y, Define.aryFixCorner[3 + bWater]);
                    }
                    else if (CheckFloor2(x, y - 1) && CheckWall(x - 1, y) && CheckWall(x, y) && CheckFloor2(x + 1, y) && CheckWall(x, y + 1))
                    {
                        SetTile(x, y, Define.aryFixCorner[2 + bWater]);
                    }
                }

                // 両端が壁ならば角壁も壁にする。両端が壁で水路ならば書き換えない
                if (bWater == 0)
                {
                    if (!bTop && !bLeft && !bBottom && CheckWall2(x, y - 1) && CheckWall2(x, y) && CheckWall2(x, y + 1) && CheckFloor2(x - 1, y))
                    {
                        SetTile(x, y, Define.aryFixWall[(int)Global.Dir.Right]);
                    }
                    else if (!bTop && !bRight && !bBottom && CheckWall2(x, y - 1) && CheckWall2(x, y) && CheckWall2(x, y + 1) && CheckFloor2(x + 1, y))
                    {
                        SetTile(x, y, Define.aryFixWall[(int)Global.Dir.Left]);
                    }
                    else if (!bRight && !bLeft && !bBottom && CheckWall2(x - 1, y) && CheckWall2(x, y) && CheckWall2(x + 1, y) && CheckFloor2(x, y + 1))
                    {
                        SetTile(x, y, Define.aryFixWall[(int)Global.Dir.Up]);
                    }
                    else if (!bTop && !bLeft && !bRight && CheckWall2(x - 1, y) && CheckWall2(x, y) && CheckWall2(x + 1, y) && CheckFloor2(x, y - 1))
                    {
                        SetTile(x, y, Define.aryFixWall[(int)Global.Dir.Down]);
                    }
                }
            }
        }
    }

    // 画面スクロール
    public void Scroll(Global.Dir direction)
    {
        switch (direction)
        {
            case Global.Dir.Up:
                transform.DOLocalMove(new Vector3(transform.position.x, transform.position.y - Define.screenY, 0), ply.GetScrollTime());
                map.stayRoom -= Global.Map.szX;
                break;
            case Global.Dir.Right:
                transform.DOLocalMove(new Vector3(transform.position.x - Define.screenX, transform.position.y, 0), ply.GetScrollTime());
                map.stayRoom++;
                break;
            case Global.Dir.Down:
                transform.DOLocalMove(new Vector3(transform.position.x, transform.position.y + Define.screenY, 0), ply.GetScrollTime());
                map.stayRoom += Global.Map.szX;
                break;
            case Global.Dir.Left:
                transform.DOLocalMove(new Vector3(transform.position.x + Define.screenX, transform.position.y, 0), ply.GetScrollTime());
                map.stayRoom--;
                break;
        }
    }

    // 部屋を作成し指定の部屋へスクロール
    public int SetCreateRoom(bool bDark)
    {
        SetupRoom(bDark);
        int startRoom;
		if(Global.Debug.startRoom == -1){ startRoom = GetAnyRoom(); }
		else {  						  startRoom = Global.Debug.startRoom; }
		float x = Define.homeX - (startRoom % Global.Map.szX) * Define.screenX;
        float y = Define.homeY + (startRoom / Global.Map.szX) * Define.screenY;
        transform.position       = new Vector3(x, y, 0);
        water.transform.position = new Vector3(x, y, 0);
        dark.transform.position  = new Vector3(x, y, 0);
        map.stayRoom = startRoom;

        return startRoom;
    }

    // 部屋データ作成
    void SetupRoom(bool bDark)
    {
        map.bDark = bDark;
        int xMax = Global.floor < Define.aryRoomXSize.Length ? Define.aryRoomXSize[Global.floor] : Define.aryRoomXSize[Global.floor - 1];
        int yMax = Global.floor < Define.aryRoomYSize.Length ? Define.aryRoomYSize[Global.floor] : Define.aryRoomYSize[Global.floor - 1];
        ene.ResetEnemy();

#if false
        // 部屋を作成
        map.cntRoom = 0;
        for (int y = 0; y < yMax; y++)
        {
            for (int x = 0; x < xMax; x++, map.cntRoom++)
            {
                int numRoom = y * Global.Map.szX + x;
                CopyRoom(numRoom, UnityEngine.Random.Range(0, Global.Map.size));
                map.aryRoom[map.cntRoom] = numRoom;
            }
        }

        // 各部屋を接続
        for (int y = 0; y < yMax - 1; y++)
        {
            for (int x = 0; x < xMax - 1; x++)
            {
                int numRoom = y * Global.Map.szX + x;
                ConnectRoom(numRoom, Global.Dir.Right, UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);
                ConnectRoom(numRoom, Global.Dir.Down,  UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);
            }
        }
#else
        map.cntRoom = 4;
        //CopyRoom(0, UnityEngine.Random.Range(0, Global.Map.size));
        //CopyRoom(1, UnityEngine.Random.Range(0, Global.Map.size));
        //CopyRoom(8, UnityEngine.Random.Range(0, Global.Map.size));
        //CopyRoom(9, UnityEngine.Random.Range(0, Global.Map.size));
        CopyRoom(0, 29);
        CopyRoom(1, 6);
        CopyRoom(8, 6);
        CopyRoom(9, 5);
        ConnectRoom(0, Global.Dir.Right, UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);
        ConnectRoom(0, Global.Dir.Down,  UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);
        ConnectRoom(8, Global.Dir.Right, UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);      
        ConnectRoom(1, Global.Dir.Down,  UnityEngine.Random.Range(0, 100) <= Define.RateSecretPassage);

        map.aryRoom[0] = 0;
        map.aryRoom[1] = 1;
        map.aryRoom[2] = 8;
        map.aryRoom[3] = 9;
#endif
		//@@
		ene.work[0].SetUp(8, Global.EnemyType.TBox);
		//ene.work[1].SetUp(1, Global.EnemyType.TBox);
		//ene.work[2].SetUp(8, Global.EnemyType.TBox);
		//ene.work[3].SetUp(9, Global.EnemyType.TBox);

		FixAllMap();

        path.SetActive(false);
    }

    // 存在する部屋からひとつ選ぶ
    public int GetAnyRoom()
    {
        return map.aryRoom[UnityEngine.Random.Range(0, map.cntRoom)];
    }

    // 階段作成
    public void SetStairs(int numRoom)
    {
        // 初期位置設定
        for (int x = 0; x < Define.aryStairsX.Length; x++)
        {
            for (int y = 0; y < Define.aryStairsY.Length; y++)
            {
				// 壁があるか調べる
				if (!CheckBlock(numRoom, Define.aryStairsX[x], Define.aryStairsY[y]))
                {
					int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
					int yy = (numRoom / Global.Map.szX) * Global.Room.szY;

					// そこにエネミーが居たらスキップ
					if (ene.Collision(xx + Define.aryStairsX[x], yy + Define.aryStairsY[y])){ continue;}

					// 階段を設置
					SetTile(numRoom, Define.aryStairsX[x], Define.aryStairsY[y], Define.numStiars);

                    int adr = GetRoomAdr(Define.aryStairsX[x], Define.aryStairsY[y]);
					map.stairsX = Define.aryStairsX[x] + xx;
                    map.stairsY = Define.aryStairsY[y] + yy;
                    room[numRoom].block[adr] = false;
                    room[numRoom].buf[  adr] = Define.numStiars;

                    // 階段は一つあればいいのでここで終わる
                    return;
                }
            }
        }
    }

    // 明るくする
    public void SetBrighten(int x, int y)
    {
        for (int yy = y - 1; yy < y + 2; yy++)
        {
            for (int xx = x - 1; xx < x + 2; xx++)
            {
                if(xx >= 0 && yy >= 0 && xx < Global.All.szX && yy < Global.All.szY)
                {
                    int adr = GetMapAdr(xx, yy);
                    if (map.dark[adr])
                    {
                        map.dark[adr] = false;
                        Vector3Int pos = new Vector3Int(xx + Global.Define.offsetX, -yy + Global.Define.offsetY, 0);
                        dark.SetTile(pos, null);
                    }
                }
            }
        }
    }

    // 周囲を調べる
    public bool CheckArround(int x, int y)
    {
        for (int yy = y - 1; yy < y + 2; yy++)
        {
            for (int xx = x - 1; xx < x + 2; xx++)
            {
                if (xx >= 0 && yy >= 0 && xx < Global.All.szX && yy < Global.All.szY)
                {
                    int adr = GetMapAdr(xx, yy);
                    if (map.secret[adr] > 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // 隠し通路があるか調べる
    public bool CheckPassage(int x, int y)
    {
        return map.secret[GetMapAdr(x, y)] > 0;
    }

    // 隠し通路を表示する
    public void OpenPassage(int x, int y)
    {
        SetTile(x, y, map.secret[GetMapAdr(x, y)]);
        map.secret[GetMapAdr(x, y)] = 0;
        SetBlock(x, y, false);
    }

    // 更新
    void Update()
    {
        // 水
        map.WaterCount += Define.speedWater;
        int a  = Define.numWaterPat + ((int)map.WaterCount & (Define.maxWaterPat - 1));

        int xx = (map.stayRoom % Global.Map.szX) * Global.Room.szX;
        int yy = (map.stayRoom / Global.Map.szX) * Global.Room.szY;
        for (int y = yy; y < yy + Global.Room.szY; y++)
        {
            for (int x = xx; x < xx + Global.Room.szX; x++)
            {
                if ((uint)(GetTileNum(x, y) - Define.numDokan) <= Define.maxDokan)
                {
                     Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
                     water.SetTile(pos, tile[a]);
                }
            }
        }
        water.transform.position = this.transform.position;

        // 闇
        map.DarkCount += Define.speedDark;
        a = Define.numDark + ((int)map.DarkCount & (Define.maxDarkPat - 1));
        for (int y = yy; y < yy + Global.Room.szY; y++)
        {
            for (int x = xx; x < xx + Global.Room.szX; x++)
            {
                if (map.dark[GetMapAdr(x, y)])
                {
                    Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
                    dark.SetTile(pos, tile[a]);
                }
            }
        }
        dark.transform.position = this.transform.position;

        // 炎
        map.FlareCount += Define.speedFlare;
        a = Define.numFlarePat + ((int)map.FlareCount & (Define.maxFlarePat - 1));
        for (int y = yy; y < yy + Global.Room.szY; y++)
        {
            for (int x = xx; x < xx + Global.Room.szX; x++)
            {
                if ((uint)(GetTileNum(x, y) - Define.numFlare) <= Define.maxFlare)
                {
                    Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
                    water.SetTile(pos, tile[a]);
                }
            }
        }
        dark.transform.position = this.transform.position;

        // ルート
        route.transform.position = this.transform.position;
    }

    /*------------------------------------------------------------------------------*
    | <<< 木(ﾓｸ)を使い目標座標をサーチする >>>
    |	入力	iPow   = 移動の残りパワー(０になると再起処理は行わない
    |			iX, iY = いまいる座標
    |	※cMap の内容より achCheck に値を書き込みます。
    |			achCheck  = 0 : そこへはいけない
    |					 != 0 : そこへ行ける
    *------------------------------------------------------------------------------*/
    public void Check(int iPow, int iX, int iY)
    {
        // マップ外はチェックしない
        if (iX < 0 || iX >= Global.All.szX) { return; }
        if (iY < 0 || iY >= Global.All.szY) { return; }

        // 障害物ならチェックしない
        if(!CheckFloor(iX, iY) && !CheckPassage(iX, iY)){ return; }

        // すでに通っていて、かつ、移動効率がよいならば以降のチェックを行わない
        if (map.route[GetMapAdr(iX, iY)] >= iPow) { return; }

        // Powをセット
        map.route[GetMapAdr(iX, iY)] = iPow;            // 移動ルートに移動コストを入れる

        iPow--;                                         // 移動コスト減
        if (iPow <= 0) { return; }                      // 移動コストが０ならば、ここで処理を止める

        // 四方向へ木を伸ばす
        Check(iPow, iX - 1, iY);
        Check(iPow, iX + 1, iY);
        Check(iPow, iX, iY - 1);
        Check(iPow, iX, iY + 1);
    }

    /*------------------------------------------------------------------------------*
    | <<< iSearchX, iSearchY へ移動できるか調べる >>>
    |	入力	iPow			   = 移動力
    |			iSearchX, iSearchY = 移動したい座標
    |			iX, iY			   = いまいる座標
    |	戻り値	0 == 移動できない
    |			0 >  移動
    *------------------------------------------------------------------------------*/
    public int CheckRoute(int iPow, int iSearchX, int iSearchY, int iX, int iY)
    {
        // 初期化
        Array.Clear(map.route, 0, Global.Room.size * Global.Map.size);
        for (int y = 0; y < Global.All.szY; y++)
        {
            for (int x = 0; x < Global.All.szX; x++)
            {
                Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
                route.SetTile(pos, null);
            }
        }

        // チェック
        Check(iPow, iX, iY);

        // 移動コスト計算
        int a = map.route[GetMapAdr(iSearchX, iSearchY)];       // 目標座標へ行けなかった場合には a には 0 が入る
        if (a > 0) { a = map.route[GetMapAdr(iX, iY)] - a; }    // 現座標より、移動コストを出す
        return a;
    }

    /*------------------------------------------------------------------------------*
    | <<< iSearchX, iSearchY へ移動できるか調べる >>>
    |	入力	iSearchX, iSearchY = 移動したい座標
    |			iX, iY			   = いまいる座標
    |	戻り値	0 == 移動できない
    |			0 >  移動
    *------------------------------------------------------------------------------*/
    public void DrawRoute(int iSearchX, int iSearchY, int iX, int iY)
    {

        int a = map.route[GetMapAdr(iSearchX, iSearchY)];

        if (a > 0)
        {
			while (map.route[GetMapAdr(iX, iY)] != a)
            {
				Vector3Int pos = new Vector3Int(iSearchX + Global.Define.offsetX, -iSearchY + Global.Define.offsetY, 0);
				route.SetTile(pos, tile[95]);
				if (     map.route[GetMapAdr(iSearchX, iSearchY - 1)] == a + 1) { iSearchY--; }
                else if (map.route[GetMapAdr(iSearchX, iSearchY + 1)] == a + 1) { iSearchY++; }
                else if (map.route[GetMapAdr(iSearchX - 1, iSearchY)] == a + 1) { iSearchX--; }
                else if (map.route[GetMapAdr(iSearchX + 1, iSearchY)] == a + 1) { iSearchX++; }
                a   = map.route[GetMapAdr(iSearchX, iSearchY)];
            }
		}
	}
}
