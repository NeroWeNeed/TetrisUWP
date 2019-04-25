using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TetrisUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

          private List<Drawable> drawables = new List<Drawable>();
          private DispatcherTimer timer = new DispatcherTimer();
          private TetrisBlockHandler handler;
          private WebView webView = new WebView();
        public MainPage()
        {
            this.InitializeComponent();
               
        }

          private void GameCanvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
          {
               args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
               
          }
          async Task CreateResourcesAsync(CanvasControl sender)
          {
               await Tetrimino.CreateResourcesAsync(sender);
               await CreateObjects(sender);
               await ticks(sender);
               
          }
          async Task CreateObjects(CanvasControl sender)
          {
               TetrisGrid grid = new TetrisGrid(200, 100);
               TetrisBlockHolder holder = new TetrisBlockHolder(-(((float) Tetrimino.Size.Width * 5)), 0, grid);
               TetrisBag bag = new TetrisBag(new Random(), grid.Width, 0, grid);
               TetrisScoreKeeper keeper = new TetrisScoreKeeper(0,holder.Height,holder);
               handler = new TetrisBlockHandler(grid,holder,bag,keeper,handleWin,handleLoss);
               drawables.Add(handler);
               handler.start();
              
          }
          async Task ticks(CanvasControl sender)
          {
               timer.Interval = new TimeSpan(0, 0, 1);
               

          }
          private void GameCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
          {
               drawables.ForEach(x =>
               {
                    x.draw(args);
               });
               
               GameCanvas.Invalidate();
          }

          private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
          {
               
               switch (e.Key)
               {
                    case Windows.System.VirtualKey.Left:
                         handler.startMovingLeft();
                         break;
                    case Windows.System.VirtualKey.Right:
                         handler.startMovingRight();
                         break;
                    case Windows.System.VirtualKey.Up:
                         handler.hardDrop();
                         break;
                    case Windows.System.VirtualKey.Down:
                         handler.startFastDrop();
                         break;
                    case Windows.System.VirtualKey.C:
                         handler.holdBlock();
                         break;
                    case Windows.System.VirtualKey.X:
                         handler.rotateClockwise();
                         break;
                    case Windows.System.VirtualKey.Z:
                         handler.rotateCounterClockwise();
                         break;
               }

          }
          private void handleWin()
          {
        }
          private void handleLoss()
          {
            Frame.Navigate(typeof(LosePage));
        }

          private async void GameCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
          {
               
               switch (e.Key)
               {
                    case Windows.System.VirtualKey.Left:
                         handler.stopMovingLeft();
                         
                         break;
                    case Windows.System.VirtualKey.Down:
                         handler.stopFastDrop();

                         break;
                    case Windows.System.VirtualKey.Right:
                         handler.stopMovingRight();
                         break;
               }
          }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartMenu));
        }
    }
}
