using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Holoville.HOTween;

public class GameController : MonoBehaviour 
{
	private enum Phases
	{
		Waiting,
		Placement,
		Reproduction,
		Migration,
		Conflict,
		Conversion,
		Aging,
		GameOver
	}
	private Phases lastPhase;
	private Phases phase;
	private Phases nextPhase;
	private int turn;

	public God nullGod;
	public God[] gods;
	
	public Landscape landscape;
	public Population popPrefab;

	public int turnsInGame = 3;
	public int maxAge = 6;
	public int startingPopulation = 2;
	
	private int godIndex;

	// Use this for initialization
	void Start () 
	{		
		HOTween.Init();

		landscape.CreateLandscape();
/*			
		foreach(var block in landscape.blocks)
		{
			//Randomly create number of pop units on tile
			int numPop = 2; // Random.Range(0, block.maxPopulation + 1);
			var god = gods[Random.Range(0, gods.Length)];
			for (int i = 0; i < numPop; i++) 
			{
				//Give the population a god
				var age = Random.Range(2, 4);
				var newPop = MakePopulation(god, age);

				block.AddPopulation(newPop);
			}
		}
*/
	
		phase = Phases.Placement;
		godIndex = 0;
		Debug.Log("Placement....");

		turn = 1;

	}

	public Population MakePopulation(God god, int age)
	{
		var newPop = GameObject.Instantiate(popPrefab) as Population;
		//Give the population a god
		newPop.owner = god;
		newPop.age = age;

		return newPop;
	}

	void Update()
	{
		if(Input.GetKeyUp(KeyCode.Escape))
			Application.Quit();


		switch (phase) 
		{
		case Phases.Waiting:
			if(Input.GetKeyUp(KeyCode.N))
				phase = nextPhase;
			break;

		case Phases.Placement:
		{
			lastPhase = Phases.Placement;
			if(godIndex == gods.Length || Input.GetKeyUp(KeyCode.N))
			{
				nextPhase = Phases.Reproduction;
				phase = Phases.Waiting;
			}
			else
			{
				if(landscape.selectedBlock != null)
				{
					Debug.Log("Placing population for god " + gods[godIndex]);

					for (int i = 0; i < startingPopulation; i++) 
					{
						//Give the population a god
						var age = Random.Range(2, 4);
						var newPop = MakePopulation(gods[godIndex], age);
						
						landscape.selectedBlock.AddPopulation(newPop);
					}

					godIndex++;
					landscape.selectedBlock = null;
				}
			}
		}
		break;

		case Phases.Reproduction:
		{
			lastPhase = Phases.Reproduction;
			Reproduction();
			phase = Phases.Waiting;
			nextPhase = Phases.Migration;
		}
		break;
					
		case Phases.Migration:
		{
			lastPhase = Phases.Migration;
			Migration();
			phase = Phases.Waiting;
			nextPhase = Phases.Conflict;
		}
		break;

		case Phases.Conflict:
		{
			lastPhase = Phases.Conflict;
			Conflict();
			phase = Phases.Waiting;
			nextPhase = Phases.Conversion;
		}
		break;

		case Phases.Conversion:
		{
			lastPhase = Phases.Conversion;
			Conversion();
			phase = Phases.Waiting;
			nextPhase = Phases.Aging;
		}
		break;

		case Phases.Aging:
		{
			lastPhase = Phases.Aging;
			Aging();
			if(++turn <= turnsInGame)
			{
				phase = Phases.Waiting;
				nextPhase = Phases.Reproduction;
			}
			else
			{
				Debug.Log("Game Over");
				phase = Phases.GameOver;
			}
		}
		break;

		case Phases.GameOver:
			lastPhase = Phases.GameOver;
			break;

			default:
				break;
		} 

	}

	void Reproduction ()
	{
		Debug.Log("Reproduction " + turn);

		foreach(var b in landscape.blocks)
		{
			b.Reproduce();
		}
	}

	void Migration ()
	{
		Debug.Log("Migration " + turn);

		//Get all migrants for each block
		foreach(var b in landscape.blocks)
		{
			b.Migration();
		}
	}

	void Conflict ()
	{
		Debug.Log("Conflict " + turn);

		var totalDead = new List<Population>();

		foreach(var b in landscape.blocks)
		{
			b.Immigration();
			totalDead.AddRange( b.Conflict() );
		}

		ResolveDead(totalDead);
	}

	void Conversion ()
	{
		Debug.Log("Conversion " + turn);

		foreach(var b in landscape.blocks)
		{
			b.Conversion();
		}
	}

	void Aging ()
	{
		Debug.Log("Death " + turn);

		var totalDead = new List<Population>();
		foreach(var b in landscape.blocks)
		{
			totalDead.AddRange( b.Aging() );
		}

		ResolveDead(totalDead);

		Debug.Log("Turn: " + turn);
		foreach(var g in gods)
		{
			Debug.Log(g.name + ", score " + g.score);
		}
	}

	void ResolveDead(List<Population> dead)
	{
		foreach(var p in dead)
		{
			p.owner.score += p.age;
			GameObject.Destroy(p.gameObject);
		}
	}

	void OnGUI()
	{
		var rightStyle = new GUIStyle(GUI.skin.label);
		rightStyle.alignment = TextAnchor.MiddleRight;

		var centerStyle = new GUIStyle(GUI.skin.label);
		centerStyle.alignment = TextAnchor.MiddleCenter;
		
		var leftStyle = new GUIStyle(GUI.skin.label);
		leftStyle.alignment = TextAnchor.MiddleLeft;

		//Top left. Tile info.
		if(landscape.selectedBlock != null)
		{
			GUILayout.BeginArea(new Rect(10, 10, 200, 200));
			GUILayout.Label(landscape.selectedBlock.Terrain.ToString(), leftStyle);
			GUILayout.Label("Pop: " + landscape.selectedBlock.population.Count, leftStyle);
			GUILayout.Label("Max: " + landscape.selectedBlock.maxPopulation, leftStyle);
			GUILayout.EndArea();
		}

		//Center. Turn and phase
		GUILayout.BeginArea(new Rect((Screen.width / 2) - 50, 10, 50, 20));
		GUILayout.Label(turn + " / " + turnsInGame, centerStyle);
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect((Screen.width / 2), 10, 200, 20));
		GUILayout.Label(lastPhase.ToString(), centerStyle);
		GUILayout.EndArea();

		//Top right. Scores
		GUILayout.BeginArea(new Rect(Screen.width - 150, 10, 140, 200));
		foreach(var god in gods)
		{
			GUILayout.Label(god.name + " : " + god.score.ToString(), rightStyle);
		}
		GUILayout.EndArea();

	}

}
