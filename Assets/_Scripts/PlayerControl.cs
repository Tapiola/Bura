using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System;
using System.Linq;


public class PlayerControl : NetworkBehaviour {
	//, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

	public GameObject CardPrefab;
	public List <string> cardNames;
	public Sprite[] sprites;
	public Dictionary<string, Sprite> allCards;
	public Dictionary<char, int> scores;
	public string[] firstHand;
	public string[] secondHand;
	public string[] theRest;
	[SyncVar]
	public string trump;
	public bool winner;
	public int overallScore = 0;
	public int enemyScore = 0;
	public int myPoints = 0;
	public int enemyPoints = 0;
	public List <char> ordering = new List<char> {'6', '7', '8', '9', '0', 'J', 'Q', 'K', 'A'};
	public int[] values = {0, 0, 0, 0, 10, 2, 3, 4, 11};
	public enum Bet {NONE, DAVI, DAVI_AGR, SE, SE_AGR, DAVI_NOPE, SE_NOPE};
	public Bet currentBet;
	public bool daviInit = false;
	public bool seInit = false;


	// Use this for initialization
	void Start () {
		if (!isLocalPlayer)
			return;
		if (isServer && connectionToClient.hostId == -1)
			return;

		CmdFillCardSprites ();
		CmdShuffleDeck ();

		CmdPutTrump ();
		CmdInitPlayers ();
		CmdGiveHand ();
	}



	/* Initialise card sprites dictionary */
	[Command]
	void CmdFillCardSprites ()
	{
		RpcFillCardSprites ();
	}


	[ClientRpc]
	void RpcFillCardSprites ()
	{
		cardNames = new List<string> {
			"c6", "c7", "c8", "c9", "c10", "cA", "cJ",  "cK", "cQ",  "d6", "d7", "d8", "d9",  
			"d10", "dA", "dJ", "dK", "dQ", "cover", "h6", "h7", "h8", "h9", "h10", "hA", "hJ", 
			"hK", "hQ", "s6", "s7", "s8", "s9", "s10", "sA", "sJ", "sK", "sQ"
		};
		sprites = Resources.LoadAll<Sprite>("_Sprites/");

		allCards = cardNames.Select ((k, i) => new { k, v = sprites[i] }).ToDictionary (x => x.k, x => x.v);
	}
		


	// Update is called once per frame
	void Update () {
		
	}


	/*Initialise basic player settings and divide tabletops */
	[Command]
	void CmdInitPlayers ()
	{
		RpcInitPlayers ();
	}


	[ClientRpc]
	void RpcInitPlayers()
	{
		if (trump [0] == 'h' || trump [0] == 'd')
			divideTabletop (isServer);		
		if (trump [0] == 's' || trump [0] == 'c') 
			divideTabletop (!isServer);
	}

	void divideTabletop (bool isStarter)
	{
		CardPrefab.GetComponent<CardDragControl> ().amTheStarter = isStarter;
		GameObject trumper = GameObject.FindWithTag ("TrumperCards");
		GameObject starter = GameObject.FindWithTag ("StarterCards");
		Button done = GameObject.FindWithTag ("Done").GetComponent<Button> ();
		Button davi = GameObject.FindWithTag ("Davi").GetComponent<Button> ();
		Button se = GameObject.FindWithTag ("Se").GetComponent<Button> ();
		if (isStarter)
		{
			trumper.GetComponent<Transform> ().transform.SetSiblingIndex (4);
			starter.GetComponent<Transform> ().transform.SetSiblingIndex (5);
			done.interactable = true;
			if (!daviInit && !seInit) {
				davi.interactable = true;
				se.interactable = true;
			}
			starter.GetComponent<DropDestinationControl> ().available = true;
			trumper.GetComponent<DropDestinationControl> ().available = true;
		} 
		else 
		{
			starter.GetComponent<Transform> ().transform.SetSiblingIndex (4);
			trumper.GetComponent<Transform> ().transform.SetSiblingIndex (5);
			done.interactable = false;
			if (!daviInit && !seInit) {
				davi.interactable = false;
				se.interactable = false;
			}
			starter.GetComponent<DropDestinationControl> ().available = false;
			trumper.GetComponent<DropDestinationControl> ().available = false;
		}
	}



	/* Initialise hands of the player and enemy on both clients */
	[Command]
	void CmdGiveHand ()
	{
		RpcGiveHand ();
	}

	[ClientRpc]
	void RpcGiveHand ()
	{
		if (isServer)
			InitHand (0, firstHand, secondHand);
		else InitHand (0, secondHand, firstHand);

	}
		
	void initHandOnUI (string panelTag, string spriteName, string cardName)
	{
		GameObject cardInstance = Instantiate (CardPrefab);
		cardInstance.GetComponent<Image> ().sprite = allCards [spriteName];
		cardInstance.GetComponent<CardDragControl> ().actual = cardName;
		Transform t = GameObject.FindWithTag (panelTag).GetComponent<Transform> ().transform;
		cardInstance.GetComponent<Transform> ().transform.SetParent (t);
	}


	void InitHand (int start, string[] hand, string[] enemyHand)
	{
		for (int i = start; i < hand.Length; i++) 
		{
			if (hand [i] != null)
				initHandOnUI ("MyCards", hand [i], hand [i]);
		}

		for (int i = start; i < enemyHand.Length; i++) 
		{
			if (enemyHand [i] != null)
				initHandOnUI ("EnemyCards", "cover", enemyHand [i]);
		}

	}


	/* Initialise the shuffled deck for this round */
	[Command]
	void CmdShuffleDeck ()
	{
		cardNames = new List<string> {
			"c6", "c7", "c8", "c9", "c10", "cA", "cJ",  "cK", "cQ",  "d6", "d7", "d8", "d9",  
			"d10", "dA", "dJ", "dK", "dQ", "cover", "h6", "h7", "h8", "h9", "h10", "hA", "hJ", 
			"hK", "hQ", "s6", "s7", "s8", "s9", "s10", "sA", "sJ", "sK", "sQ"
		};
		sprites = Resources.LoadAll<Sprite>("_Sprites/");

		allCards = cardNames.Select ((k, i) => new { k, v = sprites[i] }).ToDictionary (x => x.k, x => x.v);

		System.Random rnd = new System.Random();
		List<string> toShuffle = cardNames.ToList ();
		toShuffle.Remove ("cover");
		List <string> shuffledCardKeys = toShuffle.OrderBy (c => rnd.Next ()).ToList ();
		List <string> firstHand2 = new List <string> ();
		List <string> secondHand2 = new List <string> ();
		string trump2 = shuffledCardKeys [0];
		shuffledCardKeys.RemoveAt (0);
		for (int i = 0; i < 5; i ++)
		{
			firstHand2.Add (shuffledCardKeys [0]);
			shuffledCardKeys.RemoveAt (0);
			secondHand2.Add (shuffledCardKeys [0]);
			shuffledCardKeys.RemoveAt (0);

		}
		RpcShuffleDeck (trump2, firstHand2.ToArray (), secondHand2.ToArray (), shuffledCardKeys.ToArray ());
	}

	[ClientRpc]
	void RpcShuffleDeck (string trump, string[] firstHand, string[] secondHand, string[] theRest)
	{

		this.trump = trump;
		this.firstHand = firstHand;
		this.secondHand = secondHand;
		this.theRest = theRest;

	}
		
	[Command]
	void CmdPutTrump ()
	{
		GameObject deckInstance = Instantiate (CardPrefab);
		NetworkServer.SpawnWithClientAuthority (deckInstance, connectionToClient);
		GameObject cardInstance = Instantiate (CardPrefab);
		NetworkServer.SpawnWithClientAuthority (cardInstance, connectionToClient);
		RpcPutTrump (deckInstance, cardInstance);
	}

	[ClientRpc]
	void RpcPutTrump (GameObject deckInstance, GameObject cardInstance)
	{
		deckInstance.GetComponent<Image> ().sprite = allCards ["cover"];
		cardInstance.GetComponent<Image> ().sprite = allCards [trump];

		Transform my = GameObject.FindWithTag ("DeckPanel").GetComponent<Transform> ().transform;
		deckInstance.GetComponent<Transform> ().transform.SetParent (my);
		cardInstance.GetComponent<Transform> ().transform.SetParent (my);	
		cardInstance.GetComponent<Transform> ().transform.Rotate (Vector3.forward * -90);

	}


	/* Mirroring the card on both players' tabletops */
	[Command]
	void CmdMirrorCard (string cardie, string tag)
	{
		RpcMirrorCard (cardie, tag);
	}
	
