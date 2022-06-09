using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
	PlayerData playerData; //プレイヤーのデータ

	[HideInInspector] public CardData.UseType nowScene; //現在の手札の状態
	
	[SerializeField] StatusCanvas statusCanvas; //ステータスのパネル

	[SerializeField] Transform haveCardArea; //手札の表示する位置
	[SerializeField] int playerID; //PlayerID

	[HideInInspector] public bool moveX2;    //移動数2倍か
	[HideInInspector] public bool notBattle; //バトルにならないかどうか
	[HideInInspector] public float overDamageArea;     //クリティカル範囲
	[HideInInspector] public float overDamageMultiple; //クリティカル倍率
	[HideInInspector] public float overDamageAreaTemp;     //クリティカル範囲の一時的強化
	[HideInInspector] public float overDamageMultipleTemp; //クリティカル倍率の一時的強化

	//プレイヤーの名前のリスト
	public enum PlayerName
	{
		Adventurer,
		Knight,
		Paladin,
		Fighter,
		Witch,
		Dancer,
		Merchant
	}
	public PlayerName playerName; //プレイヤーの名前

	//所持金
	[HideInInspector] public int money;

	//所持Exp
	[HideInInspector] public int level = 1; //現在のレベル
	[HideInInspector] public int needExp;   //残りの経験値
	[HideInInspector] public int nextExp;   //必要経験値
	[HideInInspector] public float nextExpMagnification = 1.2f; //レベル毎に必要な経験値の倍率

	//プレイヤーのステータス
	[HideInInspector] public Status defaultStatus = new Status();

	//所持品によるステータス上昇値
	[HideInInspector] public Status equipmentStatus = new Status();

	//バフステータス
	[HideInInspector] public Status bufStatus = new Status();

	//デバフステータス
	[HideInInspector] public Status debufStatus = new Status();

	//合計ステータス
	[HideInInspector] public Status status = new Status();

	//計算ステータス
	[HideInInspector] public Status clacStatus = new Status();

	//ステータスの初期値
	StatusP statusOffset = new StatusP();

	//ステータスの増加量
	StatusP statusAddValue = new StatusP();

	[HideInInspector] public List<CardScript> haveCards = new List<CardScript>(); //現在の手札

	[Space(15)]
	[SerializeField] Image statusPlayerImage; //プレイヤーのイメージ
	[SerializeField] AnimatorManager gridPlayer;   //グリッド時のプレイヤーのイメージ
	[SerializeField] AnimatorManager battlePlayer; //バトル中のプレイヤーのイメージ

	[Space(15)]
	[SerializeField] StatusBarSerializeField hpBar;    //hpの情報
	[SerializeField] StatusBarSerializeField manaBar;  //manaの情報
	[SerializeField] StatusBarSerializeField levelBar; //levelの情報
	[SerializeField] TextMeshProUGUI moneyText;        //所持金のテキスト
	[SerializeField] TextMeshProUGUI nowLevelText;     //現在のレベルのテキスト

	private void Awake()
	{
		//プレイヤーの情報を取得
		playerData = Resources.Load("PlayerDataBase") as PlayerData;

		//プレイヤーのIDと名前を取得
		playerID = PlayerPrefs.GetInt("Character");
		playerName = (PlayerName)playerID;

		//基準ステータス
		statusOffset.maxHp   = 1250;
		statusOffset.maxMana = 50;
		statusOffset.atk     = 150;
		statusOffset.def     = 100;
		statusOffset.spd     = 300;
		statusOffset.avo     = 50;
		statusOffset.cri     = 50;

		Debug.Log(playerData.sheets[0].list.Count);

		//レベルアップ時のステータス
		statusAddValue.maxHp   = playerData.sheets[0].list[playerID].HP;
		statusAddValue.maxMana = playerData.sheets[0].list[playerID].Mana;
		statusAddValue.atk     = playerData.sheets[0].list[playerID].ATK;
		statusAddValue.def     = playerData.sheets[0].list[playerID].DEF;
		statusAddValue.spd     = playerData.sheets[0].list[playerID].AVO;
		statusAddValue.avo     = playerData.sheets[0].list[playerID].AVO;
		statusAddValue.cri     = playerData.sheets[0].list[playerID].CRI;

		//レベル当たりのステータス
		defaultStatus.maxHp   = statusOffset.maxHp   + statusAddValue.maxHp   * (level - 1);
		defaultStatus.maxMana = statusOffset.maxMana + statusAddValue.maxMana * (level - 1);
		defaultStatus.atk     = statusOffset.atk     + statusAddValue.atk     * (level - 1);
		defaultStatus.def     = statusOffset.def     + statusAddValue.def     * (level - 1);
		defaultStatus.spd     = statusOffset.spd     + statusAddValue.spd     * (level - 1);
		defaultStatus.avo     = statusOffset.avo     + statusAddValue.avo     * (level - 1);
		defaultStatus.cri     = statusOffset.cri     + statusAddValue.cri     * (level - 1);
		
		//HPとManaを最大値に
		defaultStatus.hp = defaultStatus.maxHp;
		defaultStatus.mana = defaultStatus.maxMana;

		//次の必要経験値
		nextExp = 100;
		needExp = nextExp;

		//初期資金
		money = 10;

		//ステータスをコピーしておく
		status = Status.Copy(defaultStatus);

		moveX2 = false;
		notBattle = false;
	}

	private void Start()
	{
		//プレイヤーのイメージを取得
		string spritePath = playerData.sheets[0].list[playerID].Path;
		Sprite[] sprites = Resources.LoadAll<Sprite>("Player/" + spritePath);
		statusPlayerImage.sprite = sprites[0];
		gridPlayer.SpritePathChange   = "Player/" + spritePath;
		battlePlayer.SpritePathChange = "Player/" + spritePath;

		moveX2 = false;
		notBattle = false;

		StartCoroutine(WaitStart());

		status.pos = transform.localPosition;
	}

	IEnumerator WaitStart()
	{
		yield return null;

		HaveCardRefresh();

		yield break;
	}

	//手札の更新
	public void HaveCardRefresh()
	{
		haveCards.Clear();
		for(int i = 0; i < haveCardArea.childCount; i++)
		{
			CardScript card = haveCardArea.GetChild(i).GetComponent<CardScript>();
			card.CanUseCheck(nowScene);

			haveCards.Add(card);
			card.haveCardIndex = haveCards.Count - 1;
			card.RedrawCard();
		}
		ClacStatus();
	}


	//引数と一致する手札を削除
	public bool RemoveEqualCard(CardData card)
	{
		for (int i = 0; i < haveCardArea.childCount; i++)
		{
			if (haveCardArea.GetChild(i).GetComponent<CardScript>().cardData.name != card.name) continue;
			haveCards.RemoveAt(i);
			Destroy(haveCardArea.GetChild(i).gameObject);
			HaveCardRefresh();
			return true;
		}

		return false;
	}

	//指定した位置の手札を破棄
	public void DestoryHaveCard(int n)
	{
		haveCards.RemoveAt(n);
		Destroy(haveCardArea.GetChild(n).gameObject);
		HaveCardRefresh();
	}

	//装備品カードの合計ステータスの処理
	public void ClacStatus()
	{
		overDamageArea = 2.5f;      //基本のクリティカル範囲
		overDamageMultiple = 1.25f; //基本男クリティカル倍率

		//装備品のステータスを初期化
		equipmentStatus.Clear();
		foreach(CardScript card in haveCards)
		{
			if (card.cardData.type != CardData.Type.Equipment) continue;

			equipmentStatus.Add(card.cardData.status);
			overDamageArea += card.cardData.overDamageArea;
			overDamageMultiple += card.cardData.overDamageMultiple;
		}

		//一時的なクリティカル範囲、倍率の加算
		overDamageArea += overDamageAreaTemp;
		overDamageMultiple += overDamageMultipleTemp;

		//基礎ステータスに現在値を反映
		defaultStatus.hp = status.hp;
		defaultStatus.mana = status.mana;

		//ステータスの初期化
		status.Clear();                   //リセット
		status.Add(defaultStatus);        //基礎ステータスを加算
		status.Multiple(equipmentStatus); //装備品ステータスを加算

		//ステータスが最大値を超えないように
		status.hp = Mathf.Min(defaultStatus.hp, status.maxHp); 
		status.mana = Mathf.Min(defaultStatus.mana, status.maxMana);

		//計算後のステータスの初期化
		clacStatus.Clear();
		clacStatus.Add(defaultStatus);
		clacStatus.Add(equipmentStatus);

		//ステータスの情報更新
		Redraw();
	}

	/// <summary>
	/// レベルアップの処理
	/// </summary>
	public void LevelUp()
	{
		defaultStatus.maxHp = statusOffset.maxHp + statusAddValue.maxHp * (level - 1);
		defaultStatus.maxMana = statusOffset.maxMana + statusAddValue.maxMana * (level - 1);
		defaultStatus.atk = statusOffset.atk + statusAddValue.atk * (level - 1);
		defaultStatus.def = statusOffset.def + statusAddValue.def * (level - 1);
		defaultStatus.spd = statusOffset.spd + statusAddValue.spd * (level - 1);
		defaultStatus.avo = statusOffset.avo + statusAddValue.avo * (level - 1);
		defaultStatus.cri = statusOffset.cri + statusAddValue.cri * (level - 1);

		status.hp += statusAddValue.maxHp;
		status.mana += statusAddValue.maxMana;
	}

	/// <summary>
	/// ステータスのリセット
	/// </summary>
	public void ResetStatus()
	{
		bufStatus.Clear();
		debufStatus.Clear();
		ClacStatus();
	}

	/// <summary>
	/// カードの使用間隔を減少させる
	/// </summary>
	public void CardCoolDown()
	{
		foreach (CardScript card in haveCards)
		{
			if (card.cardData.costType != CardData.CostType.Cooldown) continue;
			card.cardData.nowCooldown = Mathf.Max(card.cardData.nowCooldown - 1, 0);
		}
	}

	//public string[] StatusText()
	//{
	//	string[] text = new string[5];

	//	text[0] = this.status.atk + " (+" + this.equipmentStatus.atk + ")";
	//	text[1] = this.status.def + " (+" + this.equipmentStatus.def + ")";
	//	text[2] = this.status.spd + " (+" + this.equipmentStatus.spd + ")";
	//	text[3] = this.status.avo + " (+" + this.equipmentStatus.avo + ")";
	//	text[4] = this.status.cri + " (+" + this.equipmentStatus.cri + ")";

	//	return text;
	//}
	
	//デバッグ用ステータスのテキスト化
	void Log(Status statusF,string label = "")
	{
		string s = label;
		s += "HP : " + statusF.hp + "\n";
		s += "MHP: " + statusF.maxHp + "\n";
		s += "MP : " + statusF.mana + "\n";
		s += "MMP: " + statusF.maxMana + "\n";
		s += "ATK: " + statusF.atk + "\n";
		s += "DEF: " + statusF.def + "\n";
		s += "SPD: " + statusF.spd + "\n";
		s += "AVO: " + statusF.avo + "\n";
		s += "CRI: " + statusF.cri + "\n";
		Debug.Log(s);
	}

	//ステータスの情報を更新
	public void Redraw()
	{
		//ゲージの更新
		hpBar.slider.value = Extend.TwoRatio(status.hp, status.maxHp);
		manaBar.slider.value = Extend.TwoRatio(status.mana, status.maxMana);
		levelBar.slider.value = Extend.TwoRatio(nextExp - needExp, nextExp);

		//スタータステキストの更新
		hpBar.statusText.text = status.hp + "/" + status.maxHp;
		manaBar.statusText.text = status.mana + "/" + status.maxMana;
		levelBar.statusText.text = (nextExp - needExp) + "/" + nextExp;

		moneyText.text = money + "G";
		nowLevelText.text = "Level " + level;

		//ステータスの更新
		statusCanvas.Redraw();
	}

	//基本のステータスをテキスト化
	public string StatusTextHP      { get { return this.defaultStatus.hp      + " (+" + this.equipmentStatus.hp      + ")"; } }
	public string StatusTextMANA    { get { return this.defaultStatus.mana    + " (+" + this.equipmentStatus.mana    + ")"; } }
	public string StatusTextMAXHP   { get { return this.defaultStatus.maxHp   + " (+" + this.equipmentStatus.maxHp   + ")"; } }
	public string StatusTextMAXMANA { get { return this.defaultStatus.maxMana + " (+" + this.equipmentStatus.maxMana + ")"; } }
	public string StatusTextATK { get { return this.defaultStatus.atk         + " (" + ((this.equipmentStatus.atk < 0) ? "" : "+") + this.equipmentStatus.atk + "%)"; } }
	public string StatusTextDEF { get { return this.defaultStatus.def         + " (" + ((this.equipmentStatus.def < 0) ? "" : "+") + this.equipmentStatus.def + "%)"; } }
	public string StatusTextSPD { get { return this.defaultStatus.spd         + " (" + ((this.equipmentStatus.spd < 0) ? "" : "+") + this.equipmentStatus.spd + "%)"; } }
	public string StatusTextAVO { get { return this.defaultStatus.avo / 10.0f + "% (" + ((this.equipmentStatus.avo < 0) ? "" : "+") + this.equipmentStatus.avo / 10.0f + "%)"; } }
	public string StatusTextCRI { get { return this.defaultStatus.cri / 10.0f + "% (" + ((this.equipmentStatus.cri < 0) ? "" : "+") + this.equipmentStatus.cri / 10.0f+ "%)"; } }

	//追加のステータスをテキスト化
	public string StatusAddMAXHP(int level)   { return "[ +" + (statusAddValue.maxHp   * level) + " ]"; }
	public string StatusAddMAXMANA(int level) { return "[ +" + (statusAddValue.maxMana * level) + " ]"; }
	public string StatusAddATK(int level) { return "[ +" + (statusAddValue.atk * level) + " ]"; }
	public string StatusAddDEF(int level) { return "[ +" + (statusAddValue.def * level) + " ]"; }
	public string StatusAddSPD(int level) { return "[ +" + (statusAddValue.spd * level) + " ]"; }
	public string StatusAddAVO(int level) { return "[ +" + (statusAddValue.avo * level) + " ]"; }
	public string StatusAddCRI(int level) { return "[ +" + (statusAddValue.cri * level) + " ]"; }

	/// <summary>
	/// ステータス情報のデータ
	/// </summary>
	[System.Serializable]
	public class StatusTextSerializeField
	{
		public TextMeshProUGUI atk;
		public TextMeshProUGUI def;
		public TextMeshProUGUI spd;
		public TextMeshProUGUI avo;
		public TextMeshProUGUI cri;
	}

	/// <summary>
	/// ステータスバーの情報のデータ
	/// </summary>
	[System.Serializable]
	class StatusBarSerializeField
	{
		public Slider slider;
		public TextMeshProUGUI statusText;
	}
}