﻿/**
 * Team: Fireflies
 * @author: Clayton Pierce, Sarah Alsmiller, Preston Turner, Justin Le, Sam Wood
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/**
 * Movement class. Should only be responsible for moving. Make an input wrapper class. 
**/
public class Movement : MonoBehaviour {
	
	//animator
	public Animator anim;
	
	private Vector3 movement;
	private Vector3 acceleration;

	private float xOffset = 0;
	private float zOffset = 0;

	private float accAmnt = 0.01f;
	private Rigidbody playerRigidbody;
	private float speed;
	float gravity = 0f;
	private bool isGrounded;
	private bool isDead;
	private Vector3 spawn;
	private Vector3 direction;
	private Quaternion? targetTurn = null;
	//private bool shooting;
	//private bool f, b, l, r;
	private float ccHeight;
	private float rotSpeed = 1f;
	public bool backwards = false;

	private int groundMask;
	
	//swaps back and forth between third person and perspective
	public bool inThirdPerson = true;
	
	public Vector3 getMove() {
		return movement;
	}
	
	private AudioSource footstepSound;

	private Rigidbody gun;

	private Dictionary<string, Vector3> savedLocalPositions = new Dictionary<string, Vector3>();
	private Dictionary<string, Quaternion> savedLocalRotations = new Dictionary<string, Quaternion>();

	// Use this for initialization
	void Awake () {
		anim = GetComponent<Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
		
		direction = Vector3.zero;
		speed = 5f;
		//shooting = false;
		isGrounded = true;
		spawn = transform.position;
		RagDoll (false);
		ccHeight = GetComponent<CapsuleCollider>().height;
		groundMask = LayerMask.GetMask ("Ground");

		//Physics.IgnoreLayerCollision(LayerMask.GetMask ("Character"), LayerMask.GetMask ("Floor"), true);
	}

	public void SetOffset(float newX, float newZ) {
		xOffset = newX - transform.position.x;
		zOffset = newZ - transform.position.z;
	}
	
	public void SetRagDoll(bool rag) {
		RagDoll (rag);
	}

	public void setTargetTurn(Quaternion target) {
		targetTurn = target;
		rotSpeed = Quaternion.Angle (transform.rotation, target) / 30f;
		if (rotSpeed < 2f) {
			rotSpeed = 2f;
		}
	}

	public void setRotSpeed(float speed) {
		rotSpeed = speed;
	}
	
	public bool GetDead() {
		return isDead;
	}
	
	public void Kill() {
		GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		setDead (true);
	}
	
	public void Revive() {
		GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		setDead (false);
		Entity e = GetComponent<Entity> ();
		e.currentHealth = e.startingHealth;
		e.isDead = false;
	}
	
	public void setDead(bool dead) {
		isDead = dead;
		RagDoll (isDead);
	}

	private bool save = false;

	void RagDoll(bool rag) {
		Transform[] transforms = GetComponentsInChildren<Transform> ();
		Vector3 lp = new Vector3();
		if (!save) {
			foreach (Transform body in transforms) {
				if (body.name == "M4MB") {
					gun = body.GetComponent<Rigidbody>();
				}
				//Debug.Log (body.name);
				if (!savedLocalPositions.ContainsKey(body.name)) {
					savedLocalPositions.Add (body.name, body.transform.localPosition);
					savedLocalRotations.Add (body.name, body.transform.localRotation);
				}
			}
			save = true;
		}
		if (rag) {
			anim.enabled = false;
			isDead = true;
		} else {
			anim.enabled = true;
			isDead = false;
		}
		Rigidbody[] bodies = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody body in bodies) {
			body.isKinematic = !rag;
		}
		GetComponent<Rigidbody> ().isKinematic = rag;
		foreach (Collider c in GetComponentsInChildren<Collider> ()) {
			c.enabled = rag;
		}
		GetComponent<CapsuleCollider>().enabled = !rag;
		//GetComponent<Rigidbody> ().isKinematic = rag;
	}
	
	void FootStep() {
		if (footstepSound && movement.magnitude >= 0.6f) {
			footstepSound.Play ();
		}
	}
	
	public void Move(float h, float v) {

		acceleration.Set (h, movement.y, v);
		acceleration = acceleration.normalized;
		
		if (inThirdPerson) {
			acceleration = transform.rotation * acceleration;
		}
		float slide = 10f;
		gravity -= Time.deltaTime / 3.5f;
		RaycastHit hitInfo = new RaycastHit();
		Vector3 pos = playerRigidbody.position;
		pos.y += 1f;
		bool overSnow = false;
		
		//THIS SHOULD ALL BE A SEPERATE CLASS
		
		if (Physics.Raycast (new Ray (pos, Vector3.down), out hitInfo, 1.1f, groundMask)) {
			if (hitInfo.collider.CompareTag ("Ice")) {
				slide = 1f;
			} else if (hitInfo.collider.CompareTag("Snow")) {
				overSnow = true;
			}
			//AudioSource[] sources = hitInfo.transform.gameObject.GetComponents<AudioSource> ();
			//if (sources.Length > 0) {
			//	footstepSound = sources [0];
			//}
			gravity = 0;
		}
		
		/*if (overSnow && movement.magnitude > 0.1) {
			ParticleSystem part = GetComponentInChildren<ParticleSystem> ();
			part.Play ();
		}*/
		
		movement = Vector3.Lerp(movement, acceleration, Time.deltaTime * slide);
		movement.y = gravity / Time.deltaTime;

		LerpRotation ();

		float xMove = xOffset / 4f;
		xOffset -= xMove;
		float zMove = zOffset / 4f;
		zOffset -= zMove;
		if (Mathf.Abs (xOffset) < .1f) {
			xOffset = 0f;
		}
		if (Mathf.Abs (zOffset) < .1f) {
			zOffset = 0f;
		}

		playerRigidbody.MovePosition (transform.position + (movement * speed * Time.deltaTime) + new Vector3(xMove, 0, zMove));
		Vector3 vl = playerRigidbody.velocity;
		float newSpeed = 1f - (vl.magnitude);
		if (newSpeed < 0.1f) {
			newSpeed = 0.1f;
		}
		anim.speed = newSpeed;
	}

	private void LerpRotation() {
		if (targetTurn != null) {
			transform.rotation = Quaternion.Lerp(transform.rotation, (Quaternion)targetTurn, rotSpeed * Time.deltaTime);
		}
	}
	
	//respawns, won't be needed in final version
	void Respawn() {
		transform.position = spawn;
	}
	
}