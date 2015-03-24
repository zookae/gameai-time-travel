using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TimeTravel
{
    public enum GameStateStatus
    {
        CONTINUE,
        RESTART,
        NEXT_LEVEL
    }

    public sealed class GameState
    {
        // Constants
        public readonly int TRUST_TRUST_VALUE = 1;
        public readonly int TRUST_BETRAY_VALUE = -3;
        public readonly int BETRAY_TRUST_VALUE = 3;
        public readonly int BETRAY_BETRAY_VALUE = -1;

        // Drawing Constants
        private static int WINDOW_WIDTH = 800;
        private static int WINDOW_HEIGHT = 600;
        
        // Panel rectangles
        private readonly Rectangle SLIDER_RECTANGLE = new Rectangle( 0, 0, WINDOW_WIDTH, WINDOW_HEIGHT / 5 );
        private readonly Rectangle GRAPH_RECTANGLE = new Rectangle( 0, WINDOW_HEIGHT / 5, (3 * WINDOW_WIDTH ) / 4, ( 3 * WINDOW_HEIGHT ) / 5 );
        private readonly Rectangle B_PANEL_RECTANGLE = new Rectangle( ( 3 * WINDOW_WIDTH ) / 4, WINDOW_HEIGHT / 5, WINDOW_WIDTH / 4, ( 4 * WINDOW_HEIGHT ) / 5 );
        private readonly Rectangle DESC_RECTANGLE = new Rectangle( 0, ( 4 * WINDOW_HEIGHT ) / 5, ( 3 * WINDOW_WIDTH ) / 4, WINDOW_HEIGHT / 5 );

        // Hard-coded rectangles
        private readonly Rectangle ENEMY_RECTANGLE = new Rectangle( 0, 0, 200, 100 ); // offset from 0, 0 to move        
        private readonly Rectangle RESULTS_RECTANGLE = new Rectangle( 200, 100, 400, 200 ); // HACK (hard-coded)

        // Colors
        private readonly Color BACKGROUND_COLOR = new Color( 47, 47, 47 );

        // More drawing constants
        private readonly int AGENT_GRAPH_SCALE = 64;
        private readonly int AGENT_PORTRAIT_SCALE = 128;
        private readonly int TICK_BUFFER = 20; // pixel offset for starting ticks
        private readonly int PORTRAIT_BUFFER = 20; // pixel offset for agent portrait
        private readonly int TEXT_BUFFER = 60; // pixel offset for text writing
        private readonly int NUM_DOTS = 8; // number of dots per line
        private readonly int DESC_TEXT_LINES = 7; // number of lines of text in the description
        private readonly static int BUTTON_HEIGHT_OFFSET = 25;
        private readonly Vector2 BUTTON_POSITION = new Vector2( 5, BUTTON_HEIGHT_OFFSET ); // for the trust button (betray/none are offsets)
        private readonly Vector2 CHANGE_BUTTON_POSITION = new Vector2( 550, 90 ); // for the change button
        private readonly Vector2 RESTART_POSITION = new Vector2( 20, 90 );
        private readonly Vector2 NEXT_LEVEL_POSITION = new Vector2( 220, 90 );
        private readonly int TRUST_BUTTON_OFFSET = 40; // in pixels

        // Font Constants
        private readonly Vector2 SELECTOR_FONT_OFFSET = new Vector2( 30, 40 );
        private readonly static int TEXT_OFFSET = 20;
        private readonly Vector2 DEFAULT_FONT_OFFSET = new Vector2( TEXT_OFFSET, TEXT_OFFSET );
        private readonly Vector2 CENTERED_BUTTON_OFFSET = new Vector2( TEXT_OFFSET * 2.25f, TEXT_OFFSET * 1.3f );

        // Parameters
        private List<IAgent> agents;        
        // agent, the adversary, the agent's action against the adversary, and the time period
        private List<Quadruple<IAgent, IAgent, AgentAction, Int32>> actionRecords; 
        private Quadruple<IAgent, IAgent, AgentAction, Int32> proposedChange;
        private List<AgentGoal> goals;
        private bool displayResults; // the flag to display the victory/loss conditions
        private bool victory;

        // GUI Parameters
        private MouseState lastMouseState; // last mouse state
        private Vector2 selectorPosition; // position of the slider
        private IAgent selectedAgent; // the selected agent
        private IAgent selectedAdversary; // the selected adversary
        private List<Vector2> agentGraphPositions; // cached positions of the graph nodes

        // Booleans for selection (this is idiotic, I am completely aware)
        private bool selectorSelected; // the slider selector
        private bool agentSelected; // an agent highlighted
        private bool adversarySelected; // an adversary highlighted
        private bool goalsSelected; // goal button pressed
        private bool trustSelected; // trust button pressed
        private bool betraySelected; // betray button pressed
        private bool changeSelected; // change button pressed
        private bool restartSelected; // restart button pressed
        private bool nextLevelSelected; // next level button pressed

        // Properties
        // Note: all times indexed from 0
        public GameResources Resources { get; set; }
        public int PresentTime { get; private set; } // the present time period (which will advance after an update)
        public int CurrentTime { get; private set; } // the current time period the player is in within the window
        public int NumTimePeriods { get; private set; }
        public int NumAgents { get { return agents.Count(); } }
        public int TimeWindowSize { get; private set; }
        public int TimeWindowMin { get; private set; } // number of periods backward allowed
        public int TimeWindowMax { get; private set; } // number of periods forward allowed
        public GameStateStatus Status { get; private set; }

        // Constructor
        public GameState( int numTimePeriods, int timeWindowMin, int timeWindowMax, GameResources resources )
        {
            this.agents = new List<IAgent>();
            this.Resources = resources;
            this.actionRecords = new List<Quadruple<IAgent, IAgent, AgentAction, Int32>>();
            this.goals = new List<AgentGoal>();
            this.proposedChange = null;
            this.displayResults = false;
            this.victory = false;

            this.NumTimePeriods = numTimePeriods;
            this.TimeWindowMin = timeWindowMin;
            this.TimeWindowMax = timeWindowMax;
            this.TimeWindowSize = timeWindowMin + timeWindowMax;
            this.PresentTime = Math.Min(this.TimeWindowSize / 2, numTimePeriods /2); // default;
            this.CurrentTime = PresentTime;
            this.Status = GameStateStatus.CONTINUE;
            
            this.lastMouseState = Mouse.GetState(); // HACK (we really shouldn't be arbitrarily determining input in this class)
            this.selectorPosition = Vector2.Zero;
            this.selectedAgent = null;
            this.selectedAdversary = null;
            this.agentGraphPositions = new List<Vector2>( NumAgents );

            this.selectorSelected = false;
            this.agentSelected = false;
            this.adversarySelected = false;
            this.goalsSelected = false;
            this.trustSelected = false;
            this.betraySelected = false;
            this.changeSelected = false;
            this.restartSelected = false;
            this.nextLevelSelected = false;
        }

        // Agent-related functions
        public void AddAgent( IAgent a )
        {
            agents.Add( a );
            foreach ( IAgent agent in agents ) {
                agent.AddNewAgentKnowledge( a, NumAgents );
            }

            // redo this computation when we add a new agent
            ComputeAgentGraphPositions( new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y ), GRAPH_RECTANGLE.Width, GRAPH_RECTANGLE.Height );
        }

        public void AddGoal( AgentGoal goal )
        {
            goals.Add( goal );
        }

        // Gets an agent by its id number
        public IAgent GetAgentByID( int agentID )
        {
            return agents[agentID];
        }

        // Gets the next agentID number that HAS NOT BEEN ASSIGNED
        // If a new agent is added, it will generate a new ID on the next call
        public int GetNextAgentID()
        {
            return agents.Count();
        }

        // Returns the number of agents alive in a particular time period.
        public int GetNumAgentsAlive(int time)
        {
            int counter = 0;
            foreach ( IAgent agent in agents ) { 
                if ( agent.GetCondition(time) == AgentCondition.ALIVE ) {
                    counter++;
                }
            }
            return counter;
        }

        // Determines if time requested is not before the beginning of the game or minimum of time window
        // or after end of game or maximum of time window.
        public void CheckTimeBounds(int checkTime)
        {
            if (checkTime < (PresentTime - TimeWindowMin) ||
                checkTime < 0)
            {
                throw new Exception("Attempting to modify time to earlier than window beginning or 0.");
            }
                //HACK!!!!!!!!!!!!
            else if (checkTime-1 > (PresentTime + TimeWindowMax) ||
                checkTime > NumTimePeriods)
            {
                throw new Exception("Attempting to modify time to later than window ending or game end.");

            }
        }

        // Primary function called whenever the user modifies an agent with an action at a proposed time
        public void ModifyTime(int proposedTime, IAgent agent, Pair<IAgent, AgentAction> targetedAction)
        {
            // Check for end conditions
            if ( displayResults ) return;
            if ( proposedTime >= NumTimePeriods - 1 ) {
                // Handle victory conditions
                victory = CheckVictoryConditions( proposedTime, false );
                displayResults = true;
                return;
            }

            // TODO: check proposedTime is valid, in bounds, etc
            CheckTimeBounds(proposedTime);

            Triple<IAgent, IAgent, AgentAction> proposedAction = 
                new Triple<IAgent, IAgent, AgentAction>(agent, targetedAction.a, targetedAction.b);

            List<Pair<IAgent, AgentAction>> allActions = new List<Pair<IAgent, AgentAction>>();
            List<Triple<IAgent, IAgent, AgentAction>> allActions2 = new List<Triple<IAgent, IAgent, AgentAction>>();
            foreach (IAgent otherAgent in agents)
            {
                //List<Pair<IAgent, AgentAction>> agentActions;
                List<Triple<IAgent, IAgent, AgentAction>> agentActions2;
                if (otherAgent.AgentID == agent.AgentID)
                {
                    // agentActions = agent.PickActions(this, proposedTime, targetedAction);
                    agentActions2 = agent.PickActions2(this, proposedTime, proposedAction);
                }
                else
                {
                    // agentActions = otherAgent.PickActions(this, proposedTime);
                    agentActions2 = otherAgent.PickActions2(this, proposedTime);
                }

                // add to all pairs
                /*foreach (Pair<IAgent, AgentAction> actionPair in agentActions)
                {
                    allActions.Add(actionPair);
                }*/
                foreach (Triple<IAgent, IAgent, AgentAction> actionTriple in agentActions2)
                {
                    allActions2.Add(actionTriple);
                }
            }

            foreach (IAgent changeAgent in agents)
            {
                //changeAgent.ReceiveActions(this, allActions, proposedTime);
                changeAgent.ReceiveActions2(this, allActions2, proposedTime);
            }
            allActions.Clear(); // clear out actions to move to next period

            // Propogate all subsequent changes
            ForwardPropogate(proposedTime);

            // Check if there are actions.  Otherwise, we check for victory conditions...
            // This prevents us from getting into scenarios where we have no actions
            int lowerTimeBound = Math.Max(0, PresentTime - TimeWindowMin);
            int upperTimeBound = Math.Min(NumTimePeriods - 1, PresentTime + TimeWindowMax);
            bool validActionPeriodFound = false;
            for ( int i = lowerTimeBound; i <= upperTimeBound; i++ ) {
                if ( GetNumAgentsAlive( i ) > 1 ) {
                    validActionPeriodFound = true;
                    break;
                }
            }
            if (!validActionPeriodFound) { // No valid actions found
                // Check victory conditions in the present time
                victory = CheckVictoryConditions( PresentTime, true );
                displayResults = true;
            }
        }

        // Propogates all changes forward from a given time period
        public void ForwardPropogate( int startingTime )
        {
            // TODO check startingTime is within correct bounds, etc
            CheckTimeBounds(startingTime);

            List<Triple<IAgent, IAgent, AgentAction>> allActions2 = new List<Triple<IAgent, IAgent, AgentAction>>();
            
            // this may be as slow as molasses, but I'll optimize it later if it's actually a problem
            for ( int i = startingTime; i < NumTimePeriods; i++ ) {
                // each agent picks its actions for a given time period
                foreach( IAgent agent in agents ) {
                    if ( agent.GetCondition(i) != AgentCondition.DEAD ) {
                        List<Triple<IAgent, IAgent, AgentAction>> agentActions2 = agent.PickActions2(this, i);
                        foreach (Triple<IAgent, IAgent, AgentAction> actionTriple in agentActions2)
                        {
                            allActions2.Add(actionTriple);
                        }
                    }
                }

                // each agent then processes the actions of this time period
                foreach ( IAgent agent in agents ) {
                    agent.ReceiveActions2(this, allActions2, i);
                    /*
                    if ( agent.HealthRecord[i] < 1 ) {
                        agent.SetCondition( AgentCondition.DEAD, i + 1 );
                    } else {
                        agent.SetCondition( AgentCondition.ALIVE, i + 1 );
                    }*/
                }
                allActions2.Clear(); // clear out actions to move to next period
            }

            PresentTime++;

            if ( PresentTime == NumTimePeriods ) {
                PresentTime = NumTimePeriods - 1; // stay on the last time period
                victory = CheckVictoryConditions( PresentTime, true );
                displayResults = true;
            }

            CurrentTime = PresentTime;
        }

        public bool CheckVictoryConditions(int proposedTime, bool earlyEvaluation)
        {
            foreach ( AgentGoal goal in goals ) {
                if ( !goal.GoalSatisfied( proposedTime, earlyEvaluation ) ) {
                    return false;
                }
            }
            return true;
        }

        // Handle Input
        // HACK (REFACTOR) this if/else bullshit is garbage and unclean...please fix into something more logically coherent
        public void HandleInput( MouseState mouseState, int width, int height )
        {

            // First determine if the mouse is interacting at all...
            if ( mouseState.LeftButton == ButtonState.Pressed ) { // Dragging
                if ( displayResults ) {
                    Vector2 resultsOffset = new Vector2( RESULTS_RECTANGLE.X, RESULTS_RECTANGLE.Y );
                    if ( DrawUtil.WithinBounds( resultsOffset + RESTART_POSITION, Resources.buttonPressable, mouseState.X, mouseState.Y ) && nothingElseSelected() ) {
                        restartSelected = true;
                    } else if ( DrawUtil.WithinBounds( resultsOffset + NEXT_LEVEL_POSITION, Resources.buttonPressable, mouseState.X, mouseState.Y ) && nothingElseSelected() ) {
                        nextLevelSelected = true;
                    }
                    lastMouseState = mouseState;
                    return;
                }

                // If we are in range
                if ( DrawUtil.WithinBounds( selectorPosition, Resources.selector, mouseState.X, mouseState.Y ) && nothingElseSelected() ) {
                    selectorSelected = true;
                }
                if ( selectorSelected ) {
                    // Checking to see if the selector is selected
                    selectorPosition.X += mouseState.X - lastMouseState.X;
                } else if ( !agentSelected ) {
                    // Checking to see if an agent is selected
                    Vector2 graphOffset = new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y );
                    for ( int i = 0; i < NumAgents; i++ ) {
                        if ( DrawUtil.WithinBounds( agentGraphPositions[i] + graphOffset, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE, mouseState.X, mouseState.Y ) ) {
                            if ( selectedAdversary != null && i == selectedAdversary.AgentID ) { // swap
                                IAgent tempAgent = selectedAgent;
                                selectedAgent = selectedAdversary;
                                selectedAdversary = tempAgent;
                            }

                            selectedAgent = GetAgentByID(i);
                            agentSelected = true;
                        }
                    }
                } else if ( agentSelected && selectedAgent != null ) {
                    // If an agent is selected and whaaat....?
                    Vector2 graphOffset = new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y );
                    Vector2 graphDimensions = new Vector2( GRAPH_RECTANGLE.Width, GRAPH_RECTANGLE.Height ); // HACK
                    if( DrawUtil.WithinPixelBounds(0, 0, (int)graphDimensions.X, (int)graphDimensions.Y, (int)(mouseState.X - graphOffset.X), (int)(mouseState.Y - graphOffset.Y) ) ) {
                        agentGraphPositions[selectedAgent.AgentID] += new Vector2( mouseState.X - lastMouseState.X, mouseState.Y - lastMouseState.Y );
                        // keep within the bounds of the graph box
                        Vector2 newGraphPosition = agentGraphPositions[selectedAgent.AgentID];
                        newGraphPosition.X = Math.Min( Math.Max( 0, newGraphPosition.X ), graphDimensions.X - AGENT_GRAPH_SCALE );
                        newGraphPosition.Y = Math.Min( Math.Max( 0, newGraphPosition.Y ), graphDimensions.Y - AGENT_GRAPH_SCALE );
                        agentGraphPositions[selectedAgent.AgentID] = newGraphPosition;
                    }
                }
                                
                // Checking the button action panel
                Vector2 bPanelOffset = new Vector2( B_PANEL_RECTANGLE.X, B_PANEL_RECTANGLE.Y );
                Vector2 goalsPosition = new Vector2( BUTTON_POSITION.X, BUTTON_POSITION.Y );
                if ( nothingElseSelected() ) {
                    if ( DrawUtil.WithinBounds( goalsPosition + bPanelOffset, Resources.buttonPressable, mouseState.X, mouseState.Y ) ) {
                        goalsSelected = true;
                    }
                    else if ( selectedAdversary != null && selectedAdversary.GetCondition( CurrentTime ) != AgentCondition.DEAD && selectedAgent != null && selectedAgent.GetCondition( CurrentTime ) != AgentCondition.DEAD ) {
                        Vector2 trustPosition = new Vector2( BUTTON_POSITION.X, goalsPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );
                        trustPosition.Y += TRUST_BUTTON_OFFSET; // HACK to push buttons down
                        Vector2 betrayPosition = new Vector2( BUTTON_POSITION.X, trustPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );
                        Vector2 changePosition = new Vector2( BUTTON_POSITION.X, betrayPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );
                        if ( DrawUtil.WithinBounds( trustPosition + bPanelOffset, Resources.buttonPressable, mouseState.X, mouseState.Y ) && selectedAdversary.GetCondition(CurrentTime) == AgentCondition.ALIVE ) {
                            trustSelected = true;
                        } else if ( DrawUtil.WithinBounds( betrayPosition + bPanelOffset, Resources.buttonPressable, mouseState.X, mouseState.Y ) && selectedAdversary.GetCondition(CurrentTime) == AgentCondition.ALIVE  ) {
                            betraySelected = true;
                        } else if ( proposedChange != null && DrawUtil.WithinBounds( changePosition + bPanelOffset, Resources.buttonPressable, mouseState.X, mouseState.Y ) ) {
                            changeSelected = true;
                        }

                    }
                }

            // Handles a finishing a mouse press (the release of a click)
            } else if ( mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed ) { // Clicking (the release)
                // If we are dragging the selector, we now stop and set it to the correct time
                if ( selectorSelected ) {
                    selectorSelected = false;
                    Vector2 selectorCenter = selectorPosition + new Vector2( Resources.selector.Width / 2, Resources.selector.Height / 2 );
                    int closestTime = CurrentTime;
                    double sqDist = Double.MaxValue;

                    // Does some logic to determine which time to snap to (because snapping is cool).
                    Vector2 tickOffset = new Vector2( TICK_BUFFER, ( height - Resources.tick.Height ) / 2 ); // offset the ticks on either end
                    int gapWidth = ( width - ( ( TICK_BUFFER * 2 ) + ( Resources.tick.Width * NumTimePeriods ) ) ) / ( NumTimePeriods - 1 ); // HACK assumes width is that of screen
                    for ( int i = 0; i < NumTimePeriods; i++ ) {
                        int tickX = ( i * Resources.tick.Width ) + ( i * gapWidth ); // offset by the previous ticks and gaps
                        Vector2 tickPosition = new Vector2( tickOffset.X + tickX, tickOffset.Y );
                        Vector2 tickCenter = tickPosition + new Vector2( Resources.tick.Width / 2, Resources.tick.Height / 2 );
                        double newSqDist = Vector2.DistanceSquared( selectorCenter, tickCenter );
                        if ( newSqDist < sqDist ) {
                            closestTime = i;
                            sqDist = newSqDist;
                        }
                    }
                    CurrentTime = closestTime;

                    // Snap current time to be within bounds
                    if (CurrentTime > PresentTime + TimeWindowMax) { 
                        CurrentTime = PresentTime + TimeWindowMax;
                    } else if (CurrentTime < Math.Max(0, PresentTime - TimeWindowMin) ) {
                        CurrentTime = Math.Max(0, PresentTime - TimeWindowMin);
                    }

                    selectorPosition = calculateSelectorPosition( width, height / 5, CurrentTime );
                } else if ( goalsSelected ) {
                    goalsSelected = false;
                } else if ( agentSelected ) {
                    agentSelected = false;
                } else if ( trustSelected ) {
                    trustSelected = false;
                    adjustProposedAction( AgentAction.TRUST );
                } else if ( betraySelected ) {
                    betraySelected = false;
                    adjustProposedAction( AgentAction.BETRAY );
                } else if ( changeSelected && proposedChange != null ) { // APPLY CHANGES
                    changeSelected = false;
                    actionRecords.Add( proposedChange );
                    // Note: this does call forward propogate
                    ModifyTime( CurrentTime, proposedChange.a, new Pair<IAgent, AgentAction>(proposedChange.b, proposedChange.c) );
                    proposedChange = null;
                    selectorPosition = calculateSelectorPosition( width, height / 5, CurrentTime );
                } else if ( restartSelected ) {
                    restartSelected = false;
                    Status = GameStateStatus.RESTART;
                    lastMouseState = mouseState;
                    return;
                } else if ( nextLevelSelected ) {
                    nextLevelSelected = false;
                    Status = GameStateStatus.NEXT_LEVEL;
                    lastMouseState = mouseState;
                    return;
                }
            }

            // looking at the right mouse click for handling adversary clicks
            if ( selectedAgent != null ) {
                if ( mouseState.RightButton == ButtonState.Pressed ) {
                    if ( !adversarySelected ) {
                        Vector2 graphOffset = new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y );
                        for ( int i = 0; i < NumAgents; i++ ) {
                            if ( i == selectedAgent.AgentID ) continue; // skip current agent
                            // Texture2D agentTexture = GetAgentByID( i ).GetCondition(CurrentTime) == AgentCondition.ALIVE ? resources.liveAgent : resources.deadAgent;
                            if ( DrawUtil.WithinBounds( agentGraphPositions[i] + graphOffset, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE, mouseState.X, mouseState.Y ) ) {
                                selectedAdversary = GetAgentByID( i );
                                adversarySelected = true;
                            }
                        }
                    }
                } else if ( mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed ) { // Clicking (the release)
                    if ( adversarySelected ) {
                        adversarySelected = false;
                    }
                }
            }

            lastMouseState = mouseState;
        }

        // Helper function to make selection easier
        private bool nothingElseSelected()
        {
            return !selectorSelected && !agentSelected && !adversarySelected && !goalsSelected && !trustSelected && !betraySelected && !changeSelected && !restartSelected && !nextLevelSelected;
        }

        private void adjustProposedAction(AgentAction action)
        {
            if ( selectedAdversary == null || selectedAgent == null ) throw new Exception( "Agents are not selected!" );
            List<Pair<IAgent, AgentAction>> actions = selectedAgent.GetActions(this, CurrentTime);
            bool actionFound = false;
            AgentAction originalAction = AgentAction.NONE; // dummy value
            foreach ( Pair<IAgent, AgentAction> pair in actions ) {
                if ( pair.a.AgentID == selectedAdversary.AgentID ) {
                    originalAction = pair.b;
                    actionFound = true;
                    break;
                }
            }
            if ( !actionFound ) throw new Exception( "Could not adjust action because an action against this agent was never determined." );
            if ( originalAction == action ) { // we reverted back to an old action or action was unchanged
                proposedChange = null; // reset to null
            } else {
                if ( ( selectedAdversary.GetCondition(CurrentTime) == AgentCondition.DEAD && ( action == AgentAction.TRUST || action == AgentAction.BETRAY ) ) ||
                     ( selectedAdversary.GetCondition(CurrentTime) == AgentCondition.ALIVE && action == AgentAction.NONE ) ) {
                    throw new Exception( "Invalid action detected!" ); // this should have been checked
                } else {
                    proposedChange = new Quadruple<IAgent, IAgent, AgentAction, Int32>( selectedAgent, selectedAdversary, action, CurrentTime );
                }
            }
        }

        // Helper, so I can stop using the ternary operator everywhere :(
