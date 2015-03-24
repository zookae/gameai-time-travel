using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    class TrustAgent : BasicAgent
    {
        public TrustAgent(string name, int numTimePeriods, int numAgents, int health, int agentID) : 
            base(name, numTimePeriods, numAgents, health, agentID)
        {
        }

        // TrustAgent loves everyone, so he/she is pink, I think.
        public override Color AgentColor { get { return Color.Orchid; } }

        // TitForTat trusts unless betrayed, then retailiates
        public override AgentAction PickAction(GameState gs, IAgent agent, int time)
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            return AgentAction.TRUST;
        }
    }
}
