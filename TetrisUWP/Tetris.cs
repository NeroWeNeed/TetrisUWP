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
               get; private set;
          }
          public Tetrimino OriginTetrimino
          {
               get; private set;
          }
          private float offsetX = 0;
          private float offsetY = 0;
          protected byte[,,] TetriminoLayout { get; private set; }
          protected Tuple<int, int>[][] TetriminoKicks { get; private set; }
          public TetrisBlock(byte[,,] layout, Tuple<int, int>[][] kicks)
          {
               this.TetriminoLayout = layout;
               this.TetriminoKicks = kicks;
               OriginTetrimino = new Tetrimino(0, 0, this);
               Tetriminos = new Tetrimino[] {
                    OriginTetrimino,
                    new Tetrimino(0, 0, this),
                    new Tetrimino(0, 0, this),
                    new Tetrimino(0, 0, this)

               };
               Rotation = 0;
               layoutTetriminos();
          }

          public int Rotation { get; set; }

          public void layoutTetriminos(Tuple<int, int> offset = null)
          {
               byte v;

               int current = 0;
               float offX = (offset != null ? offset.Item1 * (float)Tetrimino.Size.Width : 0);
               float offY = (offset != null ? offset.Item2 * (float)Tetrimino.Size.Height : 0);
               this.X += offX;
               this.Y += offY;
               int firstX = -1, lastX = -1, firstY = -1, lastY = -1;
               Tuple<int, int>[] coords = new Tuple<int, int>[4];
               for (int i = 0; i < TetriminoLayout.GetLength(1); i++)
               {
                    for (int j = 0; j < TetriminoLayout.GetLength(2); j++)
                    {
                         v = TetriminoLayout[Rotation, i, j];
                         if (v == 0)
                              continue;



                         Tetriminos[current].X = i * ((float)Tetrimino.Size.Width);
                         Tetriminos[current].Y = j * ((float)Tetrimino.Size.Height);
                         current += 1;
                         firstX = (firstX == -1 || i < firstX ? i : firstX);
                         firstY = (firstY == -1 || j < firstY ? j : firstY);
                         lastX = (lastX == -1 || i > lastX ? i : lastX);
                         lastY = (lastY == -1 || j > firstY ? j : lastY);

                    }

               }
               Width = (float)((lastX - firstX + 1) * Tetrimino.Size.Width);
               Height = (float)((lastY - firstY + 1) * Tetrimino.Size.Height);
               offsetX = (float)(firstX * Tetrimino.Size.Width);
               offsetY = (float)(firstY * Tetrimino.Size.Height);

          }


          public Tuple<int, int> canRotate(TetrisGrid grid, bool clockwise)
          {
               byte v;
               bool passing = true;
               int target;
               if (clockwise)
                    target = (Rotation + 1) % 4;
               else
               {
                    if (Rotation - 1 < 0)
                         target = 3;
                    else
                         target = Rotation - 1;
               }
               BlockTester tester = new BlockTester(0, 0, (float)Tetrimino.Size.Width, (float)Tetrimino.Size.Height);

               foreach (Tuple<int, int> kick in TetriminoKicks[Util.rotation_id(target, clockwise)])
               {
                    passing = true;
                    for (int i = 0; i < TetriminoLayout.GetLength(1); i++)
                    {
                         for (int j = 0; j < TetriminoLayout.GetLength(2); j++)
                         {
                              v = TetriminoLayout[target, i, j];
                              if (v == 0)
                                   continue;

                              tester.X = i * ((float)Tetrimino.Size.Width) + (kick.Item1 * (float)Tetrimino.Size.Width) + this.x();
                              tester.Y = j * ((float)Tetrimino.Size.Height) + (kick.Item2 * (float)Tetrimino.Size.Height) + this.y();
                              if (grid.isCollision(tester))
                              {
                                   passing = false;
                                   break;
                              }




                         }
                         if (passing == false)
                              break;
                    }
                    if (passing == true)
                         return kick;

               }
               return null;

          }


          public void rotateClockwise(Tuple<int, int> kick = null)
          {
               Rotation = (Rotation + 1) % 4;
               layoutTetriminos(kick);

          }
          public void rotateCounterClockwise(Tuple<int, int> kick = null)
          {
               if (Rotation - 1 < 0)
                    Rotation = 3;
               else
                    Rotation = (Rotation - 1) % 4;
               layoutTetriminos(kick);
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
          public void draw(CanvasDrawEventArgs args, float x, float y)
          {
               foreach (var t in Tetriminos)
               {
                    t.draw(args, x - offsetX, y - offsetY);
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
            args.DrawingSession.DrawImage(Graphic, x(), y());
          }
          public void draw(CanvasDrawEventArgs args, float x, float y)
          {

               args.DrawingSession.DrawImage(Graphic, x + X, y + Y);
          }


     }

     class TetrisGrid : Drawable
     {
          public static int HorizontalBlocks = 10;
          public static int VerticalBlocks = 20;
          public bool[,] Filled { get; private set; } = new bool[HorizontalBlocks, VerticalBlocks];


          public TetrisGrid() : this(0, 0)
          {


          }
          public TetrisGrid(float x, float y)
          {
               this.Height = (float)(VerticalBlocks * Tetrimino.Size.Height);
               this.Width = (float)(HorizontalBlocks * Tetrimino.Size.Width);
               this.X = x;
               this.Y = y;

          }


          public override void draw(CanvasDrawEventArgs args)
          {

               for (int i = 0; i < VerticalBlocks + 1; i++)
               {

                    args.DrawingSession.DrawLine(x(), y() + (float)(Tetrimino.Size.Height * i), x() + Width, y() + (float)(Tetrimino.Size.Height * i), Colors.Black);

               }
               for (int i = 0; i < HorizontalBlocks + 1; i++)
               {

                    args.DrawingSession.DrawLine(x() + (float)(Tetrimino.Size.Height * i), y(), x() + (float)(Tetrimino.Size.Height * i), y() + Height, Colors.Black);

               }
               for (int i = 0; i < Filled.GetLength(1); i++)
               {
                    for (int j = 0; j < Filled.GetLength(0); j++)
                    {
                         if (Filled[j, i])
                         {
                              args.DrawingSession.DrawImage(Tetrimino.Graphic, j * (float)Tetrimino.Size.Width + x(), i * (float)Tetrimino.Size.Height + y());

                         }
                    }
               }
          }



          public void setBlock(TetrisBlock block)
          {
               int x, y;
               foreach (var z in block.Tetriminos)
               {
                    x = (int)((z.x() - this.x()) / Tetrimino.Size.Width);
                    y = (int)((z.y() - this.y()) / Tetrimino.Size.Height);
                    Filled[x, y] = true;
               }
          }

          public override bool isCollision(BoundingBox box)
          {

               if (
                    box.left() < left() ||
                    box.right() > right() ||
                    box.top() < top() ||
                    box.bottom() > bottom()
                    )
                    return true;

               int testX = (int)((box.left() - x()) / Tetrimino.Size.Width);
               int testY = (int)((box.top() - y()) / Tetrimino.Size.Height);
               return Filled[testX, testY];



          }
          public bool isCollision(TetrisBlock block, float offX, float offY)
          {
               BlockTester tester;
               foreach (var tetrimino in block.Tetriminos)
               {
                    tester = new BlockTester(tetrimino.x() + offX, tetrimino.y() + offY, (float)Tetrimino.Size.Width, (float)Tetrimino.Size.Height);
                    if (isCollision(tester))
                         return true;
               }

               return false;
          }


          public override bool isCollision(float x, float y)
          {
               float boxX, boxY, boxW = (float)Tetrimino.Size.Width, boxH = (float)Tetrimino.Size.Height;
               for (int i = 0; i < Filled.GetLength(1); i++)
               {
                    for (int j = 0; j < Filled.GetLength(0); j++)
                    {
                         if (!Filled[j, i])
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
     class TetrisBag : Drawable
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
          public int DisplayCount { get; set; } = 4;

          public TetrisBag(Random random = null, float x = 0, float y = 0, Locatable relativeTo = null)
          {
               if (random == null)
                    this.random = new Random();
               else
                    this.random = random;
               List<TetrisBlock> bag = new List<TetrisBlock>();
               this.X = x;
               this.Y = y;
               this.RelativeTo = relativeTo;
               bag.AddRange(generateBag());
               bag.AddRange(generateBag());
               bag.ForEach(zx => tetriminos.Enqueue(zx));
               Width = ((float)Tetrimino.Size.Width * 5);
               Height = ((float)Tetrimino.Size.Height * 5 * DisplayCount);

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

          public override void draw(CanvasDrawEventArgs args)
          {
               args.DrawingSession.DrawRectangle(x(), y(), Width, Height, Colors.Black);
               TetrisBlock b;
               for (int i = 0; i < DisplayCount; i++)
               {
                    b = tetriminos.ElementAt(i);
                    b.draw(args, x() + (Width - b.Width) / 2, y() + ((float)Tetrimino.Size.Height * 5 * (i)) + ((((float)Tetrimino.Size.Height * 5) - b.Height) / 2));

               }
          }
     }

     class TetrisBlockHandler : Drawable
     {
          public enum Action
          {
               FastDropping, RotatingClockWise, RotatingCounterClockwise
          }
          public enum MoveAction
          {
               None, Left, Right
          }

          private TetrisBlock block;
          private TetrisGrid grid;
          private TetrisBag bag;
          private TetrisBlockHolder holder;
          private ThreadPoolTimer timer;
          private TetrisScoreKeeper scoreKeeper = new TetrisScoreKeeper();
          public MoveAction CurrentMoveAction { get; set; }

          public Action CurrentAction { get; set; }

          public bool FastDrop { get; set; } = false;

          //Counters
          public uint GravityTicks { get; set; } = 60;
          public uint FastDropGravityTicks { get; set; } = 2;
          public uint MoveTicks { get; set; } = 5;
          private uint ticks = 0;
          private uint moveTicks = 0;
          private bool swapped = false;


          public TetrisBlockHandler(TetrisGrid grid, TetrisBlockHolder holder, TetrisBag bag)
          {
               this.grid = grid;
               this.holder = holder;
               this.bag = bag;
               //timer = new Timer(tick,null, (int)TimeSpan.FromSeconds(1).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds)
               timer = ThreadPoolTimer.CreatePeriodicTimer(tick, TimeSpan.FromMilliseconds(16));



          }

          public override void draw(CanvasDrawEventArgs args)
          {
               if (block != null)
                    block.draw(args);
               grid.draw(args);
               holder.draw(args);
               bag.draw(args);
          }

          public void start()
          {
               spawnBlock();

          }
          public void tick(ThreadPoolTimer timer)
          {
               ticks += 1;
               if (CurrentMoveAction != MoveAction.None && moveTicks % MoveTicks == 0)
               {
                    if (CurrentMoveAction == MoveAction.Left)
                         moveLeft();
                    else if (CurrentMoveAction == MoveAction.Right)
                         moveRight();
               }
               if (CurrentMoveAction == MoveAction.Left || CurrentMoveAction == MoveAction.Right)
                    moveTicks += 1;





               if (ticks % (FastDrop ? (Math.Min(FastDropGravityTicks, GravityTicks)) : GravityTicks) == 0)
                    handleGravityTick();



          }
          private void handleGravityTick()
          {
               if (block != null)
               {

                    if (grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
                    {
                         setBlock();

                         
                         //Check for matching lines

                         spawnBlock();
                    }
                    else
                    {
                         block.Y += (float)Tetrimino.Size.Height;
                    }



               }
               else
               {
                    spawnBlock();


               }
          }

          private void spawnBlock(TetrisBlock block = null)
          {

               TetrisBlock b;
               if (block == null)
                    b = bag.next();
               else
                    b = block;

               b.X = grid.x() + (((TetrisGrid.HorizontalBlocks / 2) - 2) * ((float)Tetrimino.Size.Width));
               b.Y = grid.y();
               this.block = b;
          }
          private void setBlock()
          {
               swapped = false;
               grid.setBlock(block);
               scoreKeeper.checkGrid(grid);
          }
          public void startMovingLeft()
          {
               CurrentMoveAction = MoveAction.Left;


          }
          public void stopMovingLeft()
          {
               if (CurrentMoveAction == MoveAction.Left)
               {
                    CurrentMoveAction = MoveAction.None;

                    moveTicks = 0;
               }

          }
          public void holdBlock()
          {
               if (!swapped)
               {
                    TetrisBlock b = holder.Block;
                    block.Rotation = 0;
                    block.layoutTetriminos();
                    if (b == null)
                    {
                         holder.Block = block;
                         spawnBlock();

                    }
                    else
                    {
                         holder.Block = block;
                         spawnBlock(b);
                         swapped = true;
                    }

               }
          }
          public void startMovingRight()
          {
               CurrentMoveAction = MoveAction.Right;


          }
          public void stopMovingRight()
          {
               if (CurrentMoveAction == MoveAction.Right)
               {
                    CurrentMoveAction = MoveAction.None;
                    moveTicks = 0;

               }

          }

          public void moveLeft()
          {
               if (CurrentMoveAction == MoveAction.Left && block != null)
               {

                    if (!grid.isCollision(block, (float)-Tetrimino.Size.Width, 0))
                    {
                         block.X += -(float)Tetrimino.Size.Width;
                    }


               }

          }


          public void moveRight()
          {
               if (CurrentMoveAction == MoveAction.Right && block != null)
               {
                    if (!grid.isCollision(block, (float)Tetrimino.Size.Width, 0))
                    {
                         block.X += (float)Tetrimino.Size.Width;
                    }


               }
          }
          public void hardDrop()
          {

               while (!grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
               {
                    block.Y += (float)Tetrimino.Size.Height;

               }
               setBlock();
               spawnBlock();

          }


          public void startFastDrop()
          {
               FastDrop = true;


          }
          public void stopFastDrop()
          {
               FastDrop = false;


          }
          public void rotateClockwise()
          {
               Tuple<int, int> kick = block.canRotate(grid, true);
               if (kick != null)
                    block.rotateClockwise(kick);


          }
          public void rotateCounterClockwise()
          {
               Tuple<int, int> kick = block.canRotate(grid, false);
               if (kick != null)
                    block.rotateCounterClockwise(kick);
          }
     }

     class TetrisBlockHolder : Drawable
     {
          public TetrisBlock Block { get; set; } = null;
          public TetrisBlockHolder(float x, float y, Locatable relativeTo)
          {
               this.X = x;
               this.Y = y;
               this.RelativeTo = relativeTo;
               Width = ((float)Tetrimino.Size.Width * 5);
               Height = ((float)Tetrimino.Size.Height * 5);
          }

          public override void draw(CanvasDrawEventArgs args)
          {
               args.DrawingSession.DrawRectangle(x(), y(), Width, Height, Colors.Black);
               if (Block != null)
                    Block.draw(args, x() + (Width - Block.Width) / 2, y() + (Height - Block.Height) / 2);



          }
     }

     class TetrisScoreKeeper
     {
          public int Score { get; set; }
          public int Level { get; set; } = 1;
          public int Combo { get; set; } = 0;
          private bool previousCheckTetris = false;

          public void checkGrid(TetrisGrid grid)
          {
               int lines = 0;
               bool lineClear;
               for (int i = grid.Filled.GetLength(1) - 1; i >= 0; i--)
               {
                    lineClear = true;
                    
                    for (int j = 0; j < grid.Filled.GetLength(0); j++)
                    {
                         
                         if (!grid.Filled[j,i])
                         {
                              lineClear = false;
                              break;
                         }

                    }
                    if (lineClear)
                    {
                         lines += 1;
                         for (int k = i; k > 0; k--)
                         {
                              for (int l = 0; l < grid.Filled.GetLength(0); l++)
                              {
                                   grid.Filled[l, k] = grid.Filled[l, k - 1];
                              }
                         }
                         for (int l = 0; l < grid.Filled.GetLength(0); l++)
                         {
                              grid.Filled[l, 0] = false;
                         }
                         i += 1;
                    }

               }
               if (lines >= 4)
               {
                    int d = 0;
               }
               switch (lines)
               {
                    case 1:
                         Score += 100 * Level;
                         Combo += 1;
                         break;
                    case 2:
                         Score += 300 * Level;
                         Combo += 1;
                         break;
                    case 3:
                         Score += 500 * Level;
                         Combo += 1;
                         break;
                    case 4:
                         if (previousCheckTetris)
                         {
                              Score += 1200 * Level;
                              Combo += 1;
                              previousCheckTetris = true;
                         }
                         else
                         {
                              Score += 800 * Level;
                              Combo += 1;
                              previousCheckTetris = true;
                         }

                         break;
                    default:
                         Combo = 0;
                         break;
               }
               Score += Combo * 50 * Level;


          }
     }
          /*
           * Blocks
           */

          class TetrisBlockO : TetrisBlock
          {

               public static TetrisBlockO create()
               {
                    return new TetrisBlockO();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 0,0,0,0},
                         { 0,1,1,0},
                         { 0,2,1,0},
                         { 0,0,0,0}

                    },
                                        {
                         { 0,0,0,0},
                         { 0,2,1,0},
                         { 0,1,1,0},
                         { 0,0,0,0}

                    },
                                        {
                         { 0,0,0,0},
                         { 0,1,2,0},
                         { 0,1,1,0},
                         { 0,0,0,0}

                    },
                                        {
                         { 0,0,0,0},
                         { 0,1,1,0},
                         { 0,1,2,0},
                         { 0,0,0,0}

                    }

               };
               private static Tuple<int, int>[][] kicks = new Tuple<int, int>[8][]
               {
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               new Tuple<int, int>[1] {
                    Tuple.Create(0,0)
                    },
               };
               public TetrisBlockO() : base(layout, kicks)
               {


               }


          }
          class TetrisBlockT : TetrisBlock
          {
               public static TetrisBlockT create()
               {
                    return new TetrisBlockT();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 0,0,0,0},
                         { 0,1,0,0},
                         { 1,2,0,0},
                         { 0,1,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 0,0,0,0},
                         { 1,2,1,0},
                         { 0,1,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 0,1,0,0},
                         { 0,2,1,0},
                         { 0,1,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 0,1,0,0},
                         { 1,2,1,0},
                         { 0,0,0,0}

                    }




               };
               public static Tuple<int, int>[][] kicks = new Tuple<int, int>[8][]
               {
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(1,-1),
                    Tuple.Create(0,2),
                    Tuple.Create(1,2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(-1,-1),
                    Tuple.Create(0,2),
                    Tuple.Create(-1,2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(1,1),
                    Tuple.Create(0,-2),
                    Tuple.Create(1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(1,1),
                    Tuple.Create(0,-2),
                    Tuple.Create(1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(-1,-1),
                    Tuple.Create(0,2),
                    Tuple.Create(-1,2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(1,-1),
                    Tuple.Create(0,2),
                    Tuple.Create(1,2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(-1,1),
                    Tuple.Create(0,-2),
                    Tuple.Create(-1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(-1,1),
                    Tuple.Create(0,-2),
                    Tuple.Create(-1,-2),

               }


               };

               public TetrisBlockT() : base(layout, kicks)
               {

               }


          }
          class TetrisBlockI : TetrisBlock
          {
               public static TetrisBlockI create()
               {
                    return new TetrisBlockI();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 0,1,0,0},
                         { 0,1,0,0},
                         { 0,2,0,0},
                         { 0,1,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 0,0,0,0},
                         { 1,1,2,1},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,1,0},
                         { 0,0,2,0},
                         { 0,0,1,0},
                         { 0,0,1,0}

                    },
                    {
                         { 0,0,0,0},
                         { 1,2,1,1},
                         { 0,0,0,0},
                         { 0,0,0,0}

                    }




               };
               private static Tuple<int, int>[][] kicks = new Tuple<int, int>[8][]
     {
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(2,0),
                    Tuple.Create(-1,-2),
                    Tuple.Create(2,1),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-2,0),
                    Tuple.Create(1,0),
                    Tuple.Create(-2,1),
                    Tuple.Create(1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(2,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(2,-1),
                    Tuple.Create(-1,2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(2,0),
                    Tuple.Create(-1,-2),
                    Tuple.Create(2,-1),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(-2,-1),
                    Tuple.Create(1,2),
                    Tuple.Create(-2,-1),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(2,0),
                    Tuple.Create(-1,0),
                    Tuple.Create(2,-1),
                    Tuple.Create(-1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(-2,0),
                    Tuple.Create(1,0),
                    Tuple.Create(-2,1),
                    Tuple.Create(1,-2),

               },
               new Tuple<int, int>[] {
                    Tuple.Create(0,0),
                    Tuple.Create(1,0),
                    Tuple.Create(-2,0),
                    Tuple.Create(1,2),
                    Tuple.Create(-2,-1),

               }


     };
               public TetrisBlockI() : base(layout, kicks)
               {

               }


          }
          class TetrisBlockZ : TetrisBlock
          {
               public static TetrisBlockZ create()
               {
                    return new TetrisBlockZ();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 0,1,0,0},
                         { 1,2,0,0},
                         { 1,0,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 1,2,0,0},
                         { 0,1,1,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,1,0},
                         { 0,2,1,0},
                         { 0,1,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 1,1,0,0},
                         { 0,2,1,0},
                         { 0,0,0,0},
                         { 0,0,0,0}

                    }




               };
               public TetrisBlockZ() : base(layout, TetrisBlockT.kicks)
               {
               }


          }
          class TetrisBlockS : TetrisBlock
          {
               public static TetrisBlockS create()
               {
                    return new TetrisBlockS();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 1,0,0,0},
                         { 1,2,0,0},
                         { 0,1,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 0,2,1,0},
                         { 1,1,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,1,0,0},
                         { 0,2,1,0},
                         { 0,0,1,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,1,1,0},
                         { 1,2,0,0},
                         { 0,0,0,0},
                         { 0,0,0,0}

                    }




               };
               public TetrisBlockS() : base(layout, TetrisBlockT.kicks)
               {
               }


          }
          class TetrisBlockL : TetrisBlock
          {
               public static TetrisBlockL create()
               {
                    return new TetrisBlockL();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                    {
                         { 1,1,0,0},
                         { 0,2,0,0},
                         { 0,1,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,0,0},
                         { 1,2,1,0},
                         { 1,0,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,1,0,0},
                         { 0,2,0,0},
                         { 0,1,1,0},
                         { 0,0,0,0}

                    },
                    {
                         { 0,0,1,0},
                         { 1,2,1,0},
                         { 0,0,0,0},
                         { 0,0,0,0}

                    },




               };
               public TetrisBlockL() : base(layout, TetrisBlockT.kicks)
               {

               }


          }
          class TetrisBlockJ : TetrisBlock
          {
               public static TetrisBlockJ create()
               {
                    return new TetrisBlockJ();
               }
               private static byte[,,] layout = new byte[4, 4, 4] {
                                        {
                         { 0,1,0,0},
                         { 0,2,0,0},
                         { 1,1,0,0},
                         { 0,0,0,0}

                    },
                                        {
                         { 0,0,0,0},
                         { 1,2,1,0},
                         { 0,0,1,0},
                         { 0,0,0,0}

                    },
                                        {
                         { 0,1,1,0},
                         { 0,2,0,0},
                         { 0,1,0,0},
                         { 0,0,0,0}

                    },
                    {
                         { 1,0,0,0},
                         { 1,2,1,0},
                         { 0,0,0,0},
                         { 0,0,0,0}

                    },




               };
               public TetrisBlockJ() : base(layout, TetrisBlockT.kicks)
               {

               }


          }

     }
