using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class StatusCanvas : MonoBehaviour
{
	RectTransform rect;

	[SerializeField] GameObject ViewStatusGameobject;
	[SerializeField] RectTransform DirectionImage;
	[SerializeField] PlayerStatus.StatusTextSerializeField StatusTextSerializeField;

	static bool isOpen = false;

	private void Start()
	{
		rect = ViewStatusGameobject.GetComponent<RectTransform>();

		isOpen = false;
		Swip(0.0f);
	}

	public void PushButton()
	{
		Redraw();
		isOpen = !isOpen;
		Swip(0.1f);
	}

	void Swip(float time)
	{
		if (isOpen) StartCoroutine(OpenStatus(time));
		else StartCoroutine(CloseStatus(time));

		float scaleY = isOpen ? -1 : 1;
		DirectionImage.DOScaleY(scaleY, 0.1f);
	}

	public void Redraw()
	{
		StatusTextSerializeField.atk.text = GameDirector.GetPlayerStatus.StatusTextATK;
		StatusTextSerializeField.def.text = GameDirector.GetPlayerStatus.StatusTextDEF;
		StatusTextSerializeField.spd.text = GameDirector.GetPlayerStatus.StatusTextSPD;
		StatusTextSerializeField.avo.text = GameDirector.GetPlayerStatus.StatusTextAVO;
		StatusTextSerializeField.cri.text = GameDirector.GetPlayerStatus.StatusTextCRI;
	}

	IEnumerator OpenStatus(float time)
	{
		Redraw();

		rect.DOScaleY(1.0f, time);
		yield break;
	}
	IEnumerator CloseStatus(float time)
	{
		rect.DOScaleY(0.0f, time);
		yield break;
	}
}
