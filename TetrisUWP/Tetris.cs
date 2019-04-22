using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;

namespace TetrisUWP
{
     abstract class TetrisBlock : Drawable, Collidable
     {
          public Tetrimino[] Tetriminos
          {
               get; protected set;
          }



          public int Rotation { get; set; }




          public void rotateClockwise()
          {
               Rotation = (Rotation + 1) % 4;
          }
          public void rotateCounterClockwise()
          {
               Rotation = (Rotation - 1) % 4;
          }


          public bool isCollision(float x, float y)
          {


               foreach (var z in Tetriminos)
               {
                    return false;
               }
               return false;
          }
          public override void draw(CanvasDrawEventArgs args)
          {
               foreach (var x in Tetriminos)
               {
                    x.draw(args);
               }

          }

     }

     public class Tetrimino : Drawable, Collidable, BoundingBox
     {


          public static CanvasBitmap Graphic
          {
               get; private set;
          }
          public static Windows.Foundation.Size Size
          {
               get; private set;
          }


          public static async Task CreateResourcesAsync(CanvasControl sender)
          {
               Graphic = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/Tetrimino.png"));

               Size = Graphic.Size;
          }
          public Tetrimino(float x, float y, Locatable relativeTo = null)
          {
               X = x;
               Y = y;
               this.Width = (float)Size.Width;
               this.Height = (float)Size.Height;
               RelativeTo = relativeTo;
          }


          public override void draw(CanvasDrawEventArgs args)
          {
               args.DrawingSession.DrawImage(Graphic, x(), y());
          }


     }

     class TetrisGrid : Drawable
     {
          public static int HorizontalBlocks = 10;
          public static int VerticalBlocks = 20;
          private bool[,] filled = new bool[HorizontalBlocks, VerticalBlocks];
          public TetrisGrid()
          {

               this.Height = (float)(VerticalBlocks * Tetrimino.Size.Height);
               this.Width = (float)(HorizontalBlocks * Tetrimino.Size.Width);
          }

          public override void draw(CanvasDrawEventArgs args)
          {

               for (int i = 0; i < VerticalBlocks + 1; i++)
               {

                    args.DrawingSession.DrawLine(0.0f, (float)(Tetrimino.Size.Height * i), Width, (float)(Tetrimino.Size.Height * i), Colors.Black);

               }
               for (int i = 0; i < HorizontalBlocks + 1; i++)
               {

                    args.DrawingSession.DrawLine((float)(Tetrimino.Size.Height * i), 0.0f, (float)(Tetrimino.Size.Height * i), Height, Colors.Black);

               }
               for (int i = 0; i < filled.GetLength(1); i++)
               {
                    for (int j = 0; j < filled.GetLength(0); j++)
                    {
                         if (filled[j, i])
                         {
                              args.DrawingSession.DrawImage(Tetrimino.Graphic, j * (float)Tetrimino.Size.Width, i * (float)Tetrimino.Size.Height);

                         }
                    }
               }
          }



          public void setBlock(TetrisBlock block)
          {
               int x, y;
               foreach (var z in block.Tetriminos)
               {
                    x = (int)(z.x() / Tetrimino.Size.Width);
                    y = (int)(z.y() / Tetrimino.Size.Height);
                    filled[x, y] = true;
               }
          }

          public override bool isCollision(BoundingBox box)
          {

               return (
                    box.left() < left() ||
                    box.right() > right() ||
                    box.top() < top() ||
                    box.bottom() > bottom()
                    );


          }
          public bool isCollision(TetrisBlock block, float offX = 0, float offY = 0)
          {
               float tileLeft, tileTop;
               foreach (var box in block.Tetriminos)
               {

                    if (box.left() + offX < left() || box.right() + offX > right() || box.top() + offY < top() || box.bottom() + offY > bottom())
                         return true;
                    for (int i = 0; i < filled.GetLength(1); i++)
                    {
                         for (int j = 0; j < filled.GetLength(0); j++)
                         {
                              if (!filled[j, i])
                                   continue;

                              tileLeft = j * box.Width;
                              tileTop = i * box.Height;
                              if (box.left() + offX < tileLeft + box.Width && box.right() + offX > tileLeft && box.top() + offY < tileTop + box.Height && box.bottom() + offY > tileTop)
                                   return true;
                         }
                    }
               }
               return false;

          }

