using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundDirector : MonoBehaviour
{
	[SerializeField] AudioMixer audioMixer;

	[Space(15)]
	[SerializeField] GameObject menuObject; //Sound設定のメニューのオブジェクト
	[SerializeField] Slider MasterSlider;   //全体の音量設定用のslider
	[SerializeField] Slider BGMSlider;      //BGMの音量設定用のslider
	[SerializeField] Slider SESlider;       //SEの音量設定用のslider

	static bool isOpenMenu; //メニューが開かれているか

	static AudioSource BGM;
	static AudioSource SE;

	public static SoundDirector ins = null;
	private void Awake()
	{
		if(ins == null)
		{
			ins = this;

			isOpenMenu = false;

			//それぞれの変数にコンポーネントを割り当てる
			AudioSource[] audioSources = GetComponents<AudioSource>();
			BGM = audioSources[0];
			SE = audioSources[1];

			//音量のデータが保存されてなければ初期化する
			if (!PlayerPrefs.HasKey("AudioMaster")) PlayerPrefs.SetFloat("AudioMaster", 0.5f);
			if (!PlayerPrefs.HasKey("AudioBGM")) PlayerPrefs.SetFloat("AudioBGM", 0.5f);
			if (!PlayerPrefs.HasKey("AudioSE")) PlayerPrefs.SetFloat("AudioSE", 0.5f);

			Debug.Log("Master: " + PlayerPrefs.GetFloat("AudioMaster"));
			Debug.Log("BGM: " + PlayerPrefs.GetFloat("AudioBGM"));
			Debug.Log("SE:" + PlayerPrefs.GetFloat("AudioSE"));

			//設定用のSliderの現在値を更新する
			MasterSlider.value = PlayerPrefs.GetFloat("AudioMaster");
			BGMSlider.value = PlayerPrefs.GetFloat("AudioBGM");
			SESlider.value = PlayerPrefs.GetFloat("AudioSE");

			//メニューを非表示
			menuObject.SetActive(isOpenMenu);

			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		//音量の更新
		audioMixer.SetFloat("Master", MasterSlider.value.Value2decibel());
		audioMixer.SetFloat("BGM", BGMSlider.value.Value2decibel());
		audioMixer.SetFloat("SE", SESlider.value.Value2decibel());

		//Escapeキーを押したら設定を開く
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isOpenMenu = !isOpenMenu;
			menuObject.SetActive(isOpenMenu);
		}
	}

	/// <summary>
	/// BGMを流す
	/// </summary>
	/// <param name="fileName">音楽ファイルのパス</param>
	/// <param name="loop">ループさせるか</param>
	public static void PlayBGM(string fileName, bool loop = true)
	{
		string path;
		path = "Sound/BGM/" + fileName;
		AudioClip audioClip = Resources.Load(path) as AudioClip;

		if(audioClip == null)
		{
			Debug.LogError("ファイルが見つかりませんでした。\n>> " + path);
			return;
		}

		BGM.loop = loop;
		BGM.clip = audioClip;
		BGM.Play();
	}

	/// <summary>
	/// SEを流す
	/// </summary>
	/// <param name="fileName">音声ファイルのパス</param>
	public static void PlaySE(string fileName)
	{
		string path;
		path = "Sound/SE/" + fileName;
		AudioClip audioClip = Resources.Load(path) as AudioClip;

		if (audioClip == null)
		{
			Debug.LogError("ファイルが見つかりませんでした。\n>> " + path);
			return;
		}

		SE.PlayOneShot(audioClip);
	}

	/// <summary>
	/// 設定の再描画
	/// </summary>
	public void Redraw()
	{
		if (isOpenMenu == false) return;
		PlayerPrefs.SetFloat("AudioMaster", MasterSlider.value);
		PlayerPrefs.SetFloat("AudioBGM", BGMSlider.value);
		PlayerPrefs.SetFloat("AudioSE", SESlider.value);

		audioMixer.SetFloat("Master", MasterSlider.value.Value2decibel());
		audioMixer.SetFloat("BGM", BGMSlider.value.Value2decibel());
		audioMixer.SetFloat("SE", SESlider.value.Value2decibel());
	}

	/// <summary>
	/// メニューを開く
	/// </summary>
	public void MenuOpen()
	{
		isOpenMenu = true;
		SoundDirector.PlaySE("UI/Select");
		menuObject.SetActive(isOpenMenu);
	}

	/// <summary>
	/// メニューを閉じる
	/// </summary>
	public void MenuClose()
	{
		isOpenMenu = false;
		SoundDirector.PlaySE("UI/Cancel");
		menuObject.SetActive(isOpenMenu);
	}

	/// <summary>
	/// タイトルに戻る
	/// </summary>
	public void GotoTitle()
	{
		MenuClose();
		Fade.ins.FadeIn("Title", 1.5f);
	}

	/// <summary>
	/// ゲームを終了する
	/// </summary>
	public void Shutdown()
	{
		MenuClose();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_STANDALONE
		UnityEngine.Application.Quit();
		#endif
	}
}
