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

            Thread oGetArgThread = new Thread(new ThreadStart(aforgereadtest2));
            oGetArgThread.IsBackground = true;
            oGetArgThread.Start();

        }

        public struct PointSource
        {
            public double X;
            public double Y;
            public double LightDistance;
            public double LightDegree;
        }

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
                writerzzz.Open("testoutput.avi", 768, 328, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap bitmapchange = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    Bitmap curbitmap = new Bitmap(bitmapchange, 768, 328);

                    //模糊化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 192, 80);

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
                    bitmapchange.Dispose();

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
        private void aforgereadtest2()
        {
            int Definition = 1;


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
                writerzzz.Open("testoutput.avi", 768, 328, readerzzz.FrameRate, VideoCodec.MPEG4, 25000000);


                // 对视频的所有帧进行操作
                for (int i = 0; i < (readerzzz.FrameCount - 1) && endflag == false; i++)
                {
                    backcounter = (((float)i) / (readerzzz.FrameCount - 1)) * 100;

                    //载入当前帧动画
                    Bitmap curbitmapsource = readerzzz.ReadVideoFrame();


                    //载入背景
                    Bitmap bitmapchange = sourcepic.Clone(new Rectangle(0, 0, (sourcepic.Width / 2) * 2, (sourcepic.Height / 2) * 2), sourcepic.PixelFormat);

                    Bitmap  curbitmap = new Bitmap(bitmapchange, 768, 328);

                    //模糊化
                    Bitmap Videochangebuf = new Bitmap(curbitmapsource, 192, 80);

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

                            //先将灯光一帧转为一个亮灯数组
                            int Lampwidth = width2;
                            int Lampheight = height2;

                            PointSource[,] Lamps = new PointSource[Lampwidth, Lampheight];

                            double lightlevel = 0;

                            for (int yy = 0; yy < Lampheight; yy++)
                            {
                                for (int xx = 0; xx < Lampwidth; xx++, p2 += pixelSize2)
                                {
                                    Lamps[xx, yy].X = xx * 4 * Definition + 2;
                                    Lamps[xx, yy].Y = yy * 4 * Definition + 2;
                                    Lamps[xx, yy].LightDegree = (((double)1 * (double)p2[RGB.R]) / (double)255);
                                    Lamps[xx, yy].LightDistance = 6.25 * Definition;

                                    lightlevel += Lamps[xx, yy].LightDegree;
                                }
                            }


                            // for each line
                            for (int y = 0; y < height; y++)
                            {

                                // for each pixel
                                for (int x = 0; x < width; x++, p += pixelSize)
                                {

                                    double finaldegreereverse = 0;
                                    bool endflag = false;

                                    int yystart = Math.Max(0, y / 4 - 8);

                                    for (int yy = yystart; yy < Lampheight; yy++)
                                    {
                                        int xxstart = Math.Max(0, x / 4 - 8);

                                        for (int xx = xxstart; xx < Lampwidth; xx++)
                                        {

                                            double buff1 = (Lamps[xx, yy].X - x);
                                            double buff2 = (Lamps[xx, yy].Y - y);

                                            if (buff1 > 30)
                                            {
                                                if (buff2 > 30)
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

                                    double Env = 1 - (0.1 + rrr * 0.6);


                                    //double finalrrr = (1 - Env*(1 - rrr));
                                    double finalrrr = (1 - Env * (1 - rrr * 0.7));


                                    p[RGB.R] = (byte)(p[RGB.R] * finalrrr);
                                    p[RGB.G] = (byte)(p[RGB.G] * finalrrr);
                                    p[RGB.B] = (byte)(p[RGB.B] * finalrrr);


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
                    bitmapchange.Dispose();

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
            finally {

            }
        }
    }
}
