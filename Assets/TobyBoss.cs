﻿using UnityEngine;
using System.Collections;

public class TobyBoss : MonoBehaviour {

	public SpriteRenderer sprite;
	public Sprite[] tobySprites;
	public Enemy enemyComponent;
	private int spriteIndex;
	private int startHealth;

	void Start () {
		startHealth = enemyComponent.health;
	}

	void OnTakeDamage (Projectile.Damage damage) {
		float percentage = 1 - (float)enemyComponent.health / (float)startHealth;
		spriteIndex = Mathf.FloorToInt (percentage * (float)tobySprites.Length);
		sprite.sprite = tobySprites [spriteIndex];
		Debug.Log ("P: " + percentage + ", S: " + spriteIndex);
	}
}
