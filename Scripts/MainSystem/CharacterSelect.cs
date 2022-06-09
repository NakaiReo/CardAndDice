using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelect : MonoBehaviour
{
	PlayerData playerData;
	PlayerData.Param data;

	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] AnimatorManager animatorManager;

	[Space(10)]
	[SerializeField] TextMeshProUGUI loreText;

	[Space(10)]
	[SerializeField] GameObject confirmPanel;
	[SerializeField] TextMeshProUGUI confirmText;

	int select = 0;

	void Start()
	{
		SoundDirector.PlayBGM("CharacterSelect");
		if (PlayerPrefs.HasKey("Character") == false) PlayerPrefs.SetInt("Character", 0);

		confirmPanel.SetActive(false);

		Fade.ins.FadeOut(1.5f);
		playerData = Resources.Load("PlayerDataBase") as PlayerData;

		Redraw();
	}

	void Redraw()
	{
		PlayerPrefs.SetInt("Character", select);
		data = playerData.sheets[0].list[select];

		nameText.text = data.Name;
		animatorManager.SpritePathChange = "Player/" + data.Path;

		loreText.text = "<line-height=150%>◇ キャラクター情報 ◇\n<line-height=100%>";
		loreText.text += data.Lore + "\n";
		loreText.text += "\n";
		loreText.text += "<line-height=150%>◇ 上昇ステータス ◇\n<line-height=100%>";
		loreText.text += "体  力 : " + data.HP + "\n";
		loreText.text += "マ  ナ : " + data.Mana + "\n";
		loreText.text += "\n";
		loreText.text += "攻撃力 : " + data.ATK + "\n";
		loreText.text += "防御力 : " + data.DEF + "\n";
		loreText.text += "速  度 : " + data.SPD + "\n";
		loreText.text += "回避率 : " + data.AVO + "\n";
		loreText.text += "会心率 : " + data.CRI + "\n";
	}

	public void SelectButton()
	{
		SoundDirector.PlaySE("UI/Select");
		confirmPanel.SetActive(true);
		data = playerData.sheets[0].list[select];
		confirmText.text = "<size=150%>" + "- " + data.Name + " -" + "\n";
		confirmText.text += "<size=100%>" + "でよろしいですか?";
	}

	public void ConfirmButton()
	{
		SoundDirector.PlaySE("UI/Select");
		PlayerPrefs.SetInt("Character", select);
		Fade.ins.FadeIn("Game", 1.5f);
	}

	public void BackButton()
	{
		SoundDirector.PlaySE("UI/Cancel");
		confirmPanel.SetActive(false);
	}

	public void SelectCount(int n)
	{
		SoundDirector.PlaySE("UI/Move");
		select += n;
		if (select >= playerData.sheets[0].list.Count) select = 0;
		if (select < 0) select = playerData.sheets[0].list.Count - 1;

		Redraw();
	}
}
