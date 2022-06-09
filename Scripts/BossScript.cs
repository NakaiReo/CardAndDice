using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
	[SerializeField] Map map;
	[SerializeField] public int bossID;
	[SerializeField] public Vector2Int pos;
	[SerializeField] public bool lastBattle;

	void Start()
    {
		GetComponent<AnimatorManager>().SpritePathChange = "Enemy/Boss" + bossID;
		map.ChangeBossGrid(pos, this);
		map.bossScripts[bossID - 1] = this;
    }

	private void OnValidate()
	{
		if (map == null) return;
		transform.position = map.GetMapTileWorldPos(pos);
	}

	public void Die()
	{
		map.DieBossGrid(pos);
		GetComponent<Animator>().SetTrigger("Down");

		if(lastBattle == true)
		{
			GameDirector.ins.GameClear();
		}
	}
}
