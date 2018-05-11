using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using System.Drawing.Imaging;
using System.Diagnostics;

using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using AForge.Video.FFMPEG;

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

        string SourcePathBlack; 
        string SourcePathWhite;
        string SourcePathColor;
        string SourcePathPictoVideo;
        string picdic ;

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
            int width = Convert.ToInt16(textBox3.Text);
            int hight = Convert.ToInt16(textBox4.Text);

            Bitmap bmpBase = new Bitmap(Image.FromFile(SourcePath));

            bmpBase = ResizeBitmap(bmpBase, width, hight);


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

            SourcePath = OriPath + "\\SourcePic.jpg";
            SavePathLeft = OriPath + "\\Left.bmp";
            SavePathRight = OriPath + "\\Right.bmp";
            SavePathLeft2 = OriPath + "\\Left2.bmp";
            SavePathRight2 = OriPath + "\\Right2.bmp";

            SourcePathBlack = OriPath + "\\pics\\BlackPic.jpg";
            SourcePathWhite = OriPath + "\\pics\\WhitePic.jpg";
            SourcePathColor = OriPath + "\\pics\\ColorPic.png";
            SourcePathPictoVideo = OriPath + "\\pics\\ColorPictoVideo.png";

            picdic = OriPath + "\\image_input";

            int x = board_adj.GetLength(1);
            int y = board_adj.GetLength(0);
            //初始化显示矩阵
            for (int i = 0; i < y; i++)
            {
                for (int ii = 0; ii < x; ii++)
                    board_adj[i, ii] =1000-((double)ii/ (double)x)*1000;
            }

            


            if (!Directory.Exists(picdic))
            {
                Directory.CreateDirectory(picdic);
            }


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

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox3.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            pictureBox3.BringToFront();
            pictureBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBox4.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox4_DoubleClick(object sender, EventArgs e)
        {
            pictureBox4.BringToFront();
            pictureBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string picdic = OriPath + "\\pics";


            if (!Directory.Exists(picdic))
            {
                Directory.CreateDirectory(picdic);
            }

            Bitmap bmpWhite = new Bitmap(Image.FromFile(SourcePathWhite));
            Bitmap bmpBlack = new Bitmap(Image.FromFile(SourcePathBlack));
            Bitmap bmpColor = new Bitmap(Image.FromFile(SourcePathColor));

            pictureBox3.Image = bmpWhite;
            pictureBox4.Image = bmpBlack;
            pictureBox5.Image = bmpColor;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            pictureBox5.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox5_DoubleClick(object sender, EventArgs e)
        {
            pictureBox5.BringToFront();
            pictureBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            zzzoutput_test();
        }

        private void zzzoutput_test()
        {
            //Bitmap curbitmap = bmpLeft.Clone(new Rectangle(0, 0, bmpLeft.Width, bmpLeft.Height), bmpLeft.PixelFormat);

            int startpoint = 0;
            int changepoint = 1;

            for (int i = 0; i < 500; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1920, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p = (byte*)img.ImageData.ToPointer();

                        double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                        double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                        int yall = board_adj.GetLength(0);
                        int xall = board_adj.GetLength(1);


                        int cur_value = startpoint;
                        int cur_changpoint = changepoint;


                        // for each line
                        for (int y = 0; y < height; y++)
                        {
                            cur_value = startpoint;
                            cur_changpoint = changepoint;
                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize)
                            {
                                /*
                                int r = (int)p[RGB.R]; //Red pixel value
                                int g = (int)p[RGB.G]; //Green pixel value
                                int b = (int)p[RGB.B]; //Blue pixel value


                                int cur_ratex = (int)(x / xlevel);
                                int cur_ratey = (int)(y / ylevel);

                                p[RGB.R] = (byte)(r * board_adj[cur_ratey, cur_ratex] / 1000);
                                p[RGB.G] = (byte)(g * board_adj[cur_ratey, cur_ratex] / 1000);
                                p[RGB.B] = (byte)(b * board_adj[cur_ratey, cur_ratex] / 1000);
                                
                                byte cur_value = 0;
                                if((x-i) < ( width / 2))
                                {
                                    cur_value = (byte)(255 * (2 * (double)(x - i) / (double)width));
                                }
                                else
                                {
                                    cur_value = (byte)(255 * ((double)(2 * width - 2 * (x-i)) / (double)width));
                                }
                                */


                                p[RGB.R] = (byte)cur_value;
                                p[RGB.G] = (byte)cur_value;
                                p[RGB.B] = (byte)cur_value;

                                if (cur_changpoint > 0)
                                {
                                    cur_value = cur_value + 1;
                                    if (cur_value >= 255)
                                    {
                                        cur_value = 255;
                                        cur_changpoint = -1;
                                    }
                                }
                                else
                                {
                                    cur_value = cur_value - 1;
                                    if (cur_value <= 0)
                                    {
                                        cur_value = 0;
                                        cur_changpoint = 1;
                                    }
                                }



                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();


                if (changepoint > 0)
                {
                    startpoint = startpoint + 1;
                    if (startpoint >= 255)
                    {
                        startpoint = 255;
                        changepoint = -1;
                    }
                }
                else
                {
                    startpoint = startpoint - 1;
                    if (startpoint <= 0)
                    {
                        startpoint = 0;
                        changepoint = 1;
                    }
                }
                //startpoint += 1;



            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //和ffmpeg差不多，不能一帧帧超高清显示，附一个ffmpeg代码
            //ffmpeg -i E:\image_input\dd%04d.bmp -r 60 -s 1920x1080 -b:v 20000k  E:\zmctest222.avi
            //将input文件夹中的视频全部生成视频


            int width = 1280;
            int height = 800;


            VideoFileWriter writer = new VideoFileWriter();

            writer.Open("sample-video.mp4", width, height, 25, VideoCodec.MPEG4, 25000000);

            Bitmap image = new Bitmap(width, height);

            /*
            using (OpenFileDialog dlg = new OpenFileDialog())

            {

                dlg.Title = "Open Image";

                dlg.Filter = "bmp files (*.bmp)|*.bmp";



                if (dlg.ShowDialog() == DialogResult.OK)

                {

                    image = new Bitmap(dlg.FileName);

                }

            }
            */

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(picdic);
            int picall=GetFilesCount(dirInfo);

            string curpicpath;

            for (int i = 0; i < (picall-2); i++)

            {
                string sdf = String.Format("{0:D4}", i);


                curpicpath = picdic + "\\dd" + sdf + ".bmp";


                image = new Bitmap(curpicpath);

                writer.WriteVideoFrame(image);

                image.Dispose();

            }



            writer.Close();

            Application.Exit();
            
        }

        public static int GetFilesCount(System.IO.DirectoryInfo dirInfo)
        {
            int totalFile = 0;
            totalFile += dirInfo.GetFiles().Length;
            foreach (System.IO.DirectoryInfo subdir in dirInfo.GetDirectories())
            {
                totalFile += GetFilesCount(subdir);
            }
            return totalFile;
        }

        private void button9_Click(object sender, EventArgs e)
        {

            double startpoint = 0;
            int changepoint = 1;

            for (int i = 0; i < 512; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p = (byte*)img.ImageData.ToPointer();

                        double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                        double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                        int yall = board_adj.GetLength(0);
                        int xall = board_adj.GetLength(1);


                        int cur_value = (int)startpoint;
                        int cur_changpoint = changepoint;


                        // for each line
                        for (int y = 0; y < height; y++)
                        {

                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize)
                            {

                                p[RGB.R] = (byte)cur_value;
                                p[RGB.G] = (byte)cur_value;
                                p[RGB.B] = (byte)cur_value;

                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();

                //变换率
                double changerate2 = 1;
                if (changepoint > 0)
                {
                    startpoint = startpoint + changerate2;
                    if (startpoint >= 255)
                    {
                        startpoint = 255;
                        changepoint = -1;
                    }
                }
                else
                {
                    startpoint = startpoint - changerate2;
                    if (startpoint <= 0)
                    {
                        startpoint = 0;
                        changepoint = 1;
                    }
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //生成带有位置信息的视频用于标定位置
            int[,,] fontnum = new int[,,] {
                {
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 1,0,0,1,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,0,1,0,0 },
                { 0,1,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,1,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,0,1,0,0 },
                { 0,1,0,0,0 },
                { 1,1,1,1,0 }
                },{
                { 1,1,1,0,0 },
                { 0,0,0,1,0 },
                { 0,0,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,0,0,1,0 },
                { 0,0,1,1,0 },
                { 0,1,0,1,0 },
                { 1,1,1,1,1 },
                { 0,0,0,1,0 }
                },{
                { 1,1,1,0,0 },
                { 1,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,1,0 },
                { 1,1,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,0,0 },
                { 1,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 1,1,1,1,1 },
                { 0,0,0,1,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,1,0 },
                { 0,0,0,1,0 },
                { 0,1,1,0,0 }
                }
            };

            int startpointx = 15;
            int startpointy = 35;
            int fontlong = 4;
            int objx = 0;
            int objy = 0;
            int objoffset = 3;

            for (int i = 0; i < 5; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;


                        double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                        double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                        int yall = board_adj.GetLength(0);
                        int xall = board_adj.GetLength(1);
                        byte* p = (byte*)img.ImageData.ToPointer();

                        // for each line

                        int cur_value = 0;

                        for (objx = 0; objx < 20; objx++)
                        {
                            for (objy = 0; objy < 20; objy++)
                            {
                                p = (byte*)img.ImageData.ToPointer();

                                startpointx = objx * 40 + objoffset;
                                startpointy = objy * 40 + objoffset;

                                for (int y = 0; y < height; y++)
                                {

                                    // for each pixel
                                    for (int x = 0; x < width; x++, p += pixelSize)
                                    {
                                        int figoffset = 10;
                                        cur_value = p[RGB.R];
                                        if (x >= startpointx && y >= startpointy)
                                        {
                                            if ((x <= (startpointx + fontlong)) && (y <= (startpointy + fontlong)))
                                            {
                                                int buf = fontnum[(objx/10), y - startpointy, x - startpointx];
                                                if (buf == 1)
                                                {
                                                    cur_value = 255;
                                                }
                                            }
                                        }
                                        if (x >= startpointx+ figoffset && y >= startpointy)
                                        {
                                            if ((x <= (startpointx+ figoffset + fontlong)) && (y <= (startpointy + fontlong)))
                                            {
                                                int buf = fontnum[(objx%10), y - startpointy, x - startpointx- figoffset];
                                                if (buf == 1)
                                                {
                                                    cur_value = 255;
                                                }
                                            }
                                        }
                                        if (x >= startpointx  && y >= startpointy + figoffset)
                                        {
                                            if ((x <= (startpointx  + fontlong)) && (y <= (startpointy + fontlong + figoffset)))
                                            {
                                                int buf = fontnum[(objy/10), y - startpointy - figoffset, x - startpointx];
                                                if (buf == 1)
                                                {
                                                    cur_value = 255;
                                                }
                                            }
                                        }
                                        if (x >= startpointx + figoffset && y >= startpointy + figoffset)
                                        {
                                            if ((x <= (startpointx + figoffset + fontlong)) && (y <= (startpointy + fontlong + figoffset)))
                                            {
                                                int buf = fontnum[(objy%10), y - startpointy - figoffset, x - startpointx - figoffset];
                                                if (buf == 1)
                                                {
                                                    cur_value = 255;
                                                }
                                            }
                                        }

                                        p[RGB.R] = (byte)cur_value;
                                        p[RGB.G] = (byte)cur_value;
                                        p[RGB.B] = (byte)cur_value;

                                    }
                                }

                            }
                        }
                        /*
                        p = (byte*)img.ImageData.ToPointer();
                        for (int y = 0; y < height; y++)
                        {

                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize)
                            {

                                cur_value = p[RGB.R];
                                cur_value = 255 - cur_value;

                                p[RGB.R] = (byte)cur_value;
                                p[RGB.G] = (byte)cur_value;
                                p[RGB.B] = (byte)cur_value;

                            }
                        }
                        */

                        
                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();



            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //生成带有灰度信息的视频用于标定灰度级


            double startpoint = 0;
            int changepoint = 1;

            int startpointx = 15;
            int startpointy = 35;
            int fontlong = 4;
            int figoffset = 6;

            int[,,] fontnum = new int[,,] {
                {
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 1,0,0,1,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,0,1,0,0 },
                { 0,1,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,1,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,0,1,0,0 },
                { 0,1,0,0,0 },
                { 1,1,1,1,0 }
                },{
                { 1,1,1,0,0 },
                { 0,0,0,1,0 },
                { 0,0,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,0,0,1,0 },
                { 0,0,1,1,0 },
                { 0,1,0,1,0 },
                { 1,1,1,1,1 },
                { 0,0,0,1,0 }
                },{
                { 1,1,1,0,0 },
                { 1,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,1,0 },
                { 1,1,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,0,0 },
                { 1,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 1,1,1,1,1 },
                { 0,0,0,1,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,0,0 }
                },{
                { 0,1,1,0,0 },
                { 1,0,0,1,0 },
                { 0,1,1,1,0 },
                { 0,0,0,1,0 },
                { 0,1,1,0,0 }
                }
            };

            for (int i = 0; i < 2500; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p = (byte*)img.ImageData.ToPointer();

                        double ylevel = ((double)img.Height) / board_adj.GetLength(0);
                        double xlevel = ((double)img.Width) / board_adj.GetLength(1);

                        int yall = board_adj.GetLength(0);
                        int xall = board_adj.GetLength(1);


                        int cur_value = (int)startpoint;
                        int cur_changpoint = changepoint;

                        startpointx = 485;
                        startpointy = 325;


                        // for each line
                        for (int y = 0; y < height; y++)
                        {

                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize)
                            {


                                cur_value = (int)startpoint;

                                double Show = (((double)cur_value) / 255)*99;
                                int show2 = (int)Show;

                                if (x >= (startpointx-5) && x <= (startpointx + 25) && y >= startpointy-5 && y <= startpointy+15)
                                {
                                    cur_value = 0;
                                }

                                if (x >= startpointx && y >= startpointy)
                                {
                                    if ((x <= (startpointx + fontlong)) && (y <= (startpointy + fontlong)))
                                    {
                                        int buf = fontnum[(show2 / 10), y - startpointy, x - startpointx];
                                        if (buf == 1)
                                        {
                                            cur_value = 255;
                                        }
                                        else
                                        {
                                            cur_value = 0;
                                        }
                                    }
                                }
                                if (x >= startpointx + figoffset && y >= startpointy)
                                {
                                    if ((x <= (startpointx + figoffset + fontlong)) && (y <= (startpointy + fontlong)))
                                    {
                                        int buf = fontnum[(show2 % 10), y - startpointy, x - startpointx - figoffset];
                                        if (buf == 1)
                                        {
                                            cur_value = 255;
                                        }
                                        else
                                        {
                                            cur_value = 0;
                                        }
                                    }
                                }

                                p[RGB.R] = (byte)cur_value;
                                p[RGB.G] = (byte)cur_value;
                                p[RGB.B] = (byte)cur_value;

                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();


                if (changepoint > 0)
                {
                    startpoint = startpoint + 0.25;
                    if (startpoint >= 255)
                    {
                        startpoint = 255;
                        changepoint = -1;
                    }
                }
                else
                {
                    startpoint = startpoint - 0.25;
                    if (startpoint <= 0)
                    {
                        startpoint = 0;
                        changepoint = 1;
                    }
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //把画面分为左中右

            for (int i = 0; i < 10; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

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
                                if (x > 20 && y > 20 && x<212 &&y<100)
                                {
                                    p[RGB.R] = (byte)(255 / 10 * ((y - 20) / 8+1));
                                    p[RGB.G] = (byte)(255 / 10 * ((y - 20) / 8+1));
                                    p[RGB.B] = (byte)(255 / 10 * ((y - 20) / 8+1));

                                }
                                else
                                {
                                    p[RGB.R] = (byte)0;
                                    p[RGB.G] = (byte)0;
                                    p[RGB.B] = (byte)0;
                                }


                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();


            }
        }

        private void button13_Click(object sender, EventArgs e)
        {

            bool gflag = true;

            for (int i = 0; i < 150; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

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

                                if (gflag)
                                {
                                    p[RGB.R] = (byte)70;
                                    p[RGB.G] = (byte)70;
                                    p[RGB.B] = (byte)70;
                                }
                                else
                                {
                                    if (x > 480 && x < 544)
                                    {
                                        p[RGB.R] = (byte)70;
                                        p[RGB.G] = (byte)70;
                                        p[RGB.B] = (byte)70;
                                    }
                                    else if (x >= 544 && x < 608)
                                    {
                                        p[RGB.R] = (byte)71;
                                        p[RGB.G] = (byte)71;
                                        p[RGB.B] = (byte)71;
                                    }
                                    else if (x >= 638 && x<676)
                                    {
                                        p[RGB.R] = (byte)75;
                                        p[RGB.G] = (byte)75;
                                        p[RGB.B] = (byte)75;
                                    }
                                    else
                                    {
                                        p[RGB.R] = (byte)70;
                                        p[RGB.G] = (byte)70;
                                        p[RGB.B] = (byte)70;
                                    }
                                }

                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();

                if (gflag)
                {
                    gflag = false;
                }
                else
                {
                    gflag = true;
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //用于生成来回两个灰度级跳变的视频以发现是否系统中存在灰度级的变化

            bool gflag = true;

            for (int i = 0; i < 150; i++)
            {
                Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //###########################left###########################//
                unsafe
                {
                    //Count red and black pixels
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

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

                                if (gflag)
                                {
                                    p[RGB.R] = (byte)75;
                                    p[RGB.G] = (byte)75;
                                    p[RGB.B] = (byte)75;
                                }
                                else
                                {
                                    p[RGB.R] = (byte)74;
                                    p[RGB.G] = (byte)74;
                                    p[RGB.B] = (byte)74;
                                }



                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }
                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".gif";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Jpeg);

                curbitmap.Dispose();

                if (gflag)
                {
                    gflag = false;
                }
                else
                {
                    gflag = true;
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            bool gflag = true;

            for (int i = 0; i < 150; i++)
            {
                //Bitmap curbitmap = new System.Drawing.Bitmap(1280, 800);

                //Bitmap curbitmap = new Bitmap(Image.FromFile(SourcePathPictoVideo));
                Bitmap curbitmap = new Bitmap(Image.FromFile(textBox5.Text));

                string sdf = String.Format("{0:D4}", i);


                string zzzzz = picdic + "\\dd" + sdf + ".bmp";


                curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

                curbitmap.Dispose();

                if (gflag)
                {
                    gflag = false;
                }
                else
                {
                    gflag = true;
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        public float backcounter = 0;
        public bool endflag = true;

        public float shadowpercent;

        private void multtest()
        {
            shadowpercent = (float)trackBar1.Value / 10;


            Thread oGetArgThread = new Thread(new ThreadStart(aforgechange));
            oGetArgThread.IsBackground = true;
            oGetArgThread.Start();

        }

        private void aforgereadtest()
        {            

            //读取图片
            Bitmap sourcepic = new Bitmap(Image.FromFile(textBox5.Text));

            // 生成视频生成读取器
            VideoFileReader readerzzz = new VideoFileReader();
            // 打开视频
            readerzzz.Open(textBox6.Text);

            // 生成视频写入器
            VideoFileWriter writerzzz = new VideoFileWriter();
            // 新建一个视频(帧必须是二的倍数)
            writerzzz.Open("testoutput.avi", (sourcepic.Width/2)*2, (sourcepic.Height/2)*2, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);



            // 对视频的所有帧进行操作
            for (int i = 0; i < (readerzzz.FrameCount-1) && endflag==false; i++)
            {
                backcounter = (((float)i)/(readerzzz.FrameCount - 1))*100;

                //载入当前帧动画
                Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                //载入背景
                Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                //模糊化
                Bitmap Videochangebuf = new Bitmap(curbitmapsource, 1024, 768);

                //投影变化
                Bitmap Videochange = new Bitmap(Videochangebuf, curbitmap.Width, curbitmap.Height);

                //背景图片
                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //灯光图片
                BitmapData curimageData2 = Videochange.LockBits(new Rectangle(0, 0, Videochange.Width, Videochange.Height),
                ImageLockMode.ReadOnly, Videochange.PixelFormat);


                unsafe
                {
                    try
                    {
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p = (byte*)img.ImageData.ToPointer();

                        UnmanagedImage img2 = new UnmanagedImage(curimageData2);

                        int height2 = img2.Height;
                        int width2 = img2.Width;
                        int pixelSize2 = (img2.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p2 = (byte*)img2.ImageData.ToPointer();




                        // for each line
                        for (int y = 0; y < height; y++)
                        {

                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize,p2+= pixelSize2)
                            {

                                float dd = ((float)p2[RGB.R]) / 255;
                                float mul2 = (shadowpercent+dd)/(1+ shadowpercent);

                                p[RGB.R] = (byte)(p[RGB.R] * mul2);
                                p[RGB.G] = (byte)(p[RGB.G] * mul2);
                                p[RGB.B] = (byte)(p[RGB.B] * mul2);

                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }

                //写入当前帧
                writerzzz.WriteVideoFrame(curbitmap);

                // 释放当前操作内存
                curbitmap.Dispose();
                curbitmapsource.Dispose();
                Videochange.Dispose();


            }
            readerzzz.Close();

            writerzzz.Close();
            //释放内存
            sourcepic.Dispose();

            endflag = true;

        }
        private void aforgechange()
        {

            //读取图片
            Bitmap sourcepic = new Bitmap(Image.FromFile(textBox5.Text));

            // 生成视频生成读取器
            VideoFileReader readerzzz = new VideoFileReader();
            // 打开视频
            readerzzz.Open(textBox6.Text);

            // 生成视频写入器
            VideoFileWriter writerzzz = new VideoFileWriter();
            // 新建一个视频(帧必须是二的倍数)
            writerzzz.Open("testoutput.avi", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);



            // 对视频的所有帧进行操作
            for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
            {
                backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                //载入当前帧动画
                Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                //载入背景
                Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                //模糊化
                Bitmap Videochangebuf = new Bitmap(curbitmapsource, 192, 80);

                //投影变化
                Bitmap Videochange = new Bitmap(Videochangebuf, curbitmap.Width, curbitmap.Height);

                //背景图片
                BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
                ImageLockMode.ReadOnly, curbitmap.PixelFormat);

                //灯光图片
                BitmapData curimageData2 = Videochange.LockBits(new Rectangle(0, 0, Videochange.Width, Videochange.Height),
                ImageLockMode.ReadOnly, Videochange.PixelFormat);


                unsafe
                {
                    try
                    {
                        //背景图片
                        UnmanagedImage img = new UnmanagedImage(curimageData);

                        int height = img.Height;
                        int width = img.Width;
                        int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p = (byte*)img.ImageData.ToPointer();

                        //灯光图片
                        UnmanagedImage img2 = new UnmanagedImage(curimageData2);

                        int height2 = img2.Height;
                        int width2 = img2.Width;
                        int pixelSize2 = (img2.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                        byte* p2 = (byte*)img2.ImageData.ToPointer();

                        int Lampwidth = width2;
                        int Lampheight = height2;

                        PointSource[] Lamps = new PointSource[Lampwidth * Lampheight];


                        for (int yy = 0; yy < Lampheight; yy++)
                        {
                            for (int xx = 0; xx < Lampwidth; xx++, p2 += pixelSize2)
                            {
                                Lamps[yy * Lampwidth + xx].X = xx * 16 + 1;
                                Lamps[yy * Lampwidth + xx].Y = yy * 16 + 1;
                                Lamps[yy * Lampwidth + xx].LightDegree = (((double)15 * (double)p2[RGB.R]) / (double)255);
                                Lamps[yy * Lampwidth + xx].LightDistance = 25;

                            }
                        }
                        // for each line
                        for (int y = 0; y < height; y++)
                        {

                            // for each pixel
                            for (int x = 0; x < width; x++, p += pixelSize)
                            {

                                double finaldegreereverse = 0;
                                for (int xx = 0; xx < (Lampheight * Lampwidth); xx++)
                                {

                                    double buff1 = (Lamps[xx].X - x);
                                    double buff2 = (Lamps[xx].Y - y);
                                    double buff3 = Lamps[xx].LightDistance;

                                    double buff4 = Math.Sqrt(buff1 * buff1 + buff2 * buff2 + buff3 * buff3);
                                    double buff5 = buff3 / buff4;
                                    //Lamps[xx].LightDegree
                                    double buff6 = buff5 * Math.Exp(-buff4 / 6);



                                    if (Math.Sqrt(buff1 * buff1 + buff2 * buff2) < 100)
                                    {
                                        finaldegreereverse += buff6 * Lamps[xx].LightDegree;
                                    }


                                }


                                //double finaldegree = LightUpChange3(finaldegreereverse);
                                double finaldegree = finaldegreereverse;
                                double rrr = Math.Min(1, (finaldegree));

                                double Env = 0.3;

                                //double finalrrr = (1 - Env*(1 - rrr));
                                double finalrrr = (1 - Env * (1 - rrr));

                                p[RGB.R] = (byte)(p[RGB.R] * finalrrr);
                                p[RGB.G] = (byte)(p[RGB.G] * finalrrr);
                                p[RGB.B] = (byte)(p[RGB.B] * finalrrr);

                            }

                        }



                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                    }

                }

                //写入当前帧
                writerzzz.WriteVideoFrame(curbitmap);

                // 释放当前操作内存
                curbitmap.Dispose();
                curbitmapsource.Dispose();
                Videochange.Dispose();


            }
            readerzzz.Close();

            writerzzz.Close();
            //释放内存
            sourcepic.Dispose();

            endflag = true;

        }
        public double[] Sub(double[]v1, double[]v2)
        {
            return new[] { 
                v1[0]-v2[0],
                v1[1]-v2[1],
                v1[2]-v2[2] };

        }
        public double[] Add(double[] v1, double[] v2)
        {
            return new[] {
                v1[0]+v2[0],
                v1[1]+v2[1],
                v1[2]+v2[2] };

        }
        public double Dot(double[] v1, double[] v2)
        {
            return (
                v1[0]*v2[0]+
                v1[1]*v2[1]+
                v1[2]*v2[2]
                );

        }
        public double LightUpChange(double degree,double Wt,double W0)
        {
            double output = 0;

            output = 1 / (1 + Math.Exp(-(Wt*degree + W0)));


            return output;
        }
        public double LightUpChange3(double degree)
        {
            double output = 0;

            output =(1 - Math.Exp(-( degree )));


            return output;
        }
        public double LightUpChange2(double degree)
        {
            double output = 0;
            
            if(0<=degree && 0.375 >= degree)
            {
                output=degree/3;
            }
            else if(0.375<degree && degree <= 0.625)
            {
                output = degree * 3 -1;
            }
            else if (0.625 < degree && degree <= 1)
            {
                output = (degree - 0.25) / 3 + 0.75;
            }


            output =Math.Min(1, output);

            return degree;
        }

        public double[] Normalize(double[] v)
        {
            double len = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            return new[]
            {
                v[ 0 ] / len,
                v[ 1 ] / len,
                v[ 2 ] / len
            };
        }


        private void button8_Click(object sender, EventArgs e)
        {
            if (endflag)
            {

                endflag = false;

                timer1.Start();
                multtest();
                button8.Text = "停止";
            }
            else
            {
                endflag = true;
                button8.Text = "视频效果转换";
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            this.progressBar1.Value = (int)backcounter;
            this.progressBar1.Visible = true;

            if (endflag)
            {
                this.progressBar1.Visible = false;
                
                timer1.Stop();
                button8.Text = "视频效果转换";
            }

        }

        private void textBox6_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox6.Text = path; //将获取到的完整路径赋值到textBox1
        }

        private void textBox5_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox5.Text = path; //将获取到的完整路径赋值到textBox1
        }

        private void textBox5_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;


        }

        private void textBox6_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_DragDrop_1(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox5.Text = path; 
        }

        private void textBox6_DragDrop_1(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox6.Text = path; 
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label7.Text= "" + trackBar1.Value;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
        public struct PointSource
        {
            public double X;
            public double Y;
            public double LightDistance;
            public double LightDegree;
        }

        private void button16_Click(object sender, EventArgs e)
        {


            int Definition = 1;

            label12.Text = "开始计时";


            DateTime dt = DateTime.Now;

            //读取图片
            Bitmap curbitmap = new Bitmap(Image.FromFile(textBox5.Text));
            //锁定图片
            BitmapData curimageData = curbitmap.LockBits(new Rectangle(0, 0, curbitmap.Width, curbitmap.Height),
            ImageLockMode.ReadOnly, curbitmap.PixelFormat);

            //读取效果图片
            Bitmap effbitmap= new Bitmap(Image.FromFile(textBox6.Text));

            BitmapData curimageData2 = effbitmap.LockBits(new Rectangle(0, 0, effbitmap.Width, effbitmap.Height),
            ImageLockMode.ReadOnly, effbitmap.PixelFormat);

            //###########################left###########################//
            unsafe
            {
                //Count red and black pixels
                try
                {
                    //背景图片
                    UnmanagedImage img = new UnmanagedImage(curimageData);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    //效果图片

                    UnmanagedImage img2 = new UnmanagedImage(curimageData2);

                    int height2 = img2.Height;
                    int width2 = img2.Width;
                    int pixelSize2 = (img2.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p2 = (byte*)img2.ImageData.ToPointer();


                    double Max = 0;

                    int Lampwidth = width2;
                    int Lampheight = height2;

                    PointSource[,] Lamps = new PointSource[Lampwidth,Lampheight];

                    double lightlevel = 0;

                    for (int yy = 0; yy < Lampheight; yy++)
                    {
                        for (int xx = 0; xx < Lampwidth; xx++, p2 += pixelSize2)
                        {
                            Lamps[xx ,yy].X = xx * 4* Definition + 2;
                            Lamps[xx, yy].Y = yy * 4 * Definition + 2;
                            Lamps[xx, yy].LightDegree = (((double)1 * (double)p2[RGB.R]) / (double)255);
                            Lamps[xx, yy].LightDistance = 6.25 * Definition;

                            lightlevel += Lamps[xx, yy].LightDegree;
                        }
                    }
                    lightlevel = lightlevel/(Lampheight * Lampwidth*15);

                    // for each line
                    for (int y = 0; y < height; y++)
                    {

                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {

                            double finaldegreereverse = 0;
                            bool endflag=false;

                            int yystart = Math.Max(0,y / 4 - 8);

                            for (int yy = yystart; yy < Lampheight; yy++)
                            {
                                int xxstart = Math.Max(0, x / 4 - 8);

                                for (int xx = xxstart; xx < Lampwidth; xx++)
                                {

                                    double buff1 = (Lamps[xx, yy].X - x);
                                    double buff2 = (Lamps[xx, yy].Y - y);

                                    if (buff1 > 30)
                                    {
                                        if(buff2>30)
                                        {
                                            endflag = true;
                                        }          
                                        break;
                                    }

                                    if (((buff1 * buff1 + buff2 * buff2) > 1000))
                                    {
                                        continue;
                                    }


                                    double buff3 = Lamps[xx, yy].LightDistance;

                                    double buff4 = Math.Sqrt(buff1 * buff1 + buff2 * buff2 + buff3 * buff3);
                                    double buff5 = buff3 / buff4;
                                    //Lamps[xx].LightDegree
                                    double buff6 = buff5 * Math.Exp(-buff4 / (3 * Definition));

                                    finaldegreereverse += buff6 * Lamps[xx, yy].LightDegree;



                                }
                                if (endflag)
                                {
                                    break;
                                }
                            }

                            //double finaldegree = LightUpChange3(finaldegreereverse);
                            double finaldegree = finaldegreereverse;

                            double rrr = Math.Min(1, (finaldegree));

                            double Env = 1-(0.1+ rrr*0.6);


                            //double finalrrr = (1 - Env*(1 - rrr));
                            double finalrrr = (1 - Env * (1 - rrr*0.7));




                            p[RGB.R] = (byte)(p[RGB.R] * finalrrr);
                            p[RGB.G] = (byte)(p[RGB.G] * finalrrr);
                            p[RGB.B] = (byte)(p[RGB.B] * finalrrr);
                            label12.Text = Convert.ToString(Max);
                        }

                    }

                }
                finally
                {
                    curbitmap.UnlockBits(curimageData); //Unlock
                    effbitmap.UnlockBits(curimageData2);
                }

            }


            string zzzzz = OriPath + "\\aa.bmp";


            curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

            curbitmap.Dispose();


            TimeSpan dt2 = DateTime.Now-dt;

            label12.Text = dt2.ToString();

        }

        private void button17_Click(object sender, EventArgs e)
        {
            String file = OriPath + "\\aa.bmp";
            FileInfo info = new FileInfo(file);
            Process p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.WorkingDirectory = info.DirectoryName;
            p.Start();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            for(double i = 0; i < 100; i++)
            {
                double kk=LightUpChange(i/100 ,10 , -5);
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }
    }
}
