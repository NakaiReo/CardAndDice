using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class WorldDamageText : MonoBehaviour
{
	[SerializeField] float endTime;
	[SerializeField] bool fadeIn;

	public int damage;

	public TextMeshProUGUI tmp;
	public Transform rect;

	Vector3 startPos;

	private void Start()
	{
		tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		rect = GetComponent<RectTransform>();

		Color color = tmp.color;
		color.a = 0;
		tmp.DOColor(color, endTime).SetEase(Ease.InBack); //色を薄くする
		rect.DOLocalMove(rect.transform.localPosition + new Vector3(0, 1.25f, 0), endTime).SetEase(Ease.InBack); //文字を上に移動させる
		Destroy(gameObject, endTime); //一定時間後に破棄する

	}
}
