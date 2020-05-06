using UnityEngine;
using System.Collections;

public class PlayerMovements : MonoBehaviour
{
	public float Speed { get { return GetComponent<Rigidbody>().velocity.magnitude; } }
	
	const float FORCE_APPLIED = 500F;
	const float SPEEDLOSS_THRERSHHERLD = 1.8f;
	public float sensitivityX;
	public Vector3 startingVelocity;
	public float angle;
	public bool startFreeze = true;
	private float lastSpeed;
	private float angleChange = 0.0f;

	void Start()
	{
		lastSpeed = 0;
		sensitivityX = Options.Sensitivity;
	}
	
	public void Go()
	{
		GetComponent<Rigidbody>().velocity = transform.rotation * startingVelocity;
		lastSpeed = startingVelocity.magnitude;
	}
	
	public void Reload()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		Physics.gravity = new Vector3 (0, -40, 0);
	}
	
	void Reset()
	{
		Start();		
	}
	
	void Update()
	{
		if (LevelStart.started && !LevelState.Dead)
		{
			float moment;
			
#if UNITY_ANDROID || UNITY_IPHONE
			foreach (Touch touch in Input.touches)
			{
				moment = touch.deltaPosition.x * 60.0f / Screen.dpi;
				
				if (touch.phase == TouchPhase.Moved && Time.timeScale > 0.0f)
				{
#else
			if(InputManager.dragging && Time.timeScale > 0.0f)
			{
				{
							
					moment = InputManager.frameDifference.x * 0.25f * (1024.0f / (float)Screen.width);
#endif
					angle = moment * sensitivityX;
					
					angleChange += angle;
					
					Quaternion a = Quaternion.Euler(0, angle, 0);
						
					Vector3 outvec = a * GetComponent<Rigidbody>().velocity;
					
					outvec.y = GetComponent<Rigidbody>().velocity.y;
					GetComponent<Rigidbody>().velocity = outvec;
					
					transform.rotation *= a;
					
				}
			}
		}
	}
	
	void FixedUpdate()
	{
		if(!LevelState.Dead && !LevelState.HasFinished && !GetComponent<Rigidbody>().isKinematic)
		{
			float speedloss = Mathf.Clamp01(((90.0f - Mathf.Abs(angleChange)) / 90.0f) + (SPEEDLOSS_THRERSHHERLD * Time.deltaTime));
			
			angleChange = 0.0f;
			
			Vector3 vel = GetComponent<Rigidbody>().velocity;
			//vel *= speedloss;
			vel.y = GetComponent<Rigidbody>().velocity.y;
			GetComponent<Rigidbody>().velocity = vel;	

		}
	}
	
	public float SpeedLost()
	{
		float score = lastSpeed - GetComponent<Rigidbody>().velocity.magnitude;
		lastSpeed = GetComponent<Rigidbody>().velocity.magnitude;
	
		//return (score > 0) ? score : 0;
		return -score;
	}
	
	/*void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Planet")
		{
			//SoundManager.Play("explosion");
		}
		
		if (collision.gameObject.tag != "Flipper")
		{
			float impactPower = collision.relativeVelocity.magnitude;
			
			Vector3 direction = collision.contacts[0].point - transform.position;
			direction.Normalize();
			
			impactPower = -Vector3.Dot(collision.relativeVelocity, direction);
			
			Debug.Log(impactPower);
			
			if(impactPower > 30)
				SoundManager.Play("smash", impactPower);
		}
	}*/
}