using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopPanel : MonoBehaviour
{
	[SerializeField]  public GameObject panel;
	[SerializeField]  public GameObject productPanel;
	[SerializeField]  public TextMeshProUGUI haveMoneyText;
	[SerializeField]  PlayerStatus.StatusTextSerializeField StatusTextSerializeField;
	[SerializeField]  public GameObject finButton;
	[HideInInspector] public bool isPush = false;

	/// <summary>
    /// 終了ボタン
    /// </summary>
	public void FinButton()
	{
		StartCoroutine(_PushButton());
	}

	IEnumerator _PushButton()
	{
		isPush = true;
		yield return null;

		isPush = false;
		yield break;
	}

	/// <summary>
    /// 再描画
    /// </summary>
	public void Redraw()
	{
		haveMoneyText.text = "所持金 : " + GameDirector.GetPlayerStatus.money + "G";
	}

	/// <summary>
    /// 現在のステータス
    /// </summary>
	public void StatusTextReload()
	{
		GameDirector.GetPlayerStatus.HaveCardRefresh();

		StatusTextSerializeField.atk.text = GameDirector.GetPlayerStatus.StatusTextATK;
		StatusTextSerializeField.def.text = GameDirector.GetPlayerStatus.StatusTextDEF;
		StatusTextSerializeField.spd.text = GameDirector.GetPlayerStatus.StatusTextSPD;
		StatusTextSerializeField.avo.text = GameDirector.GetPlayerStatus.StatusTextAVO;
		StatusTextSerializeField.cri.text = GameDirector.GetPlayerStatus.StatusTextCRI;
	}
}
