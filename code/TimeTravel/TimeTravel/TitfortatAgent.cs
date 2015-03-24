using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    // Dummy implementation of the IAgent interface for testing
    public class TitForTatAgent : BasicAgent
    {

        // Constructor
        public TitForTatAgent(string name, int numTimePeriods, int numAgents, int health, int agentID) : 
            base(name, numTimePeriods, numAgents, health, agentID)
        {
        }

        // TitForTatAgent is generally a mellow (and yellow) unless you betray him
        public override Color AgentColor { get { return Color.Goldenrod; } }

        // TitForTat trusts unless betrayed, then retailiates
        public override AgentAction PickAction(GameState gs, IAgent agent, int time)
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            if (time == 0) return AgentAction.TRUST; // default is to trust
            if (trustRecords[agent.AgentID][time - 1] < 0)
            {
                return AgentAction.BETRAY;
            }
            return AgentAction.TRUST;
        }

    }
}
