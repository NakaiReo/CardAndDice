using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiceRoll : MonoBehaviour
{
	public static DiceRoll ins;
	[SerializeField] int cheat = -1; //デバッグ用の確定出目

	public static bool active; //有効状態かどうか
	Rigidbody rb;

	public int value { get; private set; } //ダイスの出目
	[HideInInspector] public static bool canDiceing; //ダイスが振れる状態かどうか
	[HideInInspector] public bool isDiceing;  //ダイスを振っている状態かどうか
	[HideInInspector] public bool isRolling;  //ダイスの回転中かどうか
	[HideInInspector] public bool isShoot;    //ダイスが飛んでいる状態かどうか
	[HideInInspector] public bool isShootEnd; //ダイスが終了状態かどうか

	Vector3 startRotation;

	/// <summary>
    /// ダイスの出目ごとの向き
    /// </summary>
	public static Vector3[] diceForward = new Vector3[6]
	{
		new Vector3(0,1,0),
		new Vector3(0,0,-1),
		new Vector3(-1,0,0),
		new Vector3(1,0,0),
		new Vector3(0,0,1),
		new Vector3(0,-1,0)
	};

	/// <summary>
    /// ダイスの出目ごとの角度
    /// </summary>
	public static Vector3[] diceOffset = new Vector3[6]
	{
		new Vector3(0,-1,0),
		new Vector3(270,45,-1),
		new Vector3(0,-1,270),
		new Vector3(0,-1,90),
		new Vector3(90,45,-1),
		new Vector3(180,-1,0)
	};

	public static Sequence diceIdel; //待機中のアニメーション

	private void Awake()
	{
		ins = this;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		canDiceing = true;

		startRotation = transform.localRotation.eulerAngles;

		//ダイスの待機アニメーション
		diceIdel = DOTween.Sequence();
		diceIdel.Append(transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.5f).SetEase(Ease.Linear));
		diceIdel.Append(transform.DOScale(new Vector3(1.00f, 1.00f, 1.00f), 0.5f).SetEase(Ease.Linear));
		diceIdel.SetLoops(-1);
	}

	/// <summary>
    /// ダイスロール
    /// </summary>
	public IEnumerator Roll()
	{
		//条件を満たしてなければ振れなくする
		if (canDiceing == false) yield break;
		if (GameDirector.isEvent == true) yield break;

		diceIdel.Pause(); //待機モーションを停止
		transform.localScale = Vector3.one; //スケールを元に戻す

		//すべてのステートを戻す
		isDiceing = false;
		isRolling = false;
		isShoot = false;
		isShootEnd = false;

		//ダイスのスピン
		isRolling = true;
		StartCoroutine("SpinDice");
		yield return null;
		while (!Input.GetButtonDown("Submit") && isDiceButton != true)
		{
			yield return null;
		}

		//ダイスの跳ねを待機
		isShoot = true;
		StartCoroutine("ShootDice");
		yield return null;

		while (!Input.GetButtonDown("Submit") && isDiceButton != true && isShootEnd == false)
		{
			yield return null;
		}

		//ダイスの後処理
		RollEnd();

		//ダイスの値を返す
		yield return value;
	}

	private void RollEnd()
	{
		int r = Random.Range(0, 6) + 1; //ダイスの値
		FaceingDiceValue(r);            //ダイスの向きを直す

		value = r;

		if (cheat > 0) value = cheat; //チートがオンならその値にする

		isDiceing = false;
		isRolling = false;
		isShoot = false;

		Debug.Log("Dice => " + r);
	}

	/// <summary>
    /// ダイスの回転処理
    /// </summary>
	IEnumerator SpinDice()
	{
		int offset = 0;
		while (true)
		{
			if (isRolling == false) yield break;
			offset += 1; if (offset >= 8) offset = 0;
			float set = 27.5f / 2.0f * offset;
			transform.rotation = Quaternion.Euler(Extend.IRandom(0 + set), Extend.IRandom(45 + set), Extend.IRandom(0 + set));
			yield return new WaitForSeconds(0.02f);
		}
	}

	/// <summary>
    /// ダイスの跳ね
    /// </summary>
	IEnumerator ShootDice()
	{
		Vector3 startPos = transform.position;
		float y = startPos.y;
		float[] time = new float[] { 1.00f, 0.70f, 0.30f };

		SoundDirector.PlaySE("DiceHop");

		for (int i = 0; i < time.Length; i++)
		{
			float t = 0;
			float deg = 0;

			while (true)
			{
				if (isShoot == false) break;

				float dy = Mathf.Lerp(0, 0 + time[i] * 3, t);

				transform.position = startPos + new Vector3(0, dy, 0);

				if (deg >= 180) break;

				yield return new WaitForSeconds(0.01f);
				deg += 180 * 0.01f / time[i];
				t = Mathf.Sin(deg * Mathf.Deg2Rad);
			}

			SoundDirector.PlaySE("DiceHop");
		}

		transform.position = startPos;
		isShootEnd = true;
		//Roll();
	}

	/// <summary>
    /// 値の方向に向きを合わせる
    /// </summary>
	public void FaceingDiceValue(int amount)
	{
		if (amount > 6) return;
		else if(amount < 1)
		{
			transform.localRotation = Quaternion.Euler(startRotation);
			return;
		}
		Vector3 v = diceOffset[amount - 1].AddOffset();
		transform.LookAt(transform.position + diceForward[amount - 1]);
	}

	//ダイスのボタンが押されたかどうか
	public bool isDiceButton = false;
	public void DiceButton()
	{
		if (active == false) return;
		StartCoroutine(_DiceButton());
	}
	IEnumerator _DiceButton()
	{
		isDiceButton = true;
		yield return null;
		isDiceButton = false;
		yield break;
	}

	/// <summary>
	/// 角度をランダムに取得
	/// </summary>
	float RandomAmount()
	{
		return Random.Range(0, 360);
	}

	/// <summary>
    /// ダイスを使用できるかどうか設定する
    /// </summary>
	public static void Enable(bool enable = true)
	{	
		ins.gameObject.SetActive(enable);
		canDiceing = enable;
		active = enable;
		if (enable) diceIdel.Restart();
		else diceIdel.Pause();
	}
}
