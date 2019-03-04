using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.EventSystems;
using System.Collections.Specialized;
using System.Linq;

public class CardDragControl : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public enum Role {LAY, TRUMP, PICKUP, PASS, MIRROR};
	public Role cardRole;
	public Transform prevParent;
	public GameObject placeholder = null;
	public Transform phParent;
	public string underCover;
	public string actual;
	public Transform mirror;
	public bool amTheStarter = false;


	// Use this for initialization
	void Start () {
		if (transform.parent.name == "EnemyCards") 
			cardRole = Role.MIRROR; 
		if (transform.parent.name == "MyCards") 
		{
			if (amTheStarter) 
				cardRole = Role.LAY;
			else cardRole = Role.TRUMP;
		}
		if (transform.parent.name == "DeckPanel")
			cardRole = Role.PICKUP; 
	}

	// Update is called once per frame
	void Update () {
			
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (transform.parent.name == "MyCards") 
		{
			if (amTheStarter) cardRole = Role.LAY;
			else cardRole = Role.TRUMP;
		}
		if (transform.parent.name == "DeckPanel")
			cardRole = Role.PICKUP; 

		prevParent = transform.parent;
		phParent = prevParent;
		//placeholder code according to quill18's tutorial
		placeholder = new GameObject ();
		placeholder.transform.SetParent (transform.parent);
		placeholder.AddComponent<RectTransform> ();
		placeholder.GetComponent<RectTransform> ().sizeDelta = new Vector2 (90, 125);
		placeholder.transform.SetSiblingIndex (transform.GetSiblingIndex ());

		transform.SetParent (prevParent.parent);

		GetComponent<CanvasGroup> ().blocksRaycasts = false;

	}

	public void OnDrag (PointerEventData eventData)
	{
		this.transform.position = eventData.position;

		if (placeholder.transform.parent != phParent)
			placeholder.transform.SetParent (phParent);
		
		int cardsCount = phParent.childCount;
		int newIndex = cardsCount;
		
		for (int i = 0; i < cardsCount; i++) 
		{
			if (transform.position.x < phParent.GetChild (i).position.x) 
			{
				newIndex = i;
				if (placeholder.transform.GetSiblingIndex () < newIndex)
					newIndex --;
				break;
			}
		}
		placeholder.transform.SetSiblingIndex (newIndex);
	}
		

	public void OnEndDrag (PointerEventData eventData)
	{
		transform.SetParent (prevParent);
		GetComponent<CanvasGroup> ().blocksRaycasts = true;



		int newIndex = placeholder.transform.GetSiblingIndex ();
		if (cardRole != Role.PASS) {
			transform.SetSiblingIndex (newIndex);
		}

		Destroy (placeholder);
	}

}
