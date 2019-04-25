using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

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
          private Color color;

          protected byte[,,] TetriminoLayout { get; private set; }
          protected Tuple<int, int>[][] TetriminoKicks { get; private set; }
          public TetrisBlock(byte[,,] layout, Tuple<int, int>[][] kicks, Color color)
          {
               this.TetriminoLayout = layout;
               this.TetriminoKicks = kicks;
               this.color = color;
               OriginTetrimino = new Tetrimino(color, 0, 0, this);
               Tetriminos = new Tetrimino[] {
                    OriginTetrimino,
                    new Tetrimino(color,0, 0, this),
                    new Tetrimino(color,0, 0, this),
                    new Tetrimino(color,0, 0, this)

               };



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





          public static Size Size { get; private set; }


          private static float DPI;
          public static async Task CreateResourcesAsync(CanvasControl sender)
          {
               var g = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/Tetrimino.png"));
               Graphics[Colors.White] = g;
               DPI = sender.Dpi;
               Size = new Size((float)g.Size.Width, (float)g.Size.Height);
          }
          public static readonly Dictionary<Color, CanvasBitmap> Graphics = new Dictionary<Color, CanvasBitmap>();
          public Color Color { get; private set; }
          private CanvasBitmap graphic;
          public Tetrimino(Color color, float x, float y, Locatable relativeTo = null)
          {
               X = x;
               Y = y;
               this.Color = color;
               this.Width = (float)Size.Width;
               this.Height = (float)Size.Height;
               RelativeTo = relativeTo;

               graphic = getGraphic(color);


          }
          private static CanvasBitmap getGraphic(Color color)
          {
               if (Graphics.ContainsKey(color))
                    return Graphics[color];

               var g2 = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), (int)Graphics[Colors.White].Size.Width, (int)Graphics[Colors.White].Size.Height, DPI);
               g2.CopyPixelsFromBitmap(Graphics[Colors.White]);
               var arr = g2.GetPixelBytes();
               for (int i = 0; i < arr.Length; i += 4)
               {
                    arr[i] = (byte)(((float)arr[i] / 255) * ((float)color.B / 255) * 255);
                    arr[i + 1] = (byte)(((float)arr[i + 1] / 255) * ((float)color.G / 255) * 255);
                    arr[i + 2] = (byte)(((float)arr[i + 2] / 255) * ((float)color.R / 255) * 255);
               }
               g2.SetPixelBytes(arr);
               Graphics[color] = g2;
               return g2;
          }




          public override void draw(CanvasDrawEventArgs args)
          {

               args.DrawingSession.DrawImage(graphic, x(), y());



          }
          public void draw(CanvasDrawEventArgs args, float x, float y)
          {

               args.DrawingSession.DrawImage(graphic, x + X, y + Y);
          }


     }

     class TetrisGrid : Drawable
     {
          public static int HorizontalBlocks = 10;
          public static int VerticalBlocks = 20;
          public bool[,] Filled { get; private set; } = new bool[HorizontalBlocks, VerticalBlocks];
          public Color[,] FilledColor { get; private set; } = new Color[HorizontalBlocks, VerticalBlocks];


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
                              args.DrawingSession.DrawImage(Tetrimino.Graphics[FilledColor[j, i]], j * Tetrimino.Size.Width + x(), i * Tetrimino.Size.Height + y());

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
                    FilledColor[x, y] = z.Color;
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
          public enum RotationAction
          {
               None, Clockwise, CounterClockwise
          }
          private const uint VICTORY_LEVEL = 5;
          private TetrisBlock block;
          private TetrisGrid grid;
          private TetrisBag bag;
          private TetrisBlockHolder holder;
          private ThreadPoolTimer timer;
          private TetrisScoreKeeper scoreKeeper;
          public MoveAction CurrentMoveAction { get; set; }
          public RotationAction CurrentRotationAction { get; set; }

          public Action CurrentAction { get; set; }

          public bool FastDrop { get; set; } = false;

          private uint[] levelTickMapping = new uint[]
          {
               60,48,37,28,21,16,11,8,6,4,3,2,1,1
          };
          //Counters
          public uint GravityTicks { get; set; } = 60;
          public uint LockDelayTicks { get; set; } = 30;

          public uint LockDelayMoves { get; set; } = 15;
          public uint FastDropGravityTicks { get; set; } = 2;
          public uint MoveTicks { get; set; } = 10;
          
          
          private uint ticks = 0;
          private uint moveTicks = 0;
          private uint lockDelay = 0;
          private uint lockDelayMoves = 0;
          private bool lockable = false;
          private bool swapped = false;
          private bool requestSwap = false;
          private bool active = false;
          private System.Action winCallback,loseCallback;

          public TetrisBlockHandler(TetrisGrid grid, TetrisBlockHolder holder, TetrisBag bag, TetrisScoreKeeper keeper,System.Action winCallback, System.Action loseCallback)
          {
               this.grid = grid;
               this.holder = holder;
               this.bag = bag;
               this.scoreKeeper = keeper;
               this.winCallback = winCallback;
               this.loseCallback = loseCallback;

               winCallback();


          }

          public override void draw(CanvasDrawEventArgs args)
          {
               if (block != null)
                    block.draw(args);
               grid.draw(args);
               holder.draw(args);
               bag.draw(args);
               scoreKeeper.draw(args);
               

          }

          public void start()
          {
               if (!active)
               {
                    timer = ThreadPoolTimer.CreatePeriodicTimer(handleTick, TimeSpan.FromMilliseconds(16));
                    spawnBlock();
               }

          }
          public void signalWin()
          {
               timer.Cancel();
               loseCallback();
          }
          public void signalLoss()
          {
               timer.Cancel();
               winCallback();
          }
          public void reset()
          {
               timer.Cancel();
               for (int i=0;i<grid.Filled.GetLength(0); i++)
               {
                    for (int j = 0; j < grid.Filled.GetLength(1); j++)
                    {
                         grid.Filled[i, j] = false;
                         grid.FilledColor[i, j] = Colors.White;
                    }
                     

               }
               scoreKeeper.Level = 1;
               scoreKeeper.Score = 0;
               scoreKeeper.Combo = 0;
               active = false;
               start();
          }
          public void handleTick(ThreadPoolTimer timer)
          {
               ticks += 1;
               if (scoreKeeper.Level >= VICTORY_LEVEL)
               {
                    signalWin();
               }

               if (CurrentMoveAction != MoveAction.None && moveTicks % MoveTicks == 0)
               {
                    if (CurrentMoveAction == MoveAction.Left)
                         moveLeft();
                    else if (CurrentMoveAction == MoveAction.Right)
                         moveRight();
               }
               if (CurrentRotationAction != RotationAction.None)
               {
                    handleRotation();
                    CurrentRotationAction = RotationAction.None;
               }
               if (CurrentMoveAction == MoveAction.Left || CurrentMoveAction == MoveAction.Right)
                    moveTicks += 1;
               handleHold();
               if (ticks % (FastDrop ? (Math.Min(FastDropGravityTicks, GravityTicks)) : GravityTicks) == 0)
                    handleGravityTick();
               if (lockable)
               tryLockingBlock();
               updateGravityTicks();
               
               







          }
          private void handleRotation()
          {
               if (CurrentRotationAction == RotationAction.Clockwise)
               {
                    if (lockDelayMoves < LockDelayMoves)
                    {
                         Tuple<int, int> kick = block.canRotate(grid, true);
                         if (kick != null)
                         {
                              block.rotateClockwise(kick);

                              if (lockDelay > 0)
                              {
                                   lockDelay = 0;
                                   lockDelayMoves += 1;
                              }
                         }
                    }
               }
               else if (CurrentRotationAction == RotationAction.CounterClockwise)
               {
                    if (lockDelayMoves < LockDelayMoves)
                    {
                         Tuple<int, int> kick = block.canRotate(grid, false);
                         if (kick != null)
                         {
                              block.rotateCounterClockwise(kick);
                              if (lockDelay > 0)
                              {
                                   lockDelay = 0;
                                   lockDelayMoves += 1;
                              }
                         }
                    }
               }
          }
          public void updateGravityTicks()
          {
               if (scoreKeeper.Level < levelTickMapping.Length)
               {
                    GravityTicks = levelTickMapping[scoreKeeper.Level];
               }

          }
          public void handleHold()
          {
               if (requestSwap)
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
                         lockDelay = 0;
                         lockDelayMoves = 0;

                    }
                    requestSwap = false;
               }
          }

          private void handleGravityTick()
          {
               if (block != null)
               {

                    if (grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
                    {
                         lockable = true;
                         lockDelay += 1;


                    }
                    else
                    {
                         block.Y += (float)Tetrimino.Size.Height;
                    }



               }
          }

          private bool tryLockingBlock()
          {
               if (lockDelay >= LockDelayTicks || lockDelayMoves >= LockDelayMoves && lockable)
               {
                    while (!grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
                    {
                         block.Y += (float)Tetrimino.Size.Height;
                    }

                         lockBlock();

                    spawnBlock();
                    return true;
               }
               else if (lockable)
               {
                    lockDelay += 1;

               }
               return false;
          }
          private void spawnBlock(TetrisBlock block = null)
          {
               
               for (int i = TetrisGrid.HorizontalBlocks / 4 - 2; i < TetrisGrid.HorizontalBlocks / 4 + 2; i++)
               {
                    if (grid.Filled[i,0] || grid.Filled[i, 1])
                    {
                         signalLoss();
                         return;
                    }
               }
               


                    lockable = false;
               TetrisBlock b;
               if (block == null)
                    b = bag.next();
               else
                    b = block;

               b.X = grid.x() + (((TetrisGrid.HorizontalBlocks / 2) - 2) * Tetrimino.Size.Width);
               b.Y = grid.y();
               this.block = b;
          }
          private void lockBlock()
          {
               swapped = false;
               lockable = false;
               grid.setBlock(block);
               scoreKeeper.checkGrid(grid);
               lockDelay = 0;
               lockDelayMoves = 0;
               updateGravityTicks();
          }

          //Actions
          public void startMovingLeft()
          {
               CurrentMoveAction = MoveAction.Left;


          }
          public void moveLeft()
          {
               if (CurrentMoveAction == MoveAction.Left && block != null)
               {

                    if (!grid.isCollision(block, (float)-Tetrimino.Size.Width, 0))
                    {
                         block.X += -(float)Tetrimino.Size.Width;
                         if (lockDelay > 0)
                         {
                              lockDelay = 0;
                              lockDelayMoves += 1;
                         }
                    }



               }

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
                    requestSwap = true;

               }
          }
          public void startMovingRight()
          {
               CurrentMoveAction = MoveAction.Right;


          }
          public void moveRight()
          {
               if (CurrentMoveAction == MoveAction.Right && block != null)
               {
                    if (!grid.isCollision(block, (float)Tetrimino.Size.Width, 0))
                    {
                         block.X += (float)Tetrimino.Size.Width;
                         if (lockDelay > 0)
                         {
                              lockDelay = 0;
                              lockDelayMoves += 1;
                         }
                    }


               }
          }
          public void stopMovingRight()
          {
               if (CurrentMoveAction == MoveAction.Right)
               {
                    CurrentMoveAction = MoveAction.None;
                    moveTicks = 0;

               }

          }
          public void hardDrop()
          {

               while (!grid.isCollision(block, 0, (float)Tetrimino.Size.Height))
               {
                    block.Y += (float)Tetrimino.Size.Height;

               }
               lockBlock();
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
               CurrentRotationAction = RotationAction.Clockwise;


          }
          public void rotateCounterClockwise()
          {
               CurrentRotationAction = RotationAction.CounterClockwise;
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

     class TetrisScoreKeeper : Drawable
     {
          private const uint MAX_SCORE = 999999999;
          public uint Score { get; set; } = 0;
          public uint Level { get; set; } = 1;
          public uint Combo { get; set; } = 0;
          private bool previousCheckTetris = false;
          private uint linesCleared = 0;
          private uint totalLinesCleared = 0;


          public TetrisScoreKeeper(float x, float y, Locatable relativeTo)
          {
               this.X = x;
               this.Y = y;
               this.RelativeTo = relativeTo;
          }
          public void checkGrid(TetrisGrid grid)
          {
               uint lines = 0;
               bool lineClear;
               for (int i = grid.Filled.GetLength(1) - 1; i >= 0; i--)
               {
                    lineClear = true;

                    for (int j = 0; j < grid.Filled.GetLength(0); j++)
                    {

                         if (!grid.Filled[j, i])
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
                                   grid.FilledColor[l, k] = grid.FilledColor[l, k - 1];
                              }
                         }
                         for (int l = 0; l < grid.Filled.GetLength(0); l++)
                         {
                              grid.Filled[l, 0] = false;
                         }
                         i += 1;
                    }

               }

               switch (lines)
               {
                    case 4:
                         if (previousCheckTetris)
                         {
                              increaseScore(Score += 1200 * Level);
                              Combo += 1;
                              previousCheckTetris = true;
                         }
                         else
                         {
                              increaseScore(Score += 800 * Level);
                              Combo += 1;
                              previousCheckTetris = true;
                         }

                         break;
                    case 3:
                         increaseScore(Score += 500 * Level);
                         Combo += 1;
                         break;
                    case 2:
                         increaseScore(300 * Level);
                         Combo += 1;
                         break;
                    case 1:
                         increaseScore(100 * Level);
                         Combo += 1;
                         break;



                    default:
                         Combo = 0;
                         break;
               }
               linesCleared += lines;
               totalLinesCleared += lines;

               Score += Combo * 50 * Level;
               tryLevelUp();

          }
          private void tryLevelUp()
          {
               if (linesCleared >= Level * 5)
               {
                    Level += 1;
                    //linesCleared = 0;
               }
          }
          private void increaseScore(uint amount)
          {
               Score = Math.Min(Score + amount, MAX_SCORE);
          }

          public override void draw(CanvasDrawEventArgs args)
          {
               args.DrawingSession.DrawText("Score", x(), y(), Colors.Black);
               args.DrawingSession.DrawText(Score.ToString(), x(), y() + 20, Colors.Black);
               args.DrawingSession.DrawText("Combo", x(), y() + 40, Colors.Black);
               args.DrawingSession.DrawText(Combo.ToString(), x(), y() + 60, Colors.Black);
               args.DrawingSession.DrawText("Level", x(), y() + 80, Colors.Black);
               args.DrawingSession.DrawText(Level.ToString(), x(), y() + 100, Colors.Black);
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
          public TetrisBlockO() : base(layout, kicks, Colors.Yellow)
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

          public TetrisBlockT() : base(layout, kicks, Colors.Violet)
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
          public TetrisBlockI() : base(layout, kicks, Colors.Cyan)
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
          public TetrisBlockZ() : base(layout, TetrisBlockT.kicks, Colors.Red)
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
          public TetrisBlockS() : base(layout, TetrisBlockT.kicks, Colors.Green)
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
          public TetrisBlockL() : base(layout, TetrisBlockT.kicks, Colors.Orange)
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
          public TetrisBlockJ() : base(layout, TetrisBlockT.kicks, Colors.Blue)
          {

          }


     }

}
