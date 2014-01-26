using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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
		{TerrainType.Desert,	1},
		{TerrainType.Mountain, 	2},
		{TerrainType.Hills,		4},
		{TerrainType.Forest,	6},
		{TerrainType.Plains,	8}
	};

	public static TerrainType RandomTerrainType()
	{
		TerrainType[] allValues =
			(TerrainType[])Enum.GetValues(typeof(TerrainType));
		TerrainType value = allValues[UnityEngine.Random.Range(0, allValues.Length-1)]; //not water
		
		return value;
	}

	public bool ShowNeighbours = false;
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

        //Wire up connections between blocks
        ConnectBlocks();

        //Create terrain
        for (int i = 0; i < mLandscape.Length; i++)
        {
			var terrain = Landscape.RandomTerrainType();
            mLandscape[i].Terrain = terrain;
			mLandscape[i].maxPopulation = TerrainPopulation[terrain];
        }

	}

    private void ConnectBlocks()
    {
        LandscapeBlock block;
        int row;
        int col;
        int neighbourIndex;
        for (int i = 0; i < mLandscape.Length; i++)
        {
            block = mLandscape[i];
            RowColFromIndex(i, out row, out col);

            if (row - 1 > 0)
            {
                neighbourIndex = IndexFromRowCol(row - 1, col);
                block[LandscapeBlock.Neighbours.North] = mLandscape[neighbourIndex];
				mLandscape[neighbourIndex][LandscapeBlock.Neighbours.South] = block;
            }
            if (row + 1 < mHeight)
            {
                neighbourIndex = IndexFromRowCol(row + 1, col);
                block[LandscapeBlock.Neighbours.South] = mLandscape[neighbourIndex];
				mLandscape[neighbourIndex][LandscapeBlock.Neighbours.North] = block;
			}
            if (col - 1 > 0)
            {
                neighbourIndex = IndexFromRowCol(row, col - 1);
                block[LandscapeBlock.Neighbours.West] = mLandscape[neighbourIndex];
				mLandscape[neighbourIndex][LandscapeBlock.Neighbours.East] = block;

            }
            if (col + 1 < mWidth)
            {
                neighbourIndex = IndexFromRowCol(row, col + 1);
                block[LandscapeBlock.Neighbours.East] = mLandscape[neighbourIndex];
				mLandscape[neighbourIndex][LandscapeBlock.Neighbours.West] = block;

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
