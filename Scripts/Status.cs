using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スキルポイント用のクラス
/// </summary>
[System.Serializable]
public class StatusP
{
	public int maxHp;
	public int maxMana;

	public int atk;
	public int def;
	public int spd;
	public int avo;
	public int cri;

	public enum StatusName
	{
		atk,
		def,
		spd,
		avo,
		cri
	}

	public ref int GetStatus(StatusName name)
	{
		switch (name)
		{
			case StatusName.atk:
				return ref atk;
			case StatusName.def:
				return ref def;
			case StatusName.spd:
				return ref spd;
			case StatusName.avo:
				return ref avo;
			case StatusName.cri:
				return ref cri;
		}

		return ref atk;
	}

	public void Clear()
	{	
		maxHp = 0;
		maxMana = 0;

		atk = 0;
		def = 0;
		spd = 0;
		avo = 0;
		cri = 0;
	}

	public static Status Copy(Status status)
	{
		Status returnStatus = new Status();
		returnStatus.hp = status.hp;
		returnStatus.maxHp = status.maxHp;
		returnStatus.mana = status.mana;
		returnStatus.maxMana = status.maxMana;

		returnStatus.atk = status.atk;
		returnStatus.def = status.def;
		returnStatus.spd = status.spd;
		returnStatus.avo = status.avo;
		returnStatus.cri = status.cri;

		return returnStatus;
	}

	public void Add(in Status statusF)
	{
		maxHp += statusF.maxHp;
		maxMana += statusF.maxMana;

		atk += statusF.atk;
		def += statusF.def;
		spd += statusF.spd;
		avo += statusF.avo;
		cri += statusF.cri;
	}

	public void Add(in StatusP statusF)
	{
		maxHp += statusF.maxHp;
		maxMana += statusF.maxMana;

		atk += statusF.atk;
		def += statusF.def;
		spd += statusF.spd;
		avo += statusF.avo;
		cri += statusF.cri;
	}

	public void Remove(in Status statusF)
	{
		maxHp -= statusF.maxHp;
		maxMana -= statusF.maxMana;

		atk -= statusF.atk;
		def -= statusF.def;
		spd -= statusF.spd;
		avo -= statusF.avo;
		cri -= statusF.cri;
	}

	public void Remove(in StatusP statusF)
	{
		maxHp -= statusF.maxHp;
		maxMana -= statusF.maxMana;

		atk -= statusF.atk;
		def -= statusF.def;
		spd -= statusF.spd;
		avo -= statusF.avo;
		cri -= statusF.cri;
	}

	public void Multiple(in StatusP statusF)
	{
		maxHp   += (int)(maxHp   * ((statusF.maxHp)   * 0.01f));
		maxMana += (int)(maxMana * ((statusF.maxMana) * 0.01f));

		atk += (int)(atk * ((statusF.atk) * 0.01f));
		def += (int)(def * ((statusF.def) * 0.01f));
		spd += (int)(spd * ((statusF.spd) * 0.01f));
		avo += statusF.avo;
		cri += statusF.cri;
	}
}

/// <summary>
/// ステータス用のクラス
/// </summary>
[System.Serializable]
public class Status : StatusP
{
	public int hp;
	public int mana;

	public Vector3 pos;

	public CharacterAnimation.Trigger trigger = new CharacterAnimation.Trigger();
	public float defMax;
}