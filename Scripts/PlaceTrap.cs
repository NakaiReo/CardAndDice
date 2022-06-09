using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceTrap : MonoBehaviour
{
	public SpriteRenderer trapSpriteRenderer;

	[SerializeField] float spinSpeed;
	[SerializeField] float moveSpeed;
	[SerializeField] float moveDistance;

	Vector3 startPos;
	Vector3 moveVector;

	private void Start()
	{
		startPos = trapSpriteRenderer.transform.localPosition;
		moveVector = new Vector3(0, moveDistance, 0);
	}

	void Update()
    {
		float sd = Time.time * spinSpeed % 1.0f;
		float md = (Mathf.Sin(Time.time * moveSpeed) + 1) / 2.0f;
		float angle = Mathf.Lerp(-90, 90, sd);

		Debug.Log(md);

		trapSpriteRenderer.transform.localPosition = startPos + Vector3.Slerp(-moveVector, moveVector, md);
		trapSpriteRenderer.transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}
