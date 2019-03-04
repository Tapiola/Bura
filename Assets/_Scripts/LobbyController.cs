using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class LobbyController : MonoBehaviour {

	private NetworkManager manager;
	private string roomName;
	private InputField roomInput;
	public Transform scrollPanel;
	public Button matchPrefab;
	public Text matchText;

	// Use this for initialization
	void Start () {
		manager = GameObject.FindGameObjectWithTag ("NetworkManager").GetComponent<NetworkManager> ();
		roomInput = GameObject.FindGameObjectWithTag ("RoomInput").GetComponent<InputField> ();
		roomName = roomInput.text;
		manager.StartMatchMaker ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RoomNameEntered ()
	{
		roomName = roomInput.text;
	}


	public void OnMatchCreate (bool success, string extendedInfo, MatchInfo matchInfo)
	{
		manager.OnMatchCreate (success, extendedInfo, matchInfo);
	}

	public void CreateMatchClicked ()
	{
		manager.matchMaker.CreateMatch (roomName, manager.matchSize, true, "", "", "", 
										0, 0, OnMatchCreate);
	}



	public void OnMatchList (bool success, string extendedInfo, 
				List<MatchInfoSnapshot> matchList)
	{
		manager.OnMatchList (success, extendedInfo, matchList);
		foreach (var match in manager.matches) 
		{
			Debug.Log (match.name);

			Button newItem = Instantiate (matchPrefab) as Button;
			newItem.GetComponentInChildren<Text>().text = "Join Game: " + match.name;
			newItem.onClick.AddListener (
				() => manager.matchMaker.JoinMatch (match.networkId, "", "", "",  0, 0, OnMatchJoined));
			newItem.transform.SetParent (scrollPanel, false);
		}
	}

		 
	public void ListAvailableMatches ()
	{
		manager.matchMaker.ListMatches (0, 20, "", true, 0, 0, OnMatchList);
	}


	public void OnMatchJoined (bool success, string extendedInfo, MatchInfo matchInfo)
	{
		manager.OnMatchJoined (success, extendedInfo, matchInfo);
	}
}
