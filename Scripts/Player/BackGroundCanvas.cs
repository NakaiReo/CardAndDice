using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundCanvas : MonoBehaviour
{
	[SerializeField] Transform pos;
    void Update()
    {
		transform.localPosition = pos.localPosition;
    }
}
