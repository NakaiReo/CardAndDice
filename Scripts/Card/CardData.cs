using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[CreateAssetMenu(fileName = "NewCardData",menuName = "NewCardData")]
public class CardData : ScriptableObject
{
	/// <summary>
    /// カードの種類
    /// </summary>
	public enum Type
	{
		Equipment,
		Magic
	}

	/// <summary>
    /// コストの種類
    /// </summary>
	public enum CostType
	{
		None,
		Mana,
		Cooldown
	}

	/// <summary>
    /// どこで使用できるか
    /// </summary>
	public enum UseType
	{
		None,
		Map,
		Battle
	}

	public int id; //CardID
	public int tier; //レア度
	public Type type; //カードの種類
	public CostType costType; //カードのコスト
	public UseType useType; //どこで使用できるか
	public new string name; //カード名
	public string lore; //カードの説明文
	public Sprite image; //カードのアイコン

	public int price; //値段
	public int manaCost; //マナのコスト
	public int cooldown; //クールダウン
	public int useCount; //使用回数

	public int nowCooldown; //つぎの使用可能まで
	public int useCountLeft; //のこり使用回数

	public StatusP status; //対象
	public float overDamageArea; //クリティカル範囲
	public float overDamageMultiple; //クリティカル倍率

	public string cardEffect; //カード効果
	public float arg; //変数

	public UnityEvent cardEvent;

	public delegate void CardEffectMethod();
	public CardEffectMethod cardEffectMethod;

	/// <summary>
    /// 使用時のコスト処理
    /// </summary>
	public void CardCost()
	{
		switch (costType)
		{
			case CardData.CostType.Mana:
				GameDirector.GetPlayerStatus.status.mana -= manaCost;
				break;
			case CardData.CostType.Cooldown:
				nowCooldown = cooldown;
				break;
		}
	}
}
