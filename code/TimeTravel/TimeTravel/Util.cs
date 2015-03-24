using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TimeTravel
{
    // Utility classes that aren't provided by the .NET framework or otherwise

    // 2-tuple
    public class Pair<A, B> {
        public Pair( A a, B b )
        {
            this.a = a;
            this.b = b;
        }

        public A a { get; set; }
        public B b { get; set; }
    }

    // 3-tuple
    public class Triple<A, B, C>
    {
        public Triple( A a, B b, C c ) 
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public A a { get; set; }
        public B b { get; set; }
        public C c { get; set; }
    }

    // 4-tuple
    public class Quadruple<A, B, C, D>
    {
        public Quadruple( A a, B b, C c, D d ) 
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public A a { get; set; }
        public B b { get; set; }
        public C c { get; set; }
        public D d { get; set; }
    }

    public static class DrawUtil
    {
        // Assumes position is upper left corner
        public static Vector2 GetCenter( Vector2 texturePosition, Texture2D texture )
        {
            return new Vector2( texturePosition.X + ( texture.Width / 2 ), texturePosition.Y + ( texture.Height / 2 ) );
        }

        public static Vector2 GetCenter( Vector2 texturePosition, int width, int height )
        {
            return new Vector2( texturePosition.X + ( width / 2 ), texturePosition.Y + ( height / 2 ) );
        }

        // Be careful when using this, as ints get casted to floats, so re-conversion might round down....
        public static Vector2 GetHalfDims( Texture2D texture )
        {
            return new Vector2( texture.Width / 2, texture.Height / 2 );
        }

        public static Vector2 GetHalfDims( int width, int height )
        {
            return new Vector2( width / 2, height / 2 );
        }
/*
        public static int GetHalfWidth( Texture2D texture )
        {
            return texture.Width / 2;
        }

        public static int GetHalfHeight( Texture2D texture )
        {
            return texture.Height / 2;
        }
 * */

        public static bool WithinBounds( Vector2 texturePosition, Texture2D texture, int mouseX, int mouseY )
        {
            return (mouseX >= texturePosition.X && mouseX <= texturePosition.X + texture.Width) && 
                (mouseY >= texturePosition.Y && mouseY <= texturePosition.Y + texture.Height);
        }

        public static bool WithinBounds( Vector2 texturePosition, int width, int height, int mouseX, int mouseY )
        {
            return ( mouseX >= texturePosition.X && mouseX <= texturePosition.X + width ) &&
                ( mouseY >= texturePosition.Y && mouseY <= texturePosition.Y + height );
        }

        public static bool WithinPixelBounds( int x0, int y0, int x1, int y1, int mouseX, int mouseY )
        {
            return ( mouseX >= x0 && mouseX <= x1 ) && ( mouseY >= y0 && mouseY <= y1 );
        }

        public static bool WidthOverlap( Vector2 texCenter1, Vector2 texCenter2, Texture2D texture1, Texture2D texture2 )
        {
            return ( Math.Abs(texCenter1.X - texCenter2.X) ) < ( texture1.Width + texture2.Width );  
        }

        public static bool HeightOverlap( Vector2 texCenter1, Vector2 texCenter2, Texture2D texture1, Texture2D texture2 )
        {
            return ( Math.Abs( texCenter1.Y - texCenter2.Y ) ) < ( texture1.Height + texture2.Height );
        }
    }
}
