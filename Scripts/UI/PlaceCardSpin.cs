using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceCardSpin : MonoBehaviour
{
	[SerializeField] float spinSpeed;
	[SerializeField] float moveSpeed;
	[SerializeField] float moveDistance;

	Vector3 startPos;
	Vector3 moveVector;

	private void Start()
	{
		startPos = transform.localPosition;
		moveVector = new Vector3(0, moveDistance, 0);
	}

	void Update()
	{
		float sd = Time.time * spinSpeed % 1.0f;
		float md = (Mathf.Sin(Time.time * moveSpeed) + 1) / 2.0f;
		float angle = Mathf.Lerp(-90, 90, sd);

		transform.localPosition = startPos + Vector3.Slerp(-moveVector, moveVector, md);
		transform.localRotation = Quaternion.Euler(0, angle, 0);

		if(spinSpeed <= 0) transform.localRotation = Quaternion.Euler(0, 0, 0);
	}
}
