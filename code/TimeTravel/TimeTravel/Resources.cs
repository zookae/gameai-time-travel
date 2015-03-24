using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TimeTravel
{
    // A class to hold the game (asset) resources
    // HACK this is all hardcoded in... if this gets out of hand, write something saner
    public class GameResources
    {
        // To add a resource:
        // 1) add the raw asset to the content project
        // 2) make a new texture/sound/etc here
        // 3) load it in LoadAssets below
        public Texture2D cursor;
        public Texture2D selector;
        public Texture2D timeline;
        public Texture2D tick;

        // Graph/Environment Resources
        public Texture2D graphPanel;
        public Texture2D dot;
        public Texture2D ghost;

        // Agent
        public Texture2D agentBase;
        public Texture2D agentRing;
        public Texture2D agentDead;
        public Texture2D agentSmile;

        // Button Panel
        public Texture2D buttonPanel;
        public Texture2D buttonPressable;
        public Texture2D buttonPressed;
        public Texture2D exclamation;

        // Description Panel
        public Texture2D descriptionPanel;

        // Enemy Popup
        public Texture2D enemyPanel;

        // Results Panel
        public Texture2D resultsPanel;

        // Primitives
        public Texture2D blankRectangle;

        // Fonts
        public SpriteFont font;
        public SpriteFont tinyFont;
        public SpriteFont boldFont;

        public GameResources() { }

        public void LoadAssets( ContentManager cm )
        {
            cursor = cm.Load<Texture2D>( "cursor2" );
            selector = cm.Load<Texture2D>( "selector" );
            timeline = cm.Load<Texture2D>( "timeline" );
            tick = cm.Load<Texture2D>( "tick" );

            graphPanel = cm.Load<Texture2D>( "graph_panel" );
            dot = cm.Load<Texture2D>( "dot" );
            ghost = cm.Load<Texture2D>( "ghost" );

            buttonPanel = cm.Load<Texture2D>( "button_panel" );
            agentBase = cm.Load<Texture2D>( "agent_base" );
            agentRing = cm.Load<Texture2D>( "agent_ring" );
            agentDead = cm.Load<Texture2D>( "agent_dead_expression" );
            agentSmile = cm.Load<Texture2D>( "agent_smile_expression" );

            buttonPressable = cm.Load<Texture2D>( "button_pressable" );
            buttonPressed = cm.Load<Texture2D>( "button_pressed" );
            exclamation = cm.Load<Texture2D>( "exclaim" );

            descriptionPanel = cm.Load<Texture2D>( "description_panel" );

            enemyPanel = cm.Load<Texture2D>( "enemy_panel" );

            resultsPanel = cm.Load<Texture2D>( "victory_panel" );

            blankRectangle = cm.Load<Texture2D>( "rectangle" );

            font = cm.Load<SpriteFont>( "font" );
            tinyFont = cm.Load<SpriteFont>( "tiny_font" );
            boldFont = cm.Load<SpriteFont>( "bold_font" );
        }
    }
}
