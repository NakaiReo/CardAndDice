using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLocation : MonoBehaviour
{
	SpriteRenderer spriteRenderer;

	public static List<MoveLocation> locations = new List<MoveLocation>();
	public static MoveLocation selectLocation = new MoveLocation();

	[HideInInspector] public Map.MovePosition haveLocation;

    void Start()
    {
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = new Color(1, 1, 1, 0.75f);
		locations.Add(this);
    }

	public void DoMoveLocation()
	{
		if (selectLocation != null) return;

		selectLocation = this;
	}

	public void Add(Vector2Int pos,List<int> list)
	{
		haveLocation = new Map.MovePosition(pos, list);
	}
}
