using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int COLOR_AMOUNT = 1 ;
    public int Seed;
    public GameObject tilePrefab;
    public GameObject[,] allBlocks;
    public List<GameObject> allPrefabs;
    public List<GameObject> Prefabs; //this will be used
    public List<Sprite> Sprites;
    public Dictionary<int, List<GameObject>> groups = new Dictionary<int, List<GameObject>> { };
    public int maxGroupNo;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = new Vector3((float)(width+1)/2, (float)(height+1)/2, -10);
        Camera.main.orthographicSize = Camera.main.transform.position.y;
        allBlocks = new GameObject[width+2, height+2];
        groups.Add(0, new List<GameObject> { allBlocks[0,0] });
        getRandomColorsInRange();
        Setup();
        padMatrix();
        //checkForGroups();
        checkForGroupsRecursively();
    }
    void getRandomColorsInRange()
    {
         List<GameObject> tempPrefabs = allPrefabs;
        for (int i = 0; i < COLOR_AMOUNT; i++)
        {
            int index = Random.Range(0, tempPrefabs.Count);
            Prefabs.Add(tempPrefabs[i]);
            tempPrefabs.Remove(tempPrefabs[i]);
        }
    }
    void Setup()
    {
        //Random.seed = 1475451507;
        Seed = Random.seed;
        int tempCount = 0;
        for(int i = 1; i < height+1; i++)
        {
            for (int j = 1; j < width+1; j++)
            {
                Vector2 tempPos = new Vector2(j, i);
                //Generate Block
                int index                  = Random.Range(0, Prefabs.Count);
                GameObject block           = Instantiate(Prefabs[index], tempPos, Quaternion.identity) as GameObject;
                SpriteRenderer renderer    = block.transform.gameObject.GetComponent<SpriteRenderer>();
                Block blockSettings        = block.transform.gameObject.GetComponent<Block>();
                block.name                 = (j-1) + "," + (i-1);
                renderer.sortingOrder      = i;
                blockSettings.currentGroup = tempCount;
                blockSettings.row          = j;
                blockSettings.col          = i;
                tempCount++;
                block.transform.position   = block.transform.position;
                block.transform.parent     = this.transform;
                block.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                allBlocks[j, i] = block;
            }
        }
    }
    void checkForGroupsRecursively()
    {
        groups.Clear();
        int curno = 0;
        for (int i = 1; i < height + 1; i++)
        {
            for (int j = 1; j < width + 1; j++)
            {
                if (allBlocks[j, i] != null)
                {
                    allBlocks[j, i].GetComponent<Block>().currentGroup = curno;
                    curno++;
                }
            }
        }

        List<GameObject> tiles = getActiveTiles();
        int count = tiles.Count;
        while (count > 0)
        {
            List<GameObject> colorGroup = new List<GameObject> { };
            Block block = tiles[0].GetComponent<Block>();
            recursiveGrouping(colorGroup, ref tiles, tiles[0]);
            if(!groups.ContainsKey( block.currentGroup ))
                groups.Add(block.currentGroup, colorGroup);
            else
                groups[block.currentGroup] = groups[block.currentGroup].Union<GameObject>(colorGroup).ToList<GameObject>();
            count = tiles.Count;
        }
        maxGroupNo = groups.Values.Last()[0].GetComponent<Block>().currentGroup;
        foreach (int index in groups.Keys)
        {
            int gCount = getGroupUniqueCount(groups[index]);
            foreach (GameObject block in groups[index])
            {
                setSprite(block, gCount);
            }
        }
    }
    private List<GameObject> getActiveTiles()
    {
        List<GameObject> notNull = new List<GameObject> { };
        for (int i = 1; i < height + 1; i++)
        {
            for (int j = 1; j < width + 1; j++)
            {
                if (allBlocks[j, i] != null)
                {
                    notNull.Add(allBlocks[j, i]);
                }
            }
        }
        return notNull;
    }
    void recursiveGrouping(List<GameObject> colorGroup, ref List<GameObject> tiles, GameObject block)
    {
        //1. start from current block and remove from tiles list
        colorGroup.Add(block);
        block.GetComponent<Block>().currentGroup = colorGroup[0].GetComponent<Block>().currentGroup;
        tiles.Remove(block);

        //2. check directions, u d l r, then recurse
        int row = (int) block.GetComponent<Block>().row;
        int col = (int)block.GetComponent<Block>().col;
        GameObject current = allBlocks[row, col];
        GameObject upper = allBlocks[row, col + 1];
        GameObject downer = allBlocks[row, col - 1];
        GameObject lefter = allBlocks[row + 1, col];
        GameObject righter = allBlocks[row - 1, col];
        if (current != null)
        {
            if (upper != null && upper.name != "padding" && !colorGroup.Contains(upper) && upper.GetComponent<Block>().colorType == current.GetComponent<Block>().colorType)
                recursiveGrouping(colorGroup, ref tiles, upper);
            if (downer != null && downer.name != "padding" && !colorGroup.Contains(downer) && downer.GetComponent<Block>().colorType == current.GetComponent<Block>().colorType)
                recursiveGrouping(colorGroup, ref tiles, downer);
            if (lefter != null && lefter.name != "padding" && !colorGroup.Contains(lefter) && lefter.GetComponent<Block>().colorType == current.GetComponent<Block>().colorType)
                recursiveGrouping(colorGroup, ref tiles, lefter);
            if (righter != null && righter.name != "padding" && !colorGroup.Contains(righter) && righter.GetComponent<Block>().colorType == current.GetComponent<Block>().colorType)
                recursiveGrouping(colorGroup, ref tiles, righter);
        }
        //return final group list
    }
    void setSprite(GameObject block, int gCount)
    {
        if(block.transform.gameObject.GetComponent<Block>().colorType == "blue")
        {
            if (gCount < 5)
            {
                //Sprite Default
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[3];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[0];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[1];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[2];
            }
        }
        else if (block.transform.gameObject.GetComponent<Block>().colorType == "green")
        {
            if (gCount < 5)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[7];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[4];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[5];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[6];
            }
        }
        else if (block.transform.gameObject.GetComponent<Block>().colorType == "pink")
        {
            if (gCount < 5)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[11];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[8];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[9];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[10];
            }
        }
        else if (block.transform.gameObject.GetComponent<Block>().colorType == "purple")
        {
            if (gCount < 5)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[15];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[12];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[13];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[14];
            }
        }
        else if (block.transform.gameObject.GetComponent<Block>().colorType == "red")
        {
            if (gCount < 5)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[19];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[16];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[17];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[18];
            }
        }
        else if (block.transform.gameObject.GetComponent<Block>().colorType == "yellow")
        {
            if (gCount < 5)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[23];
            }
            else if (gCount > 4 && gCount < 8)
            {
                //Sprite A
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[20];
            }
            else if (gCount > 7 && gCount < 10)
            {
                //Sprite B
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[21];
            }
            else if (gCount > 10)
            {
                //Sprite C
                block.transform.gameObject.GetComponent<SpriteRenderer>().sprite = Sprites[22];
            }
        }

    }
    void padMatrix()
    {
        for (int i = 0; i < width + 2; i++)
        {
            allBlocks[i, 0] = new GameObject("padding");
            allBlocks[i, height +1] = new GameObject("padding");
        }
        for (int i = 0; i < height + 2; i++)
        {
            allBlocks[0, i] = new GameObject("padding");
            allBlocks[width + 1, i] = new GameObject("padding");
        }
    }
    void deleteGroup(List<GameObject> group)
    {
        while(group.Count > 0)
        {
            DestroyImmediate(group[0]);
            group.RemoveAt(0);
        }
    }
    private IEnumerator decreaseRowCo()
    {
        int nullcount = 0;
        for(int i = 1; i < width +1; i++)
        {
            for (int j = 1; j < height + 1; j++)
            {
                if(allBlocks[i,j] == null)
                {
                    nullcount++;
                }
                else if(nullcount > 0)
                {
                    allBlocks[i, j].GetComponent<Block>().col -= nullcount;
                }
            }
            nullcount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }
    private void RefillBoard()
    {
        for (int i = 1; i < height + 1; i++)
        {
            for (int j = 1; j < width + 1; j++)
            {
                if (allBlocks[j,i] == null)
                {
                    Vector2 tempPos = new Vector2(j, i + 1);
                    int index = Random.Range(0, Prefabs.Count);
                    GameObject block = Instantiate(Prefabs[index], tempPos, Quaternion.identity);
                    SpriteRenderer renderer = block.transform.gameObject.GetComponent<SpriteRenderer>();
                    Block blockSettings = block.transform.gameObject.GetComponent<Block>();
                    block.name = (j - 1) + "," + (i - 1);
                    renderer.sortingOrder = i;
                    maxGroupNo++;
                    blockSettings.currentGroup = maxGroupNo;
                    blockSettings.row = j;
                    blockSettings.col = i;
                    block.transform.position = block.transform.position;
                    block.transform.parent = this.transform;
                    block.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                    allBlocks[j, i] = block;
                }
            }
        }
    }
    private bool possibleMoveExists()
    {
        foreach (int index in groups.Keys)
        {
            if (groups[index].Count > 1)
                return true;
        }
        return false;
    }
    private void ShuffleBoard()
    {
        //1. Copy to a new board
        List<GameObject> newboard = new List<GameObject>();
        for (int i = 1; i < height + 1; i++)
        {
            for (int j = 1; j < width + 1; j++)
            {
                if (allBlocks[j, i] == null)
                {
                    newboard.Add(allBlocks[j, i]);
                }
            }
        }
        for (int i = 1; i < height + 1; i++)
        {
            for (int j = 1; j < width + 1; j++)
            {
                int rngIndex = Random.Range(0, newboard.Count);
                Block block = newboard[rngIndex].GetComponent<Block>();
                block.col = i;
                block.row = j;
                allBlocks[j, i] = newboard[rngIndex];
                newboard.Remove(newboard[rngIndex]);

            }
        }
    }
    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while(possibleMoveExists())
        {
            yield return new WaitForSeconds(.5f);
        }
        if(!possibleMoveExists())
        {
            Debug.Log("Deadlocked");
            //ShuffleBoard(); Buggy
            yield return new WaitForSeconds(.5f);
        }
    }
    private int getGroupUniqueCount(List<GameObject> group)
    {
        List<GameObject> seen = new List<GameObject> { group[0] };
        int groupCount = 1;
        for(int i = 0; i < group.Count; i++)
        {
            if (!seen.Contains(group[i]))
            {
                groupCount++;
                seen.Add(group[i]);
            }
        }
        return groupCount;
    }
    void Update()
    {
        //checkForGroups();
        checkForGroupsRecursively();
        if (Input.GetMouseButtonDown(0))
        {
            int row = (int)(Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0.5f)).x;
            int col = (int)(Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0.5f)).y;
            if(width +1 > row && row > 0 && height + 1 > col && col > 0)
                if(allBlocks[row, col] != null && groups[allBlocks[row, col].GetComponent<Block>().currentGroup].Count > 1 )
                {
                    deleteGroup(groups[allBlocks[row, col].GetComponent<Block>().currentGroup]);
                    StartCoroutine(decreaseRowCo());
                }
        }
    }
}
