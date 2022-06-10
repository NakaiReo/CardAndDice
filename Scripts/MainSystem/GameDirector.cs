using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameDirector : MonoBehaviour
{
	public static GameDirector ins;

	public static PlayerStatus GetPlayerStatus
	{
		get
		{
			return ins.playerStatus;
		}
	}
	[SerializeField] PlayerStatus playerStatus;
	//public static CardDataBaseData CardBaseData
	//{
	//	get
	//	{
	//		return ins.cardBaseData;
	//	}
	//}
	//[SerializeField] CardDataBaseData cardBaseData;

	public static GameObject CardPrefab
	{
		get
		{
			return ins.cardPrefab;
		}
	}
	[SerializeField] GameObject cardPrefab;

	public static Transform HaveCardArea
	{
		get
		{
			return ins.haveCardArea;
		}
	}
	[SerializeField] Transform haveCardArea;

	public static GameObject GameoverCanvas
	{
		get
		{
			return ins.gameoverCanvas;
		}
	}
	[SerializeField] GameObject gameoverCanvas;

	[Space(15)]
	[SerializeField] GameObject gameClearCanvas;     //ゲームをクリアした時のパネル
	[SerializeField] TextMeshProUGUI gameClearText;  //ゲームをクリアいたときのテキスト
	[SerializeField] TextMeshProUGUI pushPleaseText; //ボタンを押してくださいのテキスト
	[SerializeField] GameObject[] allCanvas;

	public static bool isEvent;

	private void Awake()
	{
		ins = this;
		if (PlayerPrefs.HasKey("Character") == false) PlayerPrefs.SetInt("Character", 0);
		isEvent = false;
	}

	private void Start()
	{
		Fade.ins.FadeOut(1.5f);
		SoundDirector.PlayBGM("Map");
		gameClearCanvas.SetActive(false);
		gameClearCanvas.SetActive(false);
	}

	/// <summary>
    /// マップに戻ってきた時の処理
    /// </summary>
	public static void ReturnMap()
	{
		SoundDirector.PlayBGM("Map");
	}

	//ゲームをクリアした時の処理
	public void GameClear() => StartCoroutine(_GameClear());
	IEnumerator _GameClear()
	{
		//初期化
		isEvent = true;
		gameClearCanvas.SetActive(true);
		gameClearText.color = new Color(1, 1, 1, 0);
		pushPleaseText.color = new Color(1, 1, 1, 0);

		//キャンバスをすべて非表示
		foreach (GameObject g in allCanvas) g.SetActive(false);
		yield return new WaitForSeconds(1.0f);

		//カメラを近づける
		Vector3 playerPos = playerStatus.transform.position;
		playerPos.z = -50;
		Camera.main.transform.DOMove(playerPos, 1.0f);
		DOTween.To(() => Camera.main.orthographicSize, (x) => Camera.main.orthographicSize = x, 1.5f, 1.0f);
		yield return new WaitForSeconds(1.5f);

		//プレイヤーを一回転
		float key = playerStatus.transform.localScale.x;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(playerStatus.transform.DOScaleX(key * -1, 0.5f));
		sequence.Append(playerStatus.transform.DOScaleX(key * +1, 0.5f));
		sequence.Pause();
		sequence.Play();

		//ガッツポーズをしてUIを表示
		playerStatus.GetComponent<Animator>().SetTrigger("Victory");
		gameClearText.DOColor(new Color(1, 1, 1, 1), 1.0f);
		gameClearText.transform.DOLocalMoveY(225, 2.0f);
		yield return new WaitForSeconds(2.5f);

		//ボタンを押してくださいを表示
		pushPleaseText.DOColor(new Color(1, 1, 1, 1), 0.5f);
		yield return new WaitForSeconds(0.5f);

		//何かが押されるまで待機
		while (true)
		{
			if (Input.anyKeyDown)
			{
				Fade.ins.FadeIn("Title", 1.5f);
			}
			yield return null;
		}

		yield break;
	}
}
