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
        double[,] board_adj = new double[10,100];

        private void button1_Click(object sender, EventArgs e)
        {
            string str2 = System.IO.Directory.GetCurrentDirectory();


            string zzzz = str2 + "\\Left.bmp";


            Image img2 = Image.FromFile(zzzz);


            Bitmap  bmp= new Bitmap(img2);

            BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, bmp.PixelFormat);




            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value
                            /*
                            if (r > g + b) //If red component is bigger then total of green and blue component
                                totalRed++;  //then its red

                            if (r <= g + b && r < 50 && g < 50 && b < 50) //If all components less 50
                                totalBlack++; //then its black

                            if(x> (width * 0.8))
                            {
                                p[RGB.R] = (byte)(r * (1-(x - 0.8 * width) / (0.2 * width)));
                                p[RGB.G]= (byte)(g * (1 - (x - 0.8 * width) / (0.2 * width)));
                                p[RGB.B]= (byte)(b * (1 - (x - 0.8 * width) / (0.2 * width)));
                            }
                            if (x < (width * 0.2))
                            {
                                p[RGB.R] = (byte)(r * (1 - (0.2 * width-x) / (0.2 * width)));
                                p[RGB.G] = (byte)(g * (1 - (0.2 * width - x) / (0.2 * width)));
                                p[RGB.B] = (byte)(b * (1 - (0.2 * width-x) / (0.2 * width)));
                            }
                            */

                            /*
                            if (x > (width * 0.5))
                            {
                                p[RGB.R] = (byte)(r * (Math.Asin(((width - x) / (0.5 * width))) / (Math.PI / 2)));
                                p[RGB.G] = (byte)(g * (Math.Asin(((width - x) / (0.5 * width))) / (Math.PI / 2)));
                                p[RGB.B] = (byte)(b * (Math.Asin(((width - x) / (0.5 * width))) / (Math.PI / 2)));
                            }
                            */
                            double change_rate = 0.1;
                            while (change_rate < 1)
                            {
                                if (x < (width * change_rate))
                                {
                                    p[RGB.R] = (byte)(r * (1.1-change_rate));
                                    p[RGB.G] = (byte)(g * (1.1- change_rate));
                                    p[RGB.B] = (byte)(b * (1.1- change_rate));
                                    break;
                                }
                                change_rate += 0.1;
                            }


                        }
                    }

                }
                finally
                {
                    bmp.UnlockBits(imageData); //Unlock
                }

            }

            string zzzz2 = str2 + "\\LeftOutput.bmp";
            bmp.Save(zzzz2, System.Drawing.Imaging.ImageFormat.Bmp);

            int zzz = 1;
            zzz++;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str2 = System.IO.Directory.GetCurrentDirectory();


            string zzzz = str2 + "\\Right.bmp";


            Image img2 = Image.FromFile(zzzz);


            Bitmap bmp = new Bitmap(img2);

            BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, bmp.PixelFormat);



            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    double ylevel = ((double)img.Height) /board_adj.GetLength(0);
                    double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value

                            /*
                            double change_rate = 0.1;
                            while (change_rate < 1)
                            {
                                if (x < (width * change_rate))
                                {
                                    p[RGB.R] = (byte)(r * change_rate);
                                    p[RGB.G] = (byte)(g * change_rate);
                                    p[RGB.B] = (byte)(b * change_rate);
                                    break;
                                }
                                change_rate += 0.1;
                            }
                            */
                            int cur_ratex = (int)(x / xlevel);
                            int cur_ratey = (int)(y / ylevel);

                            p[RGB.R] = (byte)(r * board_adj[cur_ratey, cur_ratex] / 100);
                            p[RGB.G] = (byte)(g * board_adj[cur_ratey, cur_ratex] / 100);
                            p[RGB.B] = (byte)(b * board_adj[cur_ratey, cur_ratex] / 100);


                        }
                    }

                }
                finally
                {
                    bmp.UnlockBits(imageData); //Unlock
                }

            }

            string zzz2 = str2 + "\\RightOutput.bmp";

            bmp.Save(zzz2, System.Drawing.Imaging.ImageFormat.Bmp);

            int zzz = 1;
            zzz++;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string str2 = System.IO.Directory.GetCurrentDirectory();


            string zzz = str2 + "\\SourcePic.jpg";


            Image img2 = Image.FromFile(zzz);

            Bitmap bmpBase = new Bitmap(img2);
            /*
            BitmapData imageData = bmpBase.LockBits(new Rectangle(0, 0, bmpBase.Width, bmpBase.Height),
            ImageLockMode.ReadOnly, bmpBase.PixelFormat);

            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            //int r = (int)p[RGB.R]; //Red pixel value
                            //int g = (int)p[RGB.G]; //Green pixel value
                            // int b = (int)p[RGB.B]; //Blue pixel value


                            
                            if (y < (height * 0.2))
                            {
                                p[RGB.R] = (byte)(255);
                                p[RGB.G] = (byte)(0);
                                p[RGB.B] = (byte)(0);

                            }
                            else if(y < (height * 0.4))
                            {
                                p[RGB.R] = (byte)(0);
                                p[RGB.G] = (byte)(255);
                                p[RGB.B] = (byte)(0);
                            }
                            else if (y < (height * 0.6))
                            {
                                p[RGB.R] = (byte)(0);
                                p[RGB.G] = (byte)(0);
                                p[RGB.B] = (byte)(255);
                            }
                            else if (y < (height * 0.8))
                            {
                                p[RGB.R] = (byte)(0);
                                p[RGB.G] = (byte)(0);
                                p[RGB.B] = (byte)(0);
                            }


                        }
                    }

                }
                finally
                {
                    bmpBase.UnlockBits(imageData); //Unlock
                }

            }


            bmpBase.Save("C:\\AA2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

*/



            double zmc = 0.90909091;
            int xx = (int)(bmpBase.Width * zmc);
            int xxx = (int)(bmpBase.Width * (1- zmc));
            int yy = bmpBase.Height;

            Rectangle rect = new Rectangle(0, 0, xx, yy);
            Bitmap bmpNew = bmpBase.Clone(rect, bmpBase.PixelFormat);


            string zzz2 = str2 + "\\Left.bmp";
            bmpNew.Save(zzz2, System.Drawing.Imaging.ImageFormat.Bmp);

            Rectangle rect2 = new Rectangle(xxx, 0, xx, yy);
            Bitmap bmpNew2 = bmpBase.Clone(rect2, bmpBase.PixelFormat);


            zzz2 = str2 + "\\Right.bmp";
            bmpNew2.Save(zzz2, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int x = board_adj.GetLength(1);
            int y = board_adj.GetLength(0);

            for (int i = 0; i < y; i++)
            {
                for (int ii = 0; ii < x; ii++)
                    board_adj[i, ii] =100-(100/x)*ii;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {




            int y = board_adj.GetLength(0);
            int x = board_adj.GetLength(1);

            for(int i = 0; i< y; i++)
            {
                for (int ii = 0; ii < x; ii++)
                    board_adj[i,ii] += 100;
            }

            board_adj[0, 1] = 1;




        }
    }
}
