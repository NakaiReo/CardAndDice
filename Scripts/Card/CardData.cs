using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[CreateAssetMenu(fileName = "NewCardData",menuName = "NewCardData")]
public class CardData : ScriptableObject
{
	public enum Type
	{
		Equipment,
		Magic
	}
	public enum CostType
	{
		None,
		Mana,
		Cooldown
	}
	public enum UseType
	{
		None,
		Map,
		Battle
	}

	public int id;
	public int tier;
	public Type type;
	public CostType costType;
	public UseType useType;
	public new string name;
	public string lore;
	public Sprite image;

	public int price;
	public int manaCost;
	public int cooldown;
	public int useCount;

	public int nowCooldown;
	public int useCountLeft;

	public StatusP status;
	public float overDamageArea;
	public float overDamageMultiple;

	public string cardEffect;
	public float arg;

	public UnityEvent cardEvent;

	public delegate void CardEffectMethod();
	public CardEffectMethod cardEffectMethod;

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
