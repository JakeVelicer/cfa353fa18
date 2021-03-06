﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicEnemyControls : MonoBehaviour {

	// Private Objects
    private Rigidbody2D Rigidbody;
	private Transform Target;
	private GameController MainController;
	private EnemyDamageValues DamageValues;
    private Animator anim;
	public System.Action OnPunch;
    public static System.Action<int> OnEnemyDeath;

	// Number Elements
	public float EnemyHealth;
    public int enemyValue;
    public float MovementSpeed;
	public float ChaseRange;
	public float FireRange;
	public float ProjectileSpeed;
	public float ProjectileHeight;
	public float CoolDown;
	private int DashDirection;
    public float groundDistance = 0.4f;
	public string groundMask = "Background";

	// Boolean Elements
	private bool CanRoam;
    private bool CanChase;
	private bool CanTakeDamage = true;
	public bool touchingGround;
	private bool Freeze;
	private bool Dead;
	private bool CanAttack = true;
	private bool takingFireDamage;
	private bool takingWaterDamage;
	private bool takingWindDamage;
	public bool ToTheRight;
	private bool CanSpawnIceBlock = true;
    private bool soundPlaying = false;

	// Attack Objects and Elements
	private PlayerControls Player;
	public Transform groundCheck;
	public GameObject BulletObject;
	public GameObject BombObject;
	public GameObject IceBlock;
	public Material DefaultMaterial;
	public Material HotFlash;
	public GameObject[] OtherEnemies;
	private Collider2D AttackCollider;
	private Collider2D Collider;

	// The type of enemy this is
	public int AlienType;

	// Sound Elements
    public AudioSource enemyAttacks;
    public AudioSource enemyVocals;
    public AudioSource enemyDamage;
    public AudioSource enemyAmbient;

    public AudioClip enemyPistol;
    public AudioClip enemyRapidFire;
    public AudioClip enemyLaser;
    public AudioClip enemyDeath1;
    public AudioClip enemyDeath2;
    public AudioClip enemyDeath3;
    public AudioClip enemyDeath4;
    public AudioClip enemyDeath5;
    public AudioClip rolyPolyRoll;
    public AudioClip blobSpit;
    public AudioClip beefySmash;
    public AudioClip machineGunRev;

    public AudioClip hitDamage;
    public AudioClip criticalDamage;

    // Use this for initialization
    void Start () {


        FloatingTextController.Initialize();
        CriticalFloatingTextController.Initialize();
		
        // Assignment Calls
        anim = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
		DamageValues = gameObject.GetComponent<EnemyDamageValues> ();
		Collider = gameObject.GetComponent<Collider2D> ();
		MainController = GameObject.Find ("Controller").GetComponent<GameController>();
		Player = GameObject.Find("Player").GetComponent<PlayerControls>();

		// Setting elements to their proper states
		InvokeRepeating ("Roam", 0, 1.5f);
		touchingGround = false;
		GetComponent<SpriteRenderer>().material = DefaultMaterial;
		MainController.EnemiesLeft++;
		
		if (AlienType == 1 || AlienType == 3) {
			AttackCollider = gameObject.transform.GetChild(0).GetComponent<Collider2D>();
			AttackCollider.enabled = false;
		}
		
	}
	
	// Update is called once per frame
	void Update () {

		// Finds the Player's transform and stores it in target
		Target = GameObject.FindGameObjectWithTag ("Player").transform;

		ChaseTarget();
		//TrackOtherEnemies();

		// Triggers Death
		if (EnemyHealth <= 0) {
			if (!Dead) {
				EnemyDeathSequence();
			}
		}

		/*
		if (transform.position.y <= -1.5) {
			touchingGround = true;
		}
		else if (transform.position.y > -1.5) {
			touchingGround = false;
		}
		*/

		// Checks if touching the ground
		touchingGround = Physics2D.OverlapCircle(groundCheck.position, groundDistance, LayerMask.GetMask(groundMask));
		
		// Call damage for stream attacks if possible
		if (takingFireDamage && CanTakeDamage) {
			StartCoroutine(TakeFireDamage());
		}
		if (takingWaterDamage && CanTakeDamage) {
			StartCoroutine(TakeWaterDamage());
		}
		if (takingWindDamage && CanTakeDamage) {
			StartCoroutine(TakeWindDamage());
		}

	}

	// Controls the actual movement of the Enemy
	void FixedUpdate() {

		// Checks if it is allowed to chase the player
		if (CanChase && !Freeze || CanRoam && !Freeze) {

			// Pushes the enemy in a direction based upon which side the player is on
			if (ToTheRight == false) {
     			if (touchingGround && CanAttack) {
					Vector2 myVel = Rigidbody.velocity;
                	myVel.x = -MovementSpeed;
					Rigidbody.velocity = myVel;
					anim.SetInteger("Near", 1);
				}
			}
			else if (ToTheRight == true) {
     			if (touchingGround && CanAttack) {
					Vector2 myVel = Rigidbody.velocity;
                	myVel.x = MovementSpeed;
					Rigidbody.velocity = myVel;
					anim.SetInteger("Near", 1);
				}
            }
		}
	}

	void ChaseTarget () {

		float Dist = Vector3.Distance(Target.position, transform.position);
		float DistX = Mathf.Abs(Target.position.x - transform.position.x);

		// Determines if the range of the player is close enough to be chased
		if (Dist <= ChaseRange && Dist > FireRange && !Freeze) {

			CanChase = true;
			CanRoam = false;
			ChaseDirection();
		}
		// Tells the player to attack if close enough
		else if (Dist <= FireRange && !Freeze) {

			CanChase = false;
			CanRoam = false;
			ChaseDirection();
			if (CanAttack) {
				if (touchingGround && !Dead && !Freeze) {
					CanAttack = false;
					anim.SetTrigger("Attack");

					// This switch assigns the proper and attack phase for each enemy type.
					switch (AlienType) {
						// Roly Poly Alien
						case 1:
							StartCoroutine(DashAttack());
							break;
						// Blob Alien
						case 2:
							StartCoroutine(BombAttack());
							break;
						// Beefy Alien
						case 3:
							StartCoroutine(JumpSmashAttack());
							break;
						// Armored Alien
						case 4:
                            StartCoroutine(GunAttack());
							break;
					}
				}
			}
		}
		// Roams out of range of chasing and attacking
		else if (!Freeze) {

			CanChase = false;
			CanRoam = true;
            enemyAttacks.Stop();
            soundPlaying = false;
		}
	}

	// Determines the direction the object faces when chasing
	void ChaseDirection () {

		if (CanRoam == false) {
			if (Target.position.x > transform.position.x + 0.5) {
				transform.localScale = new Vector3(-1, 1, 1);
				ToTheRight = true;
			}
			else if (Target.position.x < transform.position.x + 0.5) {
				transform.localScale = new Vector3(1, 1, 1);
				ToTheRight = false;
			}
		}
		else if (CanRoam == true) {
			if (ToTheRight == false) {
				transform.localScale = new Vector3(-1, 1, 1);
				ToTheRight = true;
			}
			else if (ToTheRight == true) {
				transform.localScale = new Vector3(1, 1, 1);
				ToTheRight = false;
			}
		}
	}

	void TrackOtherEnemies () {

		OtherEnemies = GameObject.FindGameObjectsWithTag("Enemy");

		for (int i = 0; i < OtherEnemies.Length; i++) {

			Vector3 toTarget = (OtherEnemies[i].transform.position - transform.position);
			
			if (Vector3.Dot(toTarget, transform.right) < 0) {
				touchingGround = false;
			} else if (Vector3.Dot(toTarget, transform.right) > 0) {
				touchingGround = true;
			}

		}

	}

    // Instantiates a chosen projectile in the scene and propels it forward like a bullet
    private IEnumerator GunAttack () {

		Rigidbody.velocity = Vector2.zero;
        SoundCall(machineGunRev, enemyAmbient);
        yield return new WaitForSeconds(0.7f);
		if (!Freeze) {
			SoundCall(enemyPistol, enemyAttacks);
		}
		if (ToTheRight == true && !Freeze)
		{
			GameObject Projectile = Instantiate(BulletObject, transform.position + new Vector3(1.0f, .10f, 0),
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.right * ProjectileSpeed);
		}
		else if (ToTheRight == false && !Freeze)
		{
			GameObject Projectile = Instantiate (BulletObject, transform.position + new Vector3(-1.0f, .10f, 0), 
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.left * ProjectileSpeed);
		}
		StartCoroutine(shootWait());
	}

	// Instantiates a chosen projectile in the scene and propels it forward and up like a thrown bomb
	private IEnumerator BombAttack () {
		
		Rigidbody.velocity = Vector2.zero;
		yield return new WaitForSeconds(0.2f);
        SoundCall(blobSpit, enemyAttacks);
        if (ToTheRight == true && !Freeze) {
			GameObject Projectile = Instantiate (BombObject, transform.position + new Vector3(0.5f, 0.5f, 0), 
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.up * ProjectileHeight);
			Projectile.GetComponent<Rigidbody2D>().AddForce(new Vector3 (Target.position.x - transform.position.x, 0, 0) * ProjectileSpeed);
		}
		else if (ToTheRight == false && !Freeze) {
			GameObject Projectile = Instantiate (BombObject, transform.position + new Vector3(-0.5f, 0.5f, 0), 
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.up * ProjectileHeight);
			Projectile.GetComponent<Rigidbody2D>().AddForce(new Vector3 (Target.position.x - transform.position.x, 0, 0) * ProjectileSpeed);
		}
        StartCoroutine(shootWait());
	}

	// Propels this enemy toward the player
	private IEnumerator JumpSmashAttack () {

		Rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);
		if (!Freeze) {
			gameObject.GetComponent<Rigidbody2D>().AddForce
				(new Vector3 (Target.position.x - transform.position.x, 0, 0) * ProjectileSpeed);
			GetComponent<Rigidbody2D>().AddForce(Vector3.up * ProjectileHeight);
		}
        yield return new WaitForSeconds(.5f);
        anim.SetTrigger("Slam");
        yield return new WaitForSeconds(.2f);
        AttackCollider.enabled = true;
		Rigidbody.gravityScale = 12;
        yield return new WaitForSeconds(0.4f);
		GameObject.Find("Controller").GetComponent<ScreenShake>().BombGoesOff(0.2f);
		Rigidbody.gravityScale = 2;
		AttackCollider.enabled = false;
        SoundCall(beefySmash, enemyAttacks);
        StartCoroutine(shootWait());

    }

	// Dash attack cycle
    private IEnumerator DashAttack()
    {
		Rigidbody.velocity = Vector2.zero;
        if (ToTheRight) {
            DashDirection = 1;
			anim.SetInteger("R_or_L", 1);
        }
        else if (!ToTheRight) {
            DashDirection = 2;
			anim.SetInteger("R_or_L", 2);
        }
		yield return new WaitForSeconds(0.6f);
		AttackCollider.enabled = true;
		SoundCall(rolyPolyRoll, enemyAttacks);
		if (!Freeze) {
			if (DashDirection == 1) {
				Rigidbody.AddForce(Vector2.right * ProjectileSpeed, ForceMode2D.Impulse);
			}
			else if (DashDirection == 2) {
				Rigidbody.AddForce(Vector2.left * ProjectileSpeed, ForceMode2D.Impulse);
			}
		}
		yield return new WaitForSeconds(0.6f);
		anim.SetTrigger("Untuck");
		Rigidbody.velocity = Vector2.zero;
		Rigidbody.angularVelocity = 0.0f;
		AttackCollider.enabled = false;
		StartCoroutine(shootWait());
	}

	// Cooldown before allowed to attack again
    private IEnumerator shootWait()
	{
    	anim.SetInteger("Near", 2);
    	yield return new WaitForSeconds(CoolDown);
        CanAttack = true;
    }

	void EnemyDeathSequence () {

		Dead = true;
		Freeze = true;
		Rigidbody.velocity = Vector2.zero;
		MainController.score += enemyValue;
		GameObject.Find("Score").GetComponent<Animator>().SetTrigger("Bulge");
		MainController.EnemiesLeft--;
		anim.SetTrigger("Die");

		if (AlienType == 1)
		{
			if (soundPlaying == false)
			{
                SoundCall(enemyDeath4, enemyVocals);
                soundPlaying = true;
			}
		}
        else if (AlienType == 2)
        {
            if (soundPlaying == false)
            {
                SoundCall(enemyDeath5, enemyVocals);
                soundPlaying = true;
            }
        }
        else if (AlienType == 3)
        {
            if (soundPlaying == false)
            {
                SoundCall(enemyDeath2, enemyVocals);
                soundPlaying = true;
            }
        }
        else if (AlienType == 4)
		{
			if (soundPlaying == false)
			{
                SoundCall(enemyDeath1, enemyVocals);
                soundPlaying = true;
			}
		}
		if (OnEnemyDeath != null)
		{
			OnEnemyDeath(AlienType);
		}

		Destroy(gameObject, 0.5f);
	}

	// All damage intake for enemies with this script is started by this on trigger enter
    void OnTriggerEnter2D(Collider2D collision) {

		if (collision.gameObject.name == "LightningBullet(Clone)") {
			Destroy(collision.gameObject);
			EnemyHealth -= DamageValues.ElectricDamage;
			StartCoroutine(HitByAttack(100, 205, 0.3f));
            if (AlienType == 4)
            {
				DisplayCriticalDamage(DamageValues.ElectricDamage);
                SoundCall(criticalDamage, enemyDamage);
            }
            if (AlienType != 4)
            {
				DisplayDamage(DamageValues.ElectricDamage);
                SoundCall(hitDamage, enemyDamage);
            }
        }
		if (collision.gameObject.name == "LightningBullet2(Clone)") {
			if (collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill < 1) {
				collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill++;
			}
			else {
				Destroy(collision.gameObject);
			}
			EnemyHealth -= DamageValues.ElectricDamage * 1.2f;
			StartCoroutine(HitByAttack(200, 210, 0.5f));
            if (AlienType == 4)
            {
				DisplayCriticalDamage(DamageValues.ElectricDamage * 1.2f);
                SoundCall(criticalDamage, enemyDamage);
            }
            if (AlienType != 4)
            {
				DisplayDamage(DamageValues.ElectricDamage * 1.2f);
                SoundCall(hitDamage, enemyDamage);
            }
        }
		if (collision.gameObject.name == "LightningBullet3(Clone)") {
			if (collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill < 1) {
				collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill++;
			}
			else {
				Destroy(collision.gameObject);
			}
			EnemyHealth -= DamageValues.ElectricDamage * 1.5f;
			StartCoroutine(HitByAttack(300, 215, 1));
            if (AlienType == 4)
            {
				DisplayCriticalDamage(DamageValues.ElectricDamage * 1.5f);
                SoundCall(criticalDamage, enemyDamage);
            }
            if (AlienType != 4)
            {
				DisplayDamage(DamageValues.ElectricDamage * 1.5f);
                SoundCall(hitDamage, enemyDamage);
            }
        }
		if (collision.gameObject.name == "LightningBullet4(Clone)") {
			if (collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill < 2) {
				collision.gameObject.GetComponent<BasicBullet>().passedThroughToKill++;
			}
			else {
				Destroy(collision.gameObject);
			}
			EnemyHealth -= DamageValues.ElectricDamage * 2.0f;
			StartCoroutine(HitByAttack(400, 220, 1.5f));
            if (AlienType == 4)
            {
				DisplayCriticalDamage(DamageValues.ElectricDamage * 2.0f);
                SoundCall(criticalDamage, enemyDamage);
            }
            if (AlienType != 4)
            {
				DisplayDamage(DamageValues.ElectricDamage * 2.0f);
                SoundCall(hitDamage, enemyDamage);
            }
        }
		else if (collision.gameObject.tag == "Earth") {
			StartCoroutine(HitByAttack(0, 400, 2));
			EnemyHealth -= DamageValues.EarthDamage;
            DisplayDamage(DamageValues.EarthDamage);
        }
		else if (collision.gameObject.tag == "Speed") {
			StartCoroutine(HitByAttack(0, 200, 0.3f));
			EnemyHealth -= DamageValues.SpeedDamage;
            DisplayDamage(DamageValues.SpeedDamage);
        }
		else if (collision.gameObject.name == "AnchorArms") {
			StartCoroutine(HitByAttack(200, 300, 1));
			EnemyHealth -= DamageValues.JackedDamage;
            DisplayKO("KO");
        }
		else if (collision.gameObject.tag == "Fire") {
			takingFireDamage = true;
        }
		else if (collision.gameObject.tag == "Water") {
			takingWaterDamage = true;
        }
		else if (collision.gameObject.tag == "Wind") {
			takingWindDamage = true;
			StartCoroutine(HitByAttack(300, 600, 2));
		}
		else if (collision.gameObject.tag == "Ice") {
			if (CanSpawnIceBlock) {
				if (collision.gameObject.name != "ExplosionHitBox") {
					Destroy(collision.gameObject);
				}
				StartCoroutine(TakeIceDamage());
			}
            if (AlienType == 3) {
                SoundCall(criticalDamage, enemyDamage);
            }
            if (AlienType != 3) {
                SoundCall(hitDamage, enemyDamage);
            }
        }
	}

	void OnTriggerExit2D(Collider2D collider) {
		if (collider.gameObject.tag == "Fire") {
			takingFireDamage = false;
		}
		else if (collider.gameObject.tag == "Water") {
			takingWaterDamage = false;
		}
		else if (collider.gameObject.tag == "Wind") {
			takingWindDamage = false;
		}
	}

	private IEnumerator TakeFireDamage() {
		CanTakeDamage = false;
		if (AlienType == 1) {
			DisplayCriticalDamage(DamageValues.FireDamage);
			SoundCall(criticalDamage, enemyDamage);
		}
		else if (AlienType != 1) {
			DisplayDamage(DamageValues.FireDamage);
			SoundCall(hitDamage, enemyDamage);
		}
		EnemyHealth -= DamageValues.FireDamage;
		StartCoroutine(HitByAttack(100, 100, 0.7f));
		yield return new WaitForSeconds(0.6f);
		CanTakeDamage = true;
	}

	private IEnumerator TakeWaterDamage() {
		CanTakeDamage = false;
		if (AlienType == 2) {
			DisplayCriticalDamage(DamageValues.WaterDamage);
			SoundCall(criticalDamage, enemyDamage);
		}
		else if (AlienType != 2) {
			DisplayDamage(DamageValues.WaterDamage);
			SoundCall(hitDamage, enemyDamage);
		}
		EnemyHealth -= DamageValues.WaterDamage;
		StartCoroutine(HitByAttack(100, 100, 0.7f));
		yield return new WaitForSeconds(0.6f);
		CanTakeDamage = true;
	}

	private IEnumerator TakeWindDamage() {
		CanTakeDamage = false;
		if (AlienType == 5) {
			DisplayCriticalDamage(DamageValues.WindDamage);
			SoundCall(criticalDamage, enemyDamage);
		}
		else if (AlienType != 5) {
			DisplayDamage(DamageValues.WindDamage);
			SoundCall(hitDamage, enemyDamage);
		}
		EnemyHealth -= DamageValues.WindDamage;
		yield return new WaitForSeconds(0.6f);
		CanTakeDamage = true;
    }

	private IEnumerator TakeIceDamage() {

		CanSpawnIceBlock = false;
		if (AlienType == 1)
		{
			anim.Play("RolyPolyIdle");
		}
		StartCoroutine(HitByAttack(0, 0, 3));
		Rigidbody.velocity = Vector2.zero;
		transform.rotation = Quaternion.Euler (transform.rotation.x, transform.rotation.y, 0);
		GameObject Projectile = Instantiate (IceBlock, transform.position + new Vector3(0, 0, 0), 
		Quaternion.identity) as GameObject;
		Projectile.transform.parent = this.gameObject.transform;
		for (int i = 0; i < 3; i++) {
			EnemyHealth -= DamageValues.IceDamage;
			if (AlienType == 3) {
				DisplayCriticalDamage(DamageValues.IceDamage);
			}
			else {
				DisplayDamage(DamageValues.IceDamage);
			}
			yield return new WaitForSeconds(1);
		}
		CanSpawnIceBlock = true;
    }

	private IEnumerator HitByAttack (int xSpeed, int ySpeed, float Seconds) {
		if (!Dead) {
			Freeze = true;
			GetComponent<SpriteRenderer>().material = HotFlash;
			if (touchingGround) {
				Rigidbody.velocity = Vector2.zero;
				Rigidbody.AddForce(Vector3.up * ySpeed);
				if (Player.facingRight) {
					Rigidbody.AddForce(Vector3.right * xSpeed);
				}
				else if (!Player.facingRight) {
					Rigidbody.AddForce(Vector3.left * xSpeed);
				}
			}
			yield return new WaitForSeconds(0.1f);
			GetComponent<SpriteRenderer>().material = DefaultMaterial;
			yield return new WaitForSeconds(Seconds);
			Freeze = false;
		}
	}

	private void Roam() {
		if (CanRoam) {
			ChaseDirection();
		}
    }

    void DisplayKO(string Word)
    {
		CriticalFloatingTextController.CreateFloatingText(Word, this.transform);
    }

    void DisplayDamage(float amount)
    {
    	FloatingTextController.CreateFloatingText("-" + (Math.Truncate((decimal)(amount))).ToString(), this.transform);
    }

    void DisplayCriticalDamage(float amount)
    {
    	CriticalFloatingTextController.CreateFloatingText("-" + (Math.Truncate((decimal)(amount))).ToString(), this.transform);
    }

    void SoundCall(AudioClip clip, AudioSource source)
    {
        source.clip = clip;
        source.loop = false;
        source.loop |= (source.clip == enemyLaser);
        source.Play();
    }

}