          public override bool isCollision(float x, float y)
          {
               float boxX, boxY, boxW = (float)Tetrimino.Size.Width, boxH = (float)Tetrimino.Size.Height;
               for (int i = 0; i < filled.GetLength(1); i++)
               {
                    for (int j = 0; j < filled.GetLength(0); j++)
                    {
                         if (!filled[j, i])
                              continue;
                         boxX = j * boxW;
                         boxY = i * boxH - boxH;
                         if (x >= boxX && x <= boxX + boxW && y >= boxY && y <= boxY + boxH)
                              return true;
                    }

               }
               return false;
          }
     }
     class TetrisBag
     {

          private Func<TetrisBlock>[] tetriminoFactories = {
               TetrisBlockO.create,
               TetrisBlockI.create,
               TetrisBlockL.create,
               TetrisBlockT.create,
               TetrisBlockJ.create,
               TetrisBlockS.create,
               TetrisBlockZ.create,
               }
               ;
          private Queue<TetrisBlock> tetriminos = new Queue<TetrisBlock>();
          private Random random;
          public TetrisBag() : this(new Random())
          {


          }
          public TetrisBag(Random random)
          {
               this.random = random;
               List<TetrisBlock> bag = new List<TetrisBlock>();

               bag.AddRange(generateBag());
               bag.AddRange(generateBag());
               bag.ForEach(x => tetriminos.Enqueue(x));

          }
          private IEnumerable<TetrisBlock> generateBag()
          {
               List<TetrisBlock> result = new List<TetrisBlock>();
               foreach (Func<TetrisBlock> factory in tetriminoFactories)
               {
                    result.Add(factory.Invoke());
               }
               
               return result.OrderBy(x => random.Next());
          }
          public TetrisBlock next()
          {
               TetrisBlock block = tetriminos.Dequeue();
               if (tetriminos.Count <= tetriminoFactories.Length)
               {
                    List<TetrisBlock> bag = generateBag().ToList();
                    bag.ForEach(x => tetriminos.Enqueue(x));
               }
               return block;
          }
     }

     class TetrisBlockHandler : Drawable
     {
          public enum Action
          {
               None,MovingLeft, MovingRight, FastDropping, RotatingClockWise, RotatingCounterClockwise
          }
          private TetrisBlock block;
          private TetrisGrid grid;
          private TetrisBag bag = new TetrisBag();
          private ThreadPoolTimer timer;
          public Action CurrentAction { get; set; }

          private DispatcherTimer dispatcherTimer = new DispatcherTimer();

          public TetrisBlockHandler(TetrisGrid grid)
          {
               this.grid = grid;
               //timer = new Timer(tick,null, (int)TimeSpan.FromSeconds(1).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds)
               timer = ThreadPoolTimer.CreatePeriodicTimer(tick, TimeSpan.FromSeconds(1));



          }

          public override void draw(CanvasDrawEventArgs args)
          {
               if (block != null)
                    block.draw(args);
               grid.draw(args);
          }

