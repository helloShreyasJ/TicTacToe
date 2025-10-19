using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace TicTacToe
{
    public partial class MainPage : ContentPage
    {
        private int _player = 1;
        private int[,] positions;
        private bool _gridCreated = false;
        private int _winner = 0;
        private int _gridSize;

        public MainPage()
        {
            InitializeComponent();
        }

        /**
         * Creates 3x3 Row and Column Definition for GameGrid
         * Creates 9 Border Objects for each Cell
         * Creates an Adds a Tap Gesture Recogniser onto the Border
         */
        public void CreateAGrid()
        {
            if (_gridCreated) {
                List<RowDefinition> rowsToRemove = new();
                List<ColumnDefinition> columnsToRemove = new();
                foreach (var item in GameGrid.RowDefinitions)
                {
                    if (item.GetType() == typeof(RowDefinition))
                    {
                        rowsToRemove.Add((RowDefinition)item);
                    }
                }

                foreach (var item in GameGrid.ColumnDefinitions)
                {
                    if (item.GetType() == typeof(ColumnDefinition))
                    {
                        columnsToRemove.Add((ColumnDefinition)item);
                    }
                }

                foreach (var item in rowsToRemove)
                {
                    GameGrid.RowDefinitions.Remove(item);
                }

                foreach (var item in columnsToRemove)
                {
                    GameGrid.ColumnDefinitions.Remove(item);
                }
            }
            for (int i = 0; i < _gridSize; i++)
            {
                GameGrid.AddRowDefinition(new RowDefinition());
                GameGrid.AddColumnDefinition(new ColumnDefinition());
            }

            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {

                    Border styledBorder = new Border
                    {
                        BackgroundColor = Colors.White,
                        Stroke = Colors.Black,
                        StrokeThickness = 5
                    };
                    TapGestureRecognizer tap = new TapGestureRecognizer();
                    tap.Tapped += OnBorderTapped;
                    styledBorder.GestureRecognizers.Add(tap);
                    GameGrid.Add(styledBorder, i, j);
                }
            }

            _gridCreated = true;
        }
        
        /**
         *  When GridCreated is clicked, a new 3x3 grid is created
         * The button gets disabled and label gets updated
         */
        private void GridCreated(object sender, EventArgs e)
        {
            int oldGridSize = _gridSize;
            _gridSize = Convert.ToInt32(EnterBoardSize.Text);
            if(_gridSize <= 0)
            {
                TurnTeller.Text = "Enter valid grid size!";
                return;
            }

            if (!_gridCreated)
            {
                positions = new int[_gridSize, _gridSize];
                CreateAGrid();
                Button b = GridCreatorButton;
                b.BackgroundColor = Colors.White;
                b.TextColor = Colors.Black;
                b.Text = "Tap to Play";
                b.IsEnabled = false;
                TurnTeller.Text = $"Player {_player}'s Turn";
            }
            else
            {
                Button b = GridCreatorButton;
                if (oldGridSize != _gridSize)
                {
                    positions = new int[_gridSize, _gridSize];
                    CreateAGrid();
                }
                RestartGame();
                _gridCreated = true;
                b.BackgroundColor = Colors.White;
                b.TextColor = Colors.Black;
                b.Text = "Tap to Play";
                b.IsEnabled = false;
            }
        }

        private void RestartGame()
        {
            _winner = 0;
            _gridCreated = false;
            _player = 1;
            TurnTeller.Text = "";
            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    positions[i, j] = 0;
                }
            }

            //foreach (var item in GameGrid.Children)
            //{
            //    if (item.GetType() == typeof(Border))
            //    {
            //        Border border = (Border)item;
            //        border.BackgroundColor = Colors.Red;
            //        border.Stroke = Colors.Black;
            //        border.StrokeThickness = 5;
            //    }
            //}

            List<View> childrenToRemove = new();
            foreach (var item in GameGrid.Children)
            {
                if (item.GetType() == typeof(Path))
                {
                    childrenToRemove.Add((Path)item);
                } else if (item.GetType() == typeof(Ellipse))
                {
                    childrenToRemove.Add((Ellipse)item);
                }
            }

            foreach (var item in childrenToRemove)
            {
                GameGrid.Remove(item);
            }
        }

        /**
         * On border tapped, make a move
         */
        private void OnBorderTapped(object? sender, TappedEventArgs e)
        {
            if(sender == null) return;
            DoMove((Border)sender);
        }
        
        /**
         * Updates the positions array when a move is made
         * Changes player when a move is made
         * Checks for a win when a move is made
         */
        private void DoMove(Border border)
        {
            if (border == null) return;

            int column = Convert.ToInt32(border.GetValue(Grid.ColumnProperty).ToString());
            int row = Convert.ToInt32(border.GetValue(Grid.RowProperty).ToString());

            double height = border.Height;

            if (_winner == 1) return;

            if (positions[row, column] == 0)
            {
                positions[row, column] = _player;
        
                // Update the border color for the CURRENT player before switching
                if (_player == 1)
                {
                    Path cross = MakeCrossUsingPath(height, 6, Color.FromRgb(0, 0, 0));
                    GameGrid.Add(cross, column, row);
                }
                else
                {
                    Ellipse ell = new Ellipse
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 12,
                        Fill = Color.FromRgb(255, 255, 255),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        HeightRequest = height - 5,
                        WidthRequest = height - 5
                    };
                    GameGrid.Add(ell, column, row);
                }
        
                // Check _winner with the current player who just made the move
                int result = Check_winner(_player);
                        
                if (result != 0)
                {
                    FinishGame(result);
                }
                else
                {
                    // Only switch player and update turn if game is not finished
                    _player = (_player == 1) ? 2 : 1;
                    TurnTeller.Text = $"Player {_player}'s Turn";
                }
            }
        }

        private static Path MakeCrossUsingPath(double dim, int stroke, Color color)
        {
            Path pth = new Path()
            {
                Stroke = color,
                StrokeThickness = stroke,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            pth.Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                    {
                        new PathFigure
                            {
                                 StartPoint = new Point(0,0),
                                 Segments = new PathSegmentCollection
                            {
                                new LineSegment(new Point(dim-10, dim-10))
                             }
                        },
                        new PathFigure
                        {
                             StartPoint = new Point(0 , dim - 10),
                             Segments = new PathSegmentCollection
                        {
                            new LineSegment(new Point(dim-10, 0))
                        }
                    }
                }
            };
            return pth;
        }

        /**
         * Rows, Columns and Diagonals are checked to see if there is a _winner
         */
        private int Check_winner(int player)
        {
            // Check if player 1 won
            if (SearchRowsComplete(positions, _gridSize, 1) || 
                SearchColsComplete(positions, _gridSize, 1) || 
                SearchDiagonalComplete(positions, _gridSize, 1))
                return 1;
            
            // Check if player 2 won
            if (SearchRowsComplete(positions, _gridSize, 2) || 
                SearchColsComplete(positions, _gridSize, 2) || 
                SearchDiagonalComplete(positions, _gridSize, 2))
                return 2;
            
            // Check for draw (no empty cells left)
            if (!HasEmptyCell(positions, _gridSize))
                return 3; // draw
            
            return 0; // game continues
        }

        /**
         * Update label whenever the game ends
         */
        private void FinishGame(int player)
        {
            if (player == 3)
            {
                TurnTeller.Text = "It's a Draw!";
            }
            else
            {
                TurnTeller.Text = $"Player {player} Wins!";
                _winner = 1;
            }

            Button b = GridCreatorButton;
            b.BackgroundColor = Colors.Black;
            b.TextColor = Colors.White;
            b.Text = "Press to Restart";
            b.IsEnabled = true;
        }
        private static bool SearchDiagonalComplete(int[,] ints, int size, int which)
        {
            //Top left to bottom right
            bool foundit = true;
            for (int i = 0; i < size; i++)
            {
                if (ints[i, i] != which)
                {
                    foundit = false;
                    break;
                }
            }
            if (foundit)
                return true;
            //Top right to bottom left
            foundit = true;
            for (int i = size - 1; i >= 0; i--)
            {
                if (ints[i, size - 1 - i] != which)
                {
                    foundit = false;
                    break;
                }
            }
            if (foundit)
                return true;
            return false;
        }
        private static bool SearchColsComplete(int[,] ints, int size, int which)
        {
            //Search for a completed column for the specified player
            for (int i = 0; i < size; i++)
            {
                bool found = true;
                for (int j = 0; j < size; j++)
                {
                    if (ints[j, i] != which)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return true;
                }
            }
            //If a completed column has not been found here, then return false
            return false;
        }
        private static bool SearchRowsComplete(int[,] ints, int size, int which)
        {
            for (int i = 0; i < size; i++)
            {
                bool found = true;
                for (int j = 0; j < size; j++)
                {
                    if (ints[i, j] != which)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return true;
            }
            //If a completed row has not been found here, then return false
            return false;
        }
        private static bool HasEmptyCell(int[,] ints, int size)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (ints[i, j] == 0)
                    {
                        return true; // Found an empty cell
                    }
                }
            }
            return false; // No empty cells found
        }
    }
}
