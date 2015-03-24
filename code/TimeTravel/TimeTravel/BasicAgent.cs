using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    // Dummy implementation of the IAgent interface for testing
    public abstract class BasicAgent : IAgent
    {
        // Parameters
        // Note: for now, positive values mean trust, negative values indicated untrustworthiness
        // zero is effectively neutral
        public int[][] trustRecords;
        public AgentAction[][] actionRecords;
        public AgentAction[][] enforcedRecords; // record any actions you were forced to do
        public List<AgentGoal> goals;
        public AgentCondition[] conditionRecords;

        public Random random;

        public BasicAgent()
        {
            Console.WriteLine("called basicagent constructor w/0 arguments");
        }

        // Constructor
        public BasicAgent( string name, int numTimePeriods, int numAgents, int health, int agentID )
        {
            this.Name = name;
            this.Health = health;
            this.HealthRecord = new int[numTimePeriods+1];
            this.trustRecords = new int[numAgents][];
            this.actionRecords = new AgentAction[numAgents][];
            this.enforcedRecords = new AgentAction[numAgents][];
            this.conditionRecords = new AgentCondition[numTimePeriods+1];
            for ( int i = 0; i < numAgents; i++ ) {
                trustRecords[i] = new int[numTimePeriods];
                actionRecords[i] = new AgentAction[numTimePeriods];
                enforcedRecords[i] = new AgentAction[numTimePeriods];
                for ( int j = 0; j < numTimePeriods; j++ ) {
                    HealthRecord[j] = health;
                    conditionRecords[j] = AgentCondition.ALIVE;
                    trustRecords[i][j] = 0;
                    actionRecords[i][j] = AgentAction.NONE;
                    enforcedRecords[i][j] = AgentAction.NONE;
                }
            }

            this.goals = new List<AgentGoal>();
            this.AgentID = agentID;
            this.random = new Random();
        }

        // Properties
        public int Health { get; set; }
        public int[] HealthRecord { get; set; }
        public int AgentID { get; set; }
        public string Name { get; set; }
        public virtual Color AgentColor { get { return Color.White; }  }

        // TODO goals are not prioritized (first in is highest priority)
        public bool AddGoal( AgentGoal goal )
        {
            goals.Add( goal );
            return true;
        }

        // This is really ugly...it's mostly just recopying tables to include a new agent
        public void AddNewAgentKnowledge( IAgent agent, int newNumAgents )
        {
            if ( trustRecords.Length != actionRecords.Length ) {
                throw new Exception("Somehow, the number of actions we perform is not equivalent to the number of agents we trust!");
            }
            int oldNumAgents = trustRecords.Length;

            // if we already store stuff for enough agents, then this function call is meaningless
            while ( oldNumAgents < newNumAgents ) {
                // Copy over old trust values/action records
                int[][] newTrustRecords = new int[oldNumAgents + 1][];
                AgentAction[][] newActionRecords = new AgentAction[oldNumAgents + 1][];

                for ( int i = 0; i < oldNumAgents; i++ ) {
                    newTrustRecords[i] = trustRecords[i];
                    newActionRecords[i] = actionRecords[i];
                }

                // Add a new row for the new agent
                if ( trustRecords[0].Length != actionRecords[0].Length ) {
                    throw new Exception( "Somehow, our trust table and action table don't agree on the number of time periods!" );
                }
                int numTimePeriods = trustRecords[0].Length;
                newTrustRecords[oldNumAgents] = new int[numTimePeriods];
                newActionRecords[oldNumAgents] = new AgentAction[numTimePeriods];
                for ( int j = 0; j < numTimePeriods; j++ ) { // sets to defaults
                    newTrustRecords[oldNumAgents][j] = 0;
                    newActionRecords[oldNumAgents][j] = AgentAction.NONE;
                }

                // Reset trust/action records
                trustRecords = newTrustRecords;
                actionRecords = newActionRecords;
                oldNumAgents = trustRecords.Length;
            }
        }

        // Gets if the agent is alive/dead/zombified/poisoned/petrified/turned into a frog/etc
        public AgentCondition GetCondition(int time)
        {
            return this.conditionRecords[time];
            /*if ( Health > 0 ) {
                return AgentCondition.ALIVE;
            }
            return AgentCondition.DEAD;
             */
        }

        public void SetCondition(AgentCondition condition, int time)
        {
            this.conditionRecords[time] = condition;
        }

        // Returns a list of actions at the given time
        public List<Pair<IAgent, AgentAction>> GetActions( GameState gs, int time )
        {
            List<Pair<IAgent, AgentAction>> actionPairs = new List<Pair<IAgent, AgentAction>>();

            // goes over the list of agents
            for ( int i = 0; i < gs.NumAgents; i++ ) {
                if ( i == AgentID ) continue; // skip over ourselves
                IAgent otherAgent = gs.GetAgentByID( i );
                //Console.WriteLine("time is : " + time);
                actionPairs.Add( new Pair<IAgent, AgentAction>( otherAgent, actionRecords[i][time] ) );
            }

            return actionPairs;
        }

        /*
        // Picks actions given a particular time and state
        public List<Pair<IAgent, AgentAction>> PickActions( GameState gs, int time )
        {
            List<Pair<IAgent, AgentAction>> actionPairs = new List<Pair<IAgent, AgentAction>>();

            // goes over the list of agents
            for ( int i = 0; i < gs.NumAgents; i++ ) {
                if ( i == AgentID ) continue; // skip over ourselves
                IAgent otherAgent = gs.GetAgentByID( i );

                // select actions (only if other agent is alive)
                if ( otherAgent.GetCondition(time) != AgentCondition.DEAD ) {
                    AgentAction action = PickAction( gs, otherAgent, time );
                    actionRecords[i][time] = action;
                    actionPairs.Add( new Pair<IAgent, AgentAction>( otherAgent, action ) );
                }
            }

            return actionPairs;
        }*/

        // Picks actions given a particular time and state
        // Triple is <FromAgent, ToAgent, Action>
        public List<Triple<IAgent, IAgent, AgentAction>> PickActions2(GameState gs, int time)
        {
            List<Triple<IAgent, IAgent, AgentAction>> actionTriples = new List<Triple<IAgent, IAgent, AgentAction>>();

            // goes over the list of agents
            for (int i = 0; i < gs.NumAgents; i++)
            {
                if (i == AgentID) continue; // skip over ourselves
                if (enforcedRecords[i][time] != AgentAction.NONE)
                {
                    actionRecords[i][time] = enforcedRecords[i][time];
                    Triple<IAgent, IAgent, AgentAction> enfTriple =
                        new Triple<IAgent, IAgent, AgentAction>(this, gs.GetAgentByID(i), enforcedRecords[i][time]);
                    actionTriples.Add(enfTriple);
                    continue; // skip over any agents with enforced actions
                }
                IAgent otherAgent = gs.GetAgentByID(i);

                // select actions (only if other agent is alive)
                if (otherAgent.GetCondition(time) != AgentCondition.DEAD)
                {
                    AgentAction action = PickAction(gs, otherAgent, time);
                    actionRecords[i][time] = action;
                    actionTriples.Add(new Triple<IAgent, IAgent, AgentAction>(this, otherAgent, action));
                }
            }

            return actionTriples;
        }

        // Picks actions given a particular time and state
        // Triple is <FromAgent, ToAgent, Action>
        public List<Triple<IAgent, IAgent, AgentAction>> PickActions2(GameState gs, int time, 
            Triple<IAgent, IAgent, AgentAction> requiredAction) {
        
            if (requiredAction.b.GetCondition(time) == AgentCondition.DEAD)
            {
                throw new Exception("Agent is attempting to perform an operation on a dead agent.");
            }
            enforcedRecords[requiredAction.b.AgentID][time] = requiredAction.c; // record enforced action

            List<Triple<IAgent, IAgent, AgentAction>> actionTriples = new List<Triple<IAgent, IAgent, AgentAction>>();
            actionTriples.Add(requiredAction);

            // goes over the list of agents
            for (int i = 0; i < gs.NumAgents; i++)
            {
                if (i == AgentID || i == requiredAction.b.AgentID)
                {
                    actionRecords[i][time] = requiredAction.c;
                    continue; // skip over ourselves, overridden agent
                }
                if (enforcedRecords[i][time] != AgentAction.NONE) // if agent is dead this might be a problem...
                {
                    actionRecords[i][time] = enforcedRecords[i][time];
                    Triple<IAgent,IAgent,AgentAction> enfTriple = 
                        new Triple<IAgent, IAgent, AgentAction>(this, gs.GetAgentByID(i), enforcedRecords[i][time]);
                    actionTriples.Add(enfTriple);
                    continue; // skip over any agents with enforced actions
                }
                IAgent otherAgent = gs.GetAgentByID(i);

                // select actions (only if other agent is alive)
                if (otherAgent.GetCondition(time) != AgentCondition.DEAD)
                {
                    AgentAction action = PickAction(gs, otherAgent, time);
                    actionRecords[i][time] = action;
                    actionTriples.Add(new Triple<IAgent, IAgent, AgentAction>(this, otherAgent, action));
                }
            }

            return actionTriples;
        }

        /*
        // A more specific version of the above function, but takes a required action that will override any action from the AI
        public List<Pair<IAgent, AgentAction>> PickActions( GameState gs, int time, Pair<IAgent, AgentAction> requiredAction )
        {
            if ( requiredAction.a.GetCondition(time) == AgentCondition.DEAD ) {
                throw new Exception( "Agent is attempting to perform an operation on a dead agent." );
            }

            List<Pair<IAgent, AgentAction>> actionPairs = new List<Pair<IAgent, AgentAction>>();
            actionPairs.Add( requiredAction );

            // goes over the list of agents
            for ( int i = 0; i < gs.NumAgents; i++ ) {
                if ( i == AgentID || i == requiredAction.a.AgentID ) {
                    actionRecords[i][time] = requiredAction.b;
                    continue; // skip over ourselves, overriden agent
                }
                IAgent otherAgent = gs.GetAgentByID( i );

                // select actions (only if other agent is alive)
                AgentAction action = PickAction( gs, otherAgent, time );
                actionRecords[i][time] = action;
                actionPairs.Add( new Pair<IAgent, AgentAction>( otherAgent, action ) );
            }

            return actionPairs;
        }*/
        
        /*
        // Processes the actions from other agents at a given time and state
        public void ReceiveActions( GameState gs, List<Pair<IAgent, AgentAction>> actions, int time )
        {
            //HealthRecord[time] = Health; // track health according to time
            foreach ( Pair<IAgent, AgentAction> p in actions ) {
                int otherAgentID = p.a.AgentID;
                if ( AgentID != otherAgentID ) continue;
                AgentAction otherAction = p.b; // get their action
                AgentAction agentAction = actionRecords[AgentID][time];//actionRecords[otherAgentID][time]; // get my action

                // compares actions and adjust health/trust accordingly
                if ( agentAction == AgentAction.TRUST && otherAction == AgentAction.TRUST ) {
                    HealthRecord[time+1] = HealthRecord[time] + gs.TRUST_TRUST_VALUE;
                    //Health += gs.TRUST_TRUST_VALUE;
                    trustRecords[otherAgentID][time] = 1;
                } else if ( agentAction == AgentAction.TRUST && otherAction == AgentAction.BETRAY ) {
                    HealthRecord[time + 1] = HealthRecord[time] + gs.TRUST_BETRAY_VALUE;
                    //Health += gs.TRUST_BETRAY_VALUE;
                    trustRecords[otherAgentID][time] = -1;
                } else if ( agentAction == AgentAction.BETRAY && otherAction == AgentAction.TRUST ) {
                    HealthRecord[time + 1] = HealthRecord[time] + gs.BETRAY_TRUST_VALUE;
                    //Health += gs.BETRAY_TRUST_VALUE;
                    trustRecords[otherAgentID][time] = -1;
                } else {
                    HealthRecord[time + 1] = HealthRecord[time] + gs.BETRAY_BETRAY_VALUE;
                    //Health += gs.BETRAY_BETRAY_VALUE;
                    trustRecords[otherAgentID][time] = -1;
                }
            }
        }*/

        // Processes the actions from other agents at a given time and state
        public void ReceiveActions2(GameState gs, List<Triple<IAgent, IAgent, AgentAction>> actions, int time)
        {
            int healthAdd = 0;
            foreach (Triple<IAgent, IAgent, AgentAction> trip in actions)
            {
                int fromAgentID = trip.a.AgentID;
                int toAgentID = trip.b.AgentID;
                if (toAgentID != AgentID) continue; // skip if not intended for us
                if (gs.GetAgentByID(toAgentID).GetCondition(time) == AgentCondition.DEAD)
                {
                    continue; // skip if sender is dead
                }
                AgentAction otherAction = trip.c; // get their action
                AgentAction agentAction = actionRecords[fromAgentID][time]; // get my action

                //Health = HealthRecord[time];
                // compares actions and adjust health/trust accordingly
                if ( agentAction == AgentAction.TRUST && otherAction == AgentAction.TRUST ) {
                    healthAdd += gs.TRUST_TRUST_VALUE;
                    //HealthRecord[time + 1] = HealthRecord[time] + gs.TRUST_TRUST_VALUE;
                    //Health += gs.TRUST_TRUST_VALUE;
                    trustRecords[fromAgentID][time] = 1;
                } else if ( agentAction == AgentAction.TRUST && otherAction == AgentAction.BETRAY ) {
                    healthAdd += gs.TRUST_BETRAY_VALUE;
                    //HealthRecord[time + 1] = HealthRecord[time] + gs.TRUST_BETRAY_VALUE;
                    //Health += gs.TRUST_BETRAY_VALUE;
                    trustRecords[fromAgentID][time] = -1;
                } else if ( agentAction == AgentAction.BETRAY && otherAction == AgentAction.TRUST ) {
                    healthAdd += gs.BETRAY_TRUST_VALUE;
                    //HealthRecord[time + 1] = HealthRecord[time] + gs.BETRAY_TRUST_VALUE;
                    //Health += gs.BETRAY_TRUST_VALUE;
                    trustRecords[fromAgentID][time] = 1;
                } else if ( agentAction == AgentAction.BETRAY && otherAction == AgentAction.BETRAY ) {
                    healthAdd += gs.BETRAY_BETRAY_VALUE;
                    //HealthRecord[time + 1] = HealthRecord[time] + gs.BETRAY_BETRAY_VALUE;
                    //Health += gs.BETRAY_BETRAY_VALUE;
                    trustRecords[fromAgentID][time] = -1;
                } else {
                    throw new Exception( "Incorrect actions received from an agent." );
                }
                
            }
            HealthRecord[time + 1] = HealthRecord[time] + healthAdd;
            if ( HealthRecord[time + 1] < 1 ) // set to dead if you are
            {
                for ( int t = time + 1; t < conditionRecords.Length; t++ ) {
                    this.SetCondition( AgentCondition.DEAD, t );
                }
            } else {
                this.SetCondition( AgentCondition.ALIVE, time + 1 );
            }
        }

        // This is where the decision-making should go (i.e. the actual heavy-duty AI component)
        public abstract AgentAction PickAction(GameState gs, IAgent agent, int time);

        // Returns the trust value towards a given agent up until this point in time
        public int ComputeTrustLevel( IAgent agent, int time )
        {
            int trustLevel = 0;
            for ( int i = 0; i <= time; i++ ) {
                trustLevel += trustRecords[agent.AgentID][i];
            }
            return trustLevel;
        }
    }
}
