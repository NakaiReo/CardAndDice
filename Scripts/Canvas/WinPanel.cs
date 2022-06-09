using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinPanel: MonoBehaviour
{
	[SerializeField] TextMeshProUGUI getExpText;
	[SerializeField] TextMeshProUGUI getMoneyText;
	[SerializeField] TextMeshProUGUI nextExpText;
	[Space(15)]
	[SerializeField] TextMeshProUGUI maxHpText;
	[SerializeField] TextMeshProUGUI maxManaText;
	[SerializeField] TextMeshProUGUI atkText;
	[SerializeField] TextMeshProUGUI defText;
	[SerializeField] TextMeshProUGUI spdText;
	[SerializeField] TextMeshProUGUI avoText;
	[SerializeField] TextMeshProUGUI criText;

	PlayerStatus player;

	public int exp;
	public int money;
	public int nextExp;
	public int upLevel;

	public void Load(int exp, int money, int nextExp, int upLevel)
	{
		player = GameDirector.GetPlayerStatus;

		this.money = money;
		this.exp = exp;
		this.nextExp = nextExp;
		this.upLevel = upLevel;

		Redraw();
	}

	void Redraw()
	{
		getExpText.text = "獲得Exp : " + exp;
		getMoneyText.text = "獲得ゴールド : " + money;

		nextExpText.text = "次のレベルまで" + nextExp + "Exp";
		if (upLevel > 0) nextExpText.text += " (レベルが" + upLevel + "上がりました)";

		maxHpText.text = player.StatusTextMAXHP + " " + player.StatusAddMAXHP(upLevel);
		maxManaText.text = player.StatusTextMAXMANA + " " + player.StatusAddMAXMANA(upLevel);
		atkText.text = player.StatusTextATK + " " + player.StatusAddATK(upLevel);
		defText.text = player.StatusTextDEF + " " + player.StatusAddDEF(upLevel);
		spdText.text = player.StatusTextSPD + " " + player.StatusAddSPD(upLevel);
		avoText.text = player.StatusTextAVO + " " + player.StatusAddAVO(upLevel);
		criText.text = player.StatusTextCRI + " " + player.StatusAddCRI(upLevel);
	}
}
