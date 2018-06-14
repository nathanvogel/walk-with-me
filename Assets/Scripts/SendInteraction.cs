using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendInteraction : MonoBehaviour
{

	public PhoneCollisionManager phoneCollisionManager;
	DatabaseMessaging messaging;

	public CanvasGroup sendTooltip;

	private bool isSendPressed = false;
	private bool hasJustSent = false;
	private float visibleUntil = 0f;

	void Start ()
	{
		messaging = GetComponent<DatabaseMessaging> ();
	}


	public void onPointerDownSendButton ()
	{
		isSendPressed = true;
		hasJustSent = false;
	}

	public void onPointerUpSendButton ()
	{
		isSendPressed = false;

		if (!hasJustSent) {
			visibleUntil = Time.time + 3f;
		}
	}

	void UpdateSendTooltipAlpha ()
	{
		sendTooltip.alpha = visibleUntil < Time.time ? 0f : 1f;
	}

	void Update ()
	{

		// SendMessage() takes care of checking if basic conditions
		// for sending messages are met, so we can call that on 
		// every frame.
		// Case not well handled : errors when sending while cause retry everytime

		if (phoneCollisionManager.isInCollision () &&
		    isSendPressed &&
		    !messaging.sending) {
			messaging.SendMessage ();
			hasJustSent = true;
		}

		UpdateSendTooltipAlpha ();
	}

}
