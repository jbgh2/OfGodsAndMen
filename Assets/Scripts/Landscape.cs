using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using SimplexNoise;

public class Landscape : MonoBehaviour 
{
	public enum TerrainType
	{
		Mountain,
		Hills,
		Forest,
		Plains,
		Desert,
		Water
	};

	public static Dictionary<TerrainType, int> TerrainPopulation = new Dictionary<TerrainType, int>()
	{
		{TerrainType.Water,		0},
		{TerrainType.Desert,	2},
		{TerrainType.Mountain, 	3},
		{TerrainType.Hills,		4},
		{TerrainType.Forest,	5},
		{TerrainType.Plains,	6}
	};

	public static TerrainType RandomTerrainType()
	{
		TerrainType[] allValues =
			(TerrainType[])Enum.GetValues(typeof(TerrainType));
		TerrainType value = allValues[UnityEngine.Random.Range(0, allValues.Length)];
		
		return value;
	}

	public bool ShowNeighbours = false;
	public bool terrainUsingNoise = true;
	public int noiseSeed = -1;
    public int NumRows = 3;
    public int NumColumns = 3;
    public GameObject LandscapeBlockPrefab = null;

	public GameController game = null;

    private LandscapeBlock[] mLandscape;
	public LandscapeBlock[] blocks
	{
		get { return mLandscape; }
	}

	[HideInInspector] public LandscapeBlock selectedBlock;

    //These are set on startup so changing NumRows and NumCols doesn't screw things if they are changed
    private int mWidth;
    private int mHeight;

	// Use this for initialization
	public void CreateLandscape() 
    {
        if (LandscapeBlockPrefab == null)
        {
            Debug.LogError("LandscapeBlock is null. Please drag in a gameobject.");
            return;
        }

        mLandscape = new LandscapeBlock[NumColumns * NumRows];

        mWidth = NumColumns;
        mHeight = NumRows;

        //Create blocks
		CreateBlocks();

		if(terrainUsingNoise)
			CreateTerrainFromNoise();
		else
		{
			//Create terrain
			for (int i = 0; i < mLandscape.Length; i++)
			{
				var terrain = Landscape.RandomTerrainType();
				mLandscape[i].Terrain = terrain;
				mLandscape[i].maxPopulation = TerrainPopulation[terrain];
			}
		}

        //Wire up connections between blocks
        ConnectBlocks();
	}

    private void ConnectBlocks()
    {
        LandscapeBlock block;
        int row;
        int col;
        int neighbourIndex;
		LandscapeBlock neighbour;
        for (int i = 0; i < mLandscape.Length; i++)
        {
            block = mLandscape[i];
			if(block.Terrain == TerrainType.Water)
				continue; //no connections 

            RowColFromIndex(i, out row, out col);

            if (row - 1 > 0)
            {
                neighbourIndex = IndexFromRowCol(row - 1, col);
				neighbour = mLandscape[neighbourIndex];
				if(neighbour.Terrain != TerrainType.Water)
				{
					block[LandscapeBlock.Neighbours.North] = mLandscape[neighbourIndex];
					mLandscape[neighbourIndex][LandscapeBlock.Neighbours.South] = block;
				}
            }
            if (row + 1 < mHeight)
            {
                neighbourIndex = IndexFromRowCol(row + 1, col);
				neighbour = mLandscape[neighbourIndex];
				if(neighbour.Terrain != TerrainType.Water)
				{
					block[LandscapeBlock.Neighbours.South] = mLandscape[neighbourIndex];
					mLandscape[neighbourIndex][LandscapeBlock.Neighbours.North] = block;
				}
			}
            if (col - 1 > 0)
            {
                neighbourIndex = IndexFromRowCol(row, col - 1);
				neighbour = mLandscape[neighbourIndex];
				if(neighbour.Terrain != TerrainType.Water)
				{
					block[LandscapeBlock.Neighbours.West] = mLandscape[neighbourIndex];
					mLandscape[neighbourIndex][LandscapeBlock.Neighbours.East] = block;
				}

            }
            if (col + 1 < mWidth)
            {
                neighbourIndex = IndexFromRowCol(row, col + 1);
				neighbour = mLandscape[neighbourIndex];
				if(neighbour.Terrain != TerrainType.Water)
				{
					block[LandscapeBlock.Neighbours.East] = mLandscape[neighbourIndex];
					mLandscape[neighbourIndex][LandscapeBlock.Neighbours.West] = block;
				}

            }
        }
    }

