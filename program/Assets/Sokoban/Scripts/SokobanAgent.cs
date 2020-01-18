using System;
using UnityEngine;
using System.Linq;
using MLAgents;

public class SokobanAgent : Agent
{
    [Header("Specific to GridWorld")]
    private SokobanAcademy academy;
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    [Tooltip("Because we want an observation right before making a decision, we can force " + 
             "a camera to render before making a decision. Place the agentCam here if using " +
             "RenderTexture as observations.")]
    public Camera renderCamera;

    [Tooltip("Selecting will turn on action masking. Note that a model trained with action " +
             "masking turned on may not behave optimally when action masking is turned off.")]
    public bool maskActions = true;

    private const int NoAction = 0;  // do nothing!
    private const int Up = 1;
    private const int Down = 2;
    private const int Left = 3;
    private const int Right = 4;

    public override void InitializeAgent()
    {
        academy = FindObjectOfType(typeof(SokobanAcademy)) as SokobanAcademy;
    }

    public override void CollectObservations()
    {
        // There are no numeric observations to collect as this environment uses visual
        // observations.

        // Mask the necessary actions if selected by the user.
        if (maskActions)
        {
            SetMask();
        }
    }

    /// <summary>
    /// Applies the mask for the agents action to disallow unnecessary actions.
    /// </summary>
    private void SetMask()
    {
        // Prevents the agent from picking an action that would make it collide with a wall
        var positionX = (int) transform.position.x;
        var positionZ = (int) transform.position.z;
        var maxPosition = academy.gridSize - 1;

        if (positionX == 0)
        {
            SetActionMask(Left);
        }

        if (positionX == maxPosition)
        {
            SetActionMask(Right);
        }

        if (positionZ == 0)
        {
            SetActionMask(Down);
        }

        if (positionZ == maxPosition)
        {
            SetActionMask(Up);
        }
    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-0.01f);
        int action = Mathf.FloorToInt(vectorAction[0]);

        var targetPos = transform.position;
        var actionDirection = GetActionDirection(action);
        targetPos += actionDirection;

        var blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));
        if (IsTag(blockTest, "wall") == false)
        {
            if (IsTag(blockTest, "pit") || IsTag(blockTest, "goal"))
            {
                Done();
                SetReward(-1F);
            } else if (IsTag(blockTest, "box"))
            {
                // 박스 밀기
                var box = blockTest[0].gameObject;
                var nextBoxPos = box.transform.position + actionDirection;
                var boxTest = Physics.OverlapBox(nextBoxPos, new Vector3(0.3f, 0.3f, 0.3f));
                if (IsTag(boxTest, "pit"))
                {
                    Done();
                    SetReward(-1F);
                } else if (IsTag(boxTest, "box") || IsTag(boxTest, "wall"))
                {
                    SetReward(-0.1F);
                } else if (IsTag(boxTest, "goal"))
                {
                    var goal = boxTest[0].gameObject;
                    transform.position = targetPos;
                    if (academy.RemoveBoxGoal(box, goal) == 0) Done();
                    SetReward(1.0F);
                }
                else
                {
                    box.transform.position = nextBoxPos;
                    transform.position = targetPos;
                    SetReward(0.1F);
                }
            }
            else
            {
                transform.position = targetPos;
            }
        }
    }

    private Vector3 GetActionDirection(int action)
    {
        switch (action)
        {
            case NoAction:
                // do nothing
                return Vector3.zero;
            case Right:
                return new Vector3(1f, 0, 0f);
            case Left:
                return new Vector3(-1f, 0, 0f);
            case Up:
                return new Vector3(0f, 0, 1f);
            case Down:
                return new Vector3(0f, 0, -1f);
            default:
                throw new ArgumentException("Invalid action value");
        }
    }

    private static bool IsTag(Collider[] blockTest, string tag)
    {
        return blockTest.Where(col => col.gameObject.CompareTag(tag)).ToArray().Length > 0;
    }

    // to be implemented by the developer
    public override void AgentReset()
    {
        academy.AcademyReset();
    }

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {
        if(renderCamera != null)
        {
            renderCamera.Render();
        }

        if (!academy.GetIsInference())
        {
            RequestDecision();
        }
        else
        {
            if (timeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                timeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                timeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
