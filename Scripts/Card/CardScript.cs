using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardScript : MonoBehaviour
{
	[SerializeField,Rename("カードID")] int cardID;
	[SerializeField,Rename("カードの情報")] CardSerializeField cardSerializeField;

	[HideInInspector] public int haveCardIndex;
	[HideInInspector] public bool canUse;

	public CardData cardData
	{
		get { return _cardData; }
		set
		{
			_cardData = value;
			RedrawCard();
		}
	}
	CardData _cardData;

public enum CardType
	{
		HaveCard,
		ShopCard,
		ViewCard
	}

	[Rename("表示方法")] public CardType type;

	public bool isFront { get { return _isFront; } private set { _isFront = value; } }
	private bool _isFront = true;

	public static bool canView;
	public static CardScript isViewCard = null;

	private void Start()
	{
		if (cardData == null) cardData = CardDataDirector.ins.cardBaseData.CardList[cardID];
		CardColor();

		if (isFront) FlipFrontCard();
		else FlipBackCard();

		RedrawCard();
		//StartCoroutine("FlipFrontCardAnim");
	}

	public void FlipFrontCard()
	{
		isFront = true;
		cardSerializeField.front.SetActive(true);
		cardSerializeField.back.SetActive(false);
		RedrawCard();
	}
	public void FlipBackCard()
	{
		isFront = false;
		cardSerializeField.front.SetActive(false);
		cardSerializeField.back.SetActive(true);
		RedrawCard();
	}

	public void ClickButton()
	{
		switch (type)
		{
			case CardType.HaveCard:
				//ViewCard(transform.parent.parent);
				ViewCard(GameDirector.HaveCardArea.parent);
				break;
			case CardType.ShopCard:
				//ViewCard(transform.parent.parent.parent);
				ViewCard(GameDirector.HaveCardArea.parent);
				break;
			case CardType.ViewCard:
				isViewCard = null;
				Destroy(gameObject);
				break;
		}
	}

	void ViewCard(Transform instanseTransform)
	{
		if (canView == false) return;
		if (isViewCard != null)
		{
			Destroy(isViewCard.gameObject);
		}

		GameObject ins = Instantiate(this.gameObject);
		ins.transform.SetParent(instanseTransform, false);

		RectTransform insRect = ins.GetComponent<RectTransform>();
		CardScript insCardScript = ins.GetComponent<CardScript>();

		Vector3 _pos = Camera.main.transform.position;
		_pos.y += 0.5f;
		_pos.z = 0;

		insCardScript.cardData = cardData;
		insRect.localScale = Vector3.one * 5.0f;
		insRect.anchorMin = new Vector2(0.5f, 0.5f);
		insRect.anchorMax = new Vector2(0.5f, 0.5f);
		insRect.position = _pos;
		insRect.pivot = new Vector2(0.5f, 0.5f);
		insCardScript.type = CardType.ViewCard;
		insCardScript.RedrawCard();

		isViewCard = insCardScript;
	}

	public static void ViewCardInfoDestory()
	{
		if (isViewCard != null)
		{
			Destroy(isViewCard.gameObject);
		}
	}

	public void FlipCardAnimation(bool front = true, float flipSpeed = 0.5f)
	{
		StartCoroutine(FlipCardAnim(front, flipSpeed));
	}

	IEnumerator FlipCardAnim(bool front = true, float flipSpeed = 0.5f)
	{
		if (front) FlipBackCard(); else FlipFrontCard();

		transform.DORotate(new Vector3(0, 90, 0), flipSpeed / 2.0f).SetEase(Ease.Linear);
		yield return new WaitForSeconds(flipSpeed / 2.0f);

		if (front) FlipFrontCard(); else FlipBackCard();
		transform.rotation = Quaternion.Euler(0, 270, 0);

		transform.DORotate(new Vector3(0, 360, 0), flipSpeed / 2.0f).SetEase(Ease.Linear);
		yield return new WaitForSeconds(flipSpeed / 2.0f);

		transform.rotation = Quaternion.Euler(0, 0, 0);

		yield break;
	}

	public void DrawCard()
	{
		string usePointString = "";
		switch (cardData.useType)
		{
			case CardData.UseType.Battle:
				usePointString = "※戦闘中でのみ使用可能";
				break;
			case CardData.UseType.Map:
				usePointString = "※マップでのみ使用可能";
				break;
		}
		if (cardData.type == CardData.Type.Equipment)
			usePointString = "※持つだけで効果があります";

		cardSerializeField.name.text = cardData.name;
		cardSerializeField.lore.text = cardData.lore;
		cardSerializeField.usePointText.text = usePointString;
		cardSerializeField.cardImage.sprite = cardData.image;
	}

	public bool CanUseCheck(CardData.UseType nowArea)
	{
		bool breakB = true;
		bool areaCheck = false;
		switch (nowArea)
		{
			case CardData.UseType.None:
				areaCheck = true;
				break;
			case CardData.UseType.Map:
				areaCheck = cardData.useType != CardData.UseType.Battle;
				break;
			case CardData.UseType.Battle:
				areaCheck = cardData.useType != CardData.UseType.Map;
				break;
		}

		if (cardData.type == CardData.Type.Equipment) breakB = false;
		if (areaCheck != true) breakB = false;
		if (cardData.costType == CardData.CostType.Mana && GameDirector.GetPlayerStatus.status.mana < cardData.manaCost) breakB = false;
		if (cardData.costType == CardData.CostType.Cooldown && cardData.nowCooldown > 0) breakB = false;

		canUse = breakB;
		return breakB;
	}

	public void RedrawCard()
	{
		DrawCard();
		CardColor();
		if (type == CardType.HaveCard)
		{
			if (GameDirector.GetPlayerStatus == null)
			{
				CanUseCheck(GameDirector.GetPlayerStatus.nowScene);
			}
		}
		else
		{
			canUse = true;
		}
		switch (cardData.costType)
		{
			case CardData.CostType.None:
				cardSerializeField.costFrame.gameObject.SetActive(false);
				cardSerializeField.cooldownObject.SetActive(false);
				break;
			case CardData.CostType.Cooldown:
				cardSerializeField.costFrame.gameObject.SetActive(isFront);
				cardSerializeField.costFrame.sprite = cardSerializeField.cooldownFrame;
				cardSerializeField.costText.text = cardData.cooldown.ToString();
				cardSerializeField.cooldownObject.SetActive(isFront && type != CardType.ViewCard);
				cardSerializeField.cooldownGauge.fillAmount = Extend.TwoRatio(cardData.nowCooldown, cardData.cooldown);
				cardSerializeField.cooldownTime.text = cardData.nowCooldown <= 0 ? "" : cardData.nowCooldown.ToString();
				break;
			case CardData.CostType.Mana:
				cardSerializeField.costFrame.gameObject.SetActive(isFront);
				cardSerializeField.costFrame.sprite = cardSerializeField.manaFrame;
				cardSerializeField.costText.text = cardData.manaCost.ToString();
				cardSerializeField.cooldownObject.SetActive(false);
				break;
		}

		cardSerializeField.cantUseCard.SetActive(!canUse && isFront);
	}

	void CardColor()
	{
		Color color = new Color();
		if (cardData.type == CardData.Type.Equipment) color = new Color32(115, 78, 48, 255);
		if (cardData.type == CardData.Type.Magic)
		{
			if(cardData.costType == CardData.CostType.Mana) color = new Color(1f, 1f, 1f, 1f);
			if (cardData.costType == CardData.CostType.Cooldown) color = new Color(0.5f, 1.0f, 0.5f, 1.0f);
		}

		cardSerializeField.cardBackground.color = color;
	}
}

[System.Serializable]
class CardSerializeField
{
	public TextMeshProUGUI name;
	public TextMeshProUGUI lore;
	public Image cardImage;
	public Image cardBackground;
	[Space(10)]
	public GameObject front;
	public GameObject back;
	public GameObject cantUseCard;
	public TextMeshProUGUI usePointText;
	[Space(10)]
	public Image costFrame;
	public TextMeshProUGUI costText;
	public Sprite manaFrame;
	public Sprite cooldownFrame;
	[Space(10)]
	public GameObject cooldownObject;
	public Image cooldownGauge;
	public TextMeshProUGUI cooldownTime;
}