          public void start()
          {
               block = new TetrisBlockO();
          }
          public void tick(ThreadPoolTimer timer)
          {

               if (block != null)
               {
                    if (grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
                    {
                         grid.setBlock(block);

                         //Check for matching lines

                         block = bag.next();
                    }
                    else
                    {
                         block.Y += (float)Tetrimino.Size.Height;
                    }



               }
               else
               {
                    block = bag.next();

               }
          }

          public void startMovingLeft()
          {
               CurrentAction = Action.MovingLeft;
               moveLeft(null,null);
          }
          public void stopMovingLeft()
          {
               if (CurrentAction == Action.MovingLeft) {
                    CurrentAction = Action.None;
                    dispatcherTimer.Stop();
               }
               
          }
          public void startMovingRight()
          {
               CurrentAction = Action.MovingRight;
               moveRight(null,null);
          }
          public void stopMovingRight()
          {
               if (CurrentAction == Action.MovingRight)
               {
                    CurrentAction = Action.None;
                    dispatcherTimer.Stop();

               }
                    
          }

          public void moveLeft(object sender, object e)
          {
               if (CurrentAction == Action.MovingLeft)
               {
                    if (!grid.isCollision(block, -(float)Tetrimino.Size.Width, 0))
                    {
                         block.X += -(float)Tetrimino.Size.Width;
                    }
                    dispatcherTimer.Stop();
                    dispatcherTimer.Tick += moveLeft;
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer.Start();

               }

          }


          public void moveRight(object sender, object e)
          {
               if (CurrentAction == Action.MovingRight)
               {
                    if (!grid.isCollision(block, (float)Tetrimino.Size.Width, 0))
                    {
                         block.X += (float)Tetrimino.Size.Width;
                    }
                    dispatcherTimer.Stop();
                    dispatcherTimer.Tick += moveRight;
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer.Start();

               }
          }
          public void fastDrop()
          {
               while (!grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
               {
                    block.Y += (float)Tetrimino.Size.Height;
               }
               grid.setBlock(block);
               block = bag.next();

          }
          public void rotateClockwise()
          {

          }
          public void rotateCounterClockwise()
          {

          }
     }

     //Blocks
     class TetrisBlockO : TetrisBlock
     {

          public static TetrisBlockO create()
          {
               return new TetrisBlockO();
          }
          public TetrisBlockO()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino(0, 0, this),
                    new Tetrimino(0, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float)Tetrimino.Size.Width, 0, this),
                    new Tetrimino((float)Tetrimino.Size.Width, (float)Tetrimino.Size.Height, this)

               };
          }


     }
     class TetrisBlockT : TetrisBlock
     {
          public static TetrisBlockT create()
          {
               return new TetrisBlockT();
          }
          public TetrisBlockT()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino((float)Tetrimino.Size.Width, 0, this),
                    new Tetrimino(0, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float)Tetrimino.Size.Width, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float)Tetrimino.Size.Width*2, (float)Tetrimino.Size.Height, this)

               };
          }


     }
     class TetrisBlockI : TetrisBlock
     {
          public static TetrisBlockI create()
          {
               return new TetrisBlockI();
          }
          public TetrisBlockI()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino(0, 0, this),
                    new Tetrimino(0, (float)Tetrimino.Size.Height, this),
                    new Tetrimino(0, (float)Tetrimino.Size.Height*2, this),
                    new Tetrimino(0, (float)Tetrimino.Size.Height*3, this)

               };
          }


     }
     class TetrisBlockZ : TetrisBlock
     {
          public static TetrisBlockZ create()
          {
               return new TetrisBlockZ();
          }
          public TetrisBlockZ()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino(0, 0, this),
                    new Tetrimino((float) Tetrimino.Size.Width, 0, this),
                    new Tetrimino((float) Tetrimino.Size.Width, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float) Tetrimino.Size.Width*2, (float)Tetrimino.Size.Height, this)


               };
          }


     }
     class TetrisBlockS : TetrisBlock
     {
          public static TetrisBlockS create()
          {
               return new TetrisBlockS();
          }
          public TetrisBlockS()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino(0, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float) Tetrimino.Size.Width, (float)Tetrimino.Size.Height, this),
                    new Tetrimino((float) Tetrimino.Size.Width,0, this),
                    new Tetrimino((float) Tetrimino.Size.Width*2, 0, this)


               };
          }


     }
     class TetrisBlockL : TetrisBlock
     {
          public static TetrisBlockL create()
          {
               return new TetrisBlockL();
          }
          public TetrisBlockL()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino(0, 0, this),
                    new Tetrimino(0,(float)Tetrimino.Size.Height,this),
                    new Tetrimino(0,(float)Tetrimino.Size.Height*2,this),
                    new Tetrimino((float) Tetrimino.Size.Width,(float)Tetrimino.Size.Height*2,this)



               };
          }


     }
     class TetrisBlockJ : TetrisBlock
     {
          public static TetrisBlockJ create()
          {
               return new TetrisBlockJ();
          }
          public TetrisBlockJ()
          {
               Tetriminos = new Tetrimino[]
               {
                    new Tetrimino((float) Tetrimino.Size.Width, 0, this),
                    new Tetrimino((float) Tetrimino.Size.Width,(float)Tetrimino.Size.Height,this),
                    new Tetrimino((float) Tetrimino.Size.Width,(float)Tetrimino.Size.Height*2,this),
                    new Tetrimino(0,(float)Tetrimino.Size.Height*2,this)



               };
          }


     }

}
