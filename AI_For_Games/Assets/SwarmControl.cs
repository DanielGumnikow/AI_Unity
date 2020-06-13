using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SwarmControl : MonoBehaviour
{
    public GameObject Zombie;               //  Prefab zombie from the actual scene
    public GameObject[] Zombies;            //  Array of all copy of the prefab zombie
    public GameObject[] spawnPoints;        //  Array of all spawnpoints in the scene
    public GameObject zombieLeader;         //  Reference to the leaderzombie
    public bool leaderExists = false;       //  Check if the leader has been assigned already
    //public Texture texture;                 //  

    public int attackingZombies = 0;        //  number of zombies which are in the attacking state 
    public int chasingZombies = 0;          //  number of zombies which are in the chasing state 

    /*
     * Call the spawnZombies method.
     */
    private void Start()
    {
        spawnZombies();
    }

    /*
     * Sets the layer of the gameobject and all of its childs
     */
    public static void SetLayer(Transform root, int layer)
    {
        Stack<Transform> children = new Stack<Transform>();
        children.Push(root);
        while (children.Count > 0)
        {
            Transform currentTransform = children.Pop();
            currentTransform.gameObject.layer = layer;
            foreach (Transform child in currentTransform)
                children.Push(child);
        }
    }


    /*
     *  Method for spawning all copys of the prefab zombie at the spawnpoint locations
     *  Setting up the leaderzombie and all of its variables, tags
     */
    private void spawnZombies()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject tempZombie = Instantiate(Zombie);
            Zombies[i] = tempZombie;
            Zombies[i].transform.position = spawnPoints[i].transform.position;
            if (i == 0)
            {
                EnemyAI zl = tempZombie.GetComponent<EnemyAI>();
                zl.SetLeader();
                tempZombie.tag = "Leader";
                SetLayer(tempZombie.transform,9);
                leaderExists = true;
            }
        }
    }
}
