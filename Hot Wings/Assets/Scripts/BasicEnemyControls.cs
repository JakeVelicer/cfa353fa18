﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyControls : MonoBehaviour {

	// Private Objects
    private Rigidbody2D Rigidbody;
	private Transform Target;
	private GameController MainController;
	private EnemyDamageValues DamageValues;
    private GameObject gameController;
	private DeathRayAnimation BeamAnimation;
    private Animator anim;
	private System.Action DestroyEnemySequence;
	public System.Action OnPunch;
	private System.Action ActivateDeathBeam;
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
	private float CoolDownTimer = 0;
	private int DashDirection;

	// Boolean Elements
	private bool CanRoam;
    private bool CanChase;
	public bool TouchStop;
	private bool CanAttack = true;
	private bool CanFireRay = true;
	public bool ToTheRight;

	// Attack Objects and Elements
	private playerControls Player;
	public GameObject BulletObject;
	public GameObject BombObject;
	public GameObject IceBlock;
	public GameObject SaucerRay;
	public GameObject[] OtherEnemies;
	private Collider2D AttackCollider;
	private Collider2D Collider;

	// The type of enemy this is
	public int AlienType;

	// Sound Elements
    private AudioSource enemySounds;
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
    public AudioClip laserCharge;

    public AudioClip hitDamage;
    public AudioClip criticalDamage;
    private bool soundPlaying = false;

    // Use this for initialization
    void Start () {


        FloatingTextController.Initialize();
        CriticalFloatingTextController.Initialize();
        // Assignment Calls
        anim = GetComponent<Animator>();
        enemySounds = gameObject.GetComponent<AudioSource>();
		Rigidbody = GetComponent<Rigidbody2D>();
		DamageValues = gameObject.GetComponent<EnemyDamageValues> ();
		Collider = gameObject.GetComponent<Collider2D> ();
		MainController = GameObject.Find ("Controller").GetComponent<GameController> ();
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerControls>();

		// Setting elements to their proper states
		InvokeRepeating ("Roam", 0, 2.0f);
		enemySounds.loop = false;
		TouchStop = false;
		MainController.EnemiesLeft++;
		DestroyEnemySequence += EnemyDeathSequence;
		
		if (AlienType == 1 || AlienType == 3) {
			AttackCollider = gameObject.transform.GetChild(0).GetComponent<Collider2D>();
			AttackCollider.enabled = false;
		}
		if (AlienType == 5) {
			TouchStop = true;
			BeamAnimation = SaucerRay.GetComponent<DeathRayAnimation>();
		}
		
	}
	
	// Update is called once per frame
	void Update () {

		// Finds the Player's transform and stores it in target
		Target = GameObject.FindGameObjectWithTag ("Player").transform;

		ChaseTarget();
		//TrackOtherEnemies();

		if (EnemyHealth <= 0) {
			if (DestroyEnemySequence != null) {
				DestroyEnemySequence();
			}
		}

		if (transform.position.y <= -1.5) {
			TouchStop = true;
		}
		else if (transform.position.y > -1.5) {
			TouchStop = false;
		}

	}

	void FixedUpdate() {
        Movement();
	}

	// Controls the actual movement of the object
	void Movement () {

		// Checks if it is allowed to chase the player
		if (CanChase == true || CanRoam == true) {

			// Pushes the enemy in a direction based upon which side the player is on
			if (ToTheRight == false) {
     			if (TouchStop && CanAttack) {
					Vector2 myVel = Rigidbody.velocity;
                	myVel.x = -MovementSpeed;
					Rigidbody.velocity = myVel;
				}
                if (anim.GetInteger("Near") != 0 && anim.GetInteger("Near") != 1)
                {

                }
                if ((anim.GetInteger("Near") == 1))
                {
                    anim.SetInteger("Near", 0);
                }
			}
			else if (ToTheRight == true) {
     			if (TouchStop && CanAttack) {
					Vector2 myVel = Rigidbody.velocity;
                	myVel.x = MovementSpeed;
					Rigidbody.velocity = myVel;
				}
                if (anim.GetInteger("Near") != 0 && anim.GetInteger("Near") != 1)
                {

                }
                if(anim.GetInteger("Near") == 1)
                {
                    anim.SetInteger("Near", 0);
                }
            }
		}
	}

	void ChaseTarget () {

		float Dist = Vector3.Distance(Target.position, transform.position);
		float DistX = Mathf.Abs(Target.position.x - transform.position.x);

		// Determines if the range of the player is close enough to be chased
		if (Dist <= ChaseRange && Dist > FireRange && AlienType != 5) {
			CanChase = true;
			CanRoam = false;
			ChaseDirection();
		}
		// Tells the player to attack if close enough
		else if (Dist <= FireRange && AlienType != 5) {
			CanChase = false;
			CanRoam = false;
			ChaseDirection();

			// This switch assigns the proper cooldown and attack phase for each enemy type.
			if (CanAttack) {
				if (TouchStop) {
					switch (AlienType) {
						// Roly Poly Alien
						case 1:
							CanAttack = false;
							anim.SetInteger("Near", 1);
							StartCoroutine(DashAttack());
							break;
						// Blob Alien
						case 2:
							CanAttack = false;
							anim.SetInteger("Near", 1);
							StartCoroutine(BombAttack());
							break;
						// Beefy Alien
						case 3:
							CanAttack = false;
							anim.SetInteger("Near", 1);
							StartCoroutine(JumpSmashAttack());
							//if (OnPunch != null) {
								//anim.SetInteger("Near", 1);
								//OnPunch();
							//}
							break;
						// Armored Alien
						case 4:
                            CanAttack = false;
                            anim.SetInteger("Near", 1);
                            StartCoroutine(GunAttack());
							break;
					}
				}
			}
		}
		// Saucer Chase Check
		else if (DistX <= FireRange && DistX > 0.5 && AlienType == 5) {
			CanChase = true;
			CanRoam = false;
			ChaseDirection();
			if (CanFireRay == true) {
				StartCoroutine(RayTime());
			}
		}
		// Saucer Attack Check
		else if (DistX <= FireRange && DistX <= 0.5 && AlienType == 5) {
			CanChase = false;
			Rigidbody.velocity = Vector2.zero;
			ChaseDirection();
		}
		// Roams out of range of chasing and attacking
		else {
			CanChase = false;
			CanRoam = true;
			if (AlienType == 5) {
				SaucerRay.SetActive(false);
			}
			CoolDownTimer = 0;
            enemySounds.Stop();
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
		if (CanRoam == true) {
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
				TouchStop = false;
			} else if (Vector3.Dot(toTarget, transform.right) > 0) {
				TouchStop = true;
			}

		}

	}

    // Instantiates a chosen projectile in the scene and propels it forward like a bullet
    private IEnumerator GunAttack () {

		Rigidbody.velocity = Vector2.zero;
        enemySounds.clip = machineGunRev;
        enemySounds.loop = false;
        enemySounds.Play();
        yield return new WaitForSeconds(0.6f);

        if (AlienType == 1) {
            enemySounds.clip = enemyPistol;
            enemySounds.loop = false;
        }
        if (AlienType == 4) {
            enemySounds.clip = enemyRapidFire;
            enemySounds.loop = true;
        }
        if (ToTheRight == true)
        {
            enemySounds.Play();
            GameObject Projectile = Instantiate(BulletObject, transform.position + new Vector3(1.0f, .10f, 0),
            Quaternion.identity) as GameObject;
            Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.right * ProjectileSpeed);
        }
        else if (ToTheRight == false)
        {
            if (soundPlaying == false)
            {
                enemySounds.Play();
            }
            soundPlaying = true;
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
        enemySounds.clip = blobSpit;
        enemySounds.loop = false;
        enemySounds.Play();
        if (ToTheRight == true) {
			GameObject Projectile = Instantiate (BombObject, transform.position + new Vector3(0.5f, 0.5f, 0), 
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.up * ProjectileSpeed);
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.right * ProjectileSpeed);
		}
		else if (ToTheRight == false) {
			GameObject Projectile = Instantiate (BombObject, transform.position + new Vector3(-0.5f, 0.5f, 0), 
			Quaternion.identity) as GameObject;
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.up * ProjectileSpeed);
			Projectile.GetComponent<Rigidbody2D>().AddForce(Vector3.left * ProjectileSpeed);
		}
        StartCoroutine(shootWait());
	}

	// Propels this enemy toward the player
	private IEnumerator JumpSmashAttack () {

		Rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(.2f);

        gameObject.GetComponent<Rigidbody2D>().AddForce
			(new Vector3 (Target.position.x - transform.position.x, 0, 0) * 43);
		GetComponent<Rigidbody2D>().AddForce(Vector3.up * 750);
        yield return new WaitForSeconds(.5f);
        anim.SetInteger("Near", 2);
        yield return new WaitForSeconds(.2f);
        AttackCollider.enabled = true;
		Rigidbody.gravityScale = 12;
        yield return new WaitForSeconds(0.4f);
		Rigidbody.gravityScale = 2;
		AttackCollider.enabled = false;
        yield return new WaitForSeconds(0.2f);
        enemySounds.clip = beefySmash;
        enemySounds.loop = false;
        enemySounds.Play();
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
		yield return new WaitForSeconds(0.7f);
		AttackCollider.enabled = true;
        enemySounds.clip = rolyPolyRoll;
        enemySounds.loop = false;
        enemySounds.Play();
        if (DashDirection == 1) {
			Rigidbody.AddForce(Vector2.right * ProjectileSpeed, ForceMode2D.Impulse);
		}
		else if (DashDirection == 2) {
			Rigidbody.AddForce(Vector2.left * ProjectileSpeed, ForceMode2D.Impulse);
		}
		yield return new WaitForSeconds(0.6f);
		anim.SetInteger("Near", 2);
		Rigidbody.velocity = Vector2.zero;
		Rigidbody.angularVelocity = 0.0f;
		AttackCollider.enabled = false;
		StartCoroutine(shootWait());
	}

	// Saucer attack cycle
	private IEnumerator RayTime () {
		
		CanFireRay = false;
        enemySounds.clip = laserCharge;
        enemySounds.loop = false;
        enemySounds.Play();
		yield return new WaitForSeconds(2.5f);
		SaucerRay.SetActive(true);
		BeamAnimation.PlayBeamAnim();
        enemySounds.clip = enemyLaser;
        enemySounds.loop = true;
        enemySounds.Play();
		yield return new WaitForSeconds(3);
		BeamAnimation.PlayRetractAnim();
		yield return new WaitForSeconds(0.2f);
        enemySounds.Stop();
        enemySounds.loop = false;
		SaucerRay.SetActive(false);
		CanFireRay = true;
	}

    private IEnumerator shootWait()
	{
    	// anim.SetInteger("Near", 0);
    	yield return new WaitForSeconds(CoolDown);
        CanAttack = true;
    }

	void EnemyDeathSequence () {

		DestroyEnemySequence = null;
		MainController.score += enemyValue;
		MainController.EnemiesLeft--;

		if (AlienType == 1)
		{
			enemySounds.clip = enemyDeath4;
			if (soundPlaying == false)
			{
				enemySounds.Play();
				soundPlaying = true;
			}
		}
        else if (AlienType == 2)
        {
            enemySounds.clip = enemyDeath5;
            if (soundPlaying == false)
            {
                enemySounds.Play();
                soundPlaying = true;
            }
        }
        else if (AlienType == 3)
        {
            enemySounds.clip = enemyDeath2;
            if (soundPlaying == false)
            {
                enemySounds.Play();
                soundPlaying = true;
            }
        }
        else if (AlienType == 4)
		{
			enemySounds.clip = enemyDeath1;
			if (soundPlaying == false)
			{
				enemySounds.Play();
				soundPlaying = true;
			}
		}
		else if (AlienType == 5)
		{
			enemySounds.clip = enemyDeath3;
			if (soundPlaying == false)
			{
				enemySounds.Play();
				soundPlaying = true;
			}
		}
		if (OnEnemyDeath != null)
		{
			OnEnemyDeath(AlienType);
		}

		if (AlienType == 5)
		{
			Destroy(gameObject, 1.0f);
		}
		else
		{
			Destroy(gameObject, 0.3f);
		}
	}

    void OnTriggerEnter2D(Collider2D collision) {

			// Takes damage from burst attacks
		if (collision.gameObject.name == "LightningBullet(Clone)") {
			EnemyHealth -= DamageValues.ElectricDamage;

            if (AlienType == 4)
            {
                CriticalTakeDamage(DamageValues.ElectricDamage);
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 4)
            {
                TakeDamage(DamageValues.ElectricDamage);
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }

        }
		if (collision.gameObject.name == "LightningBullet2(Clone)") {
			EnemyHealth -= DamageValues.ElectricDamage * 1.2f;
           
            if (AlienType == 4)
            {
                CriticalTakeDamage(DamageValues.ElectricDamage * 1.2f);
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 4)
            {
                TakeDamage(DamageValues.ElectricDamage * 1.2f);
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		if (collision.gameObject.name == "LightningBullet3(Clone)") {
			
            EnemyHealth -= DamageValues.ElectricDamage * 1.5f;

            if (AlienType == 4)
            {
                CriticalTakeDamage(DamageValues.ElectricDamage * 1.5f);
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 4)
            {
                TakeDamage(DamageValues.ElectricDamage * 1.5f);
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		if (collision.gameObject.name == "LightningBullet4(Clone)") {
			EnemyHealth -= DamageValues.ElectricDamage * 2.0f;
            CriticalTakeDamage(DamageValues.ElectricDamage * 2.0f);
            if (AlienType == 4)
            {
                CriticalTakeDamage(DamageValues.ElectricDamage * 2.0f);
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 4)
            {
                TakeDamage(DamageValues.ElectricDamage * 2.0f);
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		else if (collision.gameObject.tag == "Earth") {
			EnemyHealth -= DamageValues.EarthDamage;
            TakeDamage(DamageValues.EarthDamage);
        }
		else if (collision.gameObject.tag == "Speed") {
			EnemyHealth -= DamageValues.SpeedDamage;
            TakeDamage(DamageValues.SpeedDamage);
        }
		else if (collision.gameObject.name == "AnchorArms") {
			EnemyHealth -= DamageValues.JackedDamage;
            TakeDamage(DamageValues.JackedDamage);
        }
			// Takes damage from stream attacks
		else if (collision.gameObject.tag == "Fire") {
			InvokeRepeating("TakeFireDamage", 0, 0.5f);
            if (AlienType == 1)
            {
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 1)
            {
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		else if (collision.gameObject.tag == "Ice") {
			GameObject Projectile = Instantiate (IceBlock, transform.position + new Vector3(0, 0, 0), 
			Quaternion.identity) as GameObject;
            if (AlienType == 3)
            {
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 3)
            {
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		else if (collision.gameObject.tag == "IceBlock") {
			InvokeRepeating("TakeIceDamage", 0, 0.5f);
			TouchStop = false;
		}
		else if (collision.gameObject.tag == "Water") {
			InvokeRepeating("TakeWaterDamage", 0, 0.5f);
            if (AlienType == 2)
            {
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 2)
            {
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
        }
		else if (collision.gameObject.tag == "Wind") {
			InvokeRepeating("TakeWindDamage", 0, 0.5f);
			Rigidbody.AddForce(Vector3.up * 600);
            if (AlienType == 5)
            {
                enemySounds.clip = criticalDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (AlienType != 5)
            {
                enemySounds.clip = hitDamage;
                enemySounds.loop = false;
                enemySounds.Play();
            }
            if (Player.facingRight) {
				Rigidbody.AddForce(Vector3.right * 300);
			}
			else if (!Player.facingRight) {
				Rigidbody.AddForce(Vector3.left * 300);
			}
		}
	}

	void OnTriggerExit2D(Collider2D collider) {
		if (collider.gameObject.tag == "Fire") {
			CancelInvoke("TakeFireDamage");
		}
		else if (collider.gameObject.tag == "Water") {
			CancelInvoke("TakeWaterDamage");
		}
		else if (collider.gameObject.tag == "Wind") {
			CancelInvoke("TakeWindDamage");
		}
		else if (collider.gameObject.tag == "IceBlock") {
			CancelInvoke("TakeIceDamage");
			TouchStop = true;
		}
	}

	void TakeFireDamage() {
        if (this.name == "Roly Poly(Clone)")
        {
            EnemyHealth -= DamageValues.FireDamage;
            CriticalTakeDamage(DamageValues.FireDamage);
        }else{
            EnemyHealth -= DamageValues.FireDamage;
            TakeDamage(DamageValues.FireDamage);
        }
    }
	void TakeWaterDamage() {
        if (this.name == "GlobBomber(Clone)")
        {
            EnemyHealth -= DamageValues.WaterDamage;
            CriticalTakeDamage(DamageValues.WaterDamage);
        }
        else
        {
            EnemyHealth -= DamageValues.WaterDamage;
            TakeDamage(DamageValues.WaterDamage);
        }
    }
	void TakeWindDamage() {
        if (this.name == "Mothership(Clone)")
        {
            EnemyHealth -= DamageValues.WindDamage;
            CriticalTakeDamage(DamageValues.WindDamage);
        }
        else
        {
            EnemyHealth -= DamageValues.WindDamage;
            TakeDamage(DamageValues.WindDamage);
        }
    }
	void TakeIceDamage() {
        if (this.name == "Beefy Alien(Clone)")
        {
            EnemyHealth -= DamageValues.IceDamage;
            CriticalTakeDamage(DamageValues.IceDamage);
        }
        else
        {
            EnemyHealth -= DamageValues.IceDamage;
            TakeDamage(DamageValues.IceDamage);
        }
       
    }

	private void Roam () {
		if (CanRoam) {
			ChaseDirection();
		}
    }
  

    void TakeDamage(float amount)
    {
        FloatingTextController.CreateFloatingText(amount.ToString(), this.transform);
       // Debug.LogFormat("{0} was dealt {1} damage", gameObject.name, amount);
    }
    void CriticalTakeDamage(float amount)
    {
        CriticalFloatingTextController.CreateFloatingText(amount.ToString(), this.transform);
        // Debug.LogFormat("{0} was dealt {1} damage", gameObject.name, amount);
    }

}