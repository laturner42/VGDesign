﻿using UnityEngine;
using System.Collections;

public class PlayerInputManager : MonoBehaviour {

	//I feel like this should be in a seperate networking manager tbh
	private float h;
	private float v;
	private float oldH = 0;
	private float oldV = 0;
	private bool needsUpdate = false;

	private Movement move;
	private Animator anim;
	private bool shooting = false;
	private Vector3 movement;

	// Use this for initialization
	void Start () {
		move = gameObject.GetComponent<Movement>();
		anim = move.anim;
		movement = move.getMove ();
	}
	
	void Update () {
		h = Input.GetAxisRaw ("Horizontal");
		v = Input.GetAxisRaw ("Vertical");
		if (h != oldH || v != oldV) {
			oldH = h;
			oldV = v;
			needsUpdate = true;
		}
		
		if (Input.GetKey ("space")) {
			shooting = true;
		} else {
			shooting = false;
		}

		//move into player health script
		if (Input.GetKey ("k")) {
			move.SetRagDoll (true);
		}
		
		if (shooting) {
			h = 0;
			v = 0;
		}

		//this should be somewhere else
		/*CapsuleCollider cc = GetComponent<CapsuleCollider> ();
		if (shooting) {
			h = 0;
			v = 0;
			cc.height = ccHeight * 0.8f;
		} else {
			cc.height = ccHeight;
		}*/
		
		move.Move (h, v);
		move.Turning ();

		Vector2 spdir = DetermineDir (h, v);
		anim.SetFloat ("Speed", spdir.x);
		anim.SetFloat ("Direction", spdir.y, .25f, Time.deltaTime);
		anim.SetBool ("Shooting", shooting);
	}

	//Determines what to feed the player animator input based on Preston's crazy math I guess
	Vector2 DetermineDir(float h, float v) {
		float rot = transform.eulerAngles.y;
		float sp = 0;
		float dir = 0; 
		
		if ( h != 0 || v != 0) {
			float angle = rot;
			float movAngle = Vector3.Angle (movement, new Vector3 (0, movement.y, 1));
			//float movAngle = Vector3.Angle (new Vector3(h, 0, v), new Vector3 (0, 0, 1));
			
			if (movement.x < -0.1f) {
				movAngle = 360f - movAngle;
			}
			
			if (angle - movAngle > 180) {
				angle -= 360f;
			} else if (movAngle - angle > 180) {
				angle += 360f;
			}
			
			sp =  Mathf.Abs( angle - movAngle ) <= 90f ? 1 : -1;
			
			
			if (movAngle < rot - 180f) {
				movAngle += 360;
			}
			angle = rot - movAngle;
			dir = 1;
			
			if (angle < 0f) {
				angle = -angle;
			} else {
				dir = -1;
			}
			
			if (angle > 90f) {
				angle = 180f - angle;
			}
			
			dir *= angle/90f;
		}
		return new Vector2(sp,dir);
	}
}