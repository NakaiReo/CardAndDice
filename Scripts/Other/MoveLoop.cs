using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveLoop : MonoBehaviour
{
	[SerializeField] Vector3 movePos;
	[SerializeField] float time;

	Sequence s;

    void Start()
    {
		s = DOTween.Sequence();
		s.Append(transform.DOMove(transform.position + movePos, time).SetEase(Ease.InBack));
		s.Append(transform.DOMove(transform.position, time));
		s.SetLoops(-1);
	}
}
