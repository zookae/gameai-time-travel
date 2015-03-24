using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TimeTravel
{
    class MemoryAgent : BasicAgent
    {
        private AgentAction lastAct;

        public MemoryAgent(string name, int numTimePeriods, int numAgents, int health, int agentID) : 
            base(name, numTimePeriods, numAgents, health, agentID)
        {
            lastAct = AgentAction.TRUST; // initially trust
        }

        // MemoryAgent is always remember (sad) things, so he is somewhat blue...
        public override Color AgentColor { get { return Color.CornflowerBlue; } }

        // MemoryAgent tries to remember what the last agent did and reacts to that
        public override AgentAction PickAction(GameState gs, IAgent agent, int time)
        {
            if ( agent.GetCondition( time ) == AgentCondition.DEAD ) return AgentAction.NONE;
            int trustLvl = ComputeTrustLevel(agent, time);
            if (trustLvl > 3) // trust those deemed trustworthy
            {
                lastAct = AgentAction.TRUST;
                return AgentAction.TRUST;
            }
            else if (trustLvl < -3) // betray treacherous
            {
                lastAct = AgentAction.BETRAY;
                return AgentAction.BETRAY;
            }
            return lastAct; // start by trusting
        }
    }
}
