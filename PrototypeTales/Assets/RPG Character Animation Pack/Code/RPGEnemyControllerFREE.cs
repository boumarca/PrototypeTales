﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.AI;

public enum EnemyType
{
    Aggressive,
    Defensive
}

public class RPGEnemyControllerFREE : MonoBehaviour 
{
	#region Variables

	//Components
	Rigidbody rb;
	protected Animator animator;
	public GameObject target;
	private Vector3 targetDashDirection;
	public Camera sceneCamera;

	//jumping variables
	public float gravity = -9.8f;
	bool canJump;
	bool isJumping = false;
	bool isGrounded;
	public float jumpSpeed = 12;
	public float doublejumpSpeed = 12;
	bool doublejumping = true;
	bool canDoubleJump = false;
	bool isDoubleJumping = false;
	bool doublejumped = false;
	bool isFalling;
	bool startFall;
	float fallingVelocity = -1f;

	// Used for continuing momentum while in air
	public float inAirSpeed = 8f;
	float maxVelocity = 2f;
	float minVelocity = -2f;

	//rolling variables
	public float rollSpeed = 8;
	bool isRolling = false;
	public float rollduration;

	//movement variables
	bool canMove = true;
	public float walkSpeed = 1.35f;
	float moveSpeed;
	public float runSpeed = 6f;
	float rotationSpeed = 40f;
  
	float x;
	float z;
	float dv;
	float dh;
	Vector3 inputVec;
	Vector3 newVelocity;

	//Weapon and Shield
	private Weapon weapon;
	int rightWeapon = 0;
	int leftWeapon = 0;
	bool isRelax = false;

	//isStrafing/action variables
	bool canAction = true;
	bool isStrafing = false;
	public bool isDead = false;
	public bool isBlocking = false;
	public float knockbackMultiplier = 1f;
	bool isKnockback;

    //My stuff
    [Header("My stuff")]
    int comboCount;
    bool hasAttacked;
    AttackType attackType;
    Stats stats;

    [SerializeField]
    int attackChargeAmount = 5;
    [SerializeField]
    int maxCombo = 3;
    [SerializeField]
    float ComboCooldownTime = 0.5f;
    [SerializeField]
    float PunchAttackDelay = 0.3f;
    [SerializeField]
    float KickAttackDelay = 0.5f;
    [SerializeField]
    float ComboWindowInput = 0.3f;
    [SerializeField]
    Collider PunchHitbox;
    [SerializeField]
    Collider KickHitbox;
    [SerializeField]
    GameObject shield;
    [SerializeField]
    Familiar[] familiars;
    [SerializeField]
    EnemyType enemyType;
    [SerializeField]
    NavMeshAgent agent;
    [SerializeField]
    Transform player;
    [SerializeField]
    SphereCollider playerDetection;
    #endregion

    #region Initialization

    void Start() 
	{
		//set the animator component
		animator = GetComponentInChildren<Animator>();
		rb = GetComponent<Rigidbody>();
        stats = GetComponent<Stats>();
        stats.Die += () => StartCoroutine(_Death()); 
	}

	#endregion

