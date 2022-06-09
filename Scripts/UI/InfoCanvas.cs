using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 操作説明の操作
/// </summary>
public class InfoCanvas : MonoBehaviour
{
	public static InfoCanvas ins = null;
	public bool isOpen = false; //開いているかどうか
	public int index = 0; //現在のページ数
	int maxIndex; //ページの総数

	[SerializeField] GameObject infoPanel; //操作説明のパネル
	[SerializeField] Transform pages; //ページのオブジェクト達

	[Space]
	[SerializeField] Button nextButton;  //次のページのボタン
	[SerializeField] Button backButton;  //前のページのボタン
	[SerializeField] Button closeButton; //閉じるボタン

	private void Awake()
	{
		if (ins == null)
		{
			ins = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		Close();

		maxIndex = pages.childCount - 1; //総ページ数の設定
	}

	/// <summary>
	/// 操作説明を開く
	/// </summary>
	public void Open()
	{
		SoundDirector.PlaySE("UI/Select");
		isOpen = true;
		infoPanel.SetActive(true);

		index = 0; //現在のページを最初のページにする
		OpenPage();
	}

	/// <summary>
	/// 操作説明を閉じる
	/// </summary>
	public void Close()
	{
		SoundDirector.PlaySE("UI/Cancel");
		isOpen = false;
		infoPanel.SetActive(false);
	}

	/// <summary>
	/// 次のページに移る
	/// </summary>
	public void NextPage()
	{
		SoundDirector.PlaySE("UI/Select");
		index = Mathf.Clamp(index + 1, 0, maxIndex);
		OpenPage();
	}

	/// <summary>
	/// 前のページに戻る
	/// </summary>
	public void BackPage()
	{
		SoundDirector.PlaySE("UI/Select");
		index = Mathf.Clamp(index - 1, 0, maxIndex);
		OpenPage();
	}

	/// <summary>
	/// ページを開く
	/// </summary>
	public void OpenPage()
	{
		//現在のページ以外を非表示にして、現在のページを表示する
		for(int i = 0; i <= maxIndex; i++)
		{
			pages.GetChild(i).gameObject.SetActive(index == i);
		}

		backButton.gameObject.SetActive(index > 0);        //最初のページだった場合戻るボタンを非表示にする
		nextButton.gameObject.SetActive(index < maxIndex); //最後のページだった場合進むボタンを非表示にする
	}
}
