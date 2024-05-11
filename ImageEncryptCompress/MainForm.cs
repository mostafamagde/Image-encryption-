using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ImageEncryptCompress
{

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            }
        }

        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox3);
        //}


        private void button1_Click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {
                ImageMatrix = ImageOperations.Encrypt(ImageMatrix, Seed_Box.Text, (int)K_value.Value);
                string path = File.ReadAllText("path.txt");
                ImageOperations.CompressImage(ImageMatrix, path);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            }
            else
            {
                MessageBox.Show("Open image, please.");
            }
        }

        private void SaveImageButton_Click(object sender, EventArgs e)
        {
            if (ImageMatrix == null)
            {
                MessageBox.Show("Open image, please.");
                return;
            }
            int width = ImageOperations.GetWidth(ImageOperations.afterDecompression);
            int height = ImageOperations.GetHeight(ImageOperations.afterDecompression);
            Bitmap bitmap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGBPixel pixel = ImageOperations.afterDecompression[y, x];
                    bitmap.SetPixel(x, y, Color.FromArgb(pixel.red, pixel.green, pixel.blue));
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    MessageBox.Show("Enter a non existing file name.");
                    SaveImageButton_Click(sender, e);
                    return;
                }

                bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                MessageBox.Show("Image saved successfully.");
            }
        }

        private void EncryptCompressButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrix = ImageOperations.Encrypt(ImageMatrix, Seed_Box.Text, (int)K_value.Value);
            string path = File.ReadAllText("path.txt");
            ImageOperations.CompressImage(ImageMatrix, path);
            sw.Stop();
            EncryptionCompressionTime.Text = sw.Elapsed.ToString();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void DecryptDecompressButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            string path = File.ReadAllText("path.txt");
            ImageMatrix = ImageOperations.Decompress(path);
            ImageMatrix = ImageOperations.Encrypt(ImageMatrix, Seed_Box.Text, (int)K_value.Value);
            sw.Stop();
            DecryptionDecompressionTime.Text = sw.Elapsed.ToString();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void Seed_Box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '1') && !(e.KeyChar == '0') && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool validateInputs()
        {
            if (ImageMatrix == null)
            {
                MessageBox.Show("Open image, please.");
                return false;
            }
            if (Seed_Box.Text == string.Empty)
            {
                MessageBox.Show("Enter Initial Seed.");
                return false;
            }
            if (K_value.Text == string.Empty)
            {
                MessageBox.Show("Enter Tap Position.");
                return false;
            }
            if (Seed_Box.Text.Length <= K_value.Value)
            {
                MessageBox.Show("Tap Position Must Be Less Than The Length Of The Initial Seed.");
                return false;
            }
            return true;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}