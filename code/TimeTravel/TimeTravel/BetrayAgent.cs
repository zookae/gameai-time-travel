using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    class BetrayAgent : BasicAgent
    {
        public BetrayAgent(string name, int numTimePeriods, int numAgents, int health, int agentID) : 
            base(name, numTimePeriods, numAgents, health, agentID)
        {
        }

        // BetrayAgent is colored red, because he/she is angry and full of spite.
        public override Color AgentColor { get { return Color.Firebrick; } }

        // BetrayAgent will always betray (or do nothing)
        public override AgentAction PickAction(GameState gs, IAgent agent, int time)
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            return AgentAction.BETRAY;
        }
    }
}
