using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 2点間を行き来する
/// </summary>
public class MoveLoop : MonoBehaviour
{
	[SerializeField] Vector3 movePos; //移動量
	[SerializeField] float time;      //片道の移動時間

	Sequence s;

    void Start()
    {
		//2点間を行ったり来たりする
		s = DOTween.Sequence();
		s.Append(transform.DOMove(transform.position + movePos, time).SetEase(Ease.InBack));
		s.Append(transform.DOMove(transform.position, time));
		s.SetLoops(-1);
	}
}
