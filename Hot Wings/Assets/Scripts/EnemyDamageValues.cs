﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageValues : MonoBehaviour {

	private BasicEnemyControls Enemy;

	public float FireDamage;
	public float WaterDamage;
	public float ElectricDamage;
	public float IceDamage;
	public float EarthDamage;
	public float WindDamage;
	private int JackedDamage;

	// Use this for initialization
	void Start () {

		Enemy = gameObject.GetComponent<BasicEnemyControls> ();

		if (Enemy.AlienType == 1) {
			FireDamage = 10;
			WaterDamage = 8;
			ElectricDamage = 10;
			IceDamage = 12;
			EarthDamage = 8;
			WindDamage = 5;
			JackedDamage = 30;
		}
		if (Enemy.AlienType == 2) {
			FireDamage = 10;
			WaterDamage = 25;
			ElectricDamage = 28;
			IceDamage = 5;
			EarthDamage = 5;
			WindDamage = 5;
			JackedDamage= 30;
		}
		if (Enemy.AlienType == 3) {
			FireDamage = 10;
			WaterDamage = 8;
			ElectricDamage = 10;
			IceDamage = 12;
			EarthDamage = 8;
			WindDamage = 5;
			JackedDamage = 30;
		}
		if (Enemy.AlienType == 4) {
			FireDamage = 10;
			WaterDamage = 25;
			ElectricDamage = 10;
			IceDamage = 12;
			EarthDamage = 8;
			WindDamage = 5;
			JackedDamage = 30;
		}
		if (Enemy.AlienType == 5) {
			FireDamage = 2;
			WaterDamage = 5;
			ElectricDamage = 10;
			IceDamage = 25;
			EarthDamage = 30;
			WindDamage = 2;
			JackedDamage = 30;
		}
	}
}