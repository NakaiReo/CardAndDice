using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopProduct : MonoBehaviour
{
	public TextMeshProUGUI priceText;
	public CardScript cardScript;
	public Button buyButton;
	public TextMeshProUGUI buyButtonText;
	public bool isBuy = false;
	public bool canBuy = false;
	public bool isSeel = false;

	public void ResetUI()
	{
		priceText.text = "No Input";
		buyButton.interactable = true;
		buyButtonText.text = "購入";
		isBuy = false;
	}

	public void ViewUI(bool b, bool ib = false)
	{
		priceText.enabled = b;
		buyButton.GetComponent<Image>().enabled = (ib == false) ? b : true;
		buyButtonText.enabled = b;
	}

	public void ScaleAnim(float time)
	{
		priceText.transform.localScale = new Vector3(0, 1, 1);
		buyButton.transform.localScale = new Vector3(0, 1, 1);

		priceText.transform.DOScaleX(1, time).SetEase(Ease.Linear);
		buyButton.transform.DOScaleX(1, time).SetEase(Ease.Linear);
		cardScript.FlipCardAnimation(true, time);
	}

	public void PushBuyButton()
	{
		priceText.text = "---";
		buyButton.interactable = false;
		buyButtonText.text = "売り切れ";
		isBuy = true;

		GameDirector.GetPlayerStatus.money -= (int)(cardScript.cardData.price * (isSeel ? 0.5f : 1.0f));
	}
}