//        private Texture2D GetAgentTexture( IAgent agent )
//        {
//            return ( agent.GetCondition(CurrentTime) == AgentCondition.ALIVE ) ? resources.liveAgent : resources.deadAgent;
//        }

        // Note: calculates this w.r.t. to the slider's space (not accounting for global offset)
        private Vector2 calculateSelectorPosition( int sliderWidth, int sliderHeight, int time )
        {
            Vector2 tickOffset = new Vector2( TICK_BUFFER, ( sliderHeight - Resources.tick.Height ) / 2 ); // offset the ticks on either end
            int gapWidth = ( sliderWidth - ( ( TICK_BUFFER * 2 ) + ( Resources.tick.Width * NumTimePeriods ) ) ) / ( NumTimePeriods - 1 );
            int tickX = ( time * Resources.tick.Width ) + ( time * gapWidth ); // offset by the previous ticks and gaps
            Vector2 tickPosition = new Vector2( tickOffset.X + tickX, tickOffset.Y );
            Vector2 tickCenter = tickPosition + new Vector2( Resources.tick.Width / 2, Resources.tick.Height / 2 );
            return tickCenter - new Vector2( Resources.selector.Width / 2, Resources.selector.Height / 2 );
        }

        // Drawing functions
        public void Draw( GraphicsDevice gd, SpriteBatch sb, int width, int height )
        {
            gd.Clear( BACKGROUND_COLOR );

            // Draw slider
            sb.Begin();
            if ( selectorPosition.Equals( Vector2.Zero ) ) {
                selectorPosition = calculateSelectorPosition( SLIDER_RECTANGLE.Width, SLIDER_RECTANGLE.Height, PresentTime );
            }
            DrawSlider( sb, new Vector2( SLIDER_RECTANGLE.X, SLIDER_RECTANGLE.Y ), SLIDER_RECTANGLE.Width, SLIDER_RECTANGLE.Height );  // HACK height is hardcoded
            
            // Draw goals or graph
            if ( goalsSelected ) {
                DrawGoals( sb, new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y ), GRAPH_RECTANGLE.Width, GRAPH_RECTANGLE.Height );
            } else {
                DrawGraph( gd, sb, new Vector2( GRAPH_RECTANGLE.X, GRAPH_RECTANGLE.Y ), GRAPH_RECTANGLE.Width, GRAPH_RECTANGLE.Height );
            }

            // Draw environment
            DrawButtonPanel( sb, new Vector2( B_PANEL_RECTANGLE.X, B_PANEL_RECTANGLE.Y ), B_PANEL_RECTANGLE.Width, B_PANEL_RECTANGLE.Height );

            // Draw detailed characteristics
            DrawDescription( sb, new Vector2( DESC_RECTANGLE.X, DESC_RECTANGLE.Y ), DESC_RECTANGLE.Width, DESC_RECTANGLE.Height  );

            // Draw the selector/cursor
            sb.Draw( Resources.selector, selectorPosition, Color.White );
            sb.DrawString( Resources.tinyFont, "Time: " + CurrentTime, selectorPosition + SELECTOR_FONT_OFFSET, Color.White );
            if ( displayResults ) {
                DrawResults( sb, new Vector2( RESULTS_RECTANGLE.X, RESULTS_RECTANGLE.Y ), RESULTS_RECTANGLE.Width, RESULTS_RECTANGLE.Height );
            }

            if ( adversarySelected ) {
                DrawEnemyBox( sb, Vector2.Zero );
            }

            sb.Draw( Resources.cursor, new Vector2( lastMouseState.X, lastMouseState.Y ), Color.White );
            sb.End();
        }

        private void DrawSlider( SpriteBatch sb, Vector2 offset, int width, int height )
        {
            // Draw the timeline graphic
            //sb.Draw( resources.blankRectangle, SLIDER_RECTANGLE, SLIDER_COLOR );
            sb.Draw( Resources.timeline, offset, Color.White );

            // Draw the ticks!
            Vector2 tickOffset = new Vector2( TICK_BUFFER, (height - Resources.tick.Height) / 2 ); // offset the ticks on either end
            int gapWidth = (width - ((TICK_BUFFER * 2) + (Resources.tick.Width * NumTimePeriods))) / (NumTimePeriods - 1); 
            for ( int i = 0; i < NumTimePeriods; i++ ) {
                int tickX = (i * Resources.tick.Width) + (i * gapWidth); // offset by the previous ticks and gaps
                Vector2 tickPosition = new Vector2( tickOffset.X + tickX , tickOffset.Y );
                
                // HACK tick-coloring is hard-coded...
                Color tickColor = ( i <= PresentTime + TimeWindowMax && i >= Math.Max(0, PresentTime - TimeWindowMin) ? Color.Orange : Color.Gray );
                if ( tickColor == Color.Orange ) { // color the current time differently
                    tickColor = ( i == PresentTime ? Color.Red : Color.Orange );
                }

                sb.Draw( Resources.tick, offset + tickPosition, tickColor );
            }
        }

        private void DrawGraph( GraphicsDevice gd, SpriteBatch sb, Vector2 offset, int width, int height )
        {
            sb.Draw( Resources.graphPanel, GRAPH_RECTANGLE, Color.White );

            // Draw selected agent stuff
            if ( selectedAgent != null ) {
                // Draw lines based on actions taking this time period
                Vector2 selectedAgentCenter = DrawUtil.GetCenter( agentGraphPositions[selectedAgent.AgentID], AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                Vector2 halfDim = DrawUtil.GetHalfDims( AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE);

                // Draw the ghosts
                for ( int i = 0; i < actionRecords.Count(); i++ ) {
                    Quadruple<IAgent, IAgent, AgentAction, Int32> actionRecord = actionRecords[i];
                    if ( ( actionRecord.a.AgentID == selectedAgent.AgentID ) && ( CurrentTime == actionRecord.d ) ) {
                        Vector2 adversaryCenter = DrawUtil.GetCenter( agentGraphPositions[actionRecord.b.AgentID], AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                        Vector2 ghostHalfDim = DrawUtil.GetHalfDims( Resources.ghost );
                        Vector2 midpoint = 0.5f * ( selectedAgentCenter + adversaryCenter );
                        sb.Draw( Resources.ghost, offset + (midpoint - ghostHalfDim), Color.White );
                    }
                }

                // DOTS!
                List< Pair<IAgent, AgentAction> > actions = selectedAgent.GetActions( this, CurrentTime );
                if ( actions.Count != NumAgents - 1 ) {
                    throw new Exception( "This agent should be acting against all agents!" );
                }
                Vector2 dotHalfDim = DrawUtil.GetHalfDims( Resources.dot );                
                for ( int i = 0; i < actions.Count; i++ ) {
                    if ( actions[i].a.AgentID == selectedAgent.AgentID ) continue;
                    // Console.Out.WriteLine( "Acting against agent : " + actions[i].a.AgentID + "!" );
                    Vector2 agentHalfDim = DrawUtil.GetHalfDims( AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                    Vector2 agentCenter = DrawUtil.GetCenter( agentGraphPositions[actions[i].a.AgentID], AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                    Vector2 v = agentCenter - selectedAgentCenter;
                    Color dotColor; 
                    switch (actions[i].b) {
                        case AgentAction.BETRAY:
                            dotColor = Color.Red;
                            break;
                        case AgentAction.TRUST:
                            dotColor = Color.Blue;
                            break;
                        default:
                            dotColor = Color.DimGray;
                            break;
                    }
                    for ( int d = 0; d < NUM_DOTS; d++ ) {
                        Vector2 dotPosition = selectedAgentCenter + ( ( (float)d / (float)NUM_DOTS ) * v );
                        sb.Draw( Resources.dot, offset + dotPosition, dotColor );
                    }
                }

                Vector2 ringPosition = offset + ( selectedAgentCenter - halfDim );
                Rectangle ringRectangle = new Rectangle( (int)ringPosition.X, (int)ringPosition.Y, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE);
                sb.Draw( Resources.agentRing, ringRectangle, Color.Green );
                if ( selectedAdversary != null ) {
                    Vector2 selectedAdversaryCenter = DrawUtil.GetCenter( agentGraphPositions[selectedAdversary.AgentID], AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                    ringPosition = offset + ( selectedAdversaryCenter - halfDim );
                    ringRectangle = new Rectangle( (int)ringPosition.X, (int)ringPosition.Y, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                    sb.Draw( Resources.agentRing, ringRectangle, Color.Red );
                }
            }

            for ( int i = 0; i < NumAgents; i++ ) {
                Vector2 agentPosition = agentGraphPositions[i] + offset;
                Rectangle agentRectangle = new Rectangle( (int)agentPosition.X, (int)agentPosition.Y, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
                Texture2D agentTexture = ( GetAgentByID( i ).GetCondition(CurrentTime) == AgentCondition.ALIVE ) ? Resources.agentSmile : Resources.agentDead;
                sb.Draw( Resources.agentBase, agentRectangle, GetAgentByID( i ).AgentColor );
                sb.Draw( agentTexture, agentRectangle, Color.White );
            }
        }

        // Draws these in graph space
        private void ComputeAgentGraphPositions(Vector2 offset, int width, int height)
        {
            // Sets up the agent positions to be around a circle (the user can screw with it later)
            Vector2 center = new Vector2( width / 2, height / 2 );
            float increment = (float)( Math.PI * 2 ) / NumAgents;
            for ( int i = 0; i < NumAgents; i++ ) {
                int radius = ( ( height < width ? height : width ) / 2 ) - AGENT_GRAPH_SCALE;
                float xOffset = radius * (float)Math.Cos( increment * i );
                float yOffset = radius * (float)Math.Sin( increment * i );
                Vector2 agentPosition = new Vector2( center.X + xOffset, center.Y + yOffset );
                if ( agentGraphPositions.Count < NumAgents ) { // adds initial positions for the first time
                    agentGraphPositions.Add( agentPosition );
                } else {
                    agentGraphPositions[i] = agentPosition;
                }
            }
        }

        private void DrawButtonPanel( SpriteBatch sb, Vector2 offset, int width, int height )
        {
            sb.Draw( Resources.buttonPanel, B_PANEL_RECTANGLE, Color.White );

            // Draw the buttons
            Texture2D goalsTexture = ( goalsSelected ) ? Resources.buttonPressed : Resources.buttonPressable;
            Texture2D trustTexture = ( trustSelected ) ? Resources.buttonPressed : Resources.buttonPressable;
            Texture2D betrayTexture = ( betraySelected ) ? Resources.buttonPressed : Resources.buttonPressable;
            Texture2D changeTexture = ( changeSelected ) ? Resources.buttonPressed : Resources.buttonPressable;

            Vector2 buttonOffset = new Vector2( ( width - Resources.buttonPressable.Width ) / 2, BUTTON_HEIGHT_OFFSET );
            Vector2 goalsPosition = buttonOffset;
            Vector2 trustPosition = new Vector2( buttonOffset.X, goalsPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );
            trustPosition.Y += TRUST_BUTTON_OFFSET; // HACK to push buttons down
            Vector2 betrayPosition = new Vector2( buttonOffset.X, trustPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );
            Vector2 changePosition = new Vector2( buttonOffset.X, betrayPosition.Y + Resources.buttonPressable.Height + BUTTON_HEIGHT_OFFSET );

            sb.Draw( goalsTexture, offset + goalsPosition, Color.White ); // view goals button
            sb.Draw( trustTexture, offset + trustPosition, Color.White ); // trust button
            sb.Draw( betrayTexture, offset + betrayPosition, Color.White ); // betray button
            sb.Draw( changeTexture, offset + changePosition, Color.White ); // change button

            Vector2 actionTextOffset = new Vector2( 25, 70 ); // HACK (*COUGH, COUGH)
            sb.DrawString( Resources.boldFont, "Actions:", offset + goalsPosition + buttonOffset + actionTextOffset, Color.Black );

            // Draw goals text
            Vector2 textPosition = new Vector2( TEXT_BUFFER, TEXT_BUFFER );
            Vector2 buttonTextOffset = new Vector2( textPosition.X, textPosition.Y * .25f );
            sb.DrawString( Resources.font, "Goals", offset + BUTTON_POSITION + CENTERED_BUTTON_OFFSET, Color.Black );

            // Draw approppriate text for active, not active buttons
            Color actionColor = Color.DimGray;
            if ( selectedAgent != null && selectedAdversary != null ) {
                List<Pair<IAgent,AgentAction>> actions = selectedAgent.GetActions(this, CurrentTime);
                Pair<IAgent, AgentAction> actionAgainstAdversary = null;
                foreach ( Pair<IAgent, AgentAction> pair in actions ) {
                    if (pair.a.AgentID == selectedAdversary.AgentID) {
                        actionAgainstAdversary = pair;
                        break;
                    }
                }
                if ( actionAgainstAdversary == null ) {
                    throw new Exception("Can't find the action we're supposed to perform....");
                }

                // Choose text colors (grayed out for not-selectable).
                if ( selectedAgent.GetCondition( CurrentTime ) == AgentCondition.DEAD ||
                     selectedAdversary.GetCondition( CurrentTime ) == AgentCondition.DEAD ) { // selected agent is dead
                    actionColor = Color.DimGray; // action text is grayed out
                } else {
                    actionColor = Color.Black;
                }
            }
            sb.DrawString( Resources.font, "Trust", offset + trustPosition + CENTERED_BUTTON_OFFSET, actionColor );
            sb.DrawString( Resources.font, "Betray", offset + betrayPosition + CENTERED_BUTTON_OFFSET, actionColor );

            // Draws text on the change button
            Color changeColor = Color.Black;
            if ( proposedChange == null ) {
                changeColor = Color.DimGray;
            }
            Vector2 centerOffset = new Vector2( -35, 10 ); // HACK
            sb.DrawString( Resources.font, "Change time!", offset + changePosition + buttonTextOffset + centerOffset, changeColor );

            // Draw exclamation to indicate proposed change
            if ( proposedChange != null && selectedAgent != null && selectedAdversary != null ) {
                if ( proposedChange.a.AgentID == selectedAgent.AgentID && proposedChange.b.AgentID == selectedAdversary.AgentID && proposedChange.d == CurrentTime ) {
                    Vector2 exclamationPosition = new Vector2( 10, 4 );
                    if ( proposedChange.c == AgentAction.TRUST ) {
                        sb.Draw( Resources.exclamation, offset + trustPosition + exclamationPosition, Color.White );                        
                    } else if ( proposedChange.c == AgentAction.BETRAY ) {
                        sb.Draw( Resources.exclamation, offset + betrayPosition + exclamationPosition, Color.White );
                    }                    
                }
            }
        }

        private void DrawDescription( SpriteBatch sb, Vector2 offset, int width, int height )
        {
            sb.Draw( Resources.descriptionPanel, DESC_RECTANGLE, Color.White );
            if ( selectedAgent != null ) { // Draw the active agent's stuff....
                float textHeight = ( height / DESC_TEXT_LINES ) + 10;
                Vector2 portraitPosition = offset + new Vector2( PORTRAIT_BUFFER, (height / 2) - AGENT_PORTRAIT_SCALE / 2 );
                Rectangle portraitRectangle = new Rectangle( (int)portraitPosition.X, (int)portraitPosition.Y, AGENT_PORTRAIT_SCALE, AGENT_PORTRAIT_SCALE );
                sb.Draw( Resources.agentBase,  portraitRectangle, selectedAgent.AgentColor );
                Texture2D agentExpression = (selectedAgent.GetCondition(CurrentTime) == AgentCondition.ALIVE) ? Resources.agentSmile : Resources.agentDead;
                sb.Draw( agentExpression, portraitRectangle, Color.White );
                sb.Draw( Resources.agentRing, portraitRectangle, Color.White );

                // Write information
                Vector2 fontOffset = new Vector2( TEXT_BUFFER + AGENT_PORTRAIT_SCALE, textHeight );
                sb.DrawString( Resources.font, "Selected Agent: " + selectedAgent.Name, fontOffset + offset, Color.Black );
                fontOffset.Y += textHeight;
//                sb.DrawString( resources.font, "Agent ID: " + selectedAgent.AgentID, fontOffset + offset, Color.Black );
//                fontOffset.Y += textHeight;
                sb.DrawString(Resources.font, "Health: " + selectedAgent.HealthRecord[CurrentTime], fontOffset + offset, Color.Black);

            }
        }

        // HACK (for multiple goals, this does not resize properly) ...
        private void DrawGoals( SpriteBatch sb, Vector2 offset, int width, int height )
        {
            sb.Draw( Resources.graphPanel, GRAPH_RECTANGLE, Color.White );
            Vector2 fontOffset = DEFAULT_FONT_OFFSET * 1.5f;
            int fontYPadding = 20; // HACK
            sb.DrawString( Resources.boldFont, "Goals:", offset + fontOffset, Color.Black );
            fontOffset.Y += fontYPadding;
            sb.DrawString( Resources.tinyFont, "(Satisfied goals are in blue, failed goals are in red.)", offset + fontOffset, Color.Black );
            fontOffset.Y += fontYPadding;
            foreach ( AgentGoal goal in goals ) {
                Color goalColor = ( goal.GoalSatisfied( CurrentTime, false ) ) ? Color.SteelBlue : Color.DarkRed;
                sb.DrawString( Resources.font, goal.ToString(), offset + fontOffset, goalColor );
                fontOffset.Y += fontYPadding;
            }
        }

        private void DrawResults( SpriteBatch sb, Vector2 offset, int width, int height )
        {
            sb.Draw( Resources.resultsPanel, RESULTS_RECTANGLE, Color.White );
            string resultText = victory ? "Victory!" : "Failure.";
            Vector2 fontOffset = new Vector2( (TEXT_BUFFER * 3) - 20, (TEXT_BUFFER / 2) + 20 );
            sb.DrawString( Resources.boldFont, resultText, offset + fontOffset, Color.Black );


            Texture2D restartTexture = (restartSelected) ? Resources.buttonPressed : Resources.buttonPressable;
            Texture2D nextLevelTexture = (nextLevelSelected) ? Resources.buttonPressed : Resources.buttonPressable;
            sb.Draw( restartTexture, offset + RESTART_POSITION, Color.White );
            sb.Draw( nextLevelTexture, offset + NEXT_LEVEL_POSITION, Color.White );

            sb.DrawString( Resources.font, "Restart", offset + RESTART_POSITION + CENTERED_BUTTON_OFFSET, Color.Black );
            sb.DrawString( Resources.font, "Next Level", offset + NEXT_LEVEL_POSITION + CENTERED_BUTTON_OFFSET + new Vector2(-10,0), Color.Black );
        }

        private void DrawEnemyBox( SpriteBatch sb, Vector2 offset ) {
            if( selectedAgent == null || selectedAdversary == null ) {
                throw new Exception("Can't render the enemy info panel because there is null badness!");
            }
            
            // This calculation is redundant (if perf is an issue, calculate this only once when you draw the graph)
            // Calculates what action you take against this adversary
            List< Pair<IAgent, AgentAction> > actions = selectedAgent.GetActions( this, CurrentTime );
            AgentAction proposedAction = AgentAction.NONE;
            foreach (Pair<IAgent, AgentAction> action in actions) {
                if (action.a.AgentID == selectedAdversary.AgentID) {
                    proposedAction = action.b;
                    break;
                }
            }             

            // Draw
            Vector2 enemyPos = agentGraphPositions[selectedAdversary.AgentID];
            Vector2 enemyCenter = DrawUtil.GetCenter( enemyPos, AGENT_GRAPH_SCALE, AGENT_GRAPH_SCALE );
            Rectangle adjustedRectangle = new Rectangle( (int)enemyCenter.X + AGENT_GRAPH_SCALE, (int)enemyCenter.Y + ( AGENT_GRAPH_SCALE / 2 ), ENEMY_RECTANGLE.Width, ENEMY_RECTANGLE.Height );
            Vector2 textStart = new Vector2(adjustedRectangle.X, adjustedRectangle.Y);
            sb.Draw(Resources.enemyPanel, adjustedRectangle, Color.White);
            string nameText = "Name: " + selectedAdversary.Name;
            string actionText = "";
            switch ( proposedAction ) {
                case AgentAction.BETRAY:
                    actionText = "I plan to betray this person!";
                    break;
                case AgentAction.TRUST:
                    actionText = "I plan to trust this person!";
                    break;
                default:
                    if ( selectedAgent.GetCondition( CurrentTime ) == AgentCondition.DEAD ) {
                        actionText = "Too dead to do an action.";
                    } else if ( selectedAdversary.GetCondition( CurrentTime ) == AgentCondition.DEAD ) {
                        actionText = "This person is dead.";
                    } else {
                        actionText = "I can do nothing.";
                    }
                    break;
            }
            sb.DrawString( Resources.tinyFont, nameText, offset + DEFAULT_FONT_OFFSET + textStart, Color.Black );
            textStart.Y += TEXT_OFFSET;
            sb.DrawString( Resources.tinyFont, actionText, offset + DEFAULT_FONT_OFFSET + textStart, Color.Black );
        }
    }
}
