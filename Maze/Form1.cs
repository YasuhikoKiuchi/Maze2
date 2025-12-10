using System;

namespace Maze
{
    public partial class Form1 : Form
    {
        private const bool SHOW_NUMBER = false;

        private const bool SHOW_COLOR = false;

        // ============================================================ 1

        private static readonly Brush[] CELL_COLORS = { Brushes.Black, Brushes.DeepSkyBlue, Brushes.Salmon, Brushes.LightGreen }; // 壁の色
        private static readonly Pen[] CELL_COLORS2 = { Pens.Black, Pens.DeepSkyBlue, Pens.Salmon, Pens.LightGreen }; // 壁の色

        private const int CELL_EMPTY = 0; // セルの値 0:何もない

        private const int CELL_GROUND = 1; // セルの値 1:地面

        private readonly int[,] cells = new int[65, 49]; // セル情報
        private readonly int[,] cells2 = new int[65, 49]; // セル情報

        private const int CELL_STEM = 2; // セルの値 2:幹

        private const int CELL_BRANCH = 3; // セルの値 3:枝

        private readonly Random random = new(); // 乱数発生用

        private static readonly int[,] VXVY = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } }; // 座標増分(右方向、下方向、左方向、上方向)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            Make(); // 迷路を作る
            Refresh(); // 画面を再描画させる
        }

        private void Make() // 迷路を作る
        {
            // セルをクリアする
            ClearCells();

            // 大地を作る
            MakeGround();

            // 幹を作る
            MakeStem();

            // 枝を作る
            MakeBranch();

            // 隙間を埋める
            MakePadding();
        }

        private void ClearCells() // セルをクリアする
        {
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y] = CELL_EMPTY;
                }
            }
        }

        private void MakeGround() // 大地を作る
        {
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                cells[x, 0] = CELL_GROUND; // 上端
                cells[x, cells.GetLength(1) - 1] = CELL_GROUND; // 下端
            }

            for (int y = 0; y < cells.GetLength(1); y++)
            {
                cells[0, y] = CELL_GROUND; // 左端
                cells[cells.GetLength(0) - 1, y] = CELL_GROUND; // 右端
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y] > 0) // 点(壁)を示す値なら壁描画
                    {
                        Brush brs = SHOW_COLOR ? CELL_COLORS[cells[x, y] - 1] : CELL_COLORS[1];
                        e.Graphics.FillRectangle(brs, x * 8 + 8, y * 8 + 8, 8, 8);
                        if (SHOW_NUMBER) e.Graphics.DrawString(cells2[x, y].ToString(), new Font("MS ゴシック", 6), Brushes.Black, x * 8 + 8, y * 8 + 8);
                    }
                }
            }
        }

        // ============================================================ 2



        private void MakeStem() // 幹を作る
        {
            int direction = -1;
            for (int i = 0; i < 100; i++)
            {
                int sx = 0;
                int sy = 0;

                if (MakeStemStartPosition(ref sx, ref sy, ref direction))
                {
                    Grow(sx, sy, direction, GetGrowLength(20, 6), 7, CELL_STEM);
                }
            }
        }

        private bool MakeStemStartPosition(ref int sx, ref int sy, ref int direction) // 幹の開始位置と伸ばす方向を決める
        {
            int ct = 0;
            while (ct++ < 100) // 100回試行する(100回やってもいい開始地点が見つからなければあきらめる)
            {
                direction = random.Next(4);
                switch (direction)
                {
                    case 0: // 左端から右方向
                        sx = 1;
                        sy = MakeRandomEvenNumber(cells.GetLength(1));
                        break;
                    case 1: // 上端から下方向
                        sx = MakeRandomEvenNumber(cells.GetLength(0));
                        sy = 1;
                        break;
                    case 2: // 右端から左方向
                        sx = cells.GetLength(0) - 2;
                        sy = MakeRandomEvenNumber(cells.GetLength(1));
                        break;
                    case 3: // 下端から上方向
                        sx = MakeRandomEvenNumber(cells.GetLength(0));
                        sy = cells.GetLength(1) - 2;
                        break;
                }

                if (CountNeighbor(sx, sy) <= 3)
                {
                    break;
                }
            }

            return ct < 100;
        }

        /// <summary>
        /// ランダムな偶数を発生させる
        /// </summary>
        /// <param name="max">最大値</param>
        /// <returns>ランダムな偶数</returns>
        private int MakeRandomEvenNumber(int max)
        {
            return random.Next(2, (max - 2) / 2) * 2;
        }

        /// <summary>
        /// 幹・枝を伸ばす長さをランダムに決める
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        private int GetGrowLength(int max, int min)
        {
            return ((random.Next(max - min) + min) / 2) * 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="glowLength"></param>
        /// <param name="maxStraightLen"></param>
        /// <param name="p"></param>
        private void Grow(int x, int y, int direction, int glowLength, int maxStraightLen, int p) // 幹や枝を伸ばす
        {
            int vx = VXVY[direction, 0];
            int vy = VXVY[direction, 1];
            int length = 0;
            if (glowLength == -1) glowLength = cells.GetLength(0) * cells.GetLength(1); // -1指定の場合は「十分な長さ」にする
            for (int i = 0; i < glowLength; i++) // ランダムな長さ、幹を伸ばす
            {
                if (cells[x, y] == 0)
                {
                    cells[x, y] = p;
                    if (branchCount >= 1)
                    {
                        cells2[x, y] = branchCount;
                    }
                }

                int x1 = x + vx; // 次に壁を置く座標を計算する
                int y1 = y + vy;
                if (CountForward(x1, y1, vx, vy) > 0 || (maxStraightLen != -1 && length > maxStraightLen)) // 他の幹や枝にくっつきそうなら"方向転換"
                {
                    direction = MakeNextDirection(x, y, direction);
                    if (direction == -1) break; // 方向転換できない場合は幹を伸ばすのをあきらめる
                    vx = VXVY[direction, 0];
                    vy = VXVY[direction, 1];
                    x += vx; // 増分を足しこむ
                    y += vy;
                    length = 0;
                }
                else
                {
                    x = x1; // 現在座標を次の座標に置き替える
                    y = y1;
                }
                length++;
            }
        }

        private int CountNeighbor(int x, int y) // 近傍の点(壁)の数を数える
        {
            int ct = 0;

            for (int xx = -1; xx <= 1; xx++) // 増分-1～1
            {
                for (int yy = -1; yy <= 1; yy++) // 増分 -1～1
                {
                    if (xx == 0 && yy == 0) continue; // 対象点の位置についてはカウント対象としない
                    int x1 = x + xx; // 近傍の座標を計算する
                    int y1 = y + yy;
                    if (x1 < 0 || y1 < 0 || x1 >= cells.GetLength(0) || y1 >= cells.GetLength(1)) ct++; // フィールド外の座標であれば壁とみなす 
                    else if (cells[x1, y1] > 0) ct++; // フィールド内の座標であればフィールドの対応する要素の値が0より大きければ壁とみなす
                }
            }

            return ct;
        }

        private int MakeNextDirection(int x, int y, int direction) // 次に進む方向を決める
        {
            int delta = random.Next(2) * 2 - 1; // 向きを変える方向を -1 か 1 か乱数で決める
            int nextDirection = GetDirection(direction, delta);
            int ct = CountNeighbor(x + VXVY[nextDirection, 0], y + VXVY[nextDirection, 1]);
            if (ct > 2) // 行こうとする位置の、周辺にある壁が「今いる位置のもの」と「1つ前の位置のもの」の2つを超える場合はNGなのでもう片方の方向を確認する
            {
                delta = -delta; // -1 => 1, 1 => -1
                nextDirection = GetDirection(direction, delta);
                ct = CountNeighbor(x + VXVY[nextDirection, 0], y + VXVY[nextDirection, 1]);
                if (ct > 2) // 行こうとする位置の、周辺にある壁が「今いる位置のもの」と「1つ前の位置のもの」の2つを超える場合はNGなので方向転換できないことを呼び元に伝える
                {
                    nextDirection = -1;
                }
            }

            return nextDirection;
        }

        private int GetDirection(int currentDirection, int delta) // 進む向きを変える
        {
            int nextDirection = currentDirection + delta;
            if (nextDirection < 0) nextDirection = 3;
            else if (nextDirection > 3) nextDirection = 0;
            return nextDirection;
        }

        // ============================================================ 2週目

        int branchCount = 0;

        private void MakeBranch() // 枝を作る
        {
            for (int i = 0; i < 500; i++)
            {
                int sx = 0;
                int sy = 0;
                if (MakeBranchStartPosition(ref sx, ref sy))
                {
                    int direction = MakeBranchGrowDirection(ref sx, ref sy);
                    if (direction > -1)
                    {
                        branchCount++;
                        Grow(sx, sy, direction, GetGrowLength(50, 20), 7, CELL_BRANCH);
                    }
                }
            }
        }

        private bool MakeBranchStartPosition(ref int sx, ref int sy) // 枝の開始位置を決める
        {
            int ct = 0;

            for (int i = 0; i < 100; i++)
            {
                sx = random.Next(2, ((cells.GetLength(0) - 2) / 2) * 2);
                sy = random.Next(2, ((cells.GetLength(1) - 2) / 2) * 2);
                if ((cells[sx, sy] == 2 || cells[sx, sy] == 3) && CountNeighbor(sx, sy) <= 2)
                {
                    break;
                }
                //if ((cells[sx, sy] == 2) && CountNeighbor(sx, sy) <= 2)
                //{
                //    break;
                //}
            }

            return ct < 100;
        }

        private int MakeBranchGrowDirection(ref int x, ref int y) // 枝を伸ばす方向を決める
        {
            int direction = -1;

            for (int i = 0; i < VXVY.GetLength(0); i++) // 4方向それぞれ調べる
            {
                int vx = VXVY[i, 0];
                int vy = VXVY[i, 1];
                int x1 = x + vx;
                int y1 = y + vy;
                if (cells[x1, y1] == 0 && CountForward(x1, y1, vx, vy) == 0) // 何もない場所かつ進む方向の壁がない
                {
                    direction = i;
                    x = x1;
                    y = y1;
                    break;
                }
            }

            return direction;
        }

        private int CountForward(int x, int y, int vx, int vy) // 前方(進む方向)に壁がいくつあるか調べる)
        {
            int ct = 0;

            if (vy == 0)
            {
                for (vy = -1; vy <= 1; vy++)
                {
                    if (cells[x + vx, y + vy] > 0) ct++;
                }
            }
            else
            {
                for (vx = -1; vx <= 1; vx++)
                {
                    if (cells[x + vx, y + vy] > 0) ct++;
                }
            }

            return ct;
        }

        // --------------------------------------------------

        private void MakePadding() // 隙間を埋める
        {
            for (int sx = 1; sx < cells.GetLength(0) - 2; sx++)
            {
                for (int sy = 1; sy < cells.GetLength(1) - 2; sy++)
                {
                    if (cells[sx, sy] == 0) continue; // 壁のないマスは飛ばす

                    for (int i = 0; i < VXVY.GetLength(0); i++)
                    {
                        if (CountForward(sx, sy, VXVY[i, 0], VXVY[i, 1]) > 0
                            || CountForward(sx + VXVY[i, 0], sy + VXVY[i, 1], VXVY[i, 0], VXVY[i, 1]) > 0) continue; // 前に進めない方向は飛ばす
                        Grow(sx, sy, i, -1, -1, 4);
                    }
                }
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            Make();
            Refresh();
        }
    }
}
