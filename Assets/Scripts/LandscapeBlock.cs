using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class LandscapeBlock : MonoBehaviour 
{
    public enum Neighbours
    {
        North = 0,
        East = 	1,
        South = 2,
        West = 	3
    };

	private Dictionary<Neighbours, Color> neighbourColors = new Dictionary<Neighbours, Color>(4) {
		{Neighbours.North, 	Color.red},
		{Neighbours.East, 	Color.green},
		{Neighbours.South, 	Color.blue},
		{Neighbours.West, 	Color.yellow},
	};

	private Dictionary<Landscape.TerrainType, int> defenderTerrainBonus = new Dictionary<Landscape.TerrainType, int>(6) {
		{Landscape.TerrainType.Desert, 	0},
		{Landscape.TerrainType.Forest, 	1},
		{Landscape.TerrainType.Hills,	2},
		{Landscape.TerrainType.Mountain,3},
		{Landscape.TerrainType.Plains, 	-1},
		{Landscape.TerrainType.Water,	0}
	};

	private Landscape.TerrainType mTerrain = Landscape.TerrainType.Plains;
	public Landscape.TerrainType Terrain
    {
        get { return mTerrain; }
        set 
        { 
            mTerrain = value;
            Color newColor;
            float height;
            switch (mTerrain)
            {
				case Landscape.TerrainType.Mountain:
                    newColor = WebColor(112, 128, 144); //Slate grey
                    height = 0.5f;
                    break;

				case Landscape.TerrainType.Hills:
                    newColor = WebColor(226, 114, 91); //Terra cotta
                    height = 0.4f;
                    break;

				case Landscape.TerrainType.Forest:
                    newColor = WebColor(34, 139, 34); //Forest green
                    height = 0.3f;
                    break;

				case Landscape.TerrainType.Plains:
                    newColor = WebColor(255, 215, 0); //Gold
                    height = 0.2f;
                    break;

				case Landscape.TerrainType.Desert:
                    newColor = WebColor(237, 201, 175); //Desert yellow
                    height = 0.1f;
                    break;

				case Landscape.TerrainType.Water:
                    newColor = WebColor(0, 43, 184); //Azul
                    height = 0f;
                    break;

                default:
                    newColor = new Color(1f, 0f, 0.6f); //Bright pink
                    height = 0f;
                    break;  
            }

			originalColor = newColor;
            renderer.material.color = originalColor;
            Vector3 newPos = transform.localPosition;
            newPos.y = height;

            transform.localPosition = newPos;

        }
    }

    private Color WebColor(byte r, byte g, byte b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f); 
    }

    private LandscapeBlock[] mNeighbours = new LandscapeBlock[4];
	public LandscapeBlock[] neighbours  {
		get { return mNeighbours; }
	}
    public LandscapeBlock this[Neighbours neighbour]
    {
        get  { return mNeighbours[(int)neighbour]; }

        set { mNeighbours[(int)neighbour] = value; }
    }

	[HideInInspector] public Landscape parent;
	[HideInInspector] public int maxPopulation = 0;
	[HideInInspector] public List<Population> population = new List<Population>();
	[HideInInspector] public List<Population> immigrants = new List<Population>(); 
	public GameObject surfaceMarkerMin = null;
	public GameObject surfaceMarkerMax = null;
	public TextMesh label;

	private Color originalColor;
	public Tweener colorTweener;

	// Update is called once per frame
	void Update () 
    {
        if (parent.ShowNeighbours)
        {
            LandscapeBlock neighbour;
            for (int i = 0; i < mNeighbours.Length; i++)
            {
                neighbour = mNeighbours[i];
                if (neighbour != null)
                {
					var dir = neighbour.transform.position - transform.position;
					dir.Normalize();
					dir *= 0.3f;

                    Debug.DrawRay(this.transform.position, 
					               dir, 
					               neighbourColors[(Neighbours)i]);
                }
            }
        }
	}
	
	void OnMouseUpAsButton()
	{
		Debug.Log("Block clicked. " + name);

		if(Terrain == Landscape.TerrainType.Water)
			return;

		parent.selectedBlock = this;
		return;

/*
		if(colorTweener == null)
			colorTweener = HOTween.To(renderer.material, 1.0f, new TweenParms().Loops(-1, LoopType.Yoyo).Prop("color", Color.grey));
		else
		{
			colorTweener.Kill();
			colorTweener = null;
			HOTween.To(renderer.material, 0.5f, new TweenParms().Prop("color", originalColor));
		}
*/
	}

	public void AddPopulation(Population newPop)
	{
		var hasCollided = true;
		while(hasCollided)
		{
			hasCollided = false;
			var pos = GetRandomWorldPositionOnSurface();
			newPop.transform.position = pos;
			var b = newPop.body.collider.bounds;

			foreach(var p in population)
			{
				if(p.body.collider.bounds.Intersects(b))
				{
					hasCollided = true;
					break;
				}
			}
		}

		newPop.gameObject.SetActive(true);

		population.Add(newPop);

	}

	public Vector3 GetRandomWorldPositionOnSurface()
	{
		var offset = 0.1f;
		var xMin = surfaceMarkerMin.transform.position.x + offset;
		//var yMin = surfaceMarkerMin.transform.position.y;
		var zMin = surfaceMarkerMin.transform.position.z + offset;

		var xRange = surfaceMarkerMax.transform.position.x - xMin - offset;
		//var yRange = surfaceMarkerMax.transform.position.y - yMin;
		var zRange = surfaceMarkerMax.transform.position.z - zMin - offset;

		var xVal = UnityEngine.Random.Range(xMin, xMin + xRange);
		//var yVal = Random.Range(yMin, yMin + yRange);
		var zVal = UnityEngine.Random.Range(zMin, zMin + zRange);

		return new Vector3(xVal, surfaceMarkerMin.transform.position.y, zVal);
	}

	private Vector3 GetRandomPointOnLine(Vector3 start, Vector3 end)
	{
		var x = UnityEngine.Random.Range(start.x, end.x);
		var y = UnityEngine.Random.Range(start.y, end.y);
		var z = UnityEngine.Random.Range(start.z, end.z);

		return new Vector3(x, y, z);
	}

	public Neighbours GetRandomEdge()
	{
		var edge = UnityEngine.Random.Range(0, mNeighbours.Length);
		var target = mNeighbours[edge];
		while(target == null)
		{
			edge = UnityEngine.Random.Range(0, mNeighbours.Length);
			target = mNeighbours[edge];
		}

		return (Neighbours)edge;
	}

	public Neighbours GetRandomEdge_old()
	{
		var numValidEdges = 0;
		foreach(var n in neighbours)
		{
			if(n != null)
				numValidEdges++;
		}

		var chosenValidEdge = UnityEngine.Random.Range(0, numValidEdges);

		var currentEdge = 0;
		Neighbours edge = Neighbours.North;
		for(int i = 0; i < mNeighbours.Length; i++)
		{
			var n = mNeighbours[i];
			if(n != null)
			{
				if(currentEdge == chosenValidEdge)
					edge = (Neighbours)i;
				else
					currentEdge++;
			}
		}

		return edge;
	}

	public Vector3 GetPositionOnEdge(Neighbours edge)
	{
		Vector3 pos = Vector3.zero;
		Vector3 start = Vector3.zero;
		Vector3 end = Vector3.one;

		switch (edge) {
		case Neighbours.North:
			end = surfaceMarkerMax.transform.position;
			start = new Vector3(surfaceMarkerMin.transform.position.x, end.y, end.z);
			break;

		case Neighbours.East:
			start = surfaceMarkerMax.transform.position;
			end = new Vector3(start.x, start.y, surfaceMarkerMin.transform.position.z);
			break;

		case Neighbours.South:
			start = surfaceMarkerMin.transform.position;
			end = new Vector3(surfaceMarkerMax.transform.position.x, start.y, start.z);
			break;

		case Neighbours.West:
			start = surfaceMarkerMin.transform.position;
			end = new Vector3(start.x, start.y, surfaceMarkerMax.transform.position.z);
			break;
		}

		pos = GetRandomPointOnLine(start, end);

		return pos;
	}

	/// <summary>
	/// Each group of the same god get together, pair up and produce a new population with the same god.
	/// Left over population gets together a produces a new unit that has no god.
	/// </summary>
	public void Reproduce ()
	{
		var groups = new Dictionary<God, Stack<Population>>(4);
		var leftOver = new Stack<Population>();

		//Sort into groups
		foreach(var p in population)
		{
			if(!groups.ContainsKey(p.owner))
			{
				groups.Add(p.owner, new Stack<Population>());
			}

			//Age 2+ reproduce
			if(p.age > 1)
				groups[p.owner].Push(p);
		}

		//Do in group reproduction. Get the same god.
		foreach(var g in groups)
		{
			var l = g.Value;
			while(l.Count > 1)
			{
				var p = l.Pop();
				l.Pop();

				var newPop = parent.game.MakePopulation(p.owner, 1);
				AddPopulation(newPop);
			}

			if(l.Count > 0)
				leftOver.Push(l.Pop());
		}

		//Do left over population. Get null god.
		while(leftOver.Count > 1)
		{
			leftOver.Pop();
			leftOver.Pop();

			var newPop = parent.game.MakePopulation(parent.game.nullGod, 1);
			AddPopulation(newPop);
		}

	}

	/// <summary>
	/// Return a list of excess population for this block.
	/// This is the youngest population (older than 1) that is in excess of the max population for that tile.
	/// </summary>
	public void Migration()
	{
		var migrants = new List<Population>();
		var eligablePop = population.FindAll( p => p.age > 1 );

		if(eligablePop.Count > this.maxPopulation)
		{
			eligablePop.Sort( (a, b) => a.age.CompareTo(b.age) );
		
			var diff = eligablePop.Count - maxPopulation;
			migrants = eligablePop.GetRange(0, diff);
		}

		//Work out where to send each migrant
		//TODO: Make smarter, choose the neighbor with the most space 
		//but bear in mind what each other migrant is going to do.
		//For now, pick at random
		foreach(var m in migrants)
		{
			population.Remove(m);

			var edge = GetRandomEdge();
			var target = this[edge];

			target.immigrants.Add(m);

			//Position the migrants at the edge of the target block
			var edgePos = GetPositionOnEdge(edge);
			var height = Mathf.Max(surfaceMarkerMax.transform.position.y, target.surfaceMarkerMax.transform.position.y);
			edgePos.Set(edgePos.x, height, edgePos.z);
			m.transform.position = edgePos;
		}
	}

	/// <summary>
	/// If there is space on the block, new immigrants settle without problem
	/// </summary>
	public void Immigration ()
	{
		var space = maxPopulation - population.Count;
		var numImmigrants = immigrants.Count;

		if(space > 0 && immigrants.Count > 0)
		{
			//Room for all, settle them.
			if(numImmigrants <= space)
			{
				immigrants.ForEach( i => AddPopulation(i) );
				immigrants.Clear();
			}
			else
			{
				//No room for all, settle some of them.
				var diff = maxPopulation - population.Count;
				immigrants.Shuffle();
				
				var newPop = immigrants.GetRange(0, diff);
				newPop.ForEach( i => AddPopulation(i) );
				immigrants.RemoveRange(0, diff);
			}

		}
	}

	/// <summary>
	/// Resolve immigration conflict.
	/// Returns list of dead population.
	/// </summary>
	public List<Population> Conflict()
	{
		var dead = new List<Population>();
		
		if(immigrants.Count == 0)
			return dead;
		
		//Fighters are age 2+
		var defenders = population.FindAll( a => a.age > 1 );
		
		defenders.Sort( (a, b) => a.age.CompareTo(b.age) );
		immigrants.Sort( (a, b) => a.age.CompareTo(b.age) );
		
		//Fight 
		var numFights = 0;
		var winners = new List<Population>();
		while(immigrants.Count > 0 && defenders.Count > 0)
		{
			var totalDefenders = defenders.Count;
			var defendersLost = new List<Population>();
			var attackersLost = new List<Population>();

			for(int i = 0; i < totalDefenders; i++)
			{
				var defender = defenders[i];
				if(i > immigrants.Count-1)
					break;

				var attacker = immigrants[i];

				//Attacker rolls d6 and adds difference in age between defender and attacker)
				//ie Attacker age = 2, Defender age = 4 then Attacker = d6 + (4 - 2).
				//or Attacker age = 3, Defender age = 2 then Attacker = d6 + (2 - 3)
				var attackerBaseRoll = UnityEngine.Random.Range(1, 7);
				var attackerRoll = attackerBaseRoll + (defender.age - attacker.age);
				
				//Defender rolls d6 plus terrain bonus minus number of fights already had
				var defenderBaseRoll = UnityEngine.Random.Range(1, 7);
				var defenderRoll = defenderBaseRoll + defenderTerrainBonus[mTerrain] - numFights;
				
				//A 6 always wins, defender checks first.
				if(defenderBaseRoll == 6)
					attackerRoll = 0;
				else if(attackerBaseRoll == 6)
					defenderRoll = 0;
				
				//Defender wins ties.
				//Defender wins 58% on time against equal opponent
				if(attackerRoll > defenderRoll)
				{
					defendersLost.Add(defender);
					winners.Add(attacker);
				}
				else
				{
					attackersLost.Add(attacker);
				}
			}

			foreach(var p in defendersLost)
			{
				dead.Add(p);
				population.Remove(p);
				defenders.Remove(p);
			}

			foreach(var w in winners)
			{
				immigrants.Remove(w);
			}

			foreach(var a in attackersLost)
			{
				dead.Add(a);
				immigrants.Remove(a);
			}

			numFights++;
		}

		//Add any remaining immigrants to winners list
		winners.AddRange(immigrants);

		//Add the winners to the block population
		foreach(var w in winners)
		{
			AddPopulation(w);
		}
		
		immigrants.Clear();
		
		return dead;
	}

	/// <summary>
	/// See if some of the population will convert to a different religion
	/// Work down pop from oldest to youngest. Pick a random pop, try and convert.
	/// Atheists don't convert others.
	/// </summary>
	public void Conversion()
	{
		var t = new List<Population>(population);

		t.Sort( (a, b) => a.age.CompareTo(b.age) );
		t.Reverse();

		foreach(var attacker in t)
		{
			//Atheists don't convert
			if(attacker.owner == parent.game.nullGod)
				continue;

			var defender = t.Random();

			if(attacker.owner == defender.owner)
				continue;

			//Attacker roll d6 + age difference.
			//Wins if greater than defenders age or 6
			var attackerBaseRoll = UnityEngine.Random.Range(1, 7);
			var attackerRoll = attackerBaseRoll + (attacker.age - defender.age);

			if(attackerBaseRoll == 6 || attackerRoll > defender.age)
			{
				defender.owner = attacker.owner;
			}

		}
	}

	/// <summary>
	/// Age the population of the block.
	/// Return a list of the population that has died of old age.
	/// </summary>
	public List<Population> Aging()
	{
		var dead = new List<Population>();

		for (int i = population.Count -1; i >=0; i--) {
			var p = population[i];
			p.age++;
			if(p.age > parent.game.maxAge)
			{
				dead.Add(p);
				population.Remove(p);
			}
		}

		//If population still over max, remove some because of starvation.
		var maxPop = Landscape.TerrainPopulation[Terrain];

		var eligiblePop = population.FindAll( a => a.age > 2 ); //Start at 2 because the whole pop has aged.
		var diff = eligiblePop.Count - maxPop;
		for (int i = 0; i < diff; i++) 
		{
			var r = UnityEngine.Random.Range(0, eligiblePop.Count);
			var p = eligiblePop[r];
			population.Remove(p);
			dead.Add(p);
		}

		return dead;
	}

}
