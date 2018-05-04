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



        private void button2_Click(object sender, EventArgs e)
        {
            string path = OriPath;

            System.Diagnostics.Process.Start("explorer.exe", path);

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
        public float Envpercent;

        private void multtest()
        {
            shadowpercent = (float)trackBar1.Value / 10;
            Envpercent= 1-(float)trackBar2.Value / 10;

            Thread oGetArgThread = new Thread(new ThreadStart(aforgereadtest));
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

                Videochangebuf.Dispose();


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

                                float rr = ((float)p2[RGB.R]) / 255;
                                float gg = ((float)p2[RGB.G]) / 255;
                                float bb = ((float)p2[RGB.B]) / 255;

                                //float mulr = (shadowpercent + rr) / (1 + shadowpercent);
                                //float mulg = (shadowpercent + gg) / (1 + shadowpercent);
                                //float mulb = (shadowpercent + bb) / (1 + shadowpercent);

                                float mulr = (shadowpercent * rr);
                                float mulg = (shadowpercent * gg);
                                float mulb = (shadowpercent * bb);

                                p[RGB.R] = (byte)(p[RGB.R] * (mulr * Envpercent + 1 - Envpercent));
                                p[RGB.G] = (byte)(p[RGB.G] * (mulg * Envpercent + 1 - Envpercent));
                                p[RGB.B] = (byte)(p[RGB.B] * (mulb * Envpercent + 1 - Envpercent));

                            }

                        }

                    }
                    finally
                    {
                        curbitmap.UnlockBits(curimageData); //Unlock
                        Videochange.UnlockBits(curimageData2);
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

        private void button16_Click(object sender, EventArgs e)
        {

            //读取图片
            Bitmap curbitmap = new Bitmap(Image.FromFile(textBox5.Text));

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
                            double lightdegree = 13;

                            double[] lamp1 = new[] { 668.0, 168, lightdegree };
                            double[] lamp2 = new[] { 684.0, 168, lightdegree };
                            double[] lamp3 = new[] { 700.0, 168, lightdegree };
                            double[] lamp4 = new[] { 652.0, 168, lightdegree };
                            double[] lamp5 = new[] { 636.0, 168, lightdegree };
                            //double[] lamp6 = new[] { 668.0, 200, 25 };
                            //double[] lamp7 = new[] { 684.0, 168, 25 };
                            //double[] lamp8 = new[] { 668.0, 168, 25 };
                            //double[] lamp9 = new[] { 700.0, 168, 25 };

                            double[] curposition = new[] { x, y, 0.0 };
                            double[] unit = new[] { 0.0, 0, 1.5 };

                            double[] lamp1vector = Normalize(Sub(lamp1, curposition));
                            double[] lamp2vector = Normalize(Sub(lamp2, curposition));
                            double[] lamp3vector = Normalize(Sub(lamp3, curposition));
                            double[] lamp4vector = Normalize(Sub(lamp4, curposition));
                            double[] lamp5vector = Normalize(Sub(lamp5, curposition));
                            //double[] lamp6vector = Normalize(Sub(lamp6, curposition));
                            //double[] lamp7vector = Normalize(Sub(lamp7, curposition));
                            //double[] lamp8vector = Normalize(Sub(lamp8, curposition));
                            //double[] lamp9vector = Normalize(Sub(lamp9, curposition));

                            double light1 = Dot(lamp1vector, unit);
                            double light2 = Dot(lamp2vector, unit);
                            double light3 = Dot(lamp3vector, unit);
                            double light4 = Dot(lamp4vector, unit);
                            double light5 = Dot(lamp5vector, unit);
                            //double light6 = Dot(lamp6vector, unit);
                            //double light7 = Dot(lamp7vector, unit);
                            //double light8 = Dot(lamp8vector, unit);
                            //double light9 = Dot(lamp9vector, unit);

                            double lightfinal = (light1 + light2 + light3 + light4 + light5) / 5;

                            //p[RGB.R] = (byte)Math.Min(255, (p[RGB.R] * (light1 + light2 + light3 + light4 + light5 + light6 + light7 + light8 + light9) / 9));
                            //p[RGB.G] = (byte)Math.Min(255, (p[RGB.G] * (light1 + light2 + light3 + light4 + light5 + light6 + light7 + light8 + light9) / 9));
                            //p[RGB.B] = (byte)Math.Min(255, (p[RGB.B] * (light1 + light2 + light3 + light4 + light5 + light6 + light7 + light8 + light9) / 9));
                            p[RGB.R] = (byte)Math.Min(255, (p[RGB.R] * lightfinal));
                            p[RGB.G] = (byte)Math.Min(255, (p[RGB.G] * lightfinal));
                            p[RGB.B] = (byte)Math.Min(255, (p[RGB.B] * lightfinal));
                        }

                    }

                }
                finally
                {
                    curbitmap.UnlockBits(curimageData); //Unlock
                }

            }


            string zzzzz = picdic + "\\aa.bmp";


            curbitmap.Save(zzzzz, System.Drawing.Imaging.ImageFormat.Bmp);

            curbitmap.Dispose();




            int dfesf = 5;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label2.Text = "" + trackBar2.Value;
        }
    }
}
