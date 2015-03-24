using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    // Dummy implementation of the IAgent interface for testing
    public class BullyAgent : BasicAgent
    {
        // Constructor
        public BullyAgent(string name, int numTimePeriods, int numAgents, int health, int agentID) :
            base(name, numTimePeriods, numAgents, health, agentID)
        {
        }

        // BullyAgent is devious and envious, so he is rather green....
        public override Color AgentColor { get { return Color.OliveDrab; } }

        // BullyAgent betrays any agent that trusted it, otherwise trusts
        public override AgentAction PickAction(GameState gs, IAgent agent, int time)
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            if (time == 0) return AgentAction.BETRAY; // default is to betray
            if (trustRecords[agent.AgentID][time - 1] > 0)
            {
                return AgentAction.BETRAY;
            }
            return AgentAction.TRUST;
        }



    }
}
