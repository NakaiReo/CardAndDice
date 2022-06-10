using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleDirector : MonoBehaviour
{
	[SerializeField] Transform battleCanvas;   //戦闘のUI
	[SerializeField] GameObject damageText;    //ダメージのUIコンテンツ
	[SerializeField] CardEffects cardEffects;  //カード効果
	[SerializeField] Image mapBackgroundImage;    //マップの背景
	[SerializeField] Image battleBackgroundImage; //戦闘背景

	[Space(15)]
	[SerializeField] PlayerStatus playerStatus; //プレイヤーのステータス
	[SerializeField] RectTransform playerRect;  //プレイヤーの位置
	[SerializeField] Animator playerAnimator;   //プレイヤーのアニメーション
	[SerializeField] StatusBarSerializeField playerHpBar;    //プレイヤーのHPコンテンツ
	[SerializeField] StatusBarSerializeField playerManaBar;  //プレイヤーのManaコンテンツ
	[SerializeField] StatusBarSerializeField playerSpeedBar; //プレイヤーのSpeedコンテンツ

	[Space(15)]
	[SerializeField] EnemyStatus enemyStatus; //敵のステータス
	[SerializeField] RectTransform enemyRect; //敵の位置
	[SerializeField] Animator enemyAnimator;  //敵のアニメーション
	[SerializeField] StatusBarSerializeField enemyHpBar;    //敵のHPコンテンツ
	[SerializeField] StatusBarSerializeField enemyManaBar;  //敵のManaコンテンツ
	[SerializeField] StatusBarSerializeField enemySpeedBar; //敵のSpeedコンテンツ

	[Space(15)]
	[SerializeField] Button[] actionButtons = new Button[3]; //戦闘ボタン
	[SerializeField] Transform actionButtonsPanel;           //戦闘ボタンのパネル

	[Space(15)]
	[SerializeField] GameObject winPanel; //処理時のパネル

	[Header("Attack")]
	[SerializeField] GameObject attackActionObject; //攻撃時のパネル
	[SerializeField] Slider attackSlider; //クリティカル処理のSlider
	[SerializeField] Slider attackPos;    //クリティカル位置
	[SerializeField] RectTransform attackArea;          //クリティカル判定
	[SerializeField] RectTransform attackPosHandleArea; //クリティカル範囲
	[SerializeField] RectTransform attackPosHandel;     //クリティカル位置
	[Header("Card")]
	[SerializeField] Transform cardInfoPanel; //カードの情報
	[SerializeField] GameObject backButton;   //戻るボタン 
	[SerializeField] GameObject submitButton; //使用ボタン
	[Space(5)]
	[SerializeField] GameObject SkillTextObejct; //スキル名のオブジェクト
	//[Header("Escape")]

	//[Space(15)]
	//[SerializeField] Image resultPanel;
	//[SerializeField] TextMeshProUGUI resultText;

	//攻撃までの時間
	float playerSpeed;
	float enemySpeed;

	//攻撃に必要な時間
	float playerNeedSpeed = 100;
	float enemyNeedSpeed = 100;

	float standardBattleSpeed = 0.05f; //Battleスピード
	KeyCode hiBattkeSpeedKey = KeyCode.LeftShift; //戦闘加速キー
	KeyCode skipKey = KeyCode.LeftControl;        //戦闘スキップキー

	int stage;

	bool  isAttack;
	float attackSpeed;
	float attackSubmit;

	bool isCritical;

	bool isBattle;
	bool isInputCommand;

	bool takeDamage;
	bool takeSpin;

	float addOverDamage { get { return playerStatus.playerName == PlayerStatus.PlayerName.Knight ? 0.25f : 0.0f; } } //クリティカル倍率の加算
	public void PushActionButton(int id) => action = (Action)id; //押されたボタンの種類
	enum Action
	{
		None,
		Attack,
		Card,
		Escape
	}
	Action action;

	/// <summary>
    /// アクションボタンの有効化
    /// </summary>
    /// <param name="b">有効にするか</param>
	void ActionButtonEnable(bool b)
	{
		actionButtonsPanel.gameObject.SetActive(b);
		foreach(Button button in actionButtons)
		{
			button.gameObject.SetActive(b);
			button.interactable = b;
			button.transform.Find("Text").gameObject.SetActive(b);
		}
	}

	Vector3 playerPos; //プレイヤーの位置
	Vector3 enemyPos;  //敵の位置

	/// <summary>
    /// 初期化
    /// </summary>
    /// <param name="stage">ステージ数</param>
	public void Reset(int stage)
	{
		this.stage = stage;
		battleBackgroundImage.sprite = mapBackgroundImage.sprite; //背景の設定

		enemyStatus.LoadDataTier(stage); //ステージに設定された敵のデータを取得

		playerStatus.overDamageAreaTemp = 0;     //クリティカル範囲の一時的強化の値
		playerStatus.overDamageMultipleTemp = 0; //クリティカル倍率の一時的強化の値

		//防御値によるダメ減衰上限値の設定
		playerStatus.status.defMax = (playerStatus.playerName == PlayerStatus.PlayerName.Paladin) ? 0.8f : 0.5f;
		enemyStatus.status.defMax = 0.5f;

		//フラグや値をリセット
		isAttack = false;
		isCritical = false;
		takeDamage = false;
		takeSpin = false;

		playerSpeed = 0;
		enemySpeed = 0;
		playerNeedSpeed = 100;
		enemyNeedSpeed = 100;

		isBattle = true;
		isInputCommand = false;

		//プレイヤーと敵の位置を渡す
		playerStatus.status.pos = playerRect.RectToWorld();
		enemyStatus.status.pos = enemyRect.RectToWorld();

		//すべてのパネルを非表示に
		ActionButtonEnable(false);
		cardInfoPanel.gameObject.SetActive(false);
		backButton.SetActive(false);
		submitButton.SetActive(false);
		attackActionObject.SetActive(false);
		SkillTextObejct.SetActive(false);
		winPanel.SetActive(false);

		//スケールをリセット
		playerRect.localScale = new Vector3(0, 1, 1);
		enemyRect.localScale = new Vector3(0, 1, 1);

		//手札を再描画
		playerStatus.HaveCardRefresh();

		//戦闘UIを再描画
		Redraw();
	}

	/// <summary>
    /// 戦闘開始
    /// </summary>
	public IEnumerator Battle()
	{
		//アニメーターを渡す
		playerStatus.status.trigger.animator = playerAnimator;
		enemyStatus.status.trigger.animator  = enemyAnimator;

		//アニメーションをリセット
		playerAnimator.SetTrigger("Reset");
		enemyAnimator.SetTrigger("Reset");
		playerRect.GetComponent<AnimatorManager>().ChangeSprite(0);
		enemyRect.GetComponent<AnimatorManager>().ChangeSprite(0);

		//向きを合わせる
		playerRect.DOScaleX(-1, 0.25f);
		enemyRect.DOScaleX(1, 0.25f);
		yield return new WaitForSeconds(0.25f);

		//戦闘中はループさせる
		while (true)
		{
			//プレイヤーの位置を更新
			playerStatus.status.pos = playerRect.RectToWorld();
			enemyStatus.status.pos = enemyRect.RectToWorld();
			
			//プレイヤーのSpeedゲージがたまったらコマンド選択させる
			if (playerSpeed >= playerNeedSpeed)
			{
				isInputCommand = true; //入力待ちに

				SoundDirector.PlaySE("Battle/Charge");

				//カードのクールダウン
				playerStatus.CardCoolDown();
				playerStatus.HaveCardRefresh();

				//Gotoフラグ
				restart:

				//入力待ち
				yield return StartCoroutine(WaitPlayerInputButton());

				//入力された物ごとに処理
				switch (action)
				{
					case Action.Attack:
						//攻撃処理
						yield return StartCoroutine(Attack());

						//ダメージ計算をし、ダメージ表示
						float damage = Damage(playerStatus.status, enemyStatus.status, (attackSubmit < playerStatus.overDamageArea / 2.0f) ? (playerStatus.overDamageMultiple + addOverDamage) : 1.0f);
						DamageText(enemyStatus.status.pos, damage, isCritical);

						break;
					case Action.Card:

						int cardIndex;
						playerStatus.HaveCardRefresh();

						var co = CardSelect();
						yield return StartCoroutine(co);
						cardIndex = (int)co.Current;

						//カードが何かしら選択されるまで待つ
						if (cardIndex == -1) goto restart;
						
						//カードの処理
						playerStatus.haveCards[cardIndex].cardData.CardCost();
						playerStatus.HaveCardRefresh();

						SkillTextObejct.SetActive(true);
						SkillTextObejct.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStatus.haveCards[cardIndex].cardData.name;
						SkillTextObejct.transform.localScale = new Vector3(0, 1, 1);
						SkillTextObejct.transform.DOScaleX(1, 0.25f);
						yield return new WaitForSeconds(1.0f);

						playerStatus.haveCards[cardIndex].cardData.cardEffectMethod();
						yield return null;
						Redraw();
						yield return new WaitForSeconds(2.0f);

						SkillTextObejct.transform.DOScaleX(0, 0.25f).OnComplete(() => SkillTextObejct.SetActive(false));

						break;
					case Action.Escape:
						CardScript.ViewCardInfoDestory();

						//脱出に成功するかどうか
						if(300 > Random.Range(0, 1000))
						{
							if(stage <= 100) goto Escape;
						}

						//逃げられなかった時の表示
						SkillTextObejct.SetActive(true);
						if (stage <= 100) SkillTextObejct.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "逃げ切れなかった";
						else SkillTextObejct.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "ボスからは逃げ切れない!";
						SkillTextObejct.transform.localScale = new Vector3(0, 1, 1);
						SkillTextObejct.transform.DOScaleX(1, 0.25f);
						yield return new WaitForSeconds(1.0f);

						SkillTextObejct.transform.DOScaleX(0, 0.25f).OnComplete(() => SkillTextObejct.SetActive(false));
						break;
				}

				CardScript.ViewCardInfoDestory();

				//プレイヤーの攻撃のチャージをリセット
				playerSpeed = 0;
				isInputCommand = false;
			}

			//敵の攻撃がチャージされたら
			else if (enemySpeed >= enemyNeedSpeed)
			{
				isInputCommand = true;
				if (true)
				{
					//ダメージを計算し、ダメージを表示
					float damage = Damage(enemyStatus.status, playerStatus.status, 1.0f);
					DamageText(playerStatus.status.pos, damage, isCritical);

					if (damage > 0) takeDamage = true; //攻撃が当たった
					else takeSpin = true;              //回避された

					//職業が踊り子で、攻撃を回避したらカウンター
					if (takeSpin && playerStatus.playerName == PlayerStatus.PlayerName.Dancer) cardEffects.ATK_PercentDamage(CardEffects.EffectTarget.Enemy, CardEffects.EffectTarget.Player, 50, CardData.CostType.Cooldown);
					takeSpin = false;

					//敵の攻撃のチャージをリセット
					enemySpeed = 0;
					isInputCommand = false;
				}
			}

			//どちらかの体力が0以下になったら戦闘終了
			if (playerStatus.status.hp <= 0 || enemyStatus.status.hp <= 0) isBattle = false;
			if (isBattle == false)
			{
				break;
			}

			//入力待ちがなければ
			if (isInputCommand == false)
			{
				float speed = standardBattleSpeed * 0.1f;

				//ダメージを食らったらチャージを加算
				if (takeDamage == true) playerSpeed += 30;
				takeDamage = false;

				//それぞれの攻撃をチャージ
				playerSpeed += playerStatus.status.spd * speed;
				enemySpeed += enemyStatus.status.spd * speed;

				//チャージが最大値を超えないように
				playerSpeed = Mathf.Min(playerSpeed, playerNeedSpeed);
				enemySpeed = Mathf.Min(enemySpeed, enemyNeedSpeed);
			}

			//再描画
			Redraw();

			//戦闘スピード
			float battleSpeed = standardBattleSpeed / (Input.GetKey(hiBattkeSpeedKey) ? 3.0f : 1.0f);

			//戦闘スキップが押されている時は待たない
			if(isInputCommand != false || !Input.GetKey(skipKey))
			yield return new WaitForSeconds(battleSpeed);
		}

		//戦闘終了の処理
		yield return StartCoroutine(BattleEnd());

		//逃げる成功時のGotoフラグ
		Escape:

		//マップに戻る処理
		playerStatus.HaveCardRefresh();
		GameDirector.ReturnMap();
		yield break;
	}

	/// <summary>
    /// プレイヤーの入力待ち
    /// </summary>
	IEnumerator WaitPlayerInputButton()
	{
		action = Action.None;

		ActionButtonEnable(true);
		actionButtonsPanel.transform.localScale = new Vector3(1, 0, 1);
		actionButtonsPanel.transform.DOScaleY(1, 0.25f);

		while (true)
		{
			if (Input.GetKeyDown(KeyCode.Q)) action = Action.Attack;
			if (Input.GetKeyDown(KeyCode.W)) action = Action.Card;
			if (Input.GetKeyDown(KeyCode.E)) action = Action.Escape;
			if (action != Action.None) break;
			yield return null;
		}

		actionButtonsPanel.DOScaleY(0, 0.25f);
		yield return new WaitForSeconds(0.25f);

		ActionButtonEnable(false);
		yield break;
	}

	/// <summary>
    /// 攻撃処理
    /// </summary>
	IEnumerator Attack()
	{
		isAttack = true;
		attackSpeed = Random.Range(0.006f, 0.0075f); //ゲージのたまる速度

		//攻撃UIの表示
		attackActionObject.SetActive(true);
		attackSlider.value = 0;
		float sizeX = (attackArea.sizeDelta.x + attackPosHandleArea.sizeDelta.x);
		Vector2 size = attackPosHandel.sizeDelta; size.x = sizeX * (playerStatus.overDamageArea / 2.0f * 0.01f);
		attackPosHandel.sizeDelta = size;
		Debug.Log(sizeX + ", " + size);
		attackPos.value = Random.Range(40f, 80f);
		attackActionObject.transform.localScale = new Vector3(1, 0, 1);
		attackActionObject.transform.DOScaleY(1, 0.25f);
		yield return new WaitForSeconds(0.5f);

		//クリティカル判定
		StartCoroutine(AttackBar());

		//クリティカル判定決定
		while (true)
		{
			if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Q)) break;
			if (attackSlider.value >= 100f) break;
			yield return null;
		}

		isAttack = false;

		//クリティカル位置からどれだけ離れているか
		attackSubmit = Mathf.Abs(attackSlider.value - attackPos.value);

		//クリティカル範囲に入っていればクリティカル判定に
		if (attackSubmit < playerStatus.overDamageArea / 2.0f)
		{
			Debug.Log("OverDamage!");
			GameObject worldIns = Instantiate(damageText) as GameObject;
			worldIns.transform.position = attackActionObject.GetComponent<RectTransform>().RectToWorld() + new Vector3(0, 2.0f, 0);
			worldIns.GetComponent<WorldDamageText>().tmp.text = "Over Damage!! (x" + (playerStatus.overDamageMultiple + addOverDamage) + ")";
			worldIns.GetComponent<WorldDamageText>().tmp.color = new Color(1, 0, 0, 1);
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.25f);

		attackActionObject.transform.DOScaleY(0, 0.25f);
		yield return new WaitForSeconds(0.3f);

		attackActionObject.SetActive(false);
		yield break;
	}

	/// <summary>
    ///クリティカル判定のバーの伸び
    /// </summary>
	IEnumerator AttackBar()
	{
		while (isAttack)
		{
			attackSlider.value += 1f;
			yield return new WaitForSeconds(0.005f);
		}
	}

	/// <summary>
    /// カード選択
    /// </summary>
	IEnumerator CardSelect()
	{
		int cardIndex = -1;

		cardInfoPanel.gameObject.SetActive(true);
		backButton.SetActive(true);

		while (cardIndex == -1)
		{
			yield return null;

			if (isBack) break; //戻るが押された

			submitButton.SetActive(false);
			CardScript cardScript = CardScript.isViewCard;

			if (cardScript == null) continue; //何も選択されていない

			if (cardScript.CanUseCheck(CardData.UseType.Battle) == false) continue; //Battle中に選択できない

			//CardData card = cardScript.cardData;
			//if (card.type == CardData.Type.Equipment) continue;
			//if (card.useType == CardData.UseType.Map) continue;
			//if (card.costType == CardData.CostType.Mana && playerStatus.status.mana < card.manaCost) continue;
			//if (card.costType == CardData.CostType.Cooldown && card.nowCooldown > 0) continue;

			submitButton.SetActive(true);
			if (isPush == false) continue; //使用が押されてない

			cardIndex = CardScript.isViewCard.haveCardIndex;
			if (cardIndex == -1) continue; //カードのインデックスが存在しない

			break;
		}

		//カードのUIを非表示
		cardInfoPanel.gameObject.SetActive(false);
		backButton.SetActive(false);
		submitButton.SetActive(false);

		CardScript.ViewCardInfoDestory();

		yield return cardIndex;
	}

	/// <summary>
	/// UIの再描画
	/// </summary>
	void Redraw()
	{
		//ステータスの再計算
		playerStatus.ResetStatus();
		enemyStatus.ResetStatus();

		//ゲージの描画
		playerHpBar.slider.value = Extend.TwoRatio(playerStatus.status.hp, playerStatus.status.maxHp);
		playerManaBar.slider.value = Extend.TwoRatio(playerStatus.status.mana, playerStatus.status.maxMana);
		playerSpeedBar.slider.value = Extend.TwoRatio(playerSpeed, playerNeedSpeed);
		enemyHpBar.slider.value = Extend.TwoRatio(enemyStatus.status.hp, enemyStatus.status.maxHp);
		enemyManaBar.slider.value = Extend.TwoRatio(enemyStatus.status.mana, enemyStatus.status.maxMana);;
		enemySpeedBar.slider.value = Extend.TwoRatio(enemySpeed, enemyNeedSpeed);

		//テキストの描画
		playerHpBar.statusText.text = playerStatus.status.hp + "/" + playerStatus.status.maxHp;
		playerManaBar.statusText.text = playerStatus.status.mana + "/" + playerStatus.status.maxMana;
		playerSpeedBar.statusText.text = playerSpeed.ToString("0") + "/" + playerNeedSpeed.ToString("0");
		enemyHpBar.statusText.text = enemyStatus.status.hp + "/" + enemyStatus.status.maxHp;
		enemyManaBar.statusText.text  =enemyStatus.status.mana + "/" + enemyStatus.status.maxMana;
		enemySpeedBar.statusText.text = enemySpeed.ToString("0") + "/" + enemyNeedSpeed.ToString("0");
	}

	/// <summary>
	/// ダメージの算出
	/// </summary>
	/// <param name="attacker">攻撃側のステータス</param>
	/// <param name="defender">防御側のステータス</param>
	/// <returns></returns>
	public int Damage(Status attacker, Status defender,float overDamage)
	{
		//クリティカル判定
		isCritical = false;
		if (attacker.cri > Random.Range(0, 1000))
		{
			isCritical = true;
		}

		//回避判定
		else if (defender.avo > Random.Range(0, 1000))
		{
			attacker.trigger.Attack();
			defender.trigger.Avoidance();

			SoundDirector.PlaySE("Battle/Spin");

			return -1;
		}

		//防御による減衰値
		float protect = defender.def / (attacker.atk * 1.5f);
		protect = Mathf.Clamp(protect, 0.0f, defender.defMax);

		//ダメージ計算
		int damage = Mathf.CeilToInt(attacker.atk * (1.0f - protect) * Random.Range(0.85f, 1.15f));
		damage = Mathf.Max(damage, 0);
		damage = (int)(damage * (isCritical ? 2.5f : 1.0f) * overDamage);

		//HPを減らす
		defender.hp -= damage;
		if (attacker.hp <= 0 || defender.hp <= 0) isBattle = false;	

		//アニメーション
		attacker.trigger.Attack();
		defender.trigger.TakeDamage();

		SoundDirector.PlaySE("Battle/Damage");

		Redraw();

		return damage;
	}

	/// <summary>
	/// ダメージテキストを生成する
	/// </summary>
	/// <param name="spawnTransform">どこに生成するか</param>
	/// <param name="damage">表示する数値(-1は回避)</param>
	public void DamageText(Vector3 vector, float damage, bool critical)
	{
		string damageTextString = (damage != -1) ? damage.ToString("0") : "回避!!";

		Color color;
		if (critical) color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		else if (damage == -1) color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		else color = new Color(1.0f, 0.5f, 0.5f, 1.0f);

		GameObject obj = Instantiate(damageText);
		//obj.transform.SetParent(transform.root, false);
		obj.transform.localPosition = vector;

		obj.GetComponent<WorldDamageText>().tmp.text = damageTextString;
		obj.GetComponent<WorldDamageText>().tmp.color = color;
	}

	/// <summary>
    /// 回復テキストを生成
    /// </summary>
    /// <param name="vector">どこに生成するか</param>
    /// <param name="value">回復量</param>
	public void HealText(Vector3 vector, float value)
	{
		Color color = new Color(0.5f, 1.0f, 0.5f, 1.0f);

		GameObject obj = Instantiate(damageText);
		//obj.transform.SetParent(transform.root, false);
		obj.transform.localPosition = vector;

		obj.GetComponent<WorldDamageText>().tmp.text = value.ToString("0");
		obj.GetComponent<WorldDamageText>().tmp.color = color;
		SoundDirector.PlaySE("Battle/Heal");
	}

	/// <summary>
    /// マナの回復を生成
    /// </summary>
    /// <param name="vector">どこに生成するか</param>
    /// <param name="value">回復量</param>
	public void ManaText(Vector3 vector, float value)
	{
		Color color = new Color(0.5f, 0.5f, 1.0f, 1.0f);

		GameObject obj = Instantiate(damageText);
		//obj.transform.SetParent(transform.root, false);
		obj.transform.localPosition = vector;

		obj.GetComponent<WorldDamageText>().tmp.text = value.ToString("0");
		obj.GetComponent<WorldDamageText>().tmp.color = color;
		SoundDirector.PlaySE("Battle/Heal");
	}

	/// <summary>
    /// お金の表示の生成
    /// </summary>
    /// <param name="vector">どこに生成するか</param>
    /// <param name="value">入手量</param>
	public void MoneyText(Vector3 vector, float value)
	{
		Color color = new Color(1.0f, 1.0f, 0.5f, 1.0f);

		GameObject obj = Instantiate(damageText);
		//obj.transform.SetParent(transform.root, false);
		obj.transform.localPosition = vector;

		obj.GetComponent<WorldDamageText>().tmp.text = value.ToString("0");
		obj.GetComponent<WorldDamageText>().tmp.color = color;
	}

	/// <summary>
    /// 戦闘終了処理
    /// </summary>
	IEnumerator BattleEnd()
	{
		int result = 0;

		//プレイヤーの死亡判定
		if(playerStatus.status.hp <= 0)
		{
			playerStatus.status.trigger.Down();
			result -= 1;
		}

		//敵の死亡判定
		if(enemyStatus.status.hp <= 0)
		{
			enemyStatus.status.trigger.Down();
			result += 1;
		}

		yield return new WaitForSeconds(1.5f);

		//resultが1でなければゲームオーバー
		if(result != 1)
		{
			SoundDirector.PlayBGM("Gameover", false);
			GameDirector.GameoverCanvas.GetComponent<Animator>().SetTrigger("Gameover");
			yield return new WaitForSeconds(5.0f);
			while (true)
			{
				if (Input.anyKeyDown)
				{
					Fade.ins.FadeIn("Title", 1.5f);
				}
				yield return null;
			}
		}

		//resultが1なら戦闘勝利
		else
		{
			SoundDirector.PlayBGM("Win");

			//お金の入手
			playerStatus.money += enemyStatus.money;

			//レベルアップ処理
			int upLevel = 0;
			playerStatus.needExp -= enemyStatus.exp;
			while(playerStatus.needExp < 0)
			{
				upLevel += 1;
				playerStatus.level += 1;
				playerStatus.LevelUp();

				playerStatus.nextExp = (int)(playerStatus.nextExp * playerStatus.nextExpMagnification);
				playerStatus.needExp += playerStatus.nextExp;

				if (upLevel > 100) break;
			}

			//リザルトUIの表示
			WinPanel winPanelScript = winPanel.GetComponent<WinPanel>();
			winPanelScript.Load(enemyStatus.exp, enemyStatus.money, playerStatus.needExp, upLevel);
			winPanel.SetActive(true);
			winPanel.transform.localScale = new Vector3(0, 1, 1);
			winPanel.transform.DOScaleX(1, 0.25f);
			yield return new WaitForSeconds(0.5f);

			//入力待ち
			while (true)
			{
				if (Input.anyKeyDown) break;
				yield return null;
			}
		}

		//手札の再描画
		playerStatus.HaveCardRefresh();
		yield break;
	}

	//ボタンが押された
	[HideInInspector] public bool isPush = false; public void PushButton() => StartCoroutine(_PushButton()); IEnumerator _PushButton()
	{
		isPush = true;
		yield return null;

		isPush = false;
		yield break;
	}

	//戻るが押された
	[HideInInspector] public bool isBack = false; public void BackButton() => StartCoroutine(_BackButton()); IEnumerator _BackButton()
	{
		isBack = true;
		yield return null;

		isBack = false;
		yield break;
	}

	/// <summary>
	/// ステータスバー用のクラス
	/// </summary>
	[System.Serializable]
	class StatusBarSerializeField
	{
		public Slider slider;
		public TextMeshProUGUI statusText;
	}
}