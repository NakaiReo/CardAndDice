using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardEffects : MonoBehaviour
{
	public PlayerStatus playerStatus;
	public EnemyStatus enemyStatus;
	public GridEvent gridEvent;

	public PlayerStatus.PlayerName playerName;

	public BattleDirector battle;

	public enum EffectTarget
	{
		Player,
		Enemy
	}

	public float effectMultiple(EffectTarget user, CardData.CostType type)
	{
		if(playerName == PlayerStatus.PlayerName.Witch && type == CardData.CostType.Mana)
		{
			return 1.25f;
		}

		return 1.0f;
	}

	public EffectTarget Revarse(EffectTarget target)
	{
		return target == EffectTarget.Player ? EffectTarget.Enemy : EffectTarget.Player;
	}

	//Item
	public void HealHerb() { HealPercent(EffectTarget.Player, 20.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void HiHeelHerb() { HealPercent(EffectTarget.Player, 50.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void FullHealHerb() { HealPercent(EffectTarget.Player, 100.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void HealPostion() { HealAmount(EffectTarget.Player, 1500.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void ManaHealPostion() { ManaAmount(EffectTarget.Player, 100.0f); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void SpeedPotion() { playerStatus.moveX2 = true; }
	public void HeiwaPotion() { playerStatus.notBattle = true; }
	public void ShopPotion() { gridEvent.Shop(); }
	public void Light() { ATK_PercentDamage(EffectTarget.Enemy, EffectTarget.Player, 175, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Fire"); }
	public void HiLight() { ATK_PercentDamage(EffectTarget.Enemy, EffectTarget.Player, 250, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Fire"); }
	public void Stone() { PercentDamage(EffectTarget.Enemy, 10.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Rock"); }
	public void HiStone() { PercentDamage(EffectTarget.Enemy, 30.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Rock"); }
	public void StarLight() { HPHalfDamage(EffectTarget.Player, EffectTarget.Enemy, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "StarAttack"); }

	//Magic
	public void Heal() { HealPercent(EffectTarget.Player, 30.0f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void HiHeel() { HealPercent(EffectTarget.Player, 50.0f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void FullHeal() { HealPercent(EffectTarget.Player, 100.0f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void Fire() { ATK_PercentDamage(EffectTarget.Enemy, EffectTarget.Player, 150, CardData.CostType.Mana); ParticleEffect(EffectTarget.Enemy, "Fire"); }
	public void HighFire() { ATK_PercentDamage(EffectTarget.Enemy, EffectTarget.Player, 200, CardData.CostType.Mana); ParticleEffect(EffectTarget.Enemy, "Fire"); }
	public void Thunder() { PercentDamage(EffectTarget.Enemy, 15.0f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Enemy, "Thunder"); }
	public void HighThunder() { PercentDamage(EffectTarget.Enemy, 30.0f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Enemy, "Thunder"); }
	public void OverBoost() { OverBoost(2.0f, 0.2f, CardData.CostType.Mana); ParticleEffect(EffectTarget.Player, "Heal"); }
	public void StarFall() { MaxManaDamage(EffectTarget.Enemy, EffectTarget.Player, 500, CardData.CostType.Mana); ParticleEffect(EffectTarget.Enemy, "StarAttack"); }


	//EnemyMagic
	public void E_Heal() { HealPercent(EffectTarget.Enemy, 30.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Heal"); }
	public void E_HiHeel() { HealPercent(EffectTarget.Enemy, 50.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Enemy, "Heal"); }
	public void E_Fire() { ATK_PercentDamage(EffectTarget.Player, EffectTarget.Enemy, 150, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Fire"); }
	public void E_HighFire() { ATK_PercentDamage(EffectTarget.Player, EffectTarget.Enemy, 200, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Fire"); }
	public void E_Stone() { PercentDamage(EffectTarget.Player, 10.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Rock"); }
	public void E_Thunder() { PercentDamage(EffectTarget.Player, 15.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Thunder"); }
	public void E_HiStone() { PercentDamage(EffectTarget.Player, 20.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Rock"); }
	public void E_HighThunder() { PercentDamage(EffectTarget.Player, 30.0f, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Thunder"); }
	public void E_Light() { ATK_PercentDamage(EffectTarget.Player, EffectTarget.Enemy, 175, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Fire"); }
	public void E_HiLight() { ATK_PercentDamage(EffectTarget.Player, EffectTarget.Enemy, 250, CardData.CostType.Cooldown); ParticleEffect(EffectTarget.Player, "Fire"); }


	private void Start()
	{
		foreach (CardData card in CardDataDirector.ins.cardBaseData.CardList)
		{
			card.nowCooldown = 0;
			if (card.type != CardData.Type.Equipment)
			{
				card.cardEffectMethod = () =>
				{
					Debug.Log(card.cardEffect);
					Invoke(card.cardEffect, 0.0f);
				};
			}
			else
			{
				card.cardEffectMethod = () =>
				{
					Debug.Log("これは装備品");
				};
			}
		}
	}

	public void OverBoost(float overDamadeArea, float overDamageMultiple, CardData.CostType type)
	{
		playerStatus.overDamageAreaTemp += overDamadeArea * effectMultiple(EffectTarget.Player, type);
		playerStatus.overDamageMultipleTemp += overDamageMultiple * effectMultiple(EffectTarget.Player, type);
		SoundDirector.PlaySE("Battle/Heal");
	}

	public void HPHalfDamage(EffectTarget user,EffectTarget target, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);
		Status userStatus = GetTargetStatus(user);

		float maxHp = userStatus.maxMana;
		float hp = userStatus.hp;
		float damage = ((maxHp * (hp / maxHp) / 2.0f)) * effectMultiple(user, type);

		targetStatus.hp -= Mathf.RoundToInt(damage);
		SoundDirector.PlaySE("Battle/Damage");
		battle.DamageText(targetStatus.pos, damage, false);
	}

	public void MaxManaDamage(EffectTarget target, EffectTarget user, float percent, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);
		Status userStatus = GetTargetStatus(user);

		float maxMana = userStatus.maxMana;
		float damage = maxMana * (percent * 0.01f) * effectMultiple(user, type);

		targetStatus.hp -= Mathf.RoundToInt(damage);
		SoundDirector.PlaySE("Battle/Damage");
		battle.DamageText(targetStatus.pos, damage, false);
	}

	public void StatusBuf(EffectTarget target, Status.StatusName status, float value, CardData.CostType type)
	{
		Status targetStatus = GetTargetBufStatus(target);

		targetStatus.GetStatus(status) += Mathf.RoundToInt(value * effectMultiple(target, type));
	}

	public void StatusBufPercent(EffectTarget target, Status.StatusName status, float value, CardData.CostType type)
	{
		Status targetStatus = GetTargetBufStatus(target);
		Status clacStatus = GetTargetClacStatus(target);

		targetStatus.GetStatus(status) += Mathf.RoundToInt(clacStatus.GetStatus(status) * (value * 0.01f) * effectMultiple(target, type));
	}

	public void StatusDebuf(EffectTarget target, Status.StatusName status, float value, CardData.CostType type)
	{
		Status targetStatus = GetTargetDebufStatus(target);

		targetStatus.GetStatus(status) += Mathf.RoundToInt(value * effectMultiple(Revarse(target), type));
	}

	public void StatusDebufPercent(EffectTarget target, Status.StatusName status, float value, CardData.CostType type)
	{
		Status targetStatus = GetTargetDebufStatus(target);
		Status clacStatus = GetTargetClacStatus(target);

		targetStatus.GetStatus(status) += Mathf.RoundToInt(clacStatus.GetStatus(status) * (value * 0.01f) * effectMultiple(Revarse(target), type));
	}

	public void ATK_PercentDamage(EffectTarget target, EffectTarget user ,float maxPercent, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);
		Status userStatus = GetTargetStatus(user);

		float atk = userStatus.atk;
		float damage = atk * (maxPercent * 0.01f) * effectMultiple(user, type);

		targetStatus.hp -= Mathf.RoundToInt(damage);
		SoundDirector.PlaySE("Battle/Damage");
		battle.DamageText(targetStatus.pos, damage, false);
	}

	public void MaxPercentDamage(EffectTarget target, float maxPercent, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);

		float maxHp = targetStatus.maxHp;
		float damage = maxHp * (maxPercent * 0.01f) * effectMultiple(Revarse(target), type);

		targetStatus.hp -= Mathf.RoundToInt(damage);
		SoundDirector.PlaySE("Battle/Damage");
		battle.DamageText(targetStatus.pos, damage, false);
	}

	public void PercentDamage(EffectTarget target, float percent, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);

		float hp = targetStatus.hp;
		float damage = hp * (percent * 0.01f) * effectMultiple(Revarse(target), type);

		targetStatus.hp -= Mathf.RoundToInt(damage);
		SoundDirector.PlaySE("Battle/Damage");
		battle.DamageText(targetStatus.pos, damage, false);
	}

	public void HealAmount(EffectTarget target, float amount, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);

		amount *= effectMultiple(target, type);
		float hp = targetStatus.hp;
		float maxHp = targetStatus.maxHp;
		targetStatus.hp = Mathf.RoundToInt(Mathf.Min(hp + amount, targetStatus.maxHp));
		SoundDirector.PlaySE("Battle/Heal");
		battle.HealText(targetStatus.pos, amount);
	}

	public void HealPercent(EffectTarget target, float percent, CardData.CostType type)
	{
		Status targetStatus = GetTargetStatus(target);

		float hp = targetStatus.hp;
		float maxHP = targetStatus.maxHp;
		float value = Mathf.RoundToInt((maxHP * (percent * 0.01f) * effectMultiple(target, type)));
		targetStatus.hp = (int)Mathf.Min(hp + value, targetStatus.maxHp);
		SoundDirector.PlaySE("Battle/Heal");
		battle.HealText(targetStatus.pos, value);
	}

	public void GetMoney(int amount)
	{
		Status targetStatus = GetTargetStatus(EffectTarget.Player);

		playerStatus.money += amount;
		battle.MoneyText(targetStatus.pos, amount);
	}

	public void ManaPercent(EffectTarget target, float percent)
	{
		Status targetStatus = GetTargetStatus(EffectTarget.Player);

		float mana = targetStatus.mana;
		float maxMana = targetStatus.maxMana;
		int value = Mathf.RoundToInt(maxMana * (percent * 0.01f));
		targetStatus.mana = (int)Mathf.Min(mana + value, maxMana);
		battle.ManaText(targetStatus.pos, value);
	}

	public void ManaAmount(EffectTarget target, float amount)
	{
		Status targetStatus = GetTargetStatus(EffectTarget.Player);

		float mana = targetStatus.mana;
		float maxMana = targetStatus.maxMana;
		targetStatus.mana = Mathf.RoundToInt(Mathf.Min(mana + amount, maxMana));
		battle.ManaText(targetStatus.pos, amount);
	}

	public void ParticleEffect(EffectTarget target, string name)
	{
		Status targetStatus = GetTargetStatus(target);
		GameObject insObject = Resources.Load("Particle/" + name) as GameObject;


		if(insObject == null) { Debug.LogError("ファイルが見つかりません!!"); return; }
		Instantiate(insObject, targetStatus.pos, Quaternion.identity);
	}

	ref Status GetTargetStatus(EffectTarget target)
	{
		if (target == EffectTarget.Player) return ref playerStatus.status;
		return ref enemyStatus.status;
	}
	ref Status GetTargetBufStatus(EffectTarget target)
	{
		if (target == EffectTarget.Player) return ref playerStatus.bufStatus;
		return ref enemyStatus.bufStatus;
	}
	ref Status GetTargetDebufStatus(EffectTarget target)
	{
		if (target == EffectTarget.Player) return ref playerStatus.debufStatus;
		return ref enemyStatus.debufStatus;
	}
	ref Status GetTargetClacStatus(EffectTarget target)
	{
		if (target == EffectTarget.Player) return ref playerStatus.clacStatus;
		return ref enemyStatus.defaultStatus;
	}
}