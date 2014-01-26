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

	private bool shownMessage = false;
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
		newPop.body.renderer.material.color = god.color;

		return newPop;
	}

	void Update()
	{
		switch (phase) 
		{
		case Phases.Waiting:
			if(Input.GetKeyUp(KeyCode.N))
				phase = nextPhase;
			break;

		case Phases.Placement:
		{
			if(godIndex == gods.Length)
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
			Reproduction();
			phase = Phases.Waiting;
			nextPhase = Phases.Migration;
		}
		break;
					
		case Phases.Migration:
		{
			Migration();
			phase = Phases.Waiting;
			nextPhase = Phases.Conflict;
		}
		break;

		case Phases.Conflict:
		{
			Conflict();
			phase = Phases.Waiting;
			nextPhase = Phases.Conversion;
		}
		break;

		case Phases.Conversion:
		{
			Conversion();
			phase = Phases.Waiting;
			nextPhase = Phases.Aging;
		}
		break;

		case Phases.Aging:
		{
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
				break;

			default:
				break;
		} 

	}

	private void ShowMessage(string message)
	{
		if(!shownMessage) {
			Debug.Log(message);
			shownMessage = true;
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

}
