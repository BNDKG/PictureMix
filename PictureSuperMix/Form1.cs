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
using System.Management;
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

        double[,] board_adj = new double[10, 1000];
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
        string picdic;



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
                    board_adj[i, ii] = 1000 - ((double)ii / (double)x) * 1000;
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
            Envpercent = 1 - (float)trackBar2.Value / 10;

            if (radioButton1.Checked == true)
            {
                Thread oGetArgThread = new Thread(new ThreadStart(aforgereadtest));
                oGetArgThread.IsBackground = true;
                oGetArgThread.Start();
            }
            else if (radioButton2.Checked == true)
            {
                Thread oGetArgThread = new Thread(new ThreadStart(aforgereadtest2));
                oGetArgThread.IsBackground = true;
                oGetArgThread.Start();
            }
            else
            {
                return;
            }


        }

        public struct PointSource
        {
            public double X;
            public double Y;
            public double LightDistance;
            public double LightDegree;
        }

        public struct changestruct
        {
            public byte R;
            public byte G;
            public byte B;
            //public double LightDegree;
        }
        /// <summary>
        /// 投影变换
        /// </summary>
        private void aforgereadtest()
        {
            try
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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {

                                    float rr = ((float)p2[RGB.R]) / 255;
                                    float gg = ((float)p2[RGB.G]) / 255;
                                    float bb = ((float)p2[RGB.B]) / 255;

                                    //float mulr = (shadowpercent + rr) / (1 + shadowpercent);
                                    //float mulg = (shadowpercent + gg) / (1 + shadowpercent);
                                    //float mulb = (shadowpercent + bb) / (1 + shadowpercent);

                                    float mulr = (shadowpercent * rr) ;
                                    float mulg = (shadowpercent * gg) ;
                                    float mulb = (shadowpercent * bb) ;

                                    if (mulr > 255) { mulr = 255; }
                                    if (mulg > 255) { mulg = 255; }
                                    if (mulb > 255) { mulb = 255; }

                                    int r3 = (int)(((int)p[RGB.R] * (mulr * Envpercent + 1 - Envpercent)) );
                                    int g3 = (int)(((int)p[RGB.G] * (mulg * Envpercent + 1 - Envpercent)) );
                                    int b3 = (int)(((int)p[RGB.B] * (mulb * Envpercent + 1 - Envpercent)) );

                                    if (r3 > 255) { r3 = 255; }
                                    if (g3 > 255) { g3 = 255; }
                                    if (b3 > 255) { b3 = 255; }

                                    p[RGB.R] = (byte)(r3);
                                    p[RGB.G] = (byte)(g3);
                                    p[RGB.B] = (byte)(b3);


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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 投影变换
        /// </summary>
        private void aforgereadtestadvanced()
        {
            try
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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {

                                    float rr = ((float)p2[RGB.R]) / 255;
                                    float gg = ((float)p2[RGB.G]) / 255;
                                    float bb = ((float)p2[RGB.B]) / 255;

                                    //float mulr = (shadowpercent + rr) / (1 + shadowpercent);
                                    //float mulg = (shadowpercent + gg) / (1 + shadowpercent);
                                    //float mulb = (shadowpercent + bb) / (1 + shadowpercent);

                                    float mulr = (shadowpercent * rr) + 30;
                                    float mulg = (shadowpercent * gg) + 30;
                                    float mulb = (shadowpercent * bb) + 30;

                                    if (mulr > 255) { mulr = 255; }
                                    if (mulg > 255) { mulg = 255; }
                                    if (mulb > 255) { mulb = 255; }

                                    int r3 = (int)(((int)p[RGB.R] * (mulr * Envpercent + 1 - Envpercent)) + 30);
                                    int g3 = (int)(((int)p[RGB.G] * (mulg * Envpercent + 1 - Envpercent)) + 30);
                                    int b3 = (int)(((int)p[RGB.B] * (mulb * Envpercent + 1 - Envpercent)) + 30);

                                    if (r3 > 255) { r3 = 255; }
                                    if (g3 > 255) { g3 = 255; }
                                    if (b3 > 255) { b3 = 255; }

                                    p[RGB.R] = (byte)(r3);
                                    p[RGB.G] = (byte)(g3);
                                    p[RGB.B] = (byte)(b3);


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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 单纯的视频编码转换
        /// </summary>
        private void aforgereadtest3()
        {
            try
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
                writerzzz.Open("testoutput.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {



                                    p[RGB.R] = (byte)(p2[RGB.R]);
                                    p[RGB.G] = (byte)(p2[RGB.G]);
                                    p[RGB.B] = (byte)(p2[RGB.B]);

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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }

        private void aforgechange2()
        {
            try
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
                writerzzz.Open("testoutput.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //模糊化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 1280, 800);

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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {
                                    if (p2[RGB.R] < 50 && p2[RGB.G] < 50 && p2[RGB.B] < 50)
                                    {
                                        p[RGB.R] = 0;
                                        p[RGB.G] = 0;
                                        p[RGB.B] = 0;
                                    }
                                    else if (p2[RGB.R] > 70)
                                    {
                                        if ((p2[RGB.R] * 2 > 255) || (p2[RGB.G] * 2 > 255) || (p2[RGB.B] * 2 > 255))
                                        {
                                            p[RGB.R] = 255;
                                            p[RGB.G] = 255;
                                            p[RGB.B] = 255;
                                        }
                                        else
                                        {
                                            p[RGB.R] = (byte)(p2[RGB.R] * 2);
                                            p[RGB.G] = (byte)(p2[RGB.R] * 2);
                                            p[RGB.B] = (byte)(p2[RGB.R] * 2);
                                        }
                                    }
                                    else
                                    {

                                    }


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


                    //单帧储存
                    /*
                    if (i == 130)
                    {

                        curbitmap.Save("snap.bmp");

                        int kkk = 1;

                    }
                    */

                    // 释放当前操作内存
                    curbitmap.Dispose();
                    curbitmapsource.Dispose();
                    Videochange.Dispose();


                }
                readerzzz.Close();

                writerzzz.Close();
                //释放内存
                sourcepic.Dispose();

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 将视频变换到图片对应大小
        /// </summary>
        private void aforgeonlychange2()
        {
            try
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
                writerzzz.Open("sizechanged.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 2500000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //标准化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 1280, 800);

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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {


                                    p[RGB.R] = p2[RGB.R];
                                    p[RGB.G] = p2[RGB.G];
                                    p[RGB.B] = p2[RGB.B];

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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                aforgeupleftchange();
                //endflag = true;
            }

            //aforgeupleftchange();
        }
        /// <summary>
        /// 适配到左上方
        /// </summary>
        private void aforgeupleftchange()
        {
            try
            {
                //读取图片
                Bitmap sourcepic = new Bitmap(Image.FromFile("1280x800.png"));

                // 生成视频生成读取器
                VideoFileReader readerzzz = new VideoFileReader();
                // 打开视频
                readerzzz.Open("sizechanged.mp4");

                // 生成视频写入器
                VideoFileWriter writerzzz = new VideoFileWriter();
                // 新建一个视频(帧必须是二的倍数)
                writerzzz.Open("testoutput.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 2500000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap Videochange = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //标准化
                    //Bitmap Videochangebuf = new Bitmap(curbitmapsource, 1280, 800);

                    //投影变化
                    //Bitmap Videochange = new Bitmap(Videochangebuf, curbitmap.Width, curbitmap.Height);

                    //Videochangebuf.Dispose();


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
                            //背景
                            UnmanagedImage img = new UnmanagedImage(curimageData);

                            int height = img.Height;
                            int width = img.Width;
                            int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                            byte* p = (byte*)img.ImageData.ToPointer();

                            //动效
                            UnmanagedImage img2 = new UnmanagedImage(curimageData2);

                            int height2 = img2.Height;
                            int width2 = img2.Width;
                            int pixelSize2 = (img2.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                            byte* p2 = (byte*)img2.ImageData.ToPointer();

                            Color[,] buff3 = new Color[width, height];
                            // 读取视频
                            for (int y = 0; y < height2; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < width2; x++, p2 += pixelSize2)
                                {
                                    buff3[x, y] = Color.FromArgb(255, p2[RGB.R], p2[RGB.G], p2[RGB.B]);

                                }
                            }

                            // 插入背景
                            for (int y = 0; y < height; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < width; x++, p += pixelSize)
                                {
                                    p[RGB.R] = buff3[x, y].R;
                                    p[RGB.G] = buff3[x, y].G;
                                    p[RGB.B] = buff3[x, y].B;

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
                    //curbitmapsource.Dispose();
                    Videochange.Dispose();


                }
                readerzzz.Close();

                writerzzz.Close();
                //释放内存
                sourcepic.Dispose();

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 缩小的编码转换可能
        /// </summary>
        private void aforgechange()
        {
            try
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

                //readerzzz.FrameRate
                writerzzz.Open("testoutput.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, 60, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //模糊化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 1280, 800);

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
                                for (int x = 0; x < width; x++, p += pixelSize, p2 += pixelSize2)
                                {
                                    p[RGB.R] = (byte)(p2[RGB.R]);
                                    p[RGB.G] = (byte)(p2[RGB.G]);
                                    p[RGB.B] = (byte)(p2[RGB.B]);

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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 黑白渐变闪烁
        /// </summary>
        private void aforgereadtestx()
        {
            try
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
                writerzzz.Open("testoutput.mp4", (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2, readerzzz.FrameRate, VideoCodec.MPEG4, 2500000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //模糊化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 640, 400);

                    //投影变化
                    Bitmap Videochange = new Bitmap(Videochangebuf, Videochangebuf.Width, Videochangebuf.Height);

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

                            changestruct[,] Lamps2 = new changestruct[width2, height2];


                            // for each line
                            for (int y = 0; y < height2; y++)
                            {

                                // for each pixel
                                for (int x = 0; x < width2; x++, p2 += pixelSize2)
                                {
                                    Lamps2[x, y].R = p2[RGB.R];
                                    Lamps2[x, y].G = p2[RGB.G];
                                    Lamps2[x, y].B = p2[RGB.B];
                                    //Lamps2[x, y].LightDistance = 6.25;

                                    //p[RGB.R] = p2[RGB.R];
                                    //p[RGB.G] = p2[RGB.G];
                                    //p[RGB.B] = p2[RGB.B];

                                }

                            }


                            // for each line
                            for (int y = 0; y < height; y++)
                            {

                                // for each pixel
                                for (int x = 0; x < width; x++, p += pixelSize)
                                {

                                    if ((x < (width / 2 - 1)) && (y < (height / 2 - 1)))
                                    {
                                        p[RGB.R] = Lamps2[x, y].R;
                                        p[RGB.G] = Lamps2[x, y].G;
                                        p[RGB.B] = Lamps2[x, y].B;
                                    }
                                    else
                                    {
                                        p[RGB.R] = 0;
                                        p[RGB.G] = 0;
                                        p[RGB.B] = 0;
                                    }


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

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }
        /// <summary>
        /// 幕动光源高级动态效果渲染
        /// </summary>
        private void aforgereadtest2()
        {
            //P16版本
            double g_w = 196;
            double g_h = 128;
            double times = 2;

            try
            {
                //读取图片

                Bitmap Videochangebuf = new Bitmap(Image.FromFile(textBox5.Text));
                Bitmap sourcepic = new Bitmap(Videochangebuf, (int)(g_w * 4 * times), (int)(g_h * 4 * times));

                Videochangebuf.Dispose();


                // 生成视频生成读取器
                VideoFileReader readerzzz = new VideoFileReader();
                // 打开视频
                readerzzz.Open(textBox6.Text);

                // 生成视频写入器
                VideoFileWriter writerzzz = new VideoFileWriter();
                // 新建一个视频(帧必须是二的倍数)
                writerzzz.Open("testoutput.avi", (int)(g_w * 4 * times), (int)(g_h * 4 * times), readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();
                    //模糊化
                    Bitmap Videochange = new Bitmap(curbitmapsource, (int)g_w, (int)g_h);

                    curbitmapsource.Dispose();


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

                            //先将灯光一帧转为一个亮灯数组
                            int Lampwidth = width2;
                            int Lampheight = height2;

                            PointSource[,] Lamps = new PointSource[Lampwidth, Lampheight];

                            double lightlevel = 0;
                            //读取当前效果图片输入灯珠
                            for (int yy = 0; yy < Lampheight; yy++)
                            {
                                for (int xx = 0; xx < Lampwidth; xx++, p2 += pixelSize2)
                                {
                                    Lamps[xx, yy].X = xx * 4;
                                    Lamps[xx, yy].Y = yy * 4;
                                    Lamps[xx, yy].LightDegree = (((double)1 * (double)p2[RGB.R]) / (double)255);
                                    Lamps[xx, yy].LightDistance = 6.25;

                                    lightlevel += Lamps[xx, yy].LightDegree;
                                }
                            }
                            lightlevel = lightlevel / (Lampheight * Lampwidth);


                            //使用灯珠生成光效矩阵

                            double[,] LampBuff = new double[(int)(g_w * 4), (int)(g_h * 4)];

                            for (int y = 0; y < g_h * 4; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < g_w * 4; x++)
                                {
                                    double finaldegreereverse = 0;
                                    bool endflag = false;

                                    int yystart = (int)Math.Max(0, y / 4 - 4);
                                    //int yystart = 0;

                                    for (int yy = yystart; yy < Lampheight; yy++)
                                    {
                                        int xxstart = (int)Math.Max(0, x / 4 - 4);
                                        //int xxstart = 0;

                                        for (int xx = xxstart; xx < Lampwidth; xx++)
                                        {

                                            double buff1 = (Lamps[xx, yy].X - x);
                                            double buff2 = (Lamps[xx, yy].Y - y);

                                            //像素跳跃
                                            if (buff1 > (16))
                                            {
                                                if (buff2 > (16))
                                                {
                                                    endflag = true;
                                                }
                                                break;
                                            }

                                            if (((buff1 * buff1 + buff2 * buff2) > (200))) { continue; }

                                            double buff3 = Lamps[xx, yy].LightDistance;
                                            double buff4 = Math.Sqrt(buff1 * buff1 + buff2 * buff2 + buff3 * buff3);
                                            double buff5 = buff3 / buff4;
                                            double buff6 = buff5 * Math.Exp(-buff4 / 3.1);

                                            finaldegreereverse += buff6 * Lamps[xx, yy].LightDegree;

                                        }
                                        if (endflag) { break; }
                                    }

                                    //double finaldegree = finaldegreereverse;

                                    double rrr = Math.Min(1, (finaldegreereverse));
                                    double Env = 1 - ((1 - Envpercent) * 0.5 + 0.25 * lightlevel);

                                    LampBuff[x, y] = (1 - Env * (1 - rrr * shadowpercent));

                                }
                            }


                            // 将光效数据叠加进原图
                            for (int y = 0; y < height; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < width; x++, p += pixelSize)
                                {
                                    p[RGB.R] = (byte)(p[RGB.R] * LampBuff[x / 2, y / 2]);
                                    p[RGB.G] = (byte)(p[RGB.G] * LampBuff[x / 2, y / 2]);
                                    p[RGB.B] = (byte)(p[RGB.B] * LampBuff[x / 2, y / 2]);

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

                    Videochange.Dispose();


                }
                readerzzz.Close();

                writerzzz.Close();
                //释放内存
                sourcepic.Dispose();

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }

        private void aforgereadtest4()
        {
            //P8版本
            //double g_w = 196*2;
            //double g_h = 128*2;
            double g_w = 1344;
            double g_h = 384;

            double times = 1;

            try
            {
                //读取图片

                Bitmap Videochangebuf = new Bitmap(Image.FromFile(textBox5.Text));
                Bitmap sourcepic = new Bitmap(Videochangebuf, (int)(g_w * 4 * times), (int)(g_h * 4 * times));

                Videochangebuf.Dispose();


                // 生成视频生成读取器
                VideoFileReader readerzzz = new VideoFileReader();
                // 打开视频
                readerzzz.Open(textBox6.Text);

                // 生成视频写入器
                VideoFileWriter writerzzz = new VideoFileWriter();
                // 新建一个视频(帧必须是二的倍数)
                writerzzz.Open("testoutput.avi", (int)(g_w * 4 * times), (int)(g_h * 4 * times), readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);

                //灯箱光效矩阵(这个可能会导致内存溢出)
                PointSource[,] Lamps = new PointSource[(int)g_w, (int)g_h];

                //使用灯珠生成光效矩阵
                double[,] LampBuff = new double[(int)(g_w * 4), (int)(g_h * 4)];

                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入背景
                    Bitmap curbitmap = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();
                    //模糊化
                    Bitmap Videochange = new Bitmap(curbitmapsource, (int)g_w, (int)g_h);

                    curbitmapsource.Dispose();


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

                            //先将灯光一帧转为一个亮灯数组
                            int Lampwidth = width2;
                            int Lampheight = height2;



                            double lightlevel = 0;
                            //读取当前效果图片输入灯珠
                            for (int yy = 0; yy < Lampheight; yy++)
                            {
                                for (int xx = 0; xx < Lampwidth; xx++, p2 += pixelSize2)
                                {
                                    Lamps[xx, yy].X = xx * 4;
                                    Lamps[xx, yy].Y = yy * 4;
                                    Lamps[xx, yy].LightDegree = (((double)1 * (double)p2[RGB.R]) / (double)255);
                                    Lamps[xx, yy].LightDistance = 6.25;

                                    lightlevel += Lamps[xx, yy].LightDegree;
                                }
                            }
                            lightlevel = lightlevel / (Lampheight * Lampwidth);




                            for (int y = 0; y < g_h * 4; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < g_w * 4; x++)
                                {
                                    double finaldegreereverse = 0;
                                    bool endflag = false;

                                    int yystart = (int)Math.Max(0, y / 4 - 4);
                                    //int yystart = 0;

                                    for (int yy = yystart; yy < Lampheight; yy++)
                                    {
                                        int xxstart = (int)Math.Max(0, x / 4 - 4);
                                        //int xxstart = 0;

                                        for (int xx = xxstart; xx < Lampwidth; xx++)
                                        {

                                            double buff1 = (Lamps[xx, yy].X - x);
                                            double buff2 = (Lamps[xx, yy].Y - y);

                                            //像素跳跃
                                            if (buff1 > (16))
                                            {
                                                if (buff2 > (16))
                                                {
                                                    endflag = true;
                                                }
                                                break;
                                            }

                                            if (((buff1 * buff1 + buff2 * buff2) > (200))) { continue; }

                                            double buff3 = Lamps[xx, yy].LightDistance;
                                            double buff4 = Math.Sqrt(buff1 * buff1 + buff2 * buff2 + buff3 * buff3);
                                            double buff5 = buff3 / buff4;
                                            double buff6 = buff5 * Math.Exp(-buff4 / 3.1);

                                            finaldegreereverse += buff6 * Lamps[xx, yy].LightDegree;

                                        }
                                        if (endflag) { break; }
                                    }

                                    //double finaldegree = finaldegreereverse;

                                    double rrr = Math.Min(1, (finaldegreereverse));
                                    double Env = 1 - ((1 - Envpercent) * 0.5 + 0.25 * lightlevel);

                                    LampBuff[x, y] = (1 - Env * (1 - rrr * shadowpercent));



                                    //Dispose();
                                }
                            }


                            // 将光效数据叠加进原图
                            for (int y = 0; y < height; y++)
                            {
                                // for each pixel
                                for (int x = 0; x < width; x++, p += pixelSize)
                                {
                                    p[RGB.R] = (byte)(p[RGB.R] * LampBuff[(int)(x / times), (int)(y / times)]);
                                    p[RGB.G] = (byte)(p[RGB.G] * LampBuff[(int)(x / times), (int)(y / times)]);
                                    p[RGB.B] = (byte)(p[RGB.B] * LampBuff[(int)(x / times), (int)(y / times)]);

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

                    Videochange.Dispose();


                }
                readerzzz.Close();

                writerzzz.Close();
                //释放内存
                sourcepic.Dispose();

            }
            catch (ArgumentException)
            {
                MessageBox.Show(" 请拖入视频文件和图片文件 ");
                return;
            }
            catch (IOException)
            {
                MessageBox.Show(" 请拖入 正确 的视频文件和图片文件 ");
                return;
            }
            finally
            {
                endflag = true;
            }


        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (endflag)
            {
                if (radioButton1.Checked == false && radioButton2.Checked == false)
                {
                    return;
                }

                endflag = false;

                timer1.Start();
                multtest();
                button8.Text = "停止";
            }
            else
            {
                endflag = true;
                button8.Text = "视频合成";
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            this.progressBar1.Value = (int)backcounter;
            this.progressBar1.Visible = true;

            if (endflag)
            {
                timer1.Stop();
                this.progressBar1.Visible = false;
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

            try
            {
                Bitmap b = new Bitmap(path);
                pictureBox1.Image = b;
            }
            catch
            {
                MessageBox.Show(" 请拖入 正确 的图片文件 ");
                return;
            }


        }

        private void textBox6_DragDrop_1(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox6.Text = path;

            try
            {
                axWindowsMediaPlayer1.close();
                /*
                // 生成视频生成读取器
                VideoFileReader readerzzz2 = new VideoFileReader();
                // 打开视频
                readerzzz2.Open(path);

                Bitmap b = readerzzz2.ReadVideoFrame();

                pictureBox2.Image = b;

                readerzzz2.Dispose();
                */

                axWindowsMediaPlayer1.URL = path;
                axWindowsMediaPlayer1.settings.setMode("loop", true);

                bottonchange();
            }
            catch
            {
                MessageBox.Show(" 请拖入 正确 的视频文件 ");
                return;
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label7.Text = "" + trackBar1.Value;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label2.Text = "" + trackBar2.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                String file = @"testoutput.avi";
                FileInfo info = new FileInfo(file);
                Process p = new Process();
                p.StartInfo.FileName = file;
                p.StartInfo.WorkingDirectory = info.DirectoryName;
                p.Start();

            }
            catch (Win32Exception)
            {
                MessageBox.Show(" 请先进行视频效果转换 ");
            }
            finally
            {

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        //用于鼠标拖动
        private Point mPoint;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.close();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            bottonchange();

        }

        private void bottonchange()
        {
            string test2 = axWindowsMediaPlayer1.URL;
            if (test2 == "")
            {
                return;
            }

            int test = (int)axWindowsMediaPlayer1.playState;

            if (test == 3)
            {
                button5.Font = new Font(button5.Font.Name, 14);
                button5.Text = "▷";
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            else
            {
                button5.Font = new Font(button5.Font.Name, 9);
                button5.Text = "■";
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (endflag)
            {
                if (radioButton1.Checked == false && radioButton2.Checked == false)
                {
                    return;
                }

                endflag = false;

                timer1.Start();
                Thread oGetArgThread = new Thread(new ThreadStart(aforgeonlychange2));
                oGetArgThread.IsBackground = true;
                oGetArgThread.Start();
                button8.Text = "停止";
            }
            else
            {
                endflag = true;
                button8.Text = "视频合成";
            }



        }
    }
}
