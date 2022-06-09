using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
	public class TileData
	{
		public Vector2Int pos;

		public bool canMovePass;

		public BossScript boss = null;


		public enum EventID
		{
			None,
			DrawCard,
			Battle,
			Shop,
			Heal,
			Boss
		}
		public EventID eventID; 

		public TileData(Vector2Int pos)
		{
			this.pos = pos;

			canMovePass = false;
			eventID = 0;
		}
	}

	public class MovePosition
	{
		public PlayerController.MoveDirectionEnum[] moveDirections;
		public Vector2Int pos;

		public MovePosition(Vector2Int pos, List<int> pass)
		{
			moveDirections = new PlayerController.MoveDirectionEnum[pass.Count];
			for (int i = 0;i < moveDirections.Length; i++){
				moveDirections[i] = (PlayerController.MoveDirectionEnum)pass[i];
			}
			this.pos = pos;
		}

		public Vector2Int GetDirectionVector(int n)
		{
			return PlayerController.DirectioVector[n];
		}
	}

	public static Map ins = null;
	Transform gridIcons;

	[SerializeField] Vector2Int mapOffset;
	[SerializeField] Vector2Int mapSize;
	[SerializeField] int stage;
	[SerializeField] int stageSize;
	[SerializeField] int betweenSize;
	[Space(5)]
	[SerializeField] TextMeshProUGUI runawayText;
	[SerializeField] int NextRunawayTurn;
	[SerializeField] int NextPowerupTurn;

	[Space(10)]
	[SerializeField] Tilemap tileMap;
	[SerializeField] Tile[] canMoveTileChip;
	[Space(5)]
	[SerializeField] Tile normalTile;
	[SerializeField] Tile cardTile;
	[SerializeField] Tile battleTile;
	[SerializeField] Tile shopTile;
	[SerializeField] Tile healTile;
	[SerializeField] Tile eventTile;

	[Space(15)]
	[SerializeField] GameObject moveLoactionPrefab;
	[SerializeField] Transform moveLocationFile;

	[Space(15)]
	[SerializeField] GameObject drawCrad;
	[SerializeField] GameObject battleGrid;
	[SerializeField] GameObject shopPrefab;
	[SerializeField] GameObject healPrefab;

	[Space(15)]
	[SerializeField] Image backImage;
	[SerializeField] Image belndImage;
	[SerializeField] Sprite[] backgroundSprite;

	const float gridSize = 0.576576f * 1.5f;

	public TileData[,] tileDatas;
	public int mapStage;
	public int runawayTurn;
	public float runawayPower;
	[HideInInspector] public BossScript[] bossScripts = new BossScript[7];
	public List<MovePosition> movePositions = new List<MovePosition>();

	static int maxStage = 0;

	public void Runaway(int stage)
	{
		if (maxStage < stage)
		{
			maxStage = stage;
			mapStage = stage;
			runawayTurn = NextRunawayTurn;
			runawayPower = 1.0f;

		}
		else
		{ 
			runawayTurn -= 1;
			if (runawayTurn <= 0)
			{
				runawayTurn = NextPowerupTurn;
				runawayPower += 0.1f;
			}
		}
		RunawayTextRedraw();
	}

	public void RunawayTextRedraw()
	{
		if (runawayPower == 1.0f)
		{
			runawayText.text = "暴走まで " + runawayTurn + "ターン";
		}
		else
		{
			runawayText.text =  "<color=red>" + "暴走 <" + runawayPower.ToString("F1") + "倍>\n";
			runawayText.text += "<color=white>" + "暴走まで " + runawayTurn + "ターン";
		}
	}

	void Awake()
	{
		if (ins == null)
		{
			ins = this;
		}

		gridIcons = new GameObject("GridIcons").transform;

		tileDatas = new TileData[mapSize.y, mapSize.x];

		for (byte x = 0; x < tileDatas.GetLength(0); x++)
		{
			for (byte y = 0; y < tileDatas.GetLength(1); y++)
			{
				tileDatas[x, y] = new TileData(new Vector2Int(x, y));

				Vector3Int pos = (Vector3Int)GetLocalMapPos(new Vector2Int(x, y));
				TileBase tileBase = tileMap.GetTile(pos);

				for (int i = 0; i < canMoveTileChip.Length; i++)
				{
					if (canMoveTileChip[i] == tileBase)
					{
						tileDatas[x, y].canMovePass = true;
						break;
					}
				}
			}
		}


		string temp = "";
		for (byte x = 0; x < tileDatas.GetLength(0); x++)
		{
			for (byte y = 0; y < tileDatas.GetLength(1); y++)
			{
				temp += (tileDatas[x, y].canMovePass == true ? "■" : "□") + ", ";
			}
			temp += "|\n";
		}
		Debug.Log("MapSize (" + tileDatas.GetLength(0) + ", " + tileDatas.GetLength(1) + ")");
		Debug.Log(temp);

		//temp
		//CreateBattleGrid(25);
		//CreateDrawCard(20);

		for (int i = 0; i < stage; i++)
		{
			int length = stageSize;
			int startIndex = (length + betweenSize) * i;

			CreateDrawCard(5, startIndex, length);
			CreateBattleGrid(10, startIndex, length);
			CreateShopGrid(3, startIndex, length);
			CreateHealGrid(3, startIndex, length);
		}

		maxStage = 0;
		Runaway(1);
	}

	public void CreateDrawCard(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.DrawCard, amount, startIndex, length);
	public void CreateBattleGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Battle, amount, startIndex, length);
	public void CreateShopGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Shop, amount, startIndex, length);
	public void CreateHealGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Heal, amount, startIndex, length);


	public void CreateEventGrid(TileData.EventID eventID,int amount, int startIndex = 0, int length = -1)
	{
		List<TileData> randomPick = new List<TileData>();
		randomPick = GetRandomEventEqual(TileData.EventID.None, amount, startIndex, length);

		if (amount > randomPick.Count) amount = randomPick.Count;

		for (int i = 0; i < amount; i++)
		{
			Vector2Int pos = randomPick[i].pos;
			Vector2Int tilePos = (Vector2Int)GetLocalMapPos(pos.Flip() + new Vector2Int(0, 0));

			tileDatas[pos.x, pos.y].eventID = eventID;

			GameObject ins = null;
			switch (eventID)
			{
				case TileData.EventID.DrawCard:
					ins = Instantiate(drawCrad, gridIcons);
					ChangeTile(tilePos, cardTile);
					break;
				case TileData.EventID.Battle:
					ins = Instantiate(battleGrid, gridIcons);
					ChangeTile(tilePos, battleTile);
					break;
				case TileData.EventID.Shop:
					ins = Instantiate(shopPrefab, gridIcons);
					ChangeTile(tilePos, shopTile);
					break;
				case TileData.EventID.Heal:
					ins = Instantiate(healPrefab, gridIcons);
					ChangeTile(tilePos, healTile);
					break;
				default:
					ChangeTile(tilePos, normalTile);
					break;
			}
			ins.transform.position = GetMapTileWorldPos(pos.Flip());
		}
	}

	public void RemoveMovePosition()
	{
		MoveLocation.locations.Clear();
		for (int i = 0; i < moveLocationFile.childCount; i++)
		{
			Destroy(moveLocationFile.GetChild(i).gameObject);
		}
	}

	public void GetMovePosition(Vector2Int pos, int moveAmount)
	{
		RemoveMovePosition();

		movePositions.Clear();
		CheckCanMovePosition(pos, new List<int>(), new Vector2Int(0, 0), moveAmount);

		Debug.Log("===== MovePos ======");
		for (int i = 0; i < movePositions.Count; i++)
		{
			string str = "";
			str += movePositions[i].pos;
			str += " => ";
			for (int j = 0; j < movePositions[i].moveDirections.Length; j++)
			{
				str += movePositions[i].moveDirections[j] + " - ";
			}
			Debug.Log(str);
		}
		Debug.Log("====================");

		for (int i = 0; i < movePositions.Count; i++)
		{
			GameObject ins = Instantiate(moveLoactionPrefab, moveLocationFile);
			ins.transform.position = Map.ins.GetMapTileWorldPos(Map.ins.movePositions[i].pos) - new Vector3(0, 0.4f, 0);
			ins.transform.localScale = Vector3.one;
			ins.GetComponent<MoveLocation>().haveLocation = Map.ins.movePositions[i];
		}
	}

	void CheckCanMovePosition(Vector2Int pos, List<int> pass, Vector2Int direction, int moveAmount)
	{
		int count = 0;
		bool[] canMove = new bool[PlayerController.DirectioVector.Length];

		for(int i = 0; i < PlayerController.DirectioVector.Length; i++)
		{
			if (direction * -1 == PlayerController.DirectioVector[i]) continue;

			Vector2Int vector = PlayerController.DirectioVector[i];

			Debug.Log(pos + vector);
			canMove[i] = GetTileData(pos + vector).canMovePass;
			if (canMove[i] == true) count++;
		}

		if (count < 1) return;

		for (int i = 0; i < PlayerController.DirectioVector.Length; i++)
		{
			if (canMove[i] == false) continue;
			List<int> newPass = new List<int>(pass); newPass.Add(i);
			CheckMovement(pos, newPass, PlayerController.DirectioVector[i], moveAmount);
		}
	}

	void CheckMovement(Vector2Int pos, List<int> pass, Vector2Int vector, int moveAmount)
	{
		if (moveAmount > 0)
		{
			moveAmount -= 1;
			pos += vector;

			if (moveAmount > 0)
			{
				CheckCanMovePosition(pos, pass, vector, moveAmount);
			}
			else
			{
				bool isAdd = true;
				foreach (MovePosition movePosition in movePositions)
				{
					if (movePosition.pos != pos) continue;

					isAdd = false;
					break;
				}
				if (isAdd == true) movePositions.Add(new MovePosition(pos, pass));
			}
		}
	}

	public int GetMapTileID(Vector2Int pos)
	{
		Vector3Int pos2 = (Vector3Int)GetLocalMapPos(pos.Flip() + new Vector2Int(0, 0));
		TileBase tileBase = tileMap.GetTile(pos2);

		int n = 0;
		for (int i = 0; i < canMoveTileChip.Length; i++)
		{
			if (canMoveTileChip[i] == tileBase)
			{
				n = i + 1;
				break;
			}
		}
		return n;
	}

	public void ChangeTile(Vector2Int pos, Tile tile)
	{
		//Vector3Int pos2 = (Vector3Int)GetLocalMapPos(pos.Flip() + new Vector2Int(0, 0));
		tileMap.SetTile((Vector3Int)pos.Flip(), tile);
	}

	public void ChangeBossGrid(Vector2Int pos, BossScript boss)
	{
		pos = pos.Flip();
		ChangeTile(GetLocalMapPos(pos.Flip()), eventTile);
		tileDatas[pos.x, pos.y].eventID = TileData.EventID.Boss;
		tileDatas[pos.x, pos.y].boss = boss;
	}

	public void DieBossGrid(Vector2Int pos)
	{
		pos = pos.Flip();
		ChangeTile(GetLocalMapPos(pos.Flip()), normalTile);
		tileDatas[pos.x, pos.y].eventID = TileData.EventID.None;
	}

	public Vector2Int GetLocalMapPos(Vector2Int pos)
	{
		return pos * -1 - mapOffset.Flip();
	}

	public Vector3 GetMapTileWorldPos(Vector2Int pos)
	{
		pos = pos.Flip();
		pos -= new Vector2Int(1, 1);
		return tileMap.GetCellCenterWorld((Vector3Int)GetLocalMapPos(pos));
	}

	public TileData GetTileData(Vector2Int pos)
	{
		pos = pos.Flip();
		return tileDatas[pos.x, pos.y];
	}

	public List<TileData> GetEventEqual(TileData.EventID eventID, int startIndex = 0, int length = -1)
	{
		List<TileData> output = new List<TileData>();

		for (int x = 0; x < tileDatas.GetLength(0); x++)
		{
			for (int y = startIndex; y < startIndex + length; y++)
			{
				if (tileDatas[x, y].canMovePass == false) continue;
				if (tileDatas[x, y].eventID != eventID) continue;

				output.Add(tileDatas[x, y]);
			}
		}

		return output;
	}
	public List<TileData> GetRandomEventEqual(TileData.EventID eventID, int amount, int startIndex = 0, int length = -1)
	{
		List<TileData> temp   = new List<TileData>();
		List<TileData> output = new List<TileData>();
		temp = GetEventEqual(eventID, startIndex, length);

		if (amount > temp.Count) amount = temp.Count;

		for (int i = 0; i < amount; i++)
		{
			int random = Random.Range(0, temp.Count);
			output.Add(temp[random]);
			temp.Remove(temp[random]);
		}

		return output;
	}

	public int CheckStage(Vector2Int pos)
	{
		int _stage = 0;
		int _stageLine;
		int x = pos.x;
		_stage = (pos.x / (stageSize + betweenSize)) + 1 ;
		_stageLine = pos.x % (stageSize + betweenSize);

		belndImage.color = new Color(1, 1, 1, 0);

		if (_stageLine >= 1)
		{
			backImage.sprite = backgroundSprite[_stage - 1];
			belndImage.sprite = backgroundSprite[_stage >= 7 ? 6 : _stage];
		}

		if (_stageLine < 9 && _stageLine > 0) return _stage;

		if (_stageLine <= 0) _stageLine = 11;

		_stageLine = _stageLine - 8;
		float a = (_stageLine) / 4.0f;

		belndImage.color = new Color(1, 1, 1, a);

		return _stage;
	}

	private void OnDrawGizmosSelected()
	{
		Vector2 offsetPos;
		Vector2 posX;
		Vector2 posY;
		posX = Vector.Angle2Vector(180 - 30 + 180) * (mapOffset.x - 1) * gridSize;
		posY = Vector.Angle2Vector(0 + 30 + 180) * (mapOffset.y - 1) * gridSize;
		offsetPos = posX + posY;

		Gizmos.color = Color.red;
		Gizmos.DrawLine(offsetPos, offsetPos + Vector.Angle2Vector(-90 + 60) * mapSize.x * gridSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(offsetPos, offsetPos + Vector.Angle2Vector(-90 - 60) * mapSize.y * gridSize);
		Gizmos.color = Color.green;
		Vector2 temp;
		temp = offsetPos + Vector.Angle2Vector(-90 + 60) * mapSize.x * gridSize;
		Gizmos.DrawLine(temp, temp + Vector.Angle2Vector(-90 - 60) * mapSize.y * gridSize);
		temp = offsetPos + Vector.Angle2Vector(-90 - 60) * mapSize.y * gridSize;
		Gizmos.DrawLine(temp, temp + Vector.Angle2Vector(-90 + 60) * mapSize.x * gridSize);
	}
}

public static class Vector
{
	public static Vector2 Angle2Vector(float angle)
	{
		Vector2 vector;
		vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
		vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);

		return vector;
	}

	public static Vector2 UseX(this Vector2 vector)
	{
		vector.y = 0;
		return vector;
	}
	public static Vector2 UseY(this Vector2 vector)
	{
		vector.x = 0;
		return vector;
	}

	public static Vector3 ReSize(this Vector3 v1, Vector3 v2)
	{
		Vector3 vector;
		vector.x = v1.x * v2.x;
		vector.y = v1.y * v2.y;
		vector.z = v1.z * v2.z;

		return vector;
	}
}