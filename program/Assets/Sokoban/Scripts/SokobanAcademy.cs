﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MLAgents;
using Random = UnityEngine.Random;


public class SokobanAcademy : Academy
{
    [HideInInspector]
    public List<GameObject> actorObjs;
    [HideInInspector]
    public int[] players;

    public GameObject trueAgent;

    public int gridSize;
    private int numBoxes;

    public GameObject camObject;
    Camera cam;
    Camera agentCam;

    public GameObject agentPref;
    public GameObject goalPref;
    public GameObject pitPref;
    public GameObject boxPref;
    GameObject[] objects;

    GameObject plane;
    GameObject sN;
    GameObject sS;
    GameObject sE;
    GameObject sW;

    public override void InitializeAcademy()
    {
        gridSize = (int)resetParameters["gridSize"];
        cam = camObject.GetComponent<Camera>();

        objects = new GameObject[] {agentPref, goalPref, pitPref, boxPref};

        agentCam = GameObject.Find("agentCam").GetComponent<Camera>();

        actorObjs = new List<GameObject>();

        plane = GameObject.Find("Plane");
        sN = GameObject.Find("sN");
        sS = GameObject.Find("sS");
        sW = GameObject.Find("sW");
        sE = GameObject.Find("sE");
    }

    public void SetEnvironment()
    {
        gridSize = (int) resetParameters["gridSize"];
        numBoxes = (int) resetParameters["numBoxes"];
        cam.transform.position = new Vector3(-((int)resetParameters["gridSize"] - 1) / 2f, 
                                             (int)resetParameters["gridSize"] * 1.25f, 
                                             -((int)resetParameters["gridSize"] - 1) / 2f);
        cam.orthographicSize = ((int)resetParameters["gridSize"] + 5f) / 2f;

        List<int> playersList = new List<int>();

        for (int i = 0; i < numBoxes; i++)
        {
            playersList.Add(3);
        }
        
        for (int i = 0; i < (int)resetParameters["numObstacles"]; i++)
        {
            playersList.Add(2);
        }

        for (int i = 0; i < (int)resetParameters["numGoals"]; i++)
        {
            playersList.Add(1);
        }
        players = playersList.ToArray();

        plane.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
        plane.transform.position = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);
        sN.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sS.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sN.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
        sS.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
        sE.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sW.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sE.transform.position = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
        sW.transform.position = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);

        agentCam.orthographicSize = (gridSize) / 2f;
        agentCam.transform.position = new Vector3((gridSize - 1) / 2f, gridSize + 1f, (gridSize - 1) / 2f);

    }

    public override void AcademyReset()
    {
        Debug.Log($"AcademyReset: {DateTime.Now}");
        foreach (GameObject actor in actorObjs)
        {
            DestroyImmediate(actor);
        }
        SetEnvironment();

        actorObjs.Clear();

        HashSet<int> numbers = new HashSet<int>();
        while (numbers.Count < numBoxes)
        {
            var x = Random.Range(1, gridSize - 1);
            var y = Random.Range(1, gridSize - 1);
            numbers.Add(x + gridSize * y);
        }
        while (numbers.Count < players.Length + 1)
        {
            numbers.Add(Random.Range(0, gridSize * gridSize));
        }
        int[] numbersA = Enumerable.ToArray(numbers);

        for (int i = 0; i < players.Length; i++)
        {
            int x = (numbersA[i]) / gridSize;
            int y = (numbersA[i]) % gridSize;
            GameObject actorObj = Instantiate(objects[players[i]]);
            actorObj.transform.position = new Vector3(x, -0.25f, y);
            actorObjs.Add(actorObj);
        }

        int x_a = (numbersA[players.Length]) / gridSize;
        int y_a = (numbersA[players.Length]) % gridSize;
        trueAgent.transform.position = new Vector3(x_a, -0.25f, y_a);

    }

    public int RemoveBoxGoal(GameObject box, GameObject goal)
    {
        DestroyImmediate(box);
        DestroyImmediate(goal);
        actorObjs.Remove(box);
        actorObjs.Remove(goal);

        return actorObjs.Count(x => x.CompareTag("box"));
    }

    public override void AcademyStep()
    {

    }
}
