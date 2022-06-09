using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DumpCanvas : MonoBehaviour
{
	public TextMeshProUGUI dumpMessage;
	public Button submitButton;
	[HideInInspector] public bool isPush = false;

	public void PushButton()
	{
		StartCoroutine(_PushButton());
	}

	IEnumerator _PushButton()
	{
		isPush = true;
		yield return null;

		isPush = false;
		yield break;
	}
}
