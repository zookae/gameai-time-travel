using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    class SimpleGoalAgent : BasicAgent
    {
        public AgentGoal Goal { get; private set; }

        public SimpleGoalAgent( string name, int numTimePeriods, int numAgents, int health, int agentID, AgentGoal goal ) :
            base( name, numTimePeriods, numAgents, health, agentID )
        {
            this.Goal = goal;
        }

        // Simple 
        public override Color AgentColor { get { return Color.Teal; } }

        // SimpleGoalAgent tries to figure out what to do in order to achieve a goal
        public override AgentAction PickAction( GameState gs, IAgent agent, int time )
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            if ( time > Goal.Deadline && !Goal.GoalSatisfied( time, false ) ) { // if we are past the deadline and the goal is not satisfied
                return AgentAction.BETRAY; // betray everyone out of spite
            }

            if ( agent.AgentID == Goal.Target.AgentID && !Goal.GoalSatisfied( time, false ) ) { // our goal isn't satisfied yet and this is the agent we're concerned with
                if ( Goal.GoalCondition == AgentCondition.ALIVE ) {
                    return AgentAction.TRUST; // trust the agent, as it will help to keep it alive (even at the cost of our health)
                } else {
                    return AgentAction.BETRAY; // if we are trying to kill the agent, always betray it
                }
            } else {
                // Try and stay alive, so we can ensure that the agent in our goal lives/dies (and we can see it through)
                int trustLvl = ComputeTrustLevel(agent, time);
                if (trustLvl > 3) // trust those deemed trustworthy
                {
                    return AgentAction.TRUST;
                }
                else if (trustLvl < -3) // betray treacherous
                {
                    return AgentAction.BETRAY;
                }
                return AgentAction.TRUST; // start by trusting
            }
        }
    }
}
