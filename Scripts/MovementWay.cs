using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class MovementWay : MonoBehaviour
{
	RectTransform rect;

	[SerializeField] PlayerController playerController;
	[SerializeField] GameObject MovementWayGameobject;
	[SerializeField] RectTransform DirectionImage;
	[SerializeField] TextMeshProUGUI nowMovementWayText;
	[SerializeField] TextMeshProUGUI onOffText;

	[SerializeField] Button directionMove;
	[SerializeField] Button locationMove;
	[SerializeField] Button quickMove;

	public static bool isDirectionMove;
	public static bool isLocationMove;
	public static bool isQuickMove;

	static bool isOpen = false;

	private void Start()
	{
		rect = MovementWayGameobject.GetComponent<RectTransform>();

		isDirectionMove = true;
		isLocationMove = false;

		isOpen = false;
		Swip(0.0f);

		Redraw();
	}

	public void PushDirectionMoveButton()
	{
		if (playerController.isWalking == true) return;

		isDirectionMove = true;
		isLocationMove = false;
		Redraw();
	}
	public void PushLocationMoveButton()
	{
		if (playerController.isWalking == true) return;

		isDirectionMove = false;
		isLocationMove = true;
		Redraw();
	}

	public void PushQuickMoveButton()
	{
		isQuickMove = !isQuickMove;
		Redraw();
	}

	void DrawButton(Button button, bool toggle)
	{
		Color color = toggle ? new Color(1.0f,1.0f,1.0f,0.78f) : new Color(0.5f, 0.5f, 0.5f, 0.58f);
		button.GetComponent<Image>().color = color;
	}

	public void PushButton()
	{
		Redraw();
		isOpen = !isOpen;
		Swip(0.1f);
	}

	void Swip(float time)
	{
		if (isOpen) StartCoroutine(Open(time));
		else StartCoroutine(Close(time));

		float scaleY = isOpen ? -1 : 1;
		DirectionImage.DOScaleY(scaleY, 0.1f);
	}

	public void Redraw()
	{
		string nowMovementWay = "";
		if (isDirectionMove) nowMovementWay = "移動方向";
		if (isLocationMove) nowMovementWay = "移動先指定";

		nowMovementWayText.text = "現在 : " + nowMovementWay;
		DrawButton(directionMove, isDirectionMove);
		DrawButton(locationMove, isLocationMove);

		onOffText.text = isQuickMove ? "ON" : "OFF";
	}

	IEnumerator Open(float time)
	{
		Redraw();

		rect.DOScaleY(1.0f, time);
		yield break;
	}
	IEnumerator Close(float time)
	{
		rect.DOScaleY(0.0f, time);
		yield break;
	}
}
