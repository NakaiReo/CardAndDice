using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
	[SerializeField] Vector3 offsetPos;
	[SerializeField] Vector3 movePos;
	[SerializeField] float endTime;
	[SerializeField] bool fadeIn;

	public int damage;

	TextMeshProUGUI tmp;
	RectTransform rect;

	Vector3 startPos;

	private void Start()
	{
		tmp = GetComponent<TextMeshProUGUI>();
		rect = GetComponent<RectTransform>();

		startPos = rect.position + offsetPos;

		Color color = tmp.color;
		color.a = 0;
		tmp.DOColor(color, endTime).SetEase(Ease.InBack); //色を薄くする
		rect.DOLocalMove(startPos + movePos, endTime).SetEase(Ease.InBack); //文字を上に移動させる
		Destroy(gameObject, endTime); //一定時間後に破棄する
	}
}