	private void CreateTerrainFromNoise()
	{
		//Init
		byte[] seeds = new byte[512];
		var rand = (noiseSeed < 0) ? new System.Random() : new System.Random(noiseSeed) ;
		rand.NextBytes(seeds);
		Noise.perm = seeds;

		//Generate array of noise
		for (int row = 0; row < mHeight; row++)
		{
			for (int col = 0; col < mWidth; col++)
			{
				var n = Noise.Generate(row, col);

				//Assume range is -1 to 1
				var height = (n + 1f) / 2f;
				height = Mathf.Clamp01(height);

				//Terrain from height
				var terrain = Landscape.TerrainType.Water;
				if(height >= 0 && height < 0.1f)
					terrain = Landscape.TerrainType.Water;
				else if(height >= 0.1f && height < 0.15f)
					terrain = Landscape.TerrainType.Desert;
				else if(height >= 0.15f && height < 0.4f)
					terrain = Landscape.TerrainType.Plains;
				else if(height >= 0.4f && height < 0.6f)
					terrain = Landscape.TerrainType.Forest;
				else if(height >= 0.6f && height < 0.9f)
					terrain = Landscape.TerrainType.Hills;
				else
					terrain = Landscape.TerrainType.Mountain;

				var i = IndexFromRowCol(row, col);
				mLandscape[i].Terrain = terrain;
				mLandscape[i].maxPopulation = TerrainPopulation[terrain];


			}
		}
	}

    private void CreateBlocks()
    {
        float blockSize = 1f;
        float blockX = 0f;
        float blockZ = 0f;
        Vector3 blockPos;
        LandscapeBlock newBlock;
        bool error = false;
        for (int row = 0; row < mHeight; row++)
        {
            for (int col = 0; col < mWidth; col++)
            {
                blockX = col * blockSize;
                blockZ = -row * blockSize;
                blockPos = new Vector3(blockX, 0f, blockZ);

                var go = GameObject.Instantiate(LandscapeBlockPrefab,
                                                  blockPos,
                                                  Quaternion.identity) as GameObject;
                newBlock = go.GetComponent<LandscapeBlock>();
				newBlock.gameObject.SetActive(true);
				newBlock.label.gameObject.SetActive(false);
				newBlock.parent = this;

                if (newBlock == null)
                {
                    Debug.LogError(string.Format("Can't instantiate {0} as LandscapeBlock",
                                                    LandscapeBlockPrefab.name));
                    error = true;
                    break;
                }

                newBlock.name = string.Format("Block_{0}_{1}", col, row);
                newBlock.transform.parent = this.transform;

                int index = IndexFromRowCol(row, col);
                mLandscape[index] = newBlock;
            }

            if (error)
                break;
        }
    }

    private int IndexFromRowCol(int row, int col)
    {
        return (row * mWidth) + col;
    }

    private void RowColFromIndex(int index, out int row, out int col)
    {
        row = index / mWidth;
        col = index % mWidth;
    }

	void Awake()
	{
		if(game == null)
			game = gameObject.GetComponent<GameController>();
	}

	void Update()
	{
		if(ShowNeighbours)
		{
			LandscapeBlock block;
			for (int i = 0; i < mLandscape.Length; i++) 
			{
				block = mLandscape[i];
				block.label.text = i.ToString();
				block.label.gameObject.SetActive(true);
			}
		}

	}

}
