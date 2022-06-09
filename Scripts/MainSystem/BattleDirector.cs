using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleDirector : MonoBehaviour
{
	[SerializeField] Transform battleCanvas;
	[SerializeField] GameObject damageText;
	[SerializeField] CardEffects cardEffects;
	[SerializeField] Image mapBackgroundImage;
	[SerializeField] Image battleBackgroundImage;

	[Space(15)]
	[SerializeField] PlayerStatus playerStatus;
	[SerializeField] RectTransform playerRect;
	[SerializeField] Animator playerAnimator;
	[SerializeField] StatusBarSerializeField playerHpBar;
	[SerializeField] StatusBarSerializeField playerManaBar;
	[SerializeField] StatusBarSerializeField playerSpeedBar;

	[Space(15)]
	[SerializeField] EnemyStatus enemyStatus;
	[SerializeField] RectTransform enemyRect;
	[SerializeField] Animator enemyAnimator;
	[SerializeField] StatusBarSerializeField enemyHpBar;
	[SerializeField] StatusBarSerializeField enemyManaBar;
	[SerializeField] StatusBarSerializeField enemySpeedBar;

	[Space(15)]
	[SerializeField] Button[] actionButtons = new Button[3];
	[SerializeField] Transform actionButtonsPanel;

	[Space(15)]
	[SerializeField] GameObject winPanel;

	[Header("Attack")]
	[SerializeField] GameObject attackActionObject;
	[SerializeField] Slider attackSlider;
	[SerializeField] Slider attackPos;
	[SerializeField] RectTransform attackArea;
	[SerializeField] RectTransform attackPosHandleArea;
	[SerializeField] RectTransform attackPosHandel;
	[Header("Card")]
	[SerializeField] Transform cardInfoPanel;
	[SerializeField] GameObject backButton;
	[SerializeField] GameObject submitButton;
	[Space(5)]
	[SerializeField] GameObject SkillTextObejct;
	//[Header("Escape")]

	//[Space(15)]
	//[SerializeField] Image resultPanel;
	//[SerializeField] TextMeshProUGUI resultText;

	//現在の攻撃までの時間
	float playerSpeed;
	float enemySpeed;

	float playerNeedSpeed = 100;
	float enemyNeedSpeed = 100;

	float standardBattleSpeed = 0.05f;
	KeyCode hiBattkeSpeedKey = KeyCode.LeftShift;
	KeyCode skipKey = KeyCode.LeftControl;

	int stage;

	bool isAttack;
	float attackSpeed;
	float attackSubmit;

	bool isCritical;

	bool isBattle;
	bool isInputCommand;

	bool takeDamage;
	bool takeSpin;

	float addOverDamage { get { return playerStatus.playerName == PlayerStatus.PlayerName.Knight ? 0.25f : 0.0f; } }
	public void PushActionButton(int id) => action = (Action)id;
	enum Action
	{
		None,
		Attack,
		Card,
		Escape
	}
	Action action;

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

	Vector3 playerPos;
	Vector3 enemyPos;

	public void Reset(int stage)
	{
		this.stage = stage;
		battleBackgroundImage.sprite = mapBackgroundImage.sprite;

		enemyStatus.LoadDataTier(stage);

		playerStatus.overDamageAreaTemp = 0;
		playerStatus.overDamageMultipleTemp = 0;

		playerStatus.status.defMax = (playerStatus.playerName == PlayerStatus.PlayerName.Paladin) ? 0.8f : 0.5f;
		enemyStatus.status.defMax = 0.5f;

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

		playerStatus.status.pos = playerRect.RectToWorld();
		enemyStatus.status.pos = enemyRect.RectToWorld();

		ActionButtonEnable(false);
		cardInfoPanel.gameObject.SetActive(false);
		backButton.SetActive(false);
		submitButton.SetActive(false);
		attackActionObject.SetActive(false);
		SkillTextObejct.SetActive(false);
		winPanel.SetActive(false);

		playerRect.localScale = new Vector3(0, 1, 1);
		enemyRect.localScale = new Vector3(0, 1, 1);

		playerStatus.HaveCardRefresh();

		Redraw();
	}

	public IEnumerator Battle()
	{
		playerStatus.status.trigger.animator = playerAnimator;
		enemyStatus.status.trigger.animator = enemyAnimator;

		playerAnimator.SetTrigger("Reset");
		enemyAnimator.SetTrigger("Reset");

		playerRect.GetComponent<AnimatorManager>().ChangeSprite(0);
		enemyRect.GetComponent<AnimatorManager>().ChangeSprite(0);

		playerRect.DOScaleX(-1, 0.25f);
		enemyRect.DOScaleX(1, 0.25f);
		yield return new WaitForSeconds(0.25f);

		while (true)
		{
			playerStatus.status.pos = playerRect.RectToWorld();
			enemyStatus.status.pos = enemyRect.RectToWorld();
			
			if (playerSpeed >= playerNeedSpeed)
			{
				isInputCommand = true;

				SoundDirector.PlaySE("Battle/Charge");

				playerStatus.CardCoolDown();
				playerStatus.HaveCardRefresh();

				restart:

				yield return StartCoroutine(WaitPlayerInputButton());

				switch (action)
				{
					case Action.Attack:
						yield return StartCoroutine(Attack());

						float damage = Damage(playerStatus.status, enemyStatus.status, (attackSubmit < playerStatus.overDamageArea / 2.0f) ? (playerStatus.overDamageMultiple + addOverDamage) : 1.0f);
						DamageText(enemyStatus.status.pos, damage, isCritical);

						break;
					case Action.Card:

						int cardIndex;
						playerStatus.HaveCardRefresh();

						var co = CardSelect();
						yield return StartCoroutine(co);
						cardIndex = (int)co.Current;

						if (cardIndex == -1) goto restart;
						
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
						if(300 > Random.Range(0, 1000))
						{
							if(stage <= 100) goto Escape;
						}
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

				playerSpeed = 0;
				isInputCommand = false;
			}
			else if (enemySpeed >= enemyNeedSpeed)
			{
				isInputCommand = true;
				if (true)
				{
					float damage = Damage(enemyStatus.status, playerStatus.status, 1.0f);
					DamageText(playerStatus.status.pos, damage, isCritical);

					if (damage > 0) takeDamage = true;
					else takeSpin = true;

					if (takeSpin && playerStatus.playerName == PlayerStatus.PlayerName.Dancer) cardEffects.ATK_PercentDamage(CardEffects.EffectTarget.Enemy, CardEffects.EffectTarget.Player, 50, CardData.CostType.Cooldown);
					takeSpin = false;

					enemySpeed = 0;
					isInputCommand = false;
				}
			}

			if (playerStatus.status.hp <= 0 || enemyStatus.status.hp <= 0) isBattle = false;
			if (isBattle == false)
			{
				break;
			}

			if (isInputCommand == false)
			{
				float speed = standardBattleSpeed * 0.1f;

				if (takeDamage == true) playerSpeed += 30;
				takeDamage = false;

				playerSpeed += playerStatus.status.spd * speed;
				enemySpeed += enemyStatus.status.spd * speed;

				playerSpeed = Mathf.Min(playerSpeed, playerNeedSpeed);
				enemySpeed = Mathf.Min(enemySpeed, enemyNeedSpeed);
			}
			Redraw();

			float battleSpeed = standardBattleSpeed / (Input.GetKey(hiBattkeSpeedKey) ? 3.0f : 1.0f);

			if(isInputCommand != false || !Input.GetKey(skipKey))
			yield return new WaitForSeconds(battleSpeed);
		}

		yield return StartCoroutine(BattleEnd());

		Escape:
		playerStatus.HaveCardRefresh();
		GameDirector.ReturnMap();
		yield break;
	}

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

	IEnumerator Attack()
	{
		isAttack = true;
		attackSpeed = Random.Range(0.006f, 0.0075f);

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

		StartCoroutine(AttackBar());

		while (true)
		{
			if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Q)) break;
			if (attackSlider.value >= 100f) break;
			yield return null;
		}

		isAttack = false;

		attackSubmit = Mathf.Abs(attackSlider.value - attackPos.value);

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

	IEnumerator AttackBar()
	{
		while (isAttack)
		{
			attackSlider.value += 1f;
			yield return new WaitForSeconds(0.005f);
		}
	}

	IEnumerator CardSelect()
	{
		int cardIndex = -1;

		cardInfoPanel.gameObject.SetActive(true);
		backButton.SetActive(true);

		while (cardIndex == -1)
		{
			yield return null;

			if (isBack) break;

			submitButton.SetActive(false);
			CardScript cardScript = CardScript.isViewCard;

			if (cardScript == null) continue;

			if (cardScript.CanUseCheck(CardData.UseType.Battle) == false) continue;

			//CardData card = cardScript.cardData;
			//if (card.type == CardData.Type.Equipment) continue;
			//if (card.useType == CardData.UseType.Map) continue;
			//if (card.costType == CardData.CostType.Mana && playerStatus.status.mana < card.manaCost) continue;
			//if (card.costType == CardData.CostType.Cooldown && card.nowCooldown > 0) continue;

			submitButton.SetActive(true);
			if (isPush == false) continue;

			cardIndex = CardScript.isViewCard.haveCardIndex;
			if (cardIndex == -1) continue;

			break;
		}

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
		playerStatus.ResetStatus();
		enemyStatus.ResetStatus();

		playerHpBar.slider.value = Extend.TwoRatio(playerStatus.status.hp, playerStatus.status.maxHp);
		playerManaBar.slider.value = Extend.TwoRatio(playerStatus.status.mana, playerStatus.status.maxMana);
		playerSpeedBar.slider.value = Extend.TwoRatio(playerSpeed, playerNeedSpeed);
		enemyHpBar.slider.value = Extend.TwoRatio(enemyStatus.status.hp, enemyStatus.status.maxHp);
		enemyManaBar.slider.value = Extend.TwoRatio(enemyStatus.status.mana, enemyStatus.status.maxMana);;
		enemySpeedBar.slider.value = Extend.TwoRatio(enemySpeed, enemyNeedSpeed);

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
		isCritical = false;
		if (attacker.cri > Random.Range(0, 1000))
		{
			isCritical = true;
		}
		else if (defender.avo > Random.Range(0, 1000))
		{
			attacker.trigger.Attack();
			defender.trigger.Avoidance();

			SoundDirector.PlaySE("Battle/Spin");

			return -1;
		}

		float protect = defender.def / (attacker.atk * 1.5f);
		protect = Mathf.Clamp(protect, 0.0f, defender.defMax);

		int damage = Mathf.CeilToInt(attacker.atk * (1.0f - protect) * Random.Range(0.85f, 1.15f));
		damage = Mathf.Max(damage, 0);
		damage = (int)(damage * (isCritical ? 2.5f : 1.0f) * overDamage);

		defender.hp -= damage;
		if (attacker.hp <= 0 || defender.hp <= 0) isBattle = false;	

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

	public void MoneyText(Vector3 vector, float value)
	{
		Color color = new Color(1.0f, 1.0f, 0.5f, 1.0f);

		GameObject obj = Instantiate(damageText);
		//obj.transform.SetParent(transform.root, false);
		obj.transform.localPosition = vector;

		obj.GetComponent<WorldDamageText>().tmp.text = value.ToString("0");
		obj.GetComponent<WorldDamageText>().tmp.color = color;
	}

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
		else
		{
			SoundDirector.PlayBGM("Win");

			playerStatus.money += enemyStatus.money;

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

			WinPanel winPanelScript = winPanel.GetComponent<WinPanel>();
			winPanelScript.Load(enemyStatus.exp, enemyStatus.money, playerStatus.needExp, upLevel);
			winPanel.SetActive(true);
			winPanel.transform.localScale = new Vector3(0, 1, 1);
			winPanel.transform.DOScaleX(1, 0.25f);
			yield return new WaitForSeconds(0.5f);

			while (true)
			{
				if (Input.anyKeyDown) break;
				yield return null;
			}
		}

		playerStatus.HaveCardRefresh();
		yield break;
	}

	[HideInInspector] public bool isPush = false; public void PushButton() => StartCoroutine(_PushButton()); IEnumerator _PushButton()
	{
		isPush = true;
		yield return null;

		isPush = false;
		yield break;
	}

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