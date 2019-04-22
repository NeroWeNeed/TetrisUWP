using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI;

namespace TetrisUWP
{
     public abstract class Drawable : Locatable
     {

          public abstract void draw(CanvasDrawEventArgs args);

     }
     public interface Collidable
     {
          bool isCollision(float x, float y);
          
     }
     public interface BoundingBox : Collidable
     {
          float top();
          float right();
          float bottom();
          float left();
          float Width { get; set; }
          float Height { get; set; }
          bool isCollision(BoundingBox box);
     }
     public abstract class Locatable : BoundingBox
     {
          public Locatable RelativeTo { get; set; }
          public float X
          {
               get; set;
          }
          public float Y
          {
               get; set;
          }

          public float Width { get; set; }
          public float Height { get; set; }

          public float bottom()
          {
               return y()+Height;
          }

          public virtual bool isCollision(BoundingBox box)
          {
               return (
                    box.left() < right() &&
                    box.right() > left() &&
                    box.top() < bottom() &&
                    box.bottom() > top()
                    );
          }

          public virtual bool isCollision(float x, float y)
          {
               return (x > left() && x < right() && y > top() && y < bottom());
          }

          public float left()
          {
               return x();
          }

          public float right()
          {
               return x()+Width;
          }

          public float top()
          {
               return y();
          }

          public float x()
          {
               if (RelativeTo != null)
               {
                    return RelativeTo.x() + X;
               }
               else
               {
                    return X;
               }
          }
          public float y()
          {
               if (RelativeTo != null)
               {
                    return RelativeTo.y() + Y;
               }
               else
               {
                    return Y;
               }
          }

     }

}