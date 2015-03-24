using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TimeTravel
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TimeTravelGame : Microsoft.Xna.Framework.Game
    {
        // Constants
        private const int NUM_LEVELS = 3;

        //private const int NUM_TIME_PERIODS = 9;
        //private const int NUM_AGENTS = 5;
        //private const int DEFAULT_AGENT_HEALTH = 1;
        private const int TIME_WINDOW_MIN = 3;
        private const int TIME_WINDOW_MAX = 1;
        //private const int NUM_PLAYERS = 1;

        private const int WINDOW_WIDTH = 800;
        private const int WINDOW_HEIGHT = 600;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState gameState;
        public GameResources gameResources;
        private int activeLevel;

        public TimeTravelGame()
        {
            graphics = new GraphicsDeviceManager( this );
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            // gameState = new GameState( NUM_TIME_PERIODS, TIME_WINDOW_MIN, TIME_WINDOW_MAX );
            gameState = null;
            activeLevel = 0;
            gameResources = new GameResources();


            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            SetupLevel( activeLevel );
            base.Initialize();
        }

        /// <summary>
        /// Levels are hardcoded into here
        /// </summary>
        /// <param name="level"></param>
        private void SetupLevel( int level )
        {
            int numTimePeriods;
            int numAgents;
            int startingAgentHealth;
            switch ( activeLevel ) {
                case 0:
                    numTimePeriods = 5;
                    numAgents = 3;
                    startingAgentHealth = 4;

                    // Agents
                    gameState = new GameState( numTimePeriods, TIME_WINDOW_MIN, TIME_WINDOW_MAX, gameResources );
                    gameState.AddAgent( new TrustAgent( "Trustworthy Tim", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new BetrayAgent( "Betrayal Bill", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new TitForTatAgent( "TitForTat Tom", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    
                    // Goals
                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.ALIVE, numTimePeriods - 1 ) );
                    gameState.ForwardPropogate( 0 );
                    break;
                case 1:
                    numTimePeriods = 7;
                    numAgents = 5;
                    startingAgentHealth = 4;

                    gameState = new GameState( numTimePeriods, TIME_WINDOW_MIN, TIME_WINDOW_MAX, gameResources );
                    gameState.AddAgent( new TrustAgent( "Mr. Trustables", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new BetrayAgent( "Backstab Barry", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new TitForTatAgent( "Titmouse Fortat", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new TitForTatAgent( "Tat For Tit", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new BullyAgent( "Foobar'n Baz", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );

                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.ALIVE, 0 ) );
                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.ALIVE, 1 ) );
                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 4 ), AgentCondition.DEAD, numTimePeriods - 1 ) );
                    gameState.ForwardPropogate( 0 );
                    break;
                case 2:
                    numTimePeriods = 9;
                    numAgents = 6;
                    startingAgentHealth = 5;

                    gameState = new GameState( numTimePeriods, TIME_WINDOW_MIN, TIME_WINDOW_MAX + 1, gameResources );
                    gameState.AddAgent( new BullyAgent( "The Dragon", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new TrustAgent( "Princess", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new MemoryAgent( "A Bystander", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    gameState.AddAgent( new MemoryAgent( "Bystander B", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID() ) );
                    AgentGoal newGoal = new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.DEAD, numTimePeriods / 2 ); // kill the bully by halfway
                    gameState.AddAgent( new SimpleGoalAgent( "Angry Knight", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID(), newGoal ) );
                    AgentGoal newGoal2 = new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.DEAD, numTimePeriods - 1 ); // kill the bully by halfway
                    gameState.AddAgent( new SimpleGoalAgent( "Level-Headed Knight", numTimePeriods, numAgents, startingAgentHealth, gameState.GetNextAgentID(), newGoal2 ) );


                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 0 ), AgentCondition.DEAD, numTimePeriods - 1 ) ); // kill the bully by the end
                    gameState.AddGoal( new AgentGoal( gameState.GetAgentByID( 1 ), AgentCondition.ALIVE, numTimePeriods - 1 ) ); // kill the bully by the end
                    gameState.ForwardPropogate( 0 );
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch( GraphicsDevice );

            // Loading content
            gameResources.LoadAssets( Content );
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            // Allows the game to exit
            if ( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed )
                this.Exit();

            // Handling input
            switch ( gameState.Status ) {
                case GameStateStatus.RESTART:
                    SetupLevel( activeLevel );
                    break;
                case GameStateStatus.NEXT_LEVEL:
                    activeLevel++;
                    if ( activeLevel == NUM_LEVELS ) {
                        Console.Out.WriteLine( "We are out of levels!  Thanks for playing!" );
                        Exit();
                    }
                    SetupLevel( activeLevel );
                    break;
                default: // GameStateStatus.CONTINUE
                    gameState.HandleInput( Mouse.GetState(), WINDOW_WIDTH, WINDOW_HEIGHT );
                    break;
            }

            base.Update( gameTime );
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( Color.CornflowerBlue );

            //Drawing 
            gameState.Draw( GraphicsDevice, spriteBatch, WINDOW_WIDTH, WINDOW_HEIGHT );
            base.Draw( gameTime );
        }
    }
}
