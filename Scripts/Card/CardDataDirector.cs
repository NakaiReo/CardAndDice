using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDataDirector : MonoBehaviour
{
	public static CardDataDirector ins = null;
	public CardDataBaseData cardBaseData;

	private void Awake()
	{
		if(ins == null)
		{
			ins = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public CardData GetRandomCardData()
	{
		int random = Random.Range(0, cardBaseData.CardList.Count);

		return cardBaseData.CardList[random];
	}
}
