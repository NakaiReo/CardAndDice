using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiceRoll : MonoBehaviour
{
	public static DiceRoll ins;
	[SerializeField] int cheat = -1;

	public static bool active;
	Rigidbody rb;

	public int value { get; private set; }
	[HideInInspector] public static bool canDiceing;
	[HideInInspector] public bool isDiceing;
	[HideInInspector] public bool isRolling;
	[HideInInspector] public bool isShoot;
	[HideInInspector] public bool isShootEnd;

	Vector3 startRotation;

	public static Vector3[] diceForward = new Vector3[6]
	{
		new Vector3(0,1,0),
		new Vector3(0,0,-1),
		new Vector3(-1,0,0),
		new Vector3(1,0,0),
		new Vector3(0,0,1),
		new Vector3(0,-1,0)
	};
	public static Vector3[] diceOffset = new Vector3[6]
	{
		new Vector3(0,-1,0),
		new Vector3(270,45,-1),
		new Vector3(0,-1,270),
		new Vector3(0,-1,90),
		new Vector3(90,45,-1),
		new Vector3(180,-1,0)
	};

	public static Sequence diceIdel;

	private void Awake()
	{
		ins = this;
	}
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		canDiceing = true;

		startRotation = transform.localRotation.eulerAngles;

		diceIdel = DOTween.Sequence();
		diceIdel.Append(transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.5f).SetEase(Ease.Linear));
		diceIdel.Append(transform.DOScale(new Vector3(1.00f, 1.00f, 1.00f), 0.5f).SetEase(Ease.Linear));
		diceIdel.SetLoops(-1);
	}

	public IEnumerator Roll()
	{
		if (canDiceing == false) yield break;
		if (GameDirector.isEvent == true) yield break;

		diceIdel.Pause();
		transform.localScale = Vector3.one;

		isDiceing = false;
		isRolling = false;
		isShoot = false;
		isShootEnd = false;

		isRolling = true;
		StartCoroutine("SpinDice");
		yield return null;
		while (!Input.GetButtonDown("Submit") && isDiceButton != true)
		{
			yield return null;
		}

		isShoot = true;
		StartCoroutine("ShootDice");
		yield return null;

		while (!Input.GetButtonDown("Submit") && isDiceButton != true && isShootEnd == false)
		{
			yield return null;
		}

		RollEnd();

		yield return value;
	}

	private void RollEnd()
	{
		int r = Random.Range(0, 6) + 1;
		FaceingDiceValue(r);

		value = r;

		if (cheat > 0) value = cheat;

		isDiceing = false;
		isRolling = false;
		isShoot = false;

		Debug.Log("Dice => " + r);
	}

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

	float RandomAmount()
	{
		return Random.Range(0, 360);
	}

	public static void Enable(bool enable = true)
	{	
		ins.gameObject.SetActive(enable);
		canDiceing = enable;
		active = enable;
		if (enable) diceIdel.Restart();
		else diceIdel.Pause();
	}
}
