using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TitleButton : MonoBehaviour
{
    enum TitleEnum
	{
		Play,
		Manual,
		Config,
		Exit
	}

	[SerializeField] TitleEnum buttonType;

    void Start()
    {
		OnPointerExit();
    }

	/// <summary>
	/// ボタンを押したときの処理
	/// </summary>
	public void OnClick()
	{
		//buttonTypeによって押された時の処理を変える
		Debug.Log("OnClick");
		switch (buttonType)
		{
			case TitleEnum.Play:
				SoundDirector.PlaySE("UI/Select");
				Fade.ins.FadeIn("CharacterSelect", 1.5f);
				break;
			case TitleEnum.Manual:
				InfoCanvas.ins.Open();
				break;
			case TitleEnum.Config:
				SoundDirector.PlaySE("UI/Select");
				SoundDirector.ins.MenuOpen();
				break;
			case TitleEnum.Exit:
				Shutdown();
				break;
		}
	}

	/// <summary>
	/// ボタンにポインターが乗った時の処理
	/// </summary>
    public void OnPointerEnter()
	{
		transform.DOScale(Vector3.one * 1.3f, 0.2f);
		Debug.Log("OnPointerEnter");
	}

	/// <summary>
	/// ボタンからポインターが離れた時の処理
	/// </summary>
	public void OnPointerExit()
	{
		transform.DOScale(Vector3.one, 0.2f);
		Debug.Log("OnPointerExit");
	}


	/// <summary>
	/// ゲームの終了
	/// </summary>
	public void Shutdown()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_STANDALONE
		UnityEngine.Application.Quit();
		#endif
	}
}
