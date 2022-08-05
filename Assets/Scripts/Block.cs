using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int currentGroup;
    public string colorType;
    private Board Board;
    public int row;
    public int col;
    // Start is called before the first frame update
    void Start()
    {
        Board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Board.allBlocks[row, col] != this.gameObject)
        {
            Board.allBlocks[(int) this.gameObject.transform.position.x, (int)this.gameObject.transform.position.y] = null;
            Board.allBlocks[row, col] = this.gameObject;
            transform.name = (row -1) + "," + (col-1);
            SpriteRenderer renderer = transform.gameObject.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = col;
        }
        transform.position = Vector2.Lerp(transform.position, new Vector2(row, col), .4f);
    }
}
