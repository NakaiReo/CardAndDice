using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
	/// <summary>
    /// マスが持っている情報
    /// </summary>
	public class TileData
	{
		public Vector2Int pos; //マスの座標

		public bool canMovePass; //通ることが出来るか

		public BossScript boss = null; //ボス情報

		/// <summary>
        /// そのマスが持つイベント
        /// </summary>
		public enum EventID
		{
			None,     //何もなし
			DrawCard, //カードドロー
			Battle,   //雑魚戦
			Shop,     //ショップ
			Heal,     //ヒール
			Boss      //ボス戦
		}
		public EventID eventID; //イベントID

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="pos">マスの座標</param>
		public TileData(Vector2Int pos)
		{
			this.pos = pos;

			canMovePass = false;
			eventID = 0;
		}
	}

	/// <summary>
    /// 移動先
    /// </summary>
	public class MovePosition
	{
		public PlayerController.MoveDirectionEnum[] moveDirections; //動ける方向
		public Vector2Int pos; //現在地

		/// <summary>
        /// 現在地から動ける方向を返す
        /// </summary>
        /// <param name="pos">現在地</param>
        /// <param name="pass">動ける方向の要素番号</param>
		public MovePosition(Vector2Int pos, List<int> pass)
		{
			moveDirections = new PlayerController.MoveDirectionEnum[pass.Count];
			for (int i = 0;i < moveDirections.Length; i++){
				moveDirections[i] = (PlayerController.MoveDirectionEnum)pass[i];
			}
			this.pos = pos;
		}

		/// <summary>
        /// 要素番号を方向に直す
        /// </summary>
        /// <param name="n">要素番号</param>
        /// <returns>座標</returns>
		public Vector2Int GetDirectionVector(int n)
		{
			return PlayerController.DirectioVector[n];
		}
	}

	public static Map ins = null;
	Transform gridIcons; //マスのアイコンが保存される場所

	[SerializeField] Vector2Int mapOffset; //マップの原点
	[SerializeField] Vector2Int mapSize;   //マップのサイズ
	[SerializeField] int stage;            //ステージの数
	[SerializeField] int stageSize;        //ステージ毎の広さ
	[SerializeField] int betweenSize;      //ステージ間の隙間
	[Space(5)]
	[SerializeField] TextMeshProUGUI runawayText; //のこりターン数のテキスト
	[SerializeField] int NextRunawayTurn;　　　　 //マップ毎のこりターン数
	[SerializeField] int NextPowerupTurn;         //マップ毎パワーアップターン数

	[Space(10)]
	[SerializeField] Tilemap tileMap;        //タイルマップ
	[SerializeField] Tile[] canMoveTileChip; //移動可能なタイルチップ
	[Space(5)]
	[SerializeField] Tile normalTile; //通常マス
	[SerializeField] Tile cardTile;   //カードマス
	[SerializeField] Tile battleTile; //雑魚戦マス
	[SerializeField] Tile shopTile;   //ショップマス
	[SerializeField] Tile healTile;   //ヒールマス
	[SerializeField] Tile eventTile;  //イベントマス

	[Space(15)]
	[SerializeField] GameObject moveLoactionPrefab; //移動方向を設定するためのプレハブ
	[SerializeField] Transform moveLocationFile;    //移動方向を設定する親オブジェクト

	[Space(15)]
	[SerializeField] GameObject drawCrad;   //カードマスのアイコン
	[SerializeField] GameObject battleGrid; //雑魚戦マスのアイコン
	[SerializeField] GameObject shopPrefab; //ショップマスのアイコン
	[SerializeField] GameObject healPrefab; //ヒールマスのアイコン

	[Space(15)]
	[SerializeField] Image backImage;  //現在のステージの背景
	[SerializeField] Image belndImage; //次のステージの背景
	[SerializeField] Sprite[] backgroundSprite; //各ステージの背景

	const float gridSize = 0.576576f * 1.5f; //グリッドごとの大きさ

	public TileData[,] tileDatas; //各マスごとのデータ
	public int mapStage;          //現在のステージ
	public int runawayTurn;       //現在ののこりターン数
	public float runawayPower;    //現在のパワーアップターン数
	[HideInInspector] public BossScript[] bossScripts = new BossScript[7]; //ボス戦のデータ
	public List<MovePosition> movePositions = new List<MovePosition>();    //移動位置のデータ

	static int maxStage = 0; //プレイヤーが進んだ最大ステージ数

	/// <summary>
    /// 残りターン数の減少やリセット
    /// </summary>
    /// <param name="stage">現在のステージ</param>
	public void Runaway(int stage)
	{
		//ステージが変わったらリセット
		if (maxStage < stage)
		{
			maxStage = stage;
			mapStage = stage;
			runawayTurn = NextRunawayTurn;
			runawayPower = 1.0f;

		}
		//ステージがそのままならのこりターン数を減少
		else
		{ 
			runawayTurn -= 1;

			//0以下になったら敵のパワーアップ
			if (runawayTurn <= 0)
			{
				runawayTurn = NextPowerupTurn;
				runawayPower += 0.1f;
			}
		}

		//再描画
		RunawayTextRedraw();
	}

	/// <summary>
    /// のこりターン数の再描画
    /// </summary>
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

		gridIcons = new GameObject("GridIcons").transform; //マスのアイコンの保存先を指定

		tileDatas = new TileData[mapSize.y, mapSize.x];    //マップの情報をサイズを設定

		//マップの初期化
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

		//Debug用
		//string temp = "";
		//for (byte x = 0; x < tileDatas.GetLength(0); x++)
		//{
		//	for (byte y = 0; y < tileDatas.GetLength(1); y++)
		//	{
		//		temp += (tileDatas[x, y].canMovePass == true ? "■" : "□") + ", ";
		//	}
		//	temp += "|\n";
		//}
		//Debug.Log("MapSize (" + tileDatas.GetLength(0) + ", " + tileDatas.GetLength(1) + ")");
		//Debug.Log(temp);

		//temp
		//CreateBattleGrid(25);
		//CreateDrawCard(20);

		//ステージごとにイベントマスを設置
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

	//イベントマスを作る
	public void CreateDrawCard(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.DrawCard, amount, startIndex, length);
	public void CreateBattleGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Battle, amount, startIndex, length);
	public void CreateShopGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Shop, amount, startIndex, length);
	public void CreateHealGrid(int amount, int startIndex, int length) => CreateEventGrid(TileData.EventID.Heal, amount, startIndex, length);

	/// <summary>
    /// イベントマスを作る処理
    /// </summary>
    /// <param name="eventID">イベントの種類</param>
    /// <param name="amount">生成数</param>
    /// <param name="startIndex">開始X座標</param>
    /// <param name="length">長さ</param>
	public void CreateEventGrid(TileData.EventID eventID,int amount, int startIndex = 0, int length = -1)
	{
		//ランダムに設定されていないマスを取得
		List<TileData> randomPick = new List<TileData>();
		randomPick = GetRandomEventEqual(TileData.EventID.None, amount, startIndex, length);

		//設定する数が取得できた数を超えないように
		if (amount > randomPick.Count) amount = randomPick.Count;

		//イベントを設定する
		for (int i = 0; i < amount; i++)
		{
			Vector2Int pos = randomPick[i].pos;
			Vector2Int tilePos = (Vector2Int)GetLocalMapPos(pos.Flip() + new Vector2Int(0, 0));

			tileDatas[pos.x, pos.y].eventID = eventID;

			//アイコンの設定とタイルの変更
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

	/// <summary>
    /// 移動先の非表示
    /// </summary>
	public void RemoveMovePosition()
	{
		MoveLocation.locations.Clear();
		for (int i = 0; i < moveLocationFile.childCount; i++)
		{
			Destroy(moveLocationFile.GetChild(i).gameObject);
		}
	}

	/// <summary>
    /// 移動先の表示
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="moveAmount"></param>
	public void GetMovePosition(Vector2Int pos, int moveAmount)
	{
		//一度すべて削除する
		RemoveMovePosition();
		movePositions.Clear();

		//移動先の取得
		CheckCanMovePosition(pos, new List<int>(), new Vector2Int(0, 0), moveAmount);

		//Debug.Log("===== MovePos ======");
		//for (int i = 0; i < movePositions.Count; i++)
		//{
		//	string str = "";
		//	str += movePositions[i].pos;
		//	str += " => ";
		//	for (int j = 0; j < movePositions[i].moveDirections.Length; j++)
		//	{
		//		str += movePositions[i].moveDirections[j] + " - ";
		//	}
		//	Debug.Log(str);
		//}
		//Debug.Log("====================");

		//移動先の表示
		for (int i = 0; i < movePositions.Count; i++)
		{
			GameObject ins = Instantiate(moveLoactionPrefab, moveLocationFile);
			ins.transform.position = Map.ins.GetMapTileWorldPos(Map.ins.movePositions[i].pos) - new Vector3(0, 0.4f, 0);
			ins.transform.localScale = Vector3.one;
			ins.GetComponent<MoveLocation>().haveLocation = Map.ins.movePositions[i];
		}
	}

	/// <summary>
    /// 移動先の取得
    /// </summary>
    /// <param name="pos">検索位置</param>
    /// <param name="pass">今までの経路</param>
    /// <param name="direction">進んでいる方向</param>
    /// <param name="moveAmount">残り歩数</param>
	void CheckCanMovePosition(Vector2Int pos, List<int> pass, Vector2Int direction, int moveAmount)
	{
		int count = 0; //移動できる方向の数	
		bool[] canMove = new bool[PlayerController.DirectioVector.Length]; //移動できる方向

		//現在地からどの方向に移動できるか
		for(int i = 0; i < PlayerController.DirectioVector.Length; i++)
		{
			//自分の前の位置は除外する
			if (direction * -1 == PlayerController.DirectioVector[i]) continue;

			Vector2Int vector = PlayerController.DirectioVector[i];

			//移動できるマスであればカウントする
			canMove[i] = GetTileData(pos + vector).canMovePass;
			if (canMove[i] == true) count++;
		}

		//移動できなければ終了する
		if (count < 1) return;

		//移動先があれば分岐して進む
		for (int i = 0; i < PlayerController.DirectioVector.Length; i++)
		{
			if (canMove[i] == false) continue;
			List<int> newPass = new List<int>(pass); newPass.Add(i);
			CheckMovement(pos, newPass, PlayerController.DirectioVector[i], moveAmount);
		}
	}

	/// <summary>
    /// 更に移動できるかどうか
    /// </summary>
    /// <param name="pos">検索位置</param>
    /// <param name="pass">今までの経路</param>
    /// <param name="vector">進んでいる方向</param>
    /// <param name="moveAmount">のこり歩数</param>
	void CheckMovement(Vector2Int pos, List<int> pass, Vector2Int vector, int moveAmount)
	{
		//のこり歩数が0より大きければカウントを減らし進む
		if (moveAmount > 0)
		{
			moveAmount -= 1;
			pos += vector;

			//それでも0より大きければ更に進む
			if (moveAmount > 0)
			{
				CheckCanMovePosition(pos, pass, vector, moveAmount);
			}

			//移動先を保存する
			else
			{
				bool isAdd = true; //追加可能か
				foreach (MovePosition movePosition in movePositions)
				{
					if (movePosition.pos != pos) continue; //すでに保存されているか

					isAdd = false;
					break;
				}

				//保存されてなければ移動先を追加
				if (isAdd == true) movePositions.Add(new MovePosition(pos, pass));
			}
		}
	}

	/// <summary>
    /// 指定位置のイベントIDを取得
    /// </summary>
    /// <param name="pos">指定位置</param>
    /// <returns>イベントID</returns>
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

	/// <summary>
    /// タイルの見た目を変更
    /// </summary>
    /// <param name="pos">変更先</param>
    /// <param name="tile">変更する見た目</param>
	public void ChangeTile(Vector2Int pos, Tile tile)
	{
		//Vector3Int pos2 = (Vector3Int)GetLocalMapPos(pos.Flip() + new Vector2Int(0, 0));
		tileMap.SetTile((Vector3Int)pos.Flip(), tile);
	}

	/// <summary>
    /// ボスマスの設定
    /// </summary>
    /// <param name="pos">ボスマスの座標</param>
    /// <param name="boss">ボスのデータ</param>
	public void ChangeBossGrid(Vector2Int pos, BossScript boss)
	{
		pos = pos.Flip();
		ChangeTile(GetLocalMapPos(pos.Flip()), eventTile);
		tileDatas[pos.x, pos.y].eventID = TileData.EventID.Boss;
		tileDatas[pos.x, pos.y].boss = boss;
	}

	/// <summary>
    /// ボスが死んだときのタイルの見た目の変更
    /// </summary>
    /// <param name="pos">ボスマスの座標</param>
	public void DieBossGrid(Vector2Int pos)
	{
		pos = pos.Flip();
		ChangeTile(GetLocalMapPos(pos.Flip()), normalTile);
		tileDatas[pos.x, pos.y].eventID = TileData.EventID.None;
	}

	/// <summary>
    /// World座標からTilemap座標へ
    /// </summary>
    /// <param name="pos">World座標</param>
    /// <returns>Tilemap座標</returns>
	public Vector2Int GetLocalMapPos(Vector2Int pos)
	{
		return pos * -1 - mapOffset.Flip();
	}

	/// <summary>
    /// Tilemap座標をWorld座標へ
    /// </summary>
    /// <param name="pos">Tilemap座標</param>
    /// <returns>World座標</returns>
	public Vector3 GetMapTileWorldPos(Vector2Int pos)
	{
		pos = pos.Flip();
		pos -= new Vector2Int(1, 1);
		return tileMap.GetCellCenterWorld((Vector3Int)GetLocalMapPos(pos));
	}


	/// <summary>
    /// マスが持つ情報を取得
    /// </summary>
    /// <param name="pos">座標</param>
    /// <returns>マス情報</returns>
	public TileData GetTileData(Vector2Int pos)
	{
		pos = pos.Flip();
		return tileDatas[pos.x, pos.y];
	}

	/// <summary>
    /// 指定したイベントをすべて取得
    /// </summary>
    /// <param name="eventID">イベントID</param>
    /// <param name="startIndex">開始X軸</param>
    /// <param name="length">検索長さ</param>
    /// <returns>取得したイベント情報</returns>
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

	/// <summary>
    /// ランダムに指定された個数分、指定したイベントから取得
    /// </summary>
    /// <param name="eventID">イベントID</param>
    /// <param name="amount">取得する数</param>
    /// <param name="startIndex">開始X軸</param>
    /// <param name="length">検索長さ</param>
    /// <returns></returns>
	public List<TileData> GetRandomEventEqual(TileData.EventID eventID, int amount, int startIndex = 0, int length = -1)
	{
		List<TileData> temp   = new List<TileData>(); //全体のイベント
		List<TileData> output = new List<TileData>(); //出力用
		temp = GetEventEqual(eventID, startIndex, length); //すべてのイベントマスを取得

		//指定した数が取得数を超えないように
		if (amount > temp.Count) amount = temp.Count;

		//ランダムに全体から取得していく
		for (int i = 0; i < amount; i++)
		{
			int random = Random.Range(0, temp.Count);
			output.Add(temp[random]);
			temp.Remove(temp[random]);
		}

		return output;
	}

	/// <summary>
    /// 指定位置のステージを取得
    /// </summary>
    /// <param name="pos">指定位置</param>
    /// <returns>現在のステージ数</returns>
	public int CheckStage(Vector2Int pos)
	{
		int _stage = 0; //ステージ数
		int _stageLine; //ステージ内の進行度
		int x = pos.x;
		_stage = (pos.x / (stageSize + betweenSize)) + 1 ;
		_stageLine = pos.x % (stageSize + betweenSize);

		belndImage.color = new Color(1, 1, 1, 0);

		if (_stageLine >= 1)
		{
			backImage.sprite  = backgroundSprite[_stage - 1];               //現在の背景を設定
			belndImage.sprite = backgroundSprite[_stage >= 7 ? 6 : _stage]; //次の背景を設定
		}


		if (_stageLine < 9 && _stageLine > 0) return _stage;

		if (_stageLine <= 0) _stageLine = 11;

		//進行度に応じて次の背景をブレンドする
		_stageLine = _stageLine - 8;
		float a = (_stageLine) / 4.0f;

		belndImage.color = new Color(1, 1, 1, a);

		return _stage;
	}

	/// <summary>
    /// デバッグ用
    /// </summary>
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
	/// <summary>
    /// 角度をベクトルに
    /// </summary>
    /// <param name="angle">度数法</param>
    /// <returns>ベクトル</returns>
	public static Vector2 Angle2Vector(float angle)
	{
		Vector2 vector;
		vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
		vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);

		return vector;
	}

	/// <summary>
    /// X成分だけを取得
    /// </summary>
	public static Vector2 UseX(this Vector2 vector)
	{
		vector.y = 0;
		return vector;
	}

	/// <summary>
    /// Y成分だけを取得
    /// </summary>
	public static Vector2 UseY(this Vector2 vector)
	{
		vector.x = 0;
		return vector;
	}

	/// <summary>
    /// 2つのベクトルをかけ合わせる
    /// </summary>
	public static Vector3 ReSize(this Vector3 v1, Vector3 v2)
	{
		Vector3 vector;
		vector.x = v1.x * v2.x;
		vector.y = v1.y * v2.y;
		vector.z = v1.z * v2.z;

		return vector;
	}
}