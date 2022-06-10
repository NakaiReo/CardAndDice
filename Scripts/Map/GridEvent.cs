using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GridEvent : MonoBehaviour
{
	[SerializeField] PlayerStatus playerStatus; //プレイヤーのステータス
	[SerializeField] Transform haveCardCanvas;  //プレイヤーの手札の保存先
	[SerializeField] BattleDirector battleDirector; //戦闘の処理
	[SerializeField] CardEffects cardEffects;       //カード効果
	[SerializeField] GameObject infoCanvas; //情報のUI
	[SerializeField] GameObject player;     //プレイヤー

	[Space(10)]
	[SerializeField] GameObject dumpPrefab;   //カードを捨てる時のUIコンテンツ
	[SerializeField] GameObject shopPrefab;   //ショップのUIコンテンツ
	[SerializeField] GameObject battleCanvas; //バトルのUIコンテンツ

	[Space(10)]
	[SerializeField] Transform playerRect; //プレイヤーの位置
	[SerializeField] GameObject healText;  //ヒール用のテキストオブジェクト

	Vector3 cameraPos { get { return Camera.main.transform.position; } }

	const float FadeInPosY = -12.0f; //カードフェードアウト位置

	public delegate void Method(float time);
	public static IEnumerator UIAnimation(float time, float addTime, Method method)
	{
		method(time);
		yield return new WaitForSeconds(time + addTime);
		yield break;
	}

	private void Start()
	{
		CardScript.canView = true;
		battleCanvas.gameObject.SetActive(false);

		playerStatus.nowScene = CardData.UseType.Map;
	}

	//private void Update()
	//{
	//	if (Input.GetKeyDown(KeyCode.Y))
	//	{
	//		DrawCard(1);
	//	}
	//	if (Input.GetKeyDown(KeyCode.U))
	//	{
	//		Shop();
	//	}
	//	if (Input.GetKeyDown(KeyCode.I))
	//	{
	//		Battle(1);
	//	}
	//	if (Input.GetKeyDown(KeyCode.O))
	//	{
	//		//Boss();
	//	}
	//	if (Input.GetKeyDown(KeyCode.P))
	//	{
	//		Heal();
	//	}
	//}

	//各イベントの呼び出し
	public void DrawCard(int stage) => StartCoroutine(DrawCardIE(stage));
	public void Shop() => StartCoroutine(ShopIE());
	public void Battle(int stage) => StartCoroutine(BattleIE(stage));
	public void Boss(Vector2Int pos) => StartCoroutine(BattleIE(100 + Map.ins.tileDatas[pos.y,pos.x].boss.bossID));
	public void Heal() => StartCoroutine(HealIE());

	/// <summary>
    /// カードを引く処理
    /// </summary>
	IEnumerator DrawCardIE(int stage)
	{
		GameDirector.isEvent = true;
		CardScript.canView = false;
		CardScript.ViewCardInfoDestory();

		//ランダムにカードを選択
		CardData cardData = RandomCardData();

		//何を引いたかのアニメーション
		GameObject ins = Instantiate(GameDirector.CardPrefab);
		ins.transform.SetParent(haveCardCanvas, false);

		RectTransform insRect = ins.GetComponent<RectTransform>();
		CardScript insCardScript = ins.GetComponent<CardScript>();

		Vector3 _pos = Camera.main.transform.position;
		_pos.y += 0.5f;
		_pos.z = 0;

		insRect.localScale = Vector3.zero;
		insRect.anchorMin = new Vector2(0.5f, 0.5f);
		insRect.anchorMax = new Vector2(0.5f, 0.5f);
		insRect.position = _pos;
		insRect.pivot = new Vector2(0.5f, 0.5f);
		insCardScript.cardData = cardData;
		insCardScript.type = CardScript.CardType.ShopCard;
		insCardScript.FlipBackCard();
		insCardScript.RedrawCard();

		//拡大して表示
		yield return StartCoroutine(UIAnimation(0.5f, 0.5f, (time) =>
		{
			insRect.DOScale(Vector3.one * 5.0f, time);
		}));

		//カードを表向きにする
		yield return StartCoroutine(UIAnimation(1.0f, 1.5f, (time) =>
		{
			insCardScript.FlipCardAnimation(true, time);
		}));

		//カードを小さくする
		yield return StartCoroutine(UIAnimation(0.5f, 0.5f, (time) =>
		{
			insRect.DOScale(Vector3.one * 1.0f, time);
		}));

		Transform haveCards = haveCardCanvas.GetChild(0);
		Vector3 pos = haveCards.localPosition;

		//手札をカードを画面外に移動させ手札に加える
		yield return StartCoroutine(UIAnimation(0.5f, 0f, (time) =>
		{
			haveCards.DOMoveY(cameraPos.y + FadeInPosY, time);
			insRect.DOMoveY(cameraPos.y + FadeInPosY, time);
		}));

		//手札を元の位置に戻す
		yield return StartCoroutine(UIAnimation(0.5f, 0.5f, (time) =>
		{
			insCardScript.type = CardScript.CardType.HaveCard;
			ins.transform.SetParent(haveCards, false);
			playerStatus.HaveCardRefresh();
			haveCards.DOLocalMoveY(pos.y, time);
		}));

		CardScript.canView = true;

		//カードを捨てる処理
		yield return StartCoroutine(DumpCard());

		GameDirector.isEvent = false;
		CardScript.canView = true;
		yield break;
	}

	/// <summary>
    /// ショップの処理
    /// </summary>
	IEnumerator ShopIE()
	{
		GameDirector.isEvent = true;
		CardScript.canView = false;
		CardScript.ViewCardInfoDestory();

		//職業が商人なら半額効果を付与
		bool isSeel = (playerStatus.playerName == PlayerStatus.PlayerName.Merchant);

		//ショップUIを設定
		Transform shopPanel = Instantiate(shopPrefab).transform;
		shopPanel.SetParent(haveCardCanvas, false);
		ShopPanel shopPanelScript = shopPanel.GetComponent<ShopPanel>();
		Transform productPanel = shopPanelScript.productPanel.transform;
		HorizontalLayoutGroup horizontal = productPanel.GetComponent<HorizontalLayoutGroup>();

		//ショップの初期化
		shopPanelScript.StatusTextReload();
		shopPanelScript.Redraw();

		//商品の初期化
		List<ShopProduct> products = new List<ShopProduct>();
		for(int i = 0; i < productPanel.childCount; i++)
		{
			products.Add(productPanel.GetChild(i).GetComponent<ShopProduct>());

			CardData randomCardData = RandomCardData();
			products[i].cardScript.cardData = randomCardData;
			products[i].isSeel = isSeel;
			products[i].ResetUI();
			products[i].priceText.text = (int)(randomCardData.price * (isSeel ? 0.5f : 1.0f)) + "G";

			products[i].cardScript.type = CardScript.CardType.ShopCard;
			products[i].cardScript.FlipBackCard();
			products[i].cardScript.RedrawCard();
			products[i].ViewUI(false);
		}

		//商品が購入可能か
		foreach (ShopProduct card in products)
		{
			bool canBuy = card.cardScript.cardData.price <= playerStatus.money;
			card.buyButton.interactable = canBuy;
			card.buyButtonText.text = canBuy ? "購入" : "購入不可";
			card.canBuy = canBuy;
		}

		//カードを中心に集める
		shopPanelScript.finButton.SetActive(false);
		horizontal.spacing = -100;
		foreach (ShopProduct card in products) card.transform.localScale = new Vector3(0, 0, 0);

		//パネル表示
		yield return StartCoroutine(UIAnimation(0.25f, 0.5f, (time) =>
		{
			shopPanelScript.transform.localScale = new Vector3(0, 1, 1);
			shopPanelScript.transform.DOScaleX(1, time);
		}));

		//カードを大きくして表示させる
		yield return StartCoroutine(UIAnimation(0.25f, 0.25f, (time) =>
		{
			foreach (ShopProduct card in products) card.transform.DOScale(new Vector3(1, 1, 1), time);
		}));

		//カードが横に広がっていく
		yield return StartCoroutine(UIAnimation(1.0f, 0.25f, (time) =>
		{
			DOTween.To(() => horizontal.spacing, (num) => horizontal.spacing = num, 200, time);
		}));

		//UIの操作ができるように
		yield return StartCoroutine(UIAnimation(1.0f, 0.5f, (time) =>
		{
			shopPanelScript.finButton.SetActive(true);
			foreach (ShopProduct card in products) card.ViewUI(true);
			foreach (ShopProduct card in products) card.ScaleAnim(time);
		}));

		CardScript.canView = true;

		//終了待ち
		while (true)
		{
			shopPanelScript.Redraw();

			foreach (ShopProduct card in products)
			{
				if (card.isBuy == true) continue;
				bool canBuy = card.cardScript.cardData.price <= playerStatus.money;
				card.buyButton.interactable = canBuy;
				card.buyButtonText.text = canBuy ? "購入" : "購入不可";
				card.canBuy = canBuy;
			}
			if (shopPanelScript.isPush == true) break;
			yield return null;
		}

		CardScript.canView = false;

		//カード情報を破棄
		CardScript.ViewCardInfoDestory();

		Transform haveCards = haveCardCanvas.GetChild(0);
		Vector3 pos = haveCards.localPosition;

		//手札をカードを画面外に移動させ手札に加える
		yield return StartCoroutine(UIAnimation(0.5f, 0.0f, (time) =>
		{
			foreach (ShopProduct card in products) card.ViewUI(false);
			haveCards.DOMoveY(cameraPos.y + FadeInPosY, time);
			foreach (ShopProduct card in products) if(card.isBuy == true) card.transform.DOMoveY(cameraPos.y + FadeInPosY, time);
		}));

		//UIをフェードイン
		yield return StartCoroutine(UIAnimation(0.5f, 0.0f, (time) =>
		{
			shopPanelScript.panel.transform.DOScaleX(0, time);
			shopPanelScript.finButton.transform.DOScaleX(0, time);
		}));

		//手札を元の位置に戻す
		yield return StartCoroutine(UIAnimation(0.5f, 0.5f, (time) =>
		{
			foreach (ShopProduct card in products)
			{
				card.cardScript.type = CardScript.CardType.HaveCard;
				if (card.isBuy == true) card.cardScript.transform.SetParent(haveCards, false);
			}
			playerStatus.HaveCardRefresh();
			haveCards.DOLocalMoveY(pos.y, time);
		}));


		//カードを捨てる処理
		yield return StartCoroutine(DumpCard());

		playerStatus.status.pos = playerStatus.transform.localPosition;

		GameDirector.isEvent = false;
		CardScript.canView = true;
		CardScript.ViewCardInfoDestory();
		yield break;
	}

	/// <summary>
    /// 戦闘の処理
    /// </summary>
	IEnumerator BattleIE(int stage)
	{
		GameDirector.isEvent = true;
		CardScript.canView = true;
		CardScript.ViewCardInfoDestory();

		playerStatus.nowScene = CardData.UseType.Battle; //プレイヤーの現在状況を戦闘状態へ

		yield return new WaitForSeconds(0.5f);

		//戦闘の種類ごとにならBGMを変更
		if (stage <= 100) SoundDirector.PlayBGM("Battle");
		else SoundDirector.PlayBGM("BossBattle");

		//戦闘のUIを表示
		Transform panelTransform = battleCanvas.transform.GetChild(0);
		battleDirector.gameObject.SetActive(true);
		battleDirector.Reset(stage);
		panelTransform.localScale = new Vector3(0, 1, 1);

		yield return StartCoroutine(UIAnimation(0.25f, 0.0f, (time) =>
		{
			panelTransform.DOScaleX(1, time);
		}));

		//戦闘が終わるまで待機
		CardScript.canView = true;
		yield return StartCoroutine(battleDirector.Battle());

		//倒したのがボスなら倒れさせる
		if (stage > 100) Map.ins.bossScripts[stage - 101].Die();

		//戦闘のUIを非表示
		yield return StartCoroutine(UIAnimation(0.25f, 0.5f, (time) =>
		{
			panelTransform.DOScaleX(0, time);
		}));

		//マップに戻す処理
		playerStatus.nowScene = CardData.UseType.Map;
		playerStatus.status.pos = player.transform.localPosition;
		playerStatus.HaveCardRefresh();
		if (stage > 100)
		{
			//最終戦だったら
			if (Map.ins.bossScripts[stage - 101].lastBattle)
			{
				CardScript.ViewCardInfoDestory();
				CardScript.canView = false;
				yield break;
			}
		}

		GameDirector.isEvent = false;
		CardScript.canView = true;
		CardScript.ViewCardInfoDestory();
		yield break;
	}

	/// <summary>
    /// 回復マスの処理
    /// </summary>
	IEnumerator HealIE()
	{
		GameDirector.isEvent = true;
		CardScript.canView = false;
		CardScript.ViewCardInfoDestory();

		yield return new WaitForSeconds(1.00f);

		playerStatus.ClacStatus(); //プレイヤーのステータスの再計算

		Vector2 random = Vector2.zero;

		//HPの回復量表示
		cardEffects.HealPercent(CardEffects.EffectTarget.Player, 25, CardData.CostType.Cooldown);
		playerStatus.Redraw();

		yield return new WaitForSeconds(0.5f);

		//マナの回復量表示
		cardEffects.ManaPercent(CardEffects.EffectTarget.Player, 25);
		playerStatus.Redraw();

		yield return new WaitForSeconds(2.0f);

		GameDirector.isEvent = false;
		CardScript.canView = true;
		CardScript.ViewCardInfoDestory();
		yield break;
	}


	/// <summary>
    /// カードデータベースからランダムにカードを選択
    /// </summary>
	CardData RandomCardData()
	{
		int random = Random.Range(0, CardDataDirector.ins.cardBaseData.CardList.Count);
		CardData cardData = CardDataDirector.ins.cardBaseData.CardList[random];

		return cardData;
	}

	/// <summary>
    /// カードを捨てる処理
    /// </summary>
	IEnumerator DumpCard()
	{
		CardScript.canView = true;
		Debug.Log(playerStatus.haveCards.Count);

		Transform haveCards = haveCardCanvas.GetChild(0);
		Vector3 pos = haveCards.localPosition;

		float time;

		//カードが5枚を超えたら捨てる処理を
		while (playerStatus.haveCards.Count > 5)
		{
			int index = -1;
			GameObject dump = Instantiate(dumpPrefab, Vector3.zero, Quaternion.identity);
			DumpCanvas dumpCanvas = dump.GetComponent<DumpCanvas>();
			dump.transform.SetParent(infoCanvas.transform, false);
			dump.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);
			dump.GetComponent<RectTransform>().DOScaleY(1, 0.25f);
			playerStatus.HaveCardRefresh();

			//捨てるカードの選択
			while (true)
			{
				bool check = CardScript.isViewCard != null;

				dumpCanvas.submitButton.gameObject.SetActive(check);

				if (dumpCanvas.isPush == true && check == true)
				{
					//選択されたカードが見つかった時にbreak
					index = CardScript.isViewCard.haveCardIndex;
					if (index != -1) break;
				}

				yield return null;
			}
			CardScript.canView = false;
			dumpCanvas.submitButton.gameObject.SetActive(false);

			//UIを小さくしてフェードイン
			time = 0.5f;
			dump.GetComponent<RectTransform>().DOScaleY(0, time);
			CardScript.isViewCard.transform.DOScale(Vector3.zero, time);
			yield return new WaitForSeconds(time + 0.25f);

			//手札を画面外へ
			time = 0.5f;
			Destroy(dump);
			CardScript.ViewCardInfoDestory();
			haveCards.DOMoveY(cameraPos.y + FadeInPosY, time);
			yield return new WaitForSeconds(time);

			//選択したカードを削除する
			playerStatus.DestoryHaveCard(index);
			yield return null;

			//手札をもとの位置に戻す
			time = 0.5f;
			playerStatus.HaveCardRefresh();
			haveCards.DOLocalMoveY(pos.y, time);
			yield return new WaitForSeconds(time + 0.5f);

			Debug.Log(playerStatus.haveCards.Count);
			CardScript.canView = true;
		}

		yield break;
	}
}
