using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStatus : MonoBehaviour
{
	EnemyData enemyData;

	//敵の基礎ステータス
	[SerializeField] public Status defaultStatus = new Status();

	//バフステータス
	[HideInInspector] public Status bufStatus = new Status();

	//敵のデバフステータス
	[HideInInspector] public Status debufStatus = new Status();

	//敵の合計ステータス
	[HideInInspector] public Status status = new Status();

	public string cardTrigger;
	public float arg;
	public string magicName;

	public int money;
	public int exp;

	/// <summary>
    /// すべての状態を計算したのちのステータス
    /// </summary>
	public void ClacStatus()
	{
		defaultStatus.hp = status.hp;
		defaultStatus.mana = status.mana;

		status.Clear();
		status.Add(defaultStatus);
	}

	/// <summary>
    /// ステータスの再計算
    /// </summary>
	public void ResetStatus()
	{
		bufStatus.Clear();
		debufStatus.Clear();
		ClacStatus();
	}

	/// <summary>
    /// Stageから敵をランダムに取得
    /// </summary>
    /// <param name="tier">ステージの難易度</param>
	public void LoadDataTier(int tier)
	{
		enemyData = Resources.Load("EnemyDataBase") as EnemyData; //すべての敵データを取得

		//同じステージのデータを取得
		List<EnemyData.Param> tierList = new List<EnemyData.Param>();
		foreach(EnemyData.Param s in enemyData.sheets[0].list)
		{
			if (s.Tier != tier) continue;
			tierList.Add(s);
		}

		//ランダムに選択
		int id = tierList[Random.Range(0, tierList.Count)].ID;

		//データを読み込む
		LoadDataID(id);
	}

	/// <summary>
    /// IDからデータを読み込む
    /// </summary>
    /// <param name="id">敵ID</param>
	public void LoadDataID(int id)
	{
		Debug.Log("ID: " + id);
		enemyData = Resources.Load("EnemyDataBase") as EnemyData;

		float runawayMultiple = Map.ins.runawayPower;

		defaultStatus.maxHp = (int)(enemyData.sheets[0].list[id].HP * runawayMultiple);
		defaultStatus.hp = defaultStatus.maxHp;
		defaultStatus.maxMana = (int)(enemyData.sheets[0].list[id].Mana * runawayMultiple);
		defaultStatus.mana = defaultStatus.maxMana;

		defaultStatus.atk = (int)(enemyData.sheets[0].list[id].ATK * runawayMultiple);
		defaultStatus.def = (int)(enemyData.sheets[0].list[id].DEF * runawayMultiple);
		defaultStatus.spd = enemyData.sheets[0].list[id].SPD;
		defaultStatus.avo = enemyData.sheets[0].list[id].AVO;
		defaultStatus.cri = enemyData.sheets[0].list[id].CRI;

		cardTrigger = enemyData.sheets[0].list[id].CardTrigger;
		arg = enemyData.sheets[0].list[id].Arg;

		exp = enemyData.sheets[0].list[id].Exp;
		money = enemyData.sheets[0].list[id].Money;
		magicName = enemyData.sheets[0].list[id].MagicName;

		GetComponent<AnimatorManager>().SpritePathChange = "Enemy/" + enemyData.sheets[0].list[id].SpriteName;

		status = Status.Copy(defaultStatus);
	}
}