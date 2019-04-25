using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisUWP
{
     
     class Util
     {
          public static int rotation_id(int rotation, bool clockwise)
          {
               return (int)((rotation % 4) * 2 + (clockwise ? 1 : 0));
          }
     }
     public class BlockTester : Locatable
     {
          public BlockTester(float x,float y,float width,float height)
          {
               this.X = x;
               this.Y = y;
               this.Width = width;
               this.Height = height;
          }
     }
     public struct Size
     {
          public Size(float width, float height) : this()
          {
               Width = width;
               Height = height;
          }

          public float Width { get; private set; }
          public float Height { get; private set; }
     }
}
