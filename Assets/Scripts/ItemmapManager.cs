using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Common;


public class ItemmapManager : MonoBehaviour
{
	private class Define
	{
		public static readonly int    maxTile = 20;                     // 読み込みタイル総数
		public static readonly string nameTileHead = "ItemTile/Item_";  // タイルの読み込み先と冒頭のファイル名
	}

	// 変数
	private TileBase[] tile = new TileBase[Define.maxTile];             // マップのタイルが入る
	private Tilemap	   bg;
	private GameObject map;
	private int[]      buf = new int[Global.Room.size * Global.Map.size];

	void Start()
	{
		for (int i = 0; i < Define.maxTile; i++)
		{
			tile[i] = Resources.Load<Tile>(Define.nameTileHead + i.ToString());
		}
		bg = GetComponent<Tilemap>();
		map = GameObject.Find("Tilemap");
		Clear();
	}

	public void Clear()
	{
		int adr = 0;
		for (int y = 0; y < Global.Map.szY; y++)
		{
			for (int x = 0; x < Global.Map.szX; x++)
			{
				Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
				bg.SetTile(pos, null);
				buf[adr] = 0;
			}
		}
	}

	void Update()
	{
		this.transform.position = map.transform.position;
	}

	// タイルマップにアイテムを設定する
	public void SetTile(int numRoom, int x, int y, int numItem)
	{
		int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
		int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
		Vector3Int pos = new Vector3Int(xx + x + Global.Define.offsetX, -yy - y + Global.Define.offsetY, 0);
		bg.SetTile(pos, tile[numItem]);
		buf[GetMapAdr(xx + x, yy + y)] = numItem;
	}
	public void SetTile(int x, int y, int numItem)
	{
		int xx = (x % Global.Room.szX);
		int yy = (y % Global.Room.szY);
		int numRoom = (y / Global.Room.szY) * Global.Map.szX + (x / Global.Room.szX);
		Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
		bg.SetTile(pos, tile[numItem]);
		buf[GetMapAdr(x, y)] = numItem;
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

	public bool Check(int numRoom, int x, int y)
	{
		int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
		int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
		return buf[GetMapAdr(xx + x, yy + y)] != 0;
	}
	public bool Check(int x, int y)
	{
		return buf[GetMapAdr(x, y)] != 0;
	}

	// 消去
	public void Erase(int numRoom, int x, int y)
	{
		int xx = (numRoom % Global.Map.szX) * Global.Room.szX;
		int yy = (numRoom / Global.Map.szX) * Global.Room.szY;
		Vector3Int pos = new Vector3Int(xx + x + Global.Define.offsetX, -yy - y + Global.Define.offsetY, 0);
		bg.SetTile(pos, null);
		buf[GetMapAdr(xx + x, yy + y)] = 0;
	}
	public void Erase(int x, int y)
	{
		int xx = (x % Global.Room.szX);
		int yy = (y % Global.Room.szY);
		int numRoom = (y / Global.Room.szY) * Global.Map.szX + (x / Global.Room.szX);
		Vector3Int pos = new Vector3Int(x + Global.Define.offsetX, -y + Global.Define.offsetY, 0);
		bg.SetTile(pos, null);
		buf[GetMapAdr(x, y)] = 0;
	}
}