	[ClientRpc]
	void RpcMirrorCard (string cardie, string tag)
	{
		Transform tabletop = GameObject.FindWithTag (tag).GetComponent<Transform> ().transform;
		if (!hasAuthority)
		{
			Transform enemy = GameObject.FindWithTag ("EnemyCards").GetComponent<Transform> ().transform;
			for (int i = 0; i < enemy.childCount; i++)
			{
				Transform child = enemy.GetChild (i);
				if (child.GetComponent<CardDragControl> ().actual == cardie) 
				{
					child.GetComponent<Image> ().sprite = allCards [cardie];
					child.GetComponent<Transform> ().transform.SetParent (tabletop);
					break;
				}
			}
		}
	}
		
	public void MirrorMe (string cardie, string tag)
	{
		if (isServer || (!isServer && hasAuthority))
		{
			CmdFillCardSprites ();
			CmdMirrorCard (cardie, tag);
		}
			
	}

	[Command]
	void CmdSyncBet (Bet bet)
	{
		currentBet = bet;
		RpcSyncBet (bet);
	}

	[ClientRpc]
	void RpcSyncBet (Bet bet)
	{
		currentBet = bet;
	}


	void TurnButtons (string[] buttons, bool flag)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			Button davi = GameObject.FindWithTag (buttons [i]).GetComponent<Button> ();
			davi.interactable = flag;
		}

	}


	/*A player has chosen დავი */
	public void Davi ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < players.Length; i++)
		{
			PlayerControl pl = players [i].GetComponent<PlayerControl> ();
			if ((pl.isServer && !pl.hasAuthority) || (!pl.isServer && pl.hasAuthority)) 
			{
				Transform starter = GameObject.FindWithTag ("StarterCards").GetComponent<Transform> ().transform;
				if (starter.childCount == 0)
					return;
				pl.TurnButtons (new string[] {"Davi"}, false);
				if (pl.currentBet == Bet.NONE) 
					pl.CmdSyncBet (Bet.DAVI);
				else if (pl.currentBet == Bet.DAVI) 
					pl.CmdSyncBet (Bet.DAVI_AGR);
				pl.daviInit = true;
				pl.DoneTurn ();
			}
		}
	}


	/*A player has chosen სე */
	public void Se ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < players.Length; i++)
		{
			PlayerControl pl = players [i].GetComponent<PlayerControl> ();
			if ((pl.isServer && !pl.hasAuthority) || (!pl.isServer && pl.hasAuthority)) 
			{
				Transform starter = GameObject.FindWithTag ("StarterCards").GetComponent<Transform> ().transform;
				Transform trumper = GameObject.FindWithTag ("TrumperCards").GetComponent<Transform> ().transform;
				if (starter.childCount == 0)
					return;
				pl.seInit = true;
				pl.TurnButtons (new string[] {"Davi", "Se"}, false);

				if (pl.currentBet == Bet.DAVI) 
				{
					pl.CmdSyncBet (Bet.SE);
					starter.SetSiblingIndex (4);
					trumper.SetSiblingIndex (5);
					pl.CmdDisablePlayer ();

				} 
				else if (pl.currentBet == Bet.SE) 
				{
					pl.CmdSyncBet (Bet.SE_AGR);
					pl.DoneTurn ();
				}
			}
		}
	}


	public void DoneTurn ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");			
		CardDragControl drag = CardPrefab.GetComponent<CardDragControl> ();
		Transform trumper = GameObject.FindWithTag ("TrumperCards").GetComponent<Transform> ().transform;
		Transform starter = GameObject.FindWithTag ("StarterCards").GetComponent<Transform> ().transform;
		if (currentBet == Bet.SE_AGR)
			drag.amTheStarter = false;
		if (drag.amTheStarter)
		{
			if (starter.childCount == 0)
				return;
			starter.SetSiblingIndex (4);
			trumper.SetSiblingIndex (5);
			for (int i = 0; i < players.Length; i++)
			{
				PlayerControl pl = players [i].GetComponent<PlayerControl> ();
				if (pl.isServer || (!pl.isServer && pl.hasAuthority)) 
				{
					pl.CmdDisablePlayer ();
				}
			}
		} 
		else 
		{
			if (currentBet == Bet.NONE && trumper.childCount != starter.childCount)
				return;
			for (int i = 0; i < players.Length; i++)
			{
				players[i].GetComponent<PlayerControl> ().FinishMove ();
			}
		}
	}


	[Command]
	void CmdClearRound (Bet bet)
	{
		RpcClearRound (bet);
	}


	[ClientRpc]
	void RpcClearRound (Bet bet)
	{
		currentBet = Bet.NONE;
		theRest = new string[0];
		firstHand = new string[0];
		secondHand = new string[0];
		destroyCards (new string[] {"MyCards", "EnemyCards", "DeckPanel", "StarterCards", "TrumperCards"});
	}

	void destroyCards (string[] tags)
	{
		for (int j = 0; j < tags.Length; j++)
		{
			Transform pn = GameObject.FindWithTag (tags[j]).GetComponent<Transform> ().transform;
			for (int i = 0; i < pn.childCount; i++)
				Destroy (pn.GetChild (i).gameObject);
		}
	}

	/*A player move is done */
	public void DoneMove ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < players.Length; i++)
		{
			PlayerControl pl = players [i].GetComponent<PlayerControl> ();
			if (pl.isServer || (!pl.isServer && pl.hasAuthority)) 
			{
				if (pl.currentBet == Bet.DAVI) {
					pl.CmdClearRound (Bet.DAVI_NOPE);
					pl.overallScore = -100;
					pl.enemyScore = 100;
					pl.CmdFillHands ();
					return;
				}
				else if (pl.currentBet == Bet.SE) {
					pl.CmdClearRound (Bet.SE_NOPE);
					pl.overallScore = -100;
					pl.enemyScore = 100;
					pl.CmdFillHands ();
					return;
				}
			}
		}

		DoneTurn ();
	}

	/* Shift move availability between players */
	[Command]
	void CmdDisablePlayer ()
	{
		RpcDisablePlayer ();
	}

	[ClientRpc]
	void RpcDisablePlayer ()
	{
		if ((isServer && !hasAuthority) || (!isServer && hasAuthority))
		{
			Button done = GameObject.FindWithTag ("Done").GetComponent<Button> ();
			done.interactable = !done.interactable;
			if (!daviInit && !seInit) {
				Button davi = GameObject.FindWithTag ("Davi").GetComponent<Button> ();
				davi.interactable = done.interactable;
				Button se = GameObject.FindWithTag ("Se").GetComponent<Button> ();
				se.interactable = done.interactable;
			}
			DropDestinationControl starter 
			= GameObject.FindWithTag ("StarterCards").GetComponent<DropDestinationControl> ();
			starter.available = !starter.available;
			DropDestinationControl trumper 
			= GameObject.FindWithTag ("TrumperCards").GetComponent<DropDestinationControl> ();
			trumper.available = !trumper.available;
		}
	}
		
	/* Check which one wins the hand */
	bool trumps (string s, string t)
	{
		if ((isServer && !hasAuthority) || (!isServer && hasAuthority))
		{
			int tInd = ordering.IndexOf (t [t.Length-1]);
			int sInd = ordering.IndexOf (s [s.Length-1]);
			if (s [0] == t [0] && tInd > sInd)
				return true;
			if (s [0] != t [0] && t [0] == trump [0])
				return true;
		}
		return false;
	}


	/* If the hand is finished with this move */
	void FinishMove ()
	{
		if ((isServer && !hasAuthority) || (!isServer && hasAuthority))
		{
			CmdFinishMove ();
			CmdFillHands ();
		}

	}


	[Command]
	void CmdFinishMove ()
	{

		Transform trumper = GameObject.FindWithTag ("TrumperCards").GetComponent<Transform> ().transform;
		Transform starter = GameObject.FindWithTag ("StarterCards").GetComponent<Transform> ().transform;
		bool wins = true;
		int score = 0;
		scores = ordering.Select ((k, i) => new { k, v = values[i] }).ToDictionary (x => x.k, x => x.v);
		for (int i = 0; i < starter.childCount; i++)
		{
			string s = starter.GetChild (i).GetComponent<CardDragControl> ().actual;
			string t = trumper.GetChild (i).GetComponent<CardDragControl> ().actual;
			score += scores[s[s.Length-1]] + scores[t[t.Length-1]];
			if (!trumps (s, t))
			{
				wins = false;
			}
		}

		RpcFinishMove (wins, score);
	}

	[ClientRpc]
	void RpcFinishMove (bool wins, int score)
	{
		bool isStarter = CardPrefab.GetComponent<CardDragControl> ().amTheStarter;
		if (isStarter)
			this.winner = !wins;
		else
			this.winner = wins;
		if (this.winner)
		{
			this.overallScore += score;
			Text t = GameObject.FindWithTag ("MyScore").GetComponent<Text> ();
			t.text = "Score: " + overallScore.ToString ();
		}
		else
		{
			this.enemyScore += score;
		}

		destroyCards (new string[] {"StarterCards", "TrumperCards"});

		GameObject.FindWithTag ("StarterCards").GetComponent<DropDestinationControl> ().firstCardSuit = "";
		Button done = GameObject.FindWithTag ("Done").GetComponent<Button> ();
		done.interactable = this.winner;
		if (!daviInit && !seInit) {
			Button davi = GameObject.FindWithTag ("Davi").GetComponent<Button> ();
			davi.interactable = this.winner;
			Button se = GameObject.FindWithTag ("Se").GetComponent<Button> ();
			se.interactable = this.winner;
		}
	}


	void ResetMove ()
	{
		overallScore = 0;
		enemyScore = 0;
		Text t2 = GameObject.FindWithTag ("MyScore").GetComponent<Text> ();
		t2.text = "Score: 0";
		currentBet = Bet.NONE;
		daviInit = false;
		seInit = false;
	}

	void CalculateRoundScore ()
	{
		int scoreToAdd = 1;
		if (currentBet == Bet.DAVI_AGR || currentBet == Bet.SE_NOPE)
			scoreToAdd = 2;
		if (currentBet == Bet.SE_AGR)
			scoreToAdd = 3;
		if (overallScore >= enemyScore)
			myPoints += scoreToAdd;
		else
			enemyPoints += scoreToAdd;

		Text t1 = GameObject.FindWithTag ("Result").GetComponent<Text> ();
		t1.text = "Result: " + myPoints.ToString () + " - " + enemyPoints.ToString ();

		if (myPoints >= 11)
		{
			t1.text = "YOU WIN!";
			return;
		}

		if (enemyPoints >= 11) 
		{
			t1.text = "YOU LOSE!";
			return;
		}
	}

	/* Refill hands */
	[Command]
	void CmdFillHands ()
	{
		RpcFillHands ();
	}

	[ClientRpc]
	void RpcFillHands ()
	{
		Transform my = GameObject.FindWithTag ("MyCards").GetComponent<Transform> ().transform;
		Transform en = GameObject.FindWithTag ("EnemyCards").GetComponent<Transform> ().transform;
		int N = my.childCount;
		bool isStarter = false;
		string[] firstHand2 = new string[5];
		string[] secondHand2 = new string[5];

		if (theRest.Length > 0) 
		{
			for (int i = 0; i < N; i++) {
				CardDragControl myChild = my.GetChild (i).GetComponent<CardDragControl> ();
				if (currentBet == Bet.DAVI)
					isStarter = !daviInit;
				else if (currentBet == Bet.SE)
					isStarter = !seInit;
				else
					isStarter = this.winner;
				myChild.amTheStarter = isStarter;
				firstHand2 [i] = firstHand [i];
				secondHand2 [i] = secondHand [i];
			}
			divideTabletop (isStarter);
		}

		if (theRest.Length > 0)
		{
			List <string> tempRest = theRest.ToList ();
			for (int i = 0; i < 5 - N; i++)
			{
				firstHand2 [i + N] = tempRest [0];
				tempRest.RemoveAt (0);
				if (tempRest.Count == 0) 
				{
					secondHand2 [i + N] = trump;
					destroyCards (new string[] {"DeckPanel"});
					break;
				}
				secondHand2 [i + N] = tempRest [0];
				tempRest.RemoveAt (0);
			}

			firstHand = firstHand2;
			secondHand = secondHand2;
			theRest = tempRest.ToArray ();

			if (isServer)
				InitHand (N, firstHand, secondHand);
			else InitHand (N, secondHand, firstHand);
		}
		else 
		{
			/* If the deck has run out */
			if (N == 0) 
			{
				CalculateRoundScore ();
				ResetMove ();
				Start ();
			}
		}

	}

}