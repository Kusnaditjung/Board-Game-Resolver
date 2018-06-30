using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoardGameResolver
{
    public partial class Main : Form
    {
        private UIState _state;
        private Game _game;        
        private bool _isAutoPlay;
        public Main()
        {
            InitializeComponent();
            SetInputStage();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SetInputStage();
        }

        private void SetInputStage()
        {
            _state = UIState.Input;
            SetBoard(null);
            SetControls();
            _isAutoPlay = false;            
        }

        private void SetSolvedStage()
        {
            _state = UIState.Solved;
            SetControls();
        }

        private bool[,] GetBoard()
        {
            bool[,] result = new bool[5, 5];

            result[0, 0] = c00.Checked;
            result[0, 1] = c01.Checked;
            result[0, 2] = c02.Checked;
            result[0, 3] = c03.Checked;
            result[0, 4] = c04.Checked;           

            result[1, 0] = c10.Checked;
            result[1, 1] = c11.Checked;
            result[1, 2] = c12.Checked;
            result[1, 3] = c13.Checked;

            result[2, 0] = c20.Checked;
            result[2, 1] = c21.Checked;
            result[2, 2] = c22.Checked;

            result[3, 0] = c30.Checked;
            result[3, 1] = c31.Checked;

            result[4, 0] = c40.Checked;

            return result;
        }

        private void SetControls()
        {
            btnClear.Enabled = _state != UIState.Processing;
            pnlBoard.Enabled = _state == UIState.Input;
            btnSolve.Enabled = _state == UIState.Input;
            btnPlay.Visible = _state == UIState.Solved;
            lstResultMoves.Visible = _state == UIState.Solved;
            lblXY.Visible = _state == UIState.Solved;
            lblPieceLeft.Visible = _state == UIState.Solved;
            lstResultPieceLeft.Visible = _state == UIState.Solved;

            if (_state == UIState.Input)
            {
                lstResultPieceLeft.Items.Clear();
                lstResultMoves.Items.Clear();
                btnSolve.Text = "Solve";
                this.Height = 310;
            }
            else if (_state == UIState.Processing)
            {
                btnSolve.Text = "Processing ...";
                this.Height = 310;
            }
            else
            {
                btnSolve.Text = "Solved (Completed)";
                this.Height = 560;
            }
        }       

        private void SetBoard(bool[,] board)
        {
            c00.Checked = board == null? true : board[0, 0];
            c01.Checked = board == null ? true : board[0, 1];
            c02.Checked = board == null ? true : board[0, 2];
            c03.Checked = board == null ? true : board[0, 3];
            c04.Checked = board == null ? true : board[0, 4];

            c10.Checked = board == null ? true : board[1, 0];
            c11.Checked = board == null ? true : board[1, 1];
            c12.Checked = board == null ? true : board[1, 2];
            c13.Checked = board == null ? true : board[1, 3];

            c20.Checked = board == null ? true : board[2, 0];
            c21.Checked = board == null ? true : board[2, 1];
            c22.Checked = board == null ? true : board[2, 2];

            c30.Checked = board == null ? true : board[3, 0];
            c31.Checked = board == null ? false : board[3, 1];

            c40.Checked = board == null ? true : board[4, 0];
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            try
            {
                SetProcessing();
                await Task.Delay(1);
                SolveGames();
                SetSolvedStage();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error in processing", ex.Message);
            }
        }

        private void SetProcessing()
        {
            _state = UIState.Processing;
            SetControls();
        }

        private void SolveGames()
        {
            _game = new Game(GetBoard())
                .Solve();

            int totalSolutions = _game.Solutions.Sum(solutionItem => solutionItem.Value.Count());
            lstResultPieceLeft
                .Items
                .AddRange(_game
                    .Solutions
                    .Select(keyItem => new PieceLeftSolution(keyItem.Key, totalSolutions, keyItem.Value))
                    .OrderBy(solutionEntry => solutionEntry.PieceLeft)
                    .ToArray());
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (lstResultMoves.SelectedIndex < 0)
            {
                lstResultMoves.SelectedIndex = 0;                
            }

            if (_isAutoPlay)
            {
                if (lstResultMoves.SelectedIndex < lstResultMoves.Items.Count - 1)
                {
                    lstResultMoves.SelectedIndex = lstResultMoves.SelectedIndex + 1;
                }
            }           

            bool[,] board = Game.Play(_game.InitialBoard, lstResultMoves.Items.OfType<string>().ToList() , lstResultMoves.SelectedIndex);
            SetBoard(board);
            _isAutoPlay = true;
        }

        private void lstResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstResultMoves.SelectedIndex >= 0)
            {
                _isAutoPlay = false;                
            }
            else
            {
                _isAutoPlay = false;                
            }            
        }

        private void lstResultPieceLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstResultPieceLeft.SelectedIndex > -1)
            {
                var pieceLeftSolution = lstResultPieceLeft.SelectedItem as PieceLeftSolution;

                lstResultMoves.Items.Clear();
                lstResultMoves.Items.Add("Start");
                lstResultMoves.Items.AddRange(pieceLeftSolution.Solutions.First().ToArray());
            }
        }
    }
}
