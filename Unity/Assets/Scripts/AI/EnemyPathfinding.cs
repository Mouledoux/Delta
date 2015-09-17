using UnityEngine;
using System.Collections;

public class EnemyPathfinding : MonoBehaviour
{
    public Transform m_Goal;
    NavMeshAgent agent;

	// Use this for initialization
	void Start ()
    {
        agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        agent.destination = m_Goal.position;
	}
}
