using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private float healthPoints = 100;   // Healthpoints of player
    public Slider healthBar;            // UI healthbar
    public GameObject UI_Fill;          // Colored fill of slider
    public bool playerDown = false;     // is player dead?
    private float speed = 10.0f;        // speed of player movement
    
    
    /*
     * Initialize the healthbar value at the start of the scene.
     */
    private void Start()
    {
        healthBar.value = healthPoints;
    }

    /*
     * Return the value of the current healthpoints of the player.
     */
    public float getHealthPoints() 
    {
        return healthPoints;
    }

    /*
     * Damage the player and his current health.
     * Update the healthbar value.
     * If the health reaches 0 the gameObject will be set to inactive.
     */
    public void damageHealthPoints(float damage)
    {
        healthPoints -= damage ;
        healthBar.value = healthPoints;

        if (healthPoints == 0) 
        {
            playerDown = true;
            gameObject.SetActive(false);
            UI_Fill.SetActive(false);
        }   
            
    }

    /*
     * Setup the simple movement for the player sphere for test purposes.
     */
    private void Update()
    {
        movePlayer(new Vector3(Input.GetAxis("Horizontal"), 0 ,Input.GetAxis("Vertical")));
    }

    /*
     * Translate the transform of the sphere depending on the player input and speed assigned to it.
     */
    private void movePlayer(Vector3 direction)
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
