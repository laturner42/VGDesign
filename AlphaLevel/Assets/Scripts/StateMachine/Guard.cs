﻿/**
 * Team: Fireflies
 * @author: Clayton Pierce, Sarah Alsmiller, Preston Turner, Justin Le, Sam Wood
 */

using UnityEngine;
using System.Collections;

public class Guard : State<OgreBehavior>
{
	NavMeshAgent agent;
	Animator anim;
	
	
	public override void CheckForNewState()
	{
		if(ownerObject.playerFound)
		{
			ownerStateMachine.CurrentState = new OgreAttack();
		}
	}
	
	public override void Update()
	{
		Debug.Log ("InGuard");
		// If the agent is close to his waypoint then move on to the next one
//		if(agent.remainingDistance < 2)
//		{
//			ownerObject.GiveHealth(10);
//			ownerObject.currPoint += 1;
//			if(ownerObject.currPoint >= ownerObject.pointsLen)
//				ownerObject.currPoint = 0;
//			ownerObject.currTarget = ownerObject.points[ownerObject.currPoint];
//		}
//		ownerObject.GiveEnergy (1);
//		agent.SetDestination (ownerObject.currTarget.position);
//		anim.SetFloat ("Speed", 1.0f);
	}
	
	public override void OnEnable(OgreBehavior owner, StateMachine<OgreBehavior> newStateMachine)
	{
		// Enable this state and grab components
		base.OnEnable (owner, newStateMachine);
		anim = owner.GetComponent<Animator> ();
		agent = owner.GetComponent<NavMeshAgent>();
	}
}