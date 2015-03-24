using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    // The condition of an agent (alive/dead/zombified/etc)
    public enum AgentCondition
    {
        ALIVE, DEAD
    }

    // Actions which an agent can take
    public enum AgentAction
    {
        TRUST, BETRAY, NONE
    }

    // Represents an objective an agent wishes to meet
    public sealed class AgentGoal
    {
        public AgentGoal(IAgent target, AgentCondition goalCondition, int deadline)
        {
            this.Target = target;
            this.GoalCondition = goalCondition;
            this.Deadline = deadline;
        }

        public IAgent Target { get; private set; }
        public AgentCondition GoalCondition { get; private set; }
        public int Deadline { get; private set; } // time by which this goal must be met

        public bool GoalSatisfied( int time, bool earlyEvaluation )
        {
            // Console.WriteLine( "Deadline = " + Deadline + " and time = " + time );
            if (Deadline <= time || (Deadline > time && earlyEvaluation))
            {
                // Console.WriteLine( "Within time!" );
                if (Target.GetCondition(time) == GoalCondition)
                {
                    // Console.WriteLine( "TRUE!" );
                    return true;
                }
            }
            return false;
        }

        public int TimeToDeadline( int time )
        {
            return Deadline - time;
        }

        public override string ToString()
        {
            return (Target.Name + " must be " + GoalCondition + " at the end of time " + Deadline + ".");
        }

        // Same as above, but allows you to pass in a name for the agent
        public string ToString( string agentName )
        {
            return agentName + " must be " + GoalCondition + " at the end of time " + Deadline + ".";
        }
    }

    // Agent interface
    public interface IAgent
    {
        // Properties
        int[] HealthRecord { get; }
        int Health { get; }
        string Name { get; }
        int AgentID { get; } // unique ID for each agent (gets used for indexing)
        Color AgentColor { get; }
        
        // Adds a goal
        bool AddGoal( AgentGoal goal );
        // Adds knowledge abouta new agent
        void AddNewAgentKnowledge( IAgent agent, int newNumAgents );
        
        // Sets the agent's condition
        void SetCondition(AgentCondition condition, int time);

        // Gets a list of actions from a given time period (does not recompute actions (see below for functions that do)))
        List<Pair<IAgent, AgentAction>> GetActions( GameState gs, int time );

        // Given a particular state/time, selects actions to perform against other agents (primary AI update)
        // List<Pair<IAgent, AgentAction>> PickActions( GameState gs, int time );
        // Same as the above, but also takes an action that *must* be performed
        // List<Pair<IAgent, AgentAction>> PickActions( GameState gs, int time, Pair<IAgent, AgentAction> requiredAction );

        // Given a particular state/time, selects actions to perform against other agents (primary AI update)
        // Returns a triple of: FromAgent, ToAgent, Action
        List<Triple<IAgent, IAgent, AgentAction>> PickActions2( GameState gameState, int i );
        // Same as the above, but also takes an action that *must* be performed
        List<Triple<IAgent, IAgent, AgentAction>> PickActions2( GameState gameState, int proposedTime, Triple<IAgent, IAgent, AgentAction> proposedAction );

        // Given a particular state/time, processes the actions performed by other agents (secondary update)
        // void ReceiveActions( GameState gs, List<Pair<IAgent, AgentAction>> actions, int time );
        void ReceiveActions2( GameState gameState, List<Triple<IAgent, IAgent, AgentAction>> allActions2, int i );

        // used to get condition at a specific time
        AgentCondition GetCondition(int time);
    }
}
