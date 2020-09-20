using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using ImageProcess2;
using Point = System.Drawing.Point;

namespace Image_Processin_g
{
    public partial class Form1 : Form
    {
        Bitmap loaded, processed, binary, flood, sub_white;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
                pictureBox1.Image.Save("C:\\Users\\maxim\\Desktop\\Class stuff\\First Sem C1\\Intelligent Systems\\My Activities\\Image processing extra\\coin2.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap temp = new Bitmap(loaded.Width, loaded.Height);
            Bitmap final_img = new Bitmap(loaded.Width, loaded.Height);
            binary = toBinary(loaded);
            temp = (Bitmap)binary.Clone();
            flood = toFloodFill(temp, true);
            sub_white = subToWhite(flood);
            final_img = toAdd(binary, sub_white);
            findCircles(final_img);
        }

        private Bitmap toBinary(Bitmap img)
        {
            Bitmap temp = new Bitmap(loaded.Width, loaded.Height);
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    temp.SetPixel(x, y, Color.FromArgb(pixel.R >= 126 ? 0 : 255, pixel.G >= 126 ? 0 : 255, pixel.B >= 126 ? 0 : 255));
                }
            }
            return temp;
        }

        
        private Bitmap toFloodFill(Bitmap bmp,  bool flag)
        {
            Color replace = Color.FromArgb(255, 255, 255);
            int x = 0,  y = 0;
            Bitmap temp_img = bmp;
            Point pt = new Point(x, y);
            Color targetColor = temp_img.GetPixel(pt.X, pt.Y);
            if (targetColor.ToArgb().Equals(replace.ToArgb()))
            {
                Console.WriteLine("Wrong");
                return null;
            }

            Stack<Point> pixels = new Stack<Point>();

            pixels.Push(pt);
            while (pixels.Count != 0)
            {
                Point temp = pixels.Pop();
                int y1 = temp.Y;
                while (y1 >= 0 && temp_img.GetPixel(temp.X, y1) == targetColor)
                {
                    y1--;
                }
                y1++;
                bool spanLeft = false;
                bool spanRight = false;
                while (y1 < temp_img.Height && temp_img.GetPixel(temp.X, y1) == targetColor)
                {
                    temp_img.SetPixel(temp.X, y1, replace);

                    if (!spanLeft && temp.X > 0 && temp_img.GetPixel(temp.X - 1, y1) == targetColor)
                    {
                        pixels.Push(new Point(temp.X - 1, y1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && temp_img.GetPixel(temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.X < temp_img.Width - 1 && temp_img.GetPixel(temp.X + 1, y1) == targetColor)
                    {
                        pixels.Push(new Point(temp.X + 1, y1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.X < temp_img.Width - 1 && temp_img.GetPixel(temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }
                    y1++;
                }
            }
             return temp_img;

        }

 
        private Bitmap subToWhite(Bitmap img)
        {
            Bitmap temp = new  Bitmap(loaded.Width, loaded.Height);
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    // Console.WriteLine("Red: {} Green: {2} Blue:{3}", pixel.R, pixel.G, pixel.B);
                    temp.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));

                }
            }
            return temp;
        }

        private Bitmap toAdd(Bitmap binary, Bitmap sub_white)
        {
            Bitmap temp = new Bitmap(binary.Width, binary.Height);
            for (int x = 0; x < binary.Width; x++)
            {
                for (int y = 0; y < binary.Height; y++)
                {
                    Color pixel = binary.GetPixel(x, y);
                    Color pixel2 = sub_white.GetPixel(x, y);
                    temp.SetPixel(x, y, Color.FromArgb(pixel2.R + pixel.R, pixel2.G + pixel.G, pixel2.B + pixel.B));

                }
            }
            return temp;
        }

        private void findCircles(Bitmap img)
        {
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            // locate objects using blob counter
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(img);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            // create Graphics object to draw on the image and a pen
            Graphics g = Graphics.FromImage(img);
            Pen redPen = new Pen(Color.Red, 2);
            // check each object and draw circle around objects, which
            // are recognized as circles
            int count = 0;
            double total_val = 0;
            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                AForge.Point center;
                float radius;
                //5 cents = 45 - 46.75
                //10 cents = 50.25 - 51
                //25 cents 59.25 - 60.25
                //1 peso = 71 - 71.25
                //5 peso = 79.25 - 80.25
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    if ((int)radius == 45 || (int)radius == 46)
                    {
                        total_val += .05;
                        count++;
                    }
                    else if((int)radius == 50 || (int)radius == 51)
                    {
                        total_val += .10;
                        count++;
                    }
                    else if ((int)radius == 59 || (int)radius == 60)
                    {
                        total_val += .25;
                        count++;
                    }
                    else if ((int)radius == 71 )
                    {
                        total_val++;
                        count++;
                    }
                    else if ((int)radius == 79 || (int)radius == 80)
                    {
                        total_val += 5;
                        count++;
                    }
                }
                
            }
            //Console.WriteLine("Count: " + count);
            //Console.WriteLine("Total: " + total_val);
            MessageBox.Show("Total: " + Convert.ToString(total_val), "TOTAL VALUE",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            redPen.Dispose();
            g.Dispose();
        }
    }
    
}
