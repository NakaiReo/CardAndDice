using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Fade : MonoBehaviour
{
	public static Fade ins;
	static bool isFade = false;

	[SerializeField] CardScript cardScript;

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

		isFade = true;
		cardScript.cardData = CardDataDirector.ins.GetRandomCardData();
		cardScript.gameObject.SetActive(true);
		cardScript.transform.localScale = Vector3.one * 15.0f;
		cardScript.FlipBackCard();

		FadeOut(1.5f);
	}

	public void FadeIn(string sceneName, float time) => StartCoroutine(_FadeIn(sceneName, time));
	public void FadeOut(float time) => StartCoroutine(_FadeOut(time));

	IEnumerator _FadeIn(string sceneName, float time)
	{
		if (isFade == true) yield break;

		//フェードの初期化
		cardScript.gameObject.SetActive(true);
		cardScript.transform.localPosition = Vector3.zero;
		cardScript.transform.localScale = Vector3.zero;
		cardScript.FlipFrontCard();
		cardScript.cardData = CardDataDirector.ins.GetRandomCardData();
		yield return null;

		//カードを拡大して画面を隠す
		cardScript.transform.DOScale(Vector3.one * 15.0f, time / 2.0f);
		yield return new WaitForSeconds(time / 2.0f);

		//カードを反転させる
		cardScript.FlipCardAnimation(false, time / 2.0f);
		yield return new WaitForSeconds(time / 2.0f);

		//シーンを切り替える
		isFade = true;
		SceneManager.LoadScene(sceneName);
		yield break;
	}

	IEnumerator _FadeOut(float time)
	{
		if (isFade == false) yield break;
		yield return new WaitForSeconds(time * 1.0f / 5.0f);

		//カードを下に移動させて消えさせる
		cardScript.transform.DOLocalMoveY(-2000, time * 4.0f / 5.0f);
		yield return new WaitForSeconds(time * 4.0f / 5.0f);

		isFade = false;
		cardScript.gameObject.SetActive(false);
		yield break;
	}
}
