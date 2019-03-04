using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class DropDestinationControl : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

	public CardDragControl.Role placeRole;
	public string firstCardSuit = "";
	public bool available = true;

	public void Start ()
	{
	}

	public void OnPointerEnter (PointerEventData eventData)
	{
		if (eventData.pointerDrag != null && available) 
		{
			CardDragControl cardToDrag = eventData.pointerDrag.GetComponent<CardDragControl> ();
			if (cardToDrag != null) {
				cardToDrag.phParent = transform;
			}
		}
	}

	public void OnPointerExit (PointerEventData eventData)
	{
		if (eventData.pointerDrag != null && available) 
		{
			CardDragControl cardToDrag = eventData.pointerDrag.GetComponent<CardDragControl> ();
			if (cardToDrag != null) {
				cardToDrag.phParent = cardToDrag.prevParent;
			}
		}
	}

	public void OnDrop (PointerEventData eventData)
	{   
		if (eventData.pointerDrag != null && available) 
		{
			CardDragControl cardToDrag = eventData.pointerDrag.GetComponent<CardDragControl> ();
			if (cardToDrag != null) 
			{
				if ((cardToDrag.cardRole == CardDragControl.Role.LAY && transform.name == "StarterTabletop")
				    || (cardToDrag.cardRole == CardDragControl.Role.TRUMP && transform.name == "TrumperTabletop")
				    || (cardToDrag.cardRole == CardDragControl.Role.PICKUP && transform.name == "MyCards")) 
				{
					if (transform.name == "StarterTabletop")
					{
						if (firstCardSuit == "") firstCardSuit = "" + cardToDrag.actual[0];
						if (firstCardSuit [0] != cardToDrag.actual [0])
							return;						
					}
						
					cardToDrag.prevParent = transform;
					cardToDrag.cardRole = placeRole;
					GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
					for (int i = 0; i < players.Length; i++)
					{
						players[i].GetComponent<PlayerControl> ().MirrorMe (cardToDrag.actual, transform.tag);
					}

				}
			}
		}
	}




}
