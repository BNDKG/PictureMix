using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Drawing.Imaging;

using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;

using Image = System.Drawing.Image; //Remove ambiguousness between AForge.Image and System.Drawing.Image
using Point = System.Drawing.Point; //Remove ambiguousness between AForge.Point and System.Drawing.Point



namespace PictureSuperMix
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /*
        double[,] board_adj = new double[,] {
                { 100,99,98,97,96,95,94,93,92,91 },
                { 81,82,83,84,85,86,87,88,89,90 },
                { 80,79,78,77,76,75,74,73,72,71 },
                { 61,62,63,64,65,66,67,68,69,70 },
                { 60,59,58,57,56,55,54,53,52,51 },
                { 41,42,43,44,45,46,47,48,49,50 },
                { 40,39,38,37,36,35,34,33,32,31 },
                { 21,22,23,24,25,26,27,28,29,30 },
                { 20,19,18,17,16,15,14,13,12,11 },
                { 1,2,3,4,5,6,7,8,9,10 }
            };
            */

        int boardheight = 1000;
        int boardwidth = 20;

        double[,] board_adj = new double[10,1000];
        Bitmap bmpLeft;
        Bitmap bmpRight;

        Bitmap bmpLeftChange;
        Bitmap bmpRightChange;

        string OriPath;

        string SourcePath;
        string SavePathLeft;
        string SavePathRight;
        string SavePathLeft2;
        string SavePathRight2;

        private void button1_Click(object sender, EventArgs e)
        {


            Bitmap bitmapleft2 = bmpLeft.Clone(new Rectangle(0, 0, bmpLeft.Width, bmpLeft.Height), bmpLeft.PixelFormat);
            Bitmap bitmapRight2 = bmpRight.Clone(new Rectangle(0, 0, bmpRight.Width, bmpRight.Height), bmpRight.PixelFormat);
            
            
            BitmapData imageDataLeft = bitmapleft2.LockBits(new Rectangle(0, 0, bitmapleft2.Width, bitmapleft2.Height),
            ImageLockMode.ReadOnly, bitmapleft2.PixelFormat);
            BitmapData imageDataRight = bitmapRight2.LockBits(new Rectangle(0, 0, bitmapRight2.Width, bitmapRight2.Height),
            ImageLockMode.ReadOnly, bitmapRight2.PixelFormat);

            //###########################left###########################//
            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageDataLeft);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                    double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                    int yall = board_adj.GetLength(0);
                    int xall = board_adj.GetLength(1);

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {

                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value


                            int cur_ratex = (int)(x / xlevel);
                            int cur_ratey = (int)(y / ylevel);

                            p[RGB.R] = (byte)(r * board_adj[cur_ratey, cur_ratex] / 1000);
                            p[RGB.G] = (byte)(g * board_adj[cur_ratey, cur_ratex] / 1000);
                            p[RGB.B] = (byte)(b * board_adj[cur_ratey, cur_ratex] / 1000);
                        }
                    }

                }
                finally
                {
                    bitmapleft2.UnlockBits(imageDataLeft); //Unlock
                }

            }


            //###########################right###########################//
            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageDataRight);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                    double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                    int yall = board_adj.GetLength(0);
                    int xall = board_adj.GetLength(1);

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value


                            int cur_ratex = (int)(x / xlevel);
                            int cur_ratey = (int)(y / ylevel);

                            p[RGB.R] = (byte)(r * board_adj[cur_ratey, xall-cur_ratex-1] / 1000);
                            p[RGB.G] = (byte)(g * board_adj[cur_ratey, xall-cur_ratex-1] / 1000);
                            p[RGB.B] = (byte)(b * board_adj[cur_ratey, xall-cur_ratex-1] / 1000);

                        }
                    }

                }
                finally
                {
                    bitmapRight2.UnlockBits(imageDataRight); //Unlock
                }

            }

            bitmapleft2.Save(SavePathLeft2, System.Drawing.Imaging.ImageFormat.Bmp);
            bitmapRight2.Save(SavePathRight2, System.Drawing.Imaging.ImageFormat.Bmp);



            pictureBox1.Image = bitmapleft2;
            pictureBox2.Image = bitmapRight2;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = OriPath;

            System.Diagnostics.Process.Start("explorer.exe", path);

        }

        private void button3_Click(object sender, EventArgs e)
        {


            Bitmap bmpBase = new Bitmap(Image.FromFile(SourcePath));

            bmpBase = ResizeBitmap(bmpBase, 1600, 500);


            double zmc = Convert.ToDouble(textBox1.Text);
            int xx = (int)(bmpBase.Width * zmc);
            int xxx = (int)(bmpBase.Width * (1- zmc));
            int yy = bmpBase.Height;

            Rectangle rect = new Rectangle(0, 0, xx, yy);
            bmpLeft = bmpBase.Clone(rect, bmpBase.PixelFormat);

            bmpLeft.Save(SavePathLeft, System.Drawing.Imaging.ImageFormat.Bmp);


            Rectangle rect2 = new Rectangle(xxx, 0, xx, yy);
            bmpRight = bmpBase.Clone(rect2, bmpBase.PixelFormat);

            bmpRight.Save(SavePathRight, System.Drawing.Imaging.ImageFormat.Bmp);


            Pic_display();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OriPath = System.IO.Directory.GetCurrentDirectory();

            SourcePath = OriPath + "\\SourcePic.png";
            SavePathLeft = OriPath + "\\Left.bmp";
            SavePathRight = OriPath + "\\Right.bmp";
            SavePathLeft2 = OriPath + "\\Left2.bmp";
            SavePathRight2 = OriPath + "\\Right2.bmp";

            int x = board_adj.GetLength(1);
            int y = board_adj.GetLength(0);

            for (int i = 0; i < y; i++)
            {
                for (int ii = 0; ii < x; ii++)
                    board_adj[i, ii] =1000-((double)ii/ (double)x)*1000;
            }            

            int zzz = 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double zmc2 = Convert.ToDouble(textBox2.Text);


            int y = board_adj.GetLength(0);
            int x = board_adj.GetLength(1);

            for(int i = 0; i< y; i++)
            {
                for (int ii = 0; ii < x; ii++)
                    if (ii <= (x * (1-zmc2)))
                        board_adj[i, ii] = 1000;
                    else
                    {
                        board_adj[i, ii] =1000*((x-ii)/(x * zmc2));
                    }
                    
            }

            //board_adj[0, 1] = 1;




        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void Pic_display()
        {

            pictureBox1.Image = bmpLeft;
            pictureBox2.Image = bmpRight;



        }
        public Bitmap ResizeBitmap(Bitmap initialBitmap, int width, int height)
        {
            try
            {
                Bitmap templateImage = new System.Drawing.Bitmap(width, height);
                Graphics templateG = Graphics.FromImage(templateImage);
                templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                templateG.Clear(Color.White);
                templateG.DrawImage(initialBitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, initialBitmap.Width, initialBitmap.Height), GraphicsUnit.Pixel);
                return templateImage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            pictureBox1.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            pictureBox1.BringToFront();
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox2.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            pictureBox2.BringToFront();
            pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }
    }
}