	#region UpdateAndInput
	float timeBetweenAction = 0.1f;
	float timeSinceLastAction = 0f;
	void Update()
	{
		timeSinceLastAction += Time.deltaTime;
		if (timeSinceLastAction < timeBetweenAction)
			return;

		timeSinceLastAction = 0;
		//make sure there is animator on character
		if(animator)
		{
			bool isNearPlayer = false;
            Collider[] overlaps = Physics.OverlapSphere(transform.position, playerDetection.radius, 1 << LayerMask.NameToLayer("Player"));
			for (int i = 0; i < overlaps.Length; i++)
			{
				if (overlaps[i].GetComponent<RPGCharacterControllerFREE>() != null)
				{
					moveSpeed = 0;
					isNearPlayer = true;
					//If near player
					if (canAction && !isBlocking)
					{
						Vector3 direction = (player.position - transform.position).normalized;
						transform.rotation = Quaternion.LookRotation(direction);

						int choice = Random.Range(0, 100);
						if (choice < 15)
							Attack();
						else if (choice < 25)
							AttackKick();

					}
				}				
			}

			//check if character can move
			if (!isNearPlayer && canMove && !isBlocking && !isDead)
			{
				//moveSpeed = UpdateMovement(); 
				if (enemyType == EnemyType.Aggressive)
				{
					moveSpeed = 1;
					Debug.Log("Move towards player");
					agent.SetDestination(player.position);
				}
			}



			if (canAction && isGrounded && moveSpeed == 0)
            {
                Block(Input.GetButton("Shield"));
            }            
            
			if(canAction && isGrounded && !isBlocking)
			{
				Attack();
			}

			if(Input.GetButtonDown("CastL") && canAction && isGrounded && !isBlocking && !isStrafing)
			{
				AttackKick();
			}
			if(Input.GetButtonDown("CastR") && canAction && isGrounded && !isBlocking && !isStrafing)
			{
				AttackKick();
			}
			//if strafing
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetAxisRaw("TargetBlock") > .1 && canAction)
			{  
				isStrafing = true;
				animator.SetBool("Strafing", true);
				if(Input.GetButtonDown("CastL") && canAction && isGrounded && !isBlocking)
				{
					CastAttack(1);
				}
				if(Input.GetButtonDown("CastR") && canAction && isGrounded && !isBlocking)
				{
					CastAttack(2);
				}
			}
			else
			{
				isStrafing = false;
				animator.SetBool("Strafing", false);
			}
		}
		else
		{
			Debug.Log("ERROR: There is no animator for character.");
		}
	}
	
	#endregion

	#region Fixed/Late Updates
	
	void FixedUpdate()
	{
		/*CheckForGrounded();
		//apply gravity force
		rb.AddForce(0, gravity, 0, ForceMode.Acceleration);
		AirControl();
		//check if character can move
		if(canMove && !isBlocking && !isDead)
		{
            //moveSpeed = UpdateMovement(); 
            if (enemyType == EnemyType.Aggressive)
            {
                moveSpeed = 1;
                agent.SetDestination(player.position);
            } 
		}
		//check if falling
		if(rb.velocity.y < fallingVelocity)
		{
			isFalling = true;
			animator.SetInteger("Jumping", 2);
			canJump = false;
		} 
		else
		{
			isFalling = false;
		}*/
	}

	//get velocity of rigid body and pass the value to the animator to control the animations
	void LateUpdate()
	{
		//Get local velocity of charcter
		float velocityXel = transform.InverseTransformDirection(rb.velocity).x;
		float velocityZel = transform.InverseTransformDirection(rb.velocity).z;
		//Update animator with movement values
		animator.SetFloat("Velocity X", velocityXel / runSpeed);
		animator.SetFloat("Velocity Z", velocityZel / runSpeed);
		//if character is alive and can move, set our animator
		if(!isDead && canMove)
		{
			if(moveSpeed > 0)
			{
				animator.SetBool("Moving", true);
			}
			else
			{
				animator.SetBool("Moving", false);
			}
		}
	}
	
	#endregion

	#region UpdateMovement

	void CameraRelativeMovement(){
		float inputDashVertical = Input.GetAxisRaw("DashVertical");
		float inputDashHorizontal = Input.GetAxisRaw("DashHorizontal");
		float inputHorizontal = Input.GetAxisRaw("Horizontal");
		float inputVertical = Input.GetAxisRaw("Vertical");

		//converts control input vectors into camera facing vectors
		Transform cameraTransform = sceneCamera.transform;
		//Forward vector relative to the camera along the x-z plane   
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		//Right vector relative to the camera always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		//directional inputs
		dv = inputDashVertical;
		dh = inputDashHorizontal;
		if(!isRolling)
		{
			targetDashDirection = dh * right + dv * -forward;
		}
		x = inputHorizontal;
		z = inputVertical;
		inputVec = x * right + z * forward;
	}

	//rotate character towards direction moved
	void RotateTowardsMovementDir()
	{
		if(inputVec != Vector3.zero && !isStrafing && !isRolling && !isBlocking)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVec), Time.deltaTime * rotationSpeed);
		}
	}

	float UpdateMovement()
	{
		//CameraRelativeMovement();  
		Vector3 motion = inputVec;
		if(isGrounded)
		{
			//reduce input for diagonal movement
			if(motion.magnitude > 1)
			{
				motion.Normalize();
			}
			if(canMove && !isBlocking)
			{
				//set speed by walking / running
				if(isStrafing)
				{
					newVelocity = motion * walkSpeed;
				}
				else
				{
					newVelocity = motion * runSpeed;
				}
				//if rolling use rolling speed and direction
				if(isRolling)
				{
					//force the dash movement to 1
					targetDashDirection.Normalize();
					newVelocity = rollSpeed * targetDashDirection;
				}
			}
		}
		else
		{
			//if we are falling use momentum
			newVelocity = rb.velocity;
		}
		if(!isStrafing || !canMove)
		{
			RotateTowardsMovementDir();
		}
		if(isStrafing && !isRelax)
		{
			//make character point at target
			Quaternion targetRotation;
			Vector3 targetPos = target.transform.position;
      		targetRotation = Quaternion.LookRotation(targetPos - new Vector3(transform.position.x,0,transform.position.z));
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetRotation.eulerAngles.y,(rotationSpeed * Time.deltaTime) * rotationSpeed);
		}
		newVelocity.y = rb.velocity.y;
		rb.velocity = newVelocity;
		//return a movement value for the animator
		return inputVec.magnitude;        
	}

	#endregion

	#region Jumping

	//checks if character is within a certain distance from the ground, and markes it IsGrounded
	void CheckForGrounded()
	{
		isGrounded = true;
		/*float distanceToGround;
		float threshold = .45f;
		RaycastHit hit;
		Vector3 offset = new Vector3(0,.4f,0);
		if(Physics.Raycast((transform.position + offset), -Vector3.up, out hit, 100f))
		{
			distanceToGround = hit.distance;
			if(distanceToGround < threshold)
			{
				isGrounded = true;
				canJump = true;
				startFall = false;
				doublejumped = false;
				canDoubleJump = false;
				isFalling = false;
				if(!isJumping) 
				{
					animator.SetInteger("Jumping", 0);
				}
			}
			else
			{
				isGrounded = false;
			}
		}*/
	}

	void Jumping()
	{
		if(isGrounded)
		{
			if(canJump && Input.GetButtonDown("Jump"))
			{
				StartCoroutine(_Jump());
			}
		}
		else
		{    
			canDoubleJump = true;
			canJump = false;
			if(isFalling)
			{
				//set the animation back to falling
				animator.SetInteger("Jumping", 2);
				//prevent from going into land animation while in air
				if(!startFall)
				{
					animator.SetTrigger("JumpTrigger");
					startFall = true;
				}
			}
			if(canDoubleJump && doublejumping && Input.GetButtonDown("Jump") && !doublejumped && isFalling)
			{
				// Apply the current movement to launch velocity
				rb.velocity += doublejumpSpeed * Vector3.up;
				animator.SetInteger("Jumping", 3);
				doublejumped = true;
			}
		}
	}

	IEnumerator _Jump()
	{
		isJumping = true;
		animator.SetInteger("Jumping", 1);
		animator.SetTrigger("JumpTrigger");
		// Apply the current movement to launch velocity
		rb.velocity += jumpSpeed * Vector3.up;
		canJump = false;
		yield return new WaitForSeconds(.5f);
		isJumping = false;
	}

	void AirControl()
	{
		if(!isGrounded)
		{
			//CameraRelativeMovement();
			Vector3 motion = inputVec;
			motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1) ? 0.7f:1;
			rb.AddForce(motion * inAirSpeed, ForceMode.Acceleration);
			//limit the amount of velocity we can achieve
			float velocityX = 0;
			float velocityZ = 0;
			if(rb.velocity.x > maxVelocity)
			{
				velocityX = GetComponent<Rigidbody>().velocity.x - maxVelocity;
				if(velocityX < 0)
				{
					velocityX = 0;
				}
				rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
			}
			if(rb.velocity.x < minVelocity)
			{
				velocityX = rb.velocity.x - minVelocity;
				if(velocityX > 0)
				{
					velocityX = 0;
				}
				rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
			}
			if(rb.velocity.z > maxVelocity)
			{
				velocityZ = rb.velocity.z - maxVelocity;
				if(velocityZ < 0)
				{
					velocityZ = 0;
				}
				rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
			}
			if(rb.velocity.z < minVelocity)
			{
				velocityZ = rb.velocity.z - minVelocity;
				if(velocityZ > 0)
				{
					velocityZ = 0;
				}
				rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
			}
		}
	}

    #endregion

    #region MiscMethods

    void Block(bool block)
    {
        shield.SetActive(block);
        isBlocking = block;
        stats.Defend(block);
    }

	//0 = No side
	//1 = Left
	//2 = Right
	//3 = Dual
	void Attack()
	{
        int attackSide = comboCount % 2 + 1;
		if(canAction)
		{
			if(weapon == Weapon.UNARMED)
			{
				int maxAttacks = 3;
				int attackNumber = 0;
				if(attackSide == 1 || attackSide == 3)
				{
					attackNumber = Random.Range(3, maxAttacks);
				}
				else if(attackSide == 2)
				{
					attackNumber = Random.Range(6, maxAttacks + 3);
				}
				
				if(attackSide != 3)
				{
					animator.SetTrigger("Attack" + (attackNumber).ToString() + "Trigger");
					//if(leftWeapon == 12 || leftWeapon == 14 || rightWeapon == 13 || rightWeapon == 15)
					//{
					//	StartCoroutine(_LockMovementAndAttack(0, .75f));
					//} 
					//else
					//{
                        float delay = PunchAttackDelay;
                        comboCount++;
                        hasAttacked = true;
                    attackType = AttackType.Punch;
                        if (comboCount == maxCombo)
                        {
                            Debug.Log("Cooldown");
                            delay += ComboCooldownTime;
                            comboCount = 0;
                        }
						StartCoroutine(_LockMovementAndAttack(0, delay));
					//}
				}
				else
				{
					animator.SetTrigger("AttackDual" + (attackNumber).ToString() + "Trigger");
					StartCoroutine(_LockMovementAndAttack(0, .75f));
				}
				
			}
			//2 handed weapons
			else
			{
				if(isGrounded)
				{
					animator.SetTrigger("Attack" + (6).ToString() + "Trigger");
					StartCoroutine(_LockMovementAndAttack(0, .85f));
				}
			}
		}
	}

	void AttackKick()
	{
        int kickSide = comboCount % 2 + 1;		
		if(kickSide == 1)
		{
			animator.SetTrigger("AttackKick1Trigger");
		}
		else
		{
			animator.SetTrigger("AttackKick2Trigger");
		}
        float delay = KickAttackDelay;
        comboCount++;
        hasAttacked = true;
        attackType = AttackType.Kick;
        if (comboCount == maxCombo)
        {
            Debug.Log("Cooldown");
            delay += ComboCooldownTime;
            comboCount = 0;
        }
        StartCoroutine(_LockMovementAndAttack(0, delay));
		
	}

	//0 = No side
	//1 = Left
	//2 = Right
	//3 = Dual
	void CastAttack(int attackSide)
	{
		if(weapon == Weapon.UNARMED)
		{
			int maxAttacks = 3;
			if(attackSide == 1)
			{
				int attackNumber = Random.Range(0, maxAttacks);
				if(isGrounded)
				{
					animator.SetTrigger("CastAttack" + (attackNumber + 1).ToString() + "Trigger");
					StartCoroutine(_LockMovementAndAttack(0, .8f));
				}
			}
			if(attackSide == 2)
			{
				int attackNumber = Random.Range(3, maxAttacks + 3);
				if(isGrounded)
				{
					animator.SetTrigger("CastAttack" + (attackNumber + 1).ToString() + "Trigger");
					StartCoroutine(_LockMovementAndAttack(0, .8f));
				}
			}
			if(attackSide == 3)
			{
				int attackNumber = Random.Range(0, maxAttacks);
				if(isGrounded)
				{
					animator.SetTrigger("CastDualAttack" + (attackNumber + 1).ToString() + "Trigger");
					StartCoroutine(_LockMovementAndAttack(0, 1f));
				}
			}
		} 
	}

    public void BreakGuard()
    {
        isBlocking = false;
        GetHit();
    }

	public void GetHit()
	{
		int hits = 5;
		int hitNumber = Random.Range(0, hits);
		animator.SetTrigger("GetHit" + (hitNumber + 1).ToString()+ "Trigger");
		StartCoroutine(_LockMovementAndAttack(.1f, .4f));
		//apply directional knockback force
		if(hitNumber <= 1)
		{
			StartCoroutine(_Knockback(-transform.forward, 8, 4));
		} 
		else if(hitNumber == 2)
		{
			StartCoroutine(_Knockback(transform.forward, 8, 4));
		}
		else if(hitNumber == 3)
		{
			StartCoroutine(_Knockback(transform.right, 8, 4));
		}
		else if(hitNumber == 4)
		{
			StartCoroutine(_Knockback(-transform.right, 8, 4));
		}
	}

	IEnumerator _Knockback(Vector3 knockDirection, int knockBackAmount, int variableAmount)
	{
		isKnockback = true;
		StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount));
		yield return new WaitForSeconds(.1f);
		isKnockback = false;
	}

	IEnumerator _KnockbackForce(Vector3 knockDirection, int knockBackAmount, int variableAmount)
	{
		while(isKnockback)
		{
			rb.AddForce(knockDirection * ((knockBackAmount + Random.Range(-variableAmount, variableAmount)) * (knockbackMultiplier * 10)), ForceMode.Impulse);
			yield return null;
		}
	}

	IEnumerator _Death()
	{
		animator.SetTrigger("Death1Trigger");
		StartCoroutine(_LockMovementAndAttack(.1f, 1.5f));
		isDead = true;
		animator.SetBool("Moving", false);
		inputVec = new Vector3(0, 0, 0);
		yield return null;
	}

	IEnumerator _Revive()
	{
		animator.SetTrigger("Revive1Trigger");
		isDead = false;
		yield return null;
	}

	//Animation Events
	void Hit()
	{
        Collider c = null;
        if (attackType == AttackType.Punch)
            c = PunchHitbox;
        else if (attackType == AttackType.Kick)
            c = KickHitbox;

        if (c != null)
        {
            Collider[] overlaps = Physics.OverlapBox(c.bounds.center, c.bounds.extents, c.transform.rotation, 1 << LayerMask.NameToLayer("Player"));
            for (int i = 0; i < overlaps.Length; i++)
            {
                RPGCharacterControllerFREE controller = overlaps[i].GetComponent<RPGCharacterControllerFREE>();
                if (controller != null && !controller.isDead)
                {    
                    if(!controller.isBlocking)
                        controller.BreakGuard();

                    Stats other = overlaps[i].GetComponent<Stats>();
                    if (attackType == AttackType.Kick)
                        stats.BuffAtk *= 1.25f;

                    int damage = stats.Attack(other);
                    other.Damage(damage);

                    bool breakGuard = other.BreakGuard(1);

                    if (breakGuard)
                        controller.GetHit();


					if (attackType == AttackType.Kick)
                        stats.BuffAtk /= 1.25f;
                }
            } 
        }
	}

	void FootL()
	{

	}

	void FootR()
	{

	}

	void Jump()
	{

	}

	void Land()
	{

	}

	#endregion

	#region Rolling

	void Rolling()
	{
		if(!isRolling && isGrounded)
		{
			if(Input.GetAxis("DashVertical") > .5 || Input.GetAxis("DashVertical") < -.5 || Input.GetAxis("DashHorizontal") > .5 || Input.GetAxis("DashHorizontal") < -.5)
			{
				StartCoroutine(_DirectionalRoll(Input.GetAxis("DashVertical"), Input.GetAxis("DashHorizontal")));
			}
		}
	}

	public IEnumerator _DirectionalRoll(float x, float v)
	{
		//check which way the dash is pressed relative to the character facing
		float angle = Vector3.Angle(targetDashDirection,-transform.forward);
		float sign = Mathf.Sign(Vector3.Dot(transform.up,Vector3.Cross(targetDashDirection,transform.forward)));
		// angle in [-179,180]
		float signed_angle = angle * sign;
		//angle in 0-360
		float angle360 = (signed_angle + 180) % 360;
		//deternime the animation to play based on the angle
		if(angle360 > 315 || angle360 < 45)
		{
			StartCoroutine(_Roll(1));
		}
		if(angle360 > 45 && angle360 < 135)
		{
			StartCoroutine(_Roll(2));
		}
		if(angle360 > 135 && angle360 < 225)
		{
			StartCoroutine(_Roll(3));
		}
		if(angle360 > 225 && angle360 < 315)
		{
			StartCoroutine(_Roll(4));
		}
		yield return null;
	}

	IEnumerator _Roll(int rollNumber)
	{
		if(rollNumber == 1)
		{
			animator.SetTrigger("RollForwardTrigger");
		}
		if(rollNumber == 2)
		{
			animator.SetTrigger("RollRightTrigger");
		}
		if(rollNumber == 3)
		{
			animator.SetTrigger("RollBackwardTrigger");
		}
		if(rollNumber == 4)
		{
			animator.SetTrigger("RollLeftTrigger");
		}
		isRolling = true;
		yield return new WaitForSeconds(rollduration);
		isRolling = false;
	}

	#endregion
	
	#region _Coroutines

	//method to keep character from moveing while attacking, etc
	public IEnumerator _LockMovementAndAttack(float delayTime, float lockTime)
	{
		yield return new WaitForSeconds(delayTime);
		canAction = false;
		canMove = false;
		animator.SetBool("Moving", false);
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		inputVec = new Vector3(0, 0, 0);
		animator.applyRootMotion = true;
		yield return new WaitForSeconds(lockTime);
        hasAttacked = false;
		canAction = true;
		canMove = true;
		animator.applyRootMotion = false;
        if (comboCount != 0)
        {
            yield return new WaitForSeconds(ComboWindowInput);
            if (!hasAttacked)
            {
                attackType = AttackType.None;
                comboCount = 0;
            }
        }
	}
	
	#endregion

	#region GUI

	void OnGUI()
	{
        return;
		if(!isDead)
		{
			if(canAction && !isRelax)
			{
				if(isGrounded)
				{
					if(!isBlocking)
					{
						if(!isBlocking)
						{
							if(GUI.Button(new Rect(25, 15, 100, 30), "Roll Forward"))
							{
								targetDashDirection = transform.forward;
								StartCoroutine(_Roll(1));
							}
							if(GUI.Button(new Rect(130, 15, 100, 30), "Roll Backward"))
							{
								targetDashDirection = -transform.forward;
								StartCoroutine(_Roll(3));
							}
							if(GUI.Button(new Rect(25, 45, 100, 30), "Roll Left"))
							{
								targetDashDirection = -transform.right;
								StartCoroutine(_Roll(4));
							}
							if(GUI.Button(new Rect(130, 45, 100, 30), "Roll Right"))
							{
								targetDashDirection = transform.right;
								StartCoroutine(_Roll(2));
							}
							//ATTACK LEFT
							if(GUI.Button(new Rect(25, 85, 100, 30), "Attack L"))
							{
								Attack();
							}
							//ATTACK RIGHT
							if(GUI.Button(new Rect(130, 85, 100, 30), "Attack R"))
							{
								Attack();
							}
							if(weapon == Weapon.UNARMED) 
							{
								if(GUI.Button (new Rect (25, 115, 100, 30), "Left Kick")) 
								{
									AttackKick ();
								}
								if(GUI.Button (new Rect (130, 115, 100, 30), "Right Kick")) 
								{
									AttackKick ();
								}
							}
							if(GUI.Button(new Rect(30, 240, 100, 30), "Get Hit"))
							{
								GetHit();
							}
						}
					}
				}
				if(canJump || canDoubleJump)
				{
					if(isGrounded)
					{
						if(GUI.Button(new Rect(25, 165, 100, 30), "Jump"))
						{
							if(canJump && isGrounded)
							{
								StartCoroutine(_Jump());
							}
						}
					} 
					else
					{
						if(GUI.Button(new Rect(25, 165, 100, 30), "Double Jump"))
						{
							if(canDoubleJump && !isDoubleJumping)
							{
								StartCoroutine(_Jump());
							}
						}
					}
				}
				if(!isBlocking && isGrounded)
				{
					if(GUI.Button(new Rect(30, 270, 100, 30), "Death"))
					{
						StartCoroutine(_Death());
					}
				}
			}
		}
		if(isDead)
		{
			if(GUI.Button(new Rect(30, 270, 100, 30), "Revive"))
			{
				StartCoroutine(_Revive());
			}
		}
	}

	#endregion
}