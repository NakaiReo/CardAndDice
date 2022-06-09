using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
	[SerializeField] GridEvent gridEvent;       //マスごとのイベント
	[SerializeField] PlayerStatus playerStatus; //プレイヤーのステータス
	[SerializeField] CardEffects cardEffects;   //カードの効果
	[Space(15)]
	[SerializeField] public Vector2Int pos; //プレイヤー座標
	[SerializeField] public Vector2 cameraOffset; //カメラのずらし
	[SerializeField] bool canBackMove; //後ろに戻れるかどうか
	[SerializeField] TextMeshProUGUI diceText; //ダイスの値のテキスト
	[Space(5)] 
	[SerializeField] GameObject useCardButton;   //カードを使用するボタン
	[SerializeField] GameObject SkillTextObejct; //スキル名のテキスト
	[Space(15)]
	[SerializeField] SerializeMoveDirectionCanvas serializeMoveDirectionCanvas; //移動方向を決めるパネル
	 
	Vector2Int backDirectionVector;      //移動前座標
	MoveDirectionEnum moveDirectionEnum; //移動方向

	public int stage = 1; //現在のステージ

	bool[] canMoveDirection = new bool[4]; //移動できる方向
	bool canUseCard;         //カードを使用できるかどうか
	public bool waitInput;   //入力待ち
	public bool isFirstMove; //最初の移動かどうか
	public bool isWalking;   //移動中
	public bool isDiceing;   //ダイス振り中
	bool isEvent{ get { return GameDirector.isEvent; } } //イベント中か

	//ダイスの値
	int _diceAmount;
	int diceAmount
	{
		get{ return _diceAmount; }
		set
		{
			_diceAmount = value;
			DiceRoll.ins.FaceingDiceValue(_diceAmount);
			diceText.text = _diceAmount < 0 ? "" : _diceAmount.ToString("");
			diceText.transform.localScale = transform.localScale;
		}
	}

	//向き情報
	public enum MoveDirectionEnum
	{
		Up,
		Left,
		Right,
		Down
	}

	//向きに対するベクトル
	public static Vector2Int[] DirectioVector = new Vector2Int[4]
	{
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0),
		new Vector2Int(1, 0),
		new Vector2Int(0, 1),
	};

	static bool cardOpen = false; //カード情報を開けるかどうか

	void Start()
	{
		serializeMoveDirectionCanvas.canvas.SetActive(false);
		transform.position = Map.ins.GetMapTileWorldPos(pos);
		stage = Map.ins.CheckStage(pos);

		waitInput   = false;
		isFirstMove = false;
		isWalking   = false;
		isDiceing   = false;

		canUseCard = true;
		Map.ins.Runaway(stage);
		diceAmount = -1;

		cardEffects.playerName = playerStatus.playerName;

		Vector3 cameraPos = Map.ins.GetMapTileWorldPos(pos) - (Vector3)cameraOffset;
		cameraPos.z = -50;
		Camera.main.transform.position = cameraPos;
	}
    void Update()
    {
		//カード状態を見ていたら実行しない
		if (CardScript.isViewCard != null)
		{
			//イベント中でなければ実行しない
			if (isEvent == false)
			{
				cardOpen = true;

				useCardButton.SetActive(false);

				CardData card = CardScript.isViewCard.cardData;

				//カード情報を見ており、そのカードが使用できる場合に実行できる
				if (CardScript.isViewCard.CanUseCheck(CardData.UseType.Map) == true)
				{
					//カードを使用するボタンを表示
					playerStatus.status.pos = transform.localPosition;
					useCardButton.SetActive(true);
				}
			}
		}
		else
		{
			//カードを使用できなくする
			cardOpen = false;
			useCardButton.SetActive(false);
		}

		bool diceActive = !cardOpen && !isEvent; //ダイスを使用できるか
		if (DiceRoll.active != diceActive) DiceRoll.Enable(diceActive); //ダイスを使用できるようにしたり、出来なくさせる

		//マウス中ボタン長押しでカメラで見渡せるように
		if (Input.GetMouseButton(2) && isEvent == false)
		{
			Vector2 move;
			move.x = Input.GetAxis("Mouse X");
			move.y = Input.GetAxis("Mouse Y");
			move *= -0.5f;

			Vector3 cameraPos = Camera.main.transform.position + (Vector3)move;
			cameraPos.x = Mathf.Clamp(cameraPos.x, transform.position.x - 7.5f - cameraOffset.x, transform.position.x + 7.5f - cameraOffset.x);
			cameraPos.y = Mathf.Clamp(cameraPos.y, transform.position.y - 5.0f - cameraOffset.y, transform.position.y + 5.0f - cameraOffset.y);

			Camera.main.transform.position = cameraPos;
		}

		//右クリックでカメラの位置をリセット
		if (Input.GetMouseButtonDown(1) && isEvent == false)
		{
			Vector3 cameraPos = Map.ins.GetMapTileWorldPos(pos) - (Vector3)cameraOffset;
			cameraPos.z = -50;
			Camera.main.transform.DOLocalMove(cameraPos, 0.5f);
		}

		//ダイスロールを開始
		if (isDiceing == false && isWalking == false && isEvent == false)
		{
			if (Input.GetButtonDown("Submit") || isDiceButton == true)
			{
				if (DiceRoll.active == false) return;

				isDiceing = true;
				isWalking = true;
				canUseCard = false;
				StartCoroutine(DiceRool());
				//DiceRoll.ins.isDiceing = true;
			}
		}

		//移動方向を決める
		if (waitInput == true)
		{
			Vector2 axis;
			axis.x = Input.GetAxisRaw("Horizontal");
			axis.y = Input.GetAxisRaw("Vertical");

				 if (axis.y >=  1.0f && canMoveDirection[0]) ChangeMoveDirectionButton(0);
			else if (axis.x <= -1.0f && canMoveDirection[1]) ChangeMoveDirectionButton(1);
			else if (axis.x >=  1.0f && canMoveDirection[2]) ChangeMoveDirectionButton(2);
			else if (axis.y <= -1.0f && canMoveDirection[3]) ChangeMoveDirectionButton(3);
		}
    }

	//サイコロ
	IEnumerator DiceRool()
	{
		CardScript.canView = false;
		CardScript.ViewCardInfoDestory();
		var ie = DiceRoll.ins.Roll();
		var co = DiceRoll.ins.StartCoroutine(ie);
		yield return co;

		diceAmount = (int)ie.Current;
		if (playerStatus.moveX2) diceAmount *= 2; //移動2倍の時値を2倍にする

		Map.ins.GetMovePosition(pos, diceAmount); //移動可能位置を表示

		yield return null;
		StartCoroutine(Movement()); //移動処理
	}

	//移動処理
	IEnumerator Movement()
	{
		yield return new WaitForSeconds(0.25f);

		isFirstMove = true;

		//移動方法が位置指定の場合
		if (MovementWay.isLocationMove)
		{
			MoveLocation.selectLocation = null;

			while (MoveLocation.selectLocation == null)
			{
				yield return null;
			}
		}

		//ダイスの値が0以下になるまで移動処理を繰り返す
		while(diceAmount > 0)
		{
			MovementChack();
			while (waitInput) yield return null;

			//移動
			isFirstMove = false;
			MovePlayer();
			diceAmount -= 1;
			stage  = Map.ins.CheckStage(pos);
			SoundDirector.PlaySE("Move");

			//移動先がボスマスならボス戦を開始
			if(GetEvent() == Map.TileData.EventID.Boss)
			{
				gridEvent.Boss(pos);
				yield return null;
			}

			//イベントが終わるまで待つ
			while (isEvent) yield return null;

			//移動を待つ
			float waitSecond = (Input.GetKey(KeyCode.LeftShift) || MovementWay.isQuickMove) ? 0.15f : 0.8f;
			yield return new WaitForSeconds(waitSecond);
		}

		playerStatus.status.pos = transform.localPosition;

		//止まったマスにイベントがあるかどうか
		CheckEvent();

		diceAmount = -1;
		DiceRoll.diceIdel.Restart();

		//イベントの終了を待つ
		yield return new WaitForSeconds(0.25f);
		while (isEvent) yield return null;

		Map.ins.Runaway(stage);
		Map.ins.RemoveMovePosition();

		isDiceing = false;
		isWalking = false;
		canUseCard = true;
		playerStatus.CardCoolDown();
		playerStatus.notBattle = false;
		playerStatus.moveX2 = false;

		//ターン終了効果
		PlayerStatus.PlayerName p = playerStatus.playerName;
		Debug.Log(playerStatus.playerName);
		switch (p)
		{
			//冒険者なら5%体力回復
			case PlayerStatus.PlayerName.Adventurer:
				cardEffects.HealPercent(CardEffects.EffectTarget.Player, 5, CardData.CostType.Cooldown);
				yield return new WaitForSeconds(1.0f);
				break;
			//魔女なら10%Mana回復
			case PlayerStatus.PlayerName.Witch:
				cardEffects.ManaPercent(CardEffects.EffectTarget.Player, 10);
				yield return new WaitForSeconds(1.0f);
				break;
			//商人なら20G入手
			case PlayerStatus.PlayerName.Merchant:
				cardEffects.GetMoney(20);
				yield return new WaitForSeconds(1.0f);
				break;
		}

		playerStatus.Redraw();
		CardScript.canView = true;
		CardScript.ViewCardInfoDestory();
		yield break;
	}

	//移動先のチェック
	void MovementChack()
	{
		if (MovementWay.isDirectionMove)
		{
			//2以上の時は選択できるようにする
			int count = 0;

			for (int i = 0; i < canMoveDirection.Length; i++)
			{
				Vector2Int vector = DirectioVector[i];

				serializeMoveDirectionCanvas.button[i].gameObject.SetActive(false);
				canMoveDirection[i] = false;

				//int id = Map.ins.GetMapTileID(pos + vector);
				bool canMovePass = Map.ins.GetTileData(pos + vector).canMovePass;

				if (canMovePass == true && (backDirectionVector != vector || canBackMove == true && isFirstMove == true))
				{
					canMoveDirection[i] = true;
					serializeMoveDirectionCanvas.button[i].gameObject.SetActive(true);
					count += 1;
				}
			}

			//2以上の場合移動先選択を出すようにする
			if (count >= 2)
			{
				waitInput = true;
				serializeMoveDirectionCanvas.canvas.SetActive(true);
			}
			else if (count == 1)
			{
				for (int i = 0; i < canMoveDirection.Length; i++)
				{
					if (canMoveDirection[i] == true)
					{
						//そのまま進む
						ChangeMoveDirectionButton(i);
						break;
					}
				}
			}
		}

		//移動方法が指定移動の場合移動方向をそのまま返す
		if (MovementWay.isLocationMove)
		{
			int value = DiceRoll.ins.value;
			int index = value - diceAmount;

			ChangeMoveDirectionButton((int)MoveLocation.selectLocation.haveLocation.moveDirections[index]);
		}
	}

	//ダイスボタンが押されたか
	public bool isDiceButton = false; 
	public void DiceButton() => StartCoroutine(_DiceButton());
	IEnumerator _DiceButton()
	{
		isDiceButton = true;
		yield return null;
		isDiceButton = false;
		yield break;
	}

	//プレイヤーの移動
	void MovePlayer()
	{
		//transform.position = Map.ins.GetMapTileWorldPos(pos);

		int direction = 0;
		switch (moveDirectionEnum)
		{
			case MoveDirectionEnum.Up:
				direction = -1;
				break;
			case MoveDirectionEnum.Left:
				direction = 1;
				break;
			case MoveDirectionEnum.Right:
				direction = -1;
				break;
			case MoveDirectionEnum.Down:
				direction = 1;
				break;
		}

		transform.DOLocalMove(Map.ins.GetMapTileWorldPos(pos), 0.25f);
		Vector3 cameraPos = Map.ins.GetMapTileWorldPos(pos) - (Vector3)cameraOffset;
		cameraPos.z = -50;
		Camera.main.transform.DOLocalMove(cameraPos, 0.25f);
		transform.localScale = new Vector3(direction, 1, 1);
		serializeMoveDirectionCanvas.canvas.transform.localScale = new Vector3(-0.01f * direction, 0.01f, 0.01f);
		serializeMoveDirectionCanvas.canvas.transform.localRotation = Quaternion.Euler(0, 0, -45 * direction);
	}

	//イベントマス処理
	void CheckEvent()
	{
		Map.TileData tileData = Map.ins.GetTileData(pos);
		switch (tileData.eventID)
		{
			case Map.TileData.EventID.DrawCard:
				Debug.Log("DrawCard!!");
				gridEvent.DrawCard(stage);
				break;
			case Map.TileData.EventID.Battle:
				Debug.Log("Battle!!");
				if (playerStatus.notBattle) break;
				gridEvent.Battle(stage);
				break;
			case Map.TileData.EventID.Shop:
				gridEvent.Shop();
				break;
			case Map.TileData.EventID.Heal:
				gridEvent.Heal();
				break;
		}
	}

	//現在マスのイベントIDを返す
	Map.TileData.EventID GetEvent()
	{
		Map.TileData tileData = Map.ins.GetTileData(pos);
		return tileData.eventID;
	}

	//移動方向指定
	public void ChangeMoveDirectionButton(int index)
	{
		serializeMoveDirectionCanvas.canvas.SetActive(false);
		pos += DirectioVector[index];
		backDirectionVector = DirectioVector[index] * -1;
		moveDirectionEnum = (MoveDirectionEnum)index;
		//MovePlayer();
		waitInput = false;
	}

	//カードの使用をするボタン
	public void UseCardButton()
	{
		if (CardScript.isViewCard != null && isEvent == false)
		{
			StartCoroutine(UseCard());
		}
	}

	//カード使用の処理
	IEnumerator UseCard()
	{
		GameDirector.isEvent = true;

		int cardIndex = CardScript.isViewCard.haveCardIndex; //手札のインデックスを取得

		CardScript.ViewCardInfoDestory(); //カード詳細を非表示に

		playerStatus.haveCards[cardIndex].cardData.CardCost(); //カードのコストを消費
		playerStatus.HaveCardRefresh(); //手札の再描画

		//スキル名の表示
		SkillTextObejct.SetActive(true);
		SkillTextObejct.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStatus.haveCards[cardIndex].cardData.name;
		SkillTextObejct.transform.localScale = new Vector3(0, 1, 1);
		SkillTextObejct.transform.DOScaleX(1, 0.25f);
		yield return new WaitForSeconds(1.0f);

		//カードの効果処理
		playerStatus.haveCards[cardIndex].cardData.cardEffectMethod();
		yield return null;

		//プレイヤーのステータスの再描画
		playerStatus.Redraw();
		yield return new WaitForSeconds(2.0f);

		//スキル名の非表示
		SkillTextObejct.transform.DOScaleX(0, 0.25f).OnComplete(() => SkillTextObejct.SetActive(false));
		yield return new WaitForSeconds(1.5f);

		SkillTextObejct.SetActive(false);
		DiceRoll.Enable(true);
		GameDirector.isEvent = false;
	}
}

[System.Serializable]
class SerializeMoveDirectionCanvas
{
	public GameObject canvas;
	public Button[] button = new Button[4];
}