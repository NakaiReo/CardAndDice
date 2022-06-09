using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AnimatorManager : MonoBehaviour
{
	[SerializeField] string spritePath;
	public string SpritePathChange
	{
		set
		{
			spritePath = value;
			LoadSprite();
		}
	}

	SpriteRenderer spriteRenderer = null;
	Image image = null;

	Sprite[] sprite;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		image = GetComponent<Image>();

		//sprite = Resources.LoadAll<Sprite>(spritePath);
	}

	public void ChangeSprite(int index)
	{
		if (spriteRenderer != null) spriteRenderer.sprite = sprite[index];
		if (image != null) image.sprite = sprite[index];
	}

	public void LoadSprite()
	{
		sprite = Resources.LoadAll<Sprite>(spritePath);
		GetComponent<Animator>().SetTrigger("Reset");
		ChangeSprite(0);
	}
}

public class CharacterAnimation : MonoBehaviour
{
	public class Trigger
	{
		public Animator animator;

		public void Attack()
		{
			animator.SetTrigger("Attack");
		}
		public void Avoidance()
		{
			animator.SetTrigger("Avoidance");
			Sequence sequence = DOTween.Sequence();
			float key = animator.transform.localScale.x;
			sequence.Append(animator.transform.DOScaleX(key * -1, 0.25f));
			sequence.Append(animator.transform.DOScaleX(key * +1, 0.25f));
		}
		public void TakeDamage()
		{
			animator.SetTrigger("TakeDamage");
			Sequence sequence = DOTween.Sequence();
			sequence.Append(animator.transform.DOPunchScale(Vector3.one * 0.25f, 0.70f));
		}
		public void Down()
		{
			Debug.Log("Down => " + animator);
			animator.SetTrigger("Down");
		}

		public void Victory()
		{
			animator.SetTrigger("Victory");
		}
	}
}