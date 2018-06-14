using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Environment : MonoBehaviour
{
    #region private variables
    private Vector2 origin = new Vector2(-2f, 2f);
    
    Vector2 goalPosition;

    private float[,] rewardMatrix;
    private float[,] q_table;

    private string[] actions = new string[] { "up", "down", "left", "right" };

    private GameObject[,] tiles;

    private struct Pair
    {
        public Vector2 next_state;
        public float reward;

        public Pair(Vector2 ns, float r)
        {
            next_state = ns;
            reward = r;
        }
    }
    #endregion

    [Header("Scene Variables")]
    public int numberOfTiles = 5;
    public int numberOfEpisodes = 200;
    public float sleepTime = 0.0001f;

    [Header("Training Variables")]
    public float GAMMA = 0.9f;
    public float ALPHA = 0.1f;
    public float EPSILON = 0.9f;

    [Header("Prefabs")]
    public GameObject tile;
    public Sprite[] policy;

    void Start ()
    {
        tiles = new GameObject[numberOfTiles, numberOfTiles];

        goalPosition = new Vector2(numberOfTiles - 1, numberOfTiles - 1);

        rewardMatrix = new float[numberOfTiles, numberOfTiles];
        Helper.InitializeMatrix(ref rewardMatrix, 0);
        rewardMatrix[(int)goalPosition.x, (int)goalPosition.y] = 1;

        q_table = new float[numberOfTiles * numberOfTiles, actions.Length];

        CreateTiles();

        StartCoroutine(Learn());
    }

    private IEnumerator Learn()
    {
        for (int i = 0; i < numberOfEpisodes; i++)
        {
            int stepCounter = 0;
            bool isTerminated = false;

            Vector2 state = new Vector2(0, 0);
            tiles[numberOfTiles - 1, numberOfTiles - 1].GetComponent<SpriteRenderer>().color = new Color(0, 0, 1);

            MoveTile(state, i, stepCounter);

            while (!isTerminated)
            {
                if(stepCounter > 200)
                {
                    break;
                }

                yield return new WaitForSeconds(sleepTime);

                string action = TakeStep(state);
                Pair pair = GetFeedback(state, action);
                
                Vector2 next_state = pair.next_state;
                float reward = pair.reward;

                if (reward == -1) continue;

                float prediction = q_table[Helper.Index(state, numberOfTiles), actions.ToList().IndexOf(action)];
                float target;

                if(next_state != new Vector2(-1, -1))
                {
                    target = reward + GAMMA * Helper.Max(Helper.GetRow(q_table, Helper.Index(next_state, numberOfTiles)));
                }
                else
                {
                    target = reward;
                    isTerminated = true;
                }

                tiles[(int)state.x, (int)state.y].GetComponent<SpriteRenderer>().color = new Color(0.9f, 0.9f, 0.9f);

                q_table[Helper.Index(state, numberOfTiles), actions.ToList().IndexOf(action)] += ALPHA * (target - prediction);

                int max = Helper.Max(Helper.GetRow(q_table, Helper.Index(state, numberOfTiles)));
                float[] row = Helper.GetRow(q_table, Helper.Index(state, numberOfTiles));

                
                if (row[max] != 0 && state != goalPosition)
                    tiles[(int)state.x, (int)state.y].GetComponent<Tile>().policy.sprite = policy[max];

                tiles[(int)state.x, (int)state.y].GetComponent<Tile>().up.text =    string.Format("{0:0.00}", row[0]);
                tiles[(int)state.x, (int)state.y].GetComponent<Tile>().down.text =  string.Format("{0:0.00}", row[1]);
                tiles[(int)state.x, (int)state.y].GetComponent<Tile>().left.text =  string.Format("{0:0.00}", row[2]);
                tiles[(int)state.x, (int)state.y].GetComponent<Tile>().right.text = string.Format("{0:0.00}", row[3]);

                state = next_state;

                MoveTile(state, i, stepCounter+1);
                stepCounter++;
            }
        }
    }

    private void MoveTile(Vector2 state, int episode, int step)
    {
        if(state == new Vector2(-1, -1))
        {
            Debug.Log(string.Format("Episode: {0} | Total Steps: {1}", episode + 1, step));
        }
        else
        {
            tiles[(int)state.x, (int)state.y].GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
        }
    }

    private Pair GetFeedback(Vector2 state, string action)
    {
        Vector2 next_state = new Vector2();
        float reward = -1;

        if (state == goalPosition)
        {
            //terminal state
            next_state = new Vector2(-1, -1);    
            reward = rewardMatrix[(int)state.x, (int)state.y];
        }
        else
        {
            switch (action)
            {
                case "up":
                    if(state.y == 0) //hit up wall
                        next_state = state;
                    else
                    {
                        next_state = new Vector2(state.x, state.y - 1);
                        reward = rewardMatrix[(int)state.x, (int)state.y];
                    }
                    break;

                case "down":
                    if (state.y == numberOfTiles - 1) //hit bottom wall
                        next_state = state;
                    else
                    {
                        next_state = new Vector2(state.x, state.y + 1);
                        reward = rewardMatrix[(int)state.x, (int)state.y];
                    }
                    break;

                case "right":
                    if (state.x == numberOfTiles - 1) //hit right wall
                        next_state = state;
                    else
                    {
                        next_state = new Vector2(state.x + 1, state.y);
                        reward = rewardMatrix[(int)state.x, (int)state.y];
                    }
                    break;

                case "left":
                    if (state.x == 0) //hit left wall
                        next_state = state;
                    else
                    {
                        next_state = new Vector2(state.x - 1, state.y);
                        reward = rewardMatrix[(int)state.x, (int)state.y];
                    }
                    break;
            }
        }
        
        return new Pair(next_state, reward);
    }

    private void CreateTiles()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            for (int j = 0; j < numberOfTiles; j++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(origin.x + i, origin.y - j), Quaternion.identity);

                if (new Vector2(i, j) == goalPosition)
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1);
                }

                tiles[i, j] = newTile;
            }
        }
    }

    private string TakeStep(Vector2 state)
    {
        string actionName = "";
        int row = Helper.Index(state, numberOfTiles);

        float[] stateActions = Helper.GetRow(q_table, row);

        if(Random.value > EPSILON || stateActions.All(f => f == 0))
        {
            actionName = actions[Random.Range(0, actions.Length)];
        }
        else
        {
            actionName = actions[Helper.Max(stateActions)];
        }

        return actionName;
    }
}
