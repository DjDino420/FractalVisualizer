using System;
using System.Drawing;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;

namespace MandelbrotZoom
{
    public partial class Form1 : Form
    {
        // Initial complex plane boundaries
        double xmin = -2.0, xmax = 1.0;
        double ymin = -1.5, ymax = 1.5;
        int maxIter = 50;
        double zoomFactor = 0.9; // Each step reduces the view size

        Timer timer;
        PictureBox pictureBox;

        public Form1()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Width = 1920;
            this.Height = 1080;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
            };
            this.Controls.Add(pictureBox);

            timer = new Timer { Interval = 100 }; // Update every 100ms
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            int width = pictureBox.Width;
            int height = pictureBox.Height;
            Bitmap bmp = new Bitmap(width, height);
            object bmpLock = new object(); // To prevent multi-threaded access issues

            double bestX = xmin, bestY = ymin;
            int bestIter = 0;

            Parallel.For(0, width, i =>
            {
                for (int j = 0; j < height; j++)
                {
                    double x = xmin + (xmax - xmin) * i / width;
                    double y = ymin + (ymax - ymin) * j / height;
                    var c = new Complex(x, y);
                    int iter = Mandelbrot(c, maxIter);
                    Color color = ColorFromIteration(iter, maxIter);

                    lock (bmpLock) bmp.SetPixel(i, j, color); // Prevent race conditions

                    if (iter > bestIter) // Track best zoom location
                    {
                        lock (bmpLock)
                        {
                            bestIter = iter;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }
            });

            pictureBox.Image?.Dispose();
            pictureBox.Image = bmp;

            // Zoom centered on most detailed area
            double newWidth = (xmax - xmin) * zoomFactor;
            double newHeight = (ymax - ymin) * zoomFactor;
            xmin = bestX - newWidth / 2;
            xmax = bestX + newWidth / 2;
            ymin = bestY - newHeight / 2;
            ymax = bestY + newHeight / 2;
        }


        // Mandelbrot iteration calculation
        private int Mandelbrot(Complex c, int maxIter)
        {
            Complex z = new Complex(0, 0);
            int iter = 0;
            while (z.Magnitude <= 2 && iter < maxIter)
            {
                z = z * z + c;
                iter++;
            }
            return iter;
        }

        // Simple grayscale coloring based on iterations
        private Color ColorFromIteration(int iter, int maxIter)
        {
            if (iter == maxIter)
                return Color.Black;

            double hue = 360.0 * iter / maxIter; // Convert iteration count to hue
            return ColorFromHSV(hue, 1.0, 1.0); // Full saturation and brightness
        }

// Convert HSV to RGB (used for coloring)
        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(v, t, p),
                1 => Color.FromArgb(q, v, p),
                2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v),
                4 => Color.FromArgb(t, p, v),
                _ => Color.FromArgb(v, p, q),
            };
        }
    }

    // A basic immutable struct for complex numbers
    public readonly struct Complex
    {
        public double Real { get; init; }
        public double Imag { get; init; }

        public Complex(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        public double Magnitude => Math.Sqrt(Real * Real + Imag * Imag);

        public static Complex operator +(Complex a, Complex b) =>
            new Complex(a.Real + b.Real, a.Imag + b.Imag);

        public static Complex operator *(Complex a, Complex b) =>
            new Complex(a.Real * b.Real - a.Imag * b.Imag,
                        a.Real * b.Imag + a.Imag * b.Real);
    }
}
