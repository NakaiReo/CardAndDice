using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelect : MonoBehaviour
{
	PlayerData playerData; //プレイヤーのデータ
	PlayerData.Param data; //プレイヤーのデータベース

	[SerializeField] TextMeshProUGUI nameText;        //職業の名前
	[SerializeField] AnimatorManager animatorManager; //職業のanimator

	[Space(10)]
	[SerializeField] TextMeshProUGUI loreText; //説明文

	[Space(10)]
	[SerializeField] GameObject confirmPanel;     //確定パネル
	[SerializeField] TextMeshProUGUI confirmText; //確定テキスト

	int select = 0; //現在選択しているインデックス

	void Start()
	{
		SoundDirector.PlayBGM("CharacterSelect");
		if (PlayerPrefs.HasKey("Character") == false) PlayerPrefs.SetInt("Character", 0);

		confirmPanel.SetActive(false);

		Fade.ins.FadeOut(1.5f);
		playerData = Resources.Load("PlayerDataBase") as PlayerData;

		Redraw();
	}

	/// <summary>
    /// 再描画
    /// </summary>
	void Redraw()
	{
		PlayerPrefs.SetInt("Character", select);  //現在の職業を設定
		data = playerData.sheets[0].list[select]; //プレイヤーのデータを取得

		nameText.text = data.Name; //職業名を表示
		animatorManager.SpritePathChange = "Player/" + data.Path; //アニメーションのパス

		//説明文を描画
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

	/// <summary>
    /// 選択を押された時の処理
    /// </summary>
	public void SelectButton()
	{
		SoundDirector.PlaySE("UI/Select");
		confirmPanel.SetActive(true);
		data = playerData.sheets[0].list[select];
		confirmText.text = "<size=150%>" + "- " + data.Name + " -" + "\n";
		confirmText.text += "<size=100%>" + "でよろしいですか?";
	}

	/// <summary>
    /// 確定を押したときの処理
    /// </summary>
	public void ConfirmButton()
	{
		SoundDirector.PlaySE("UI/Select");
		PlayerPrefs.SetInt("Character", select);
		Fade.ins.FadeIn("Game", 1.5f);
	}

	/// <summary>
    /// 戻るを押したときの処理
    /// </summary>
	public void BackButton()
	{
		SoundDirector.PlaySE("UI/Cancel");
		confirmPanel.SetActive(false);
	}

	/// <summary>
    /// 職業の選択の処理
    /// </summary>
    /// <param name="n">いくつずらすか</param>
	public void SelectCount(int n)
	{
		SoundDirector.PlaySE("UI/Move");
		select += n;
		if (select >= playerData.sheets[0].list.Count) select = 0;
		if (select < 0) select = playerData.sheets[0].list.Count - 1;

		Redraw();
	}
}
