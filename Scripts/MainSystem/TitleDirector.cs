using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleDirector : MonoBehaviour
{
	[SerializeField] Transform titleImage;
	[SerializeField] Transform card1;
	[SerializeField] Transform card2;

	Sequence titleSeq;
	Sequence card1Seq;
	Sequence card2Seq;

	void Start()
    {
		SoundDirector.PlayBGM("Title");
		Fade.ins.FadeOut(1.5f);

		float titleMoveValue = -50.0f;
		float titleTime = 1.0f;
		float titleSpan = 0.25f;

		card1.GetComponent<CardScript>().cardData = CardDataDirector.ins.GetRandomCardData();
		card2.GetComponent<CardScript>().cardData = CardDataDirector.ins.GetRandomCardData();

		titleSeq = DOTween.Sequence();
		titleSeq.Append(titleImage.DOLocalMoveY(titleImage.localPosition.y + titleMoveValue, titleTime));
		titleSeq.AppendInterval(titleSpan);
		titleSeq.Append(titleImage.DOLocalMoveY(titleImage.localPosition.y, titleTime));
		titleSeq.AppendInterval(titleSpan);
		titleSeq.SetLoops(-1);
		titleSeq.Pause();

		Vector3 cardMoveValue = new Vector3(1.0f, 1.0f, 1.0f);

		CardScript card1Script = card1.GetComponent<CardScript>();
		CardScript card2Script = card1.GetComponent<CardScript>();

		float cardTime = 3.0f;
		float cardSpan = 0.5f;

		card1Seq = DOTween.Sequence();
		card1Seq.Append(card1.DOScale(card1.transform.localScale + cardMoveValue, 3f));
		card1Seq.AppendInterval(cardSpan);
		card1Seq.Append(card1.DOScale(card1.transform.localScale, 3f));
		card1Seq.AppendInterval(cardSpan);
		card1Seq.SetLoops(-1);
		card1Seq.Pause();

		card2Seq = DOTween.Sequence();
		card2Seq.Append(card2.DOScale(card2.transform.localScale + cardMoveValue, cardTime));
		card2Seq.AppendInterval(cardSpan);
		card2Seq.Append(card2.DOScale(card2.transform.localScale, cardTime));
		card2Seq.AppendInterval(cardSpan);
		card2Seq.SetLoops(-1);
		card2Seq.Pause();

		//titleSeq.Play();
		card1Seq.Play();
		card2Seq.Play();
	}
}
