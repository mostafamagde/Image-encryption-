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

        RGBPixel[,] OriginalImageMatrix;
        RGBPixel[,] ImageMatrixAfterOperation;
        string path;
        string pathWithoutFileName;
        string fileExtension;
        string fileNameWithoutExtension;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImageMatrixAfterOperation = null;
                OriginalImageMatrix = null;
                ResetFormComponents();
                //Open the browsed image and display it
                path = openFileDialog.FileName;
                fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                pathWithoutFileName = Path.GetDirectoryName(path);
                fileExtension = Path.GetExtension(path);
                if (fileExtension == ".bmp")
                {
                    OriginalImageMatrix = ImageOperations.OpenImage(path);
                    ImageOperations.DisplayImage(OriginalImageMatrix, pictureBox1);
                    txtWidth.Text = ImageOperations.GetWidth(OriginalImageMatrix).ToString();
                    txtHeight.Text = ImageOperations.GetHeight(OriginalImageMatrix).ToString();
                }
            }
        }

        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox3);
        //}


        private void SaveImageButton_Click(object sender, EventArgs e)
        {
            if (ImageMatrixAfterOperation == null)
            {
                MessageBox.Show("Make operation, please.");
                return;
            }
            int width = ImageOperations.GetWidth(ImageMatrixAfterOperation);
            int height = ImageOperations.GetHeight(ImageMatrixAfterOperation);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    RGBPixel pixel = ImageMatrixAfterOperation[y, x];
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

        private void EncryptDecryptButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Encrypt(OriginalImageMatrix, Seed_Box.Text, (int)K_value.Value);
           // ImageOperations.CompressImage(ImageMatrixAfterOperation, $"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin");
            sw.Stop();
            EncryptDecryptTime.Text = sw.Elapsed.ToString();
            ImageOperations.DisplayImage(ImageMatrixAfterOperation, pictureBox2);
        }

        private void EncryptCompressButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Encrypt(OriginalImageMatrix, Seed_Box.Text, (int)K_value.Value);
            ImageOperations.CompressImage(ImageMatrixAfterOperation, $"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin");
            sw.Stop();
            EncryptionCompressionTime.Text = sw.Elapsed.ToString();
            ImageOperations.DisplayImage(ImageMatrixAfterOperation, pictureBox2);
        }

        private void DecryptDecompressButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs(validateImage: false))
                return;

            if (fileExtension != ".bin")
            {
                MessageBox.Show("Enter .bin file!");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Decompress($"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin");
            ImageMatrixAfterOperation = ImageOperations.Encrypt(ImageMatrixAfterOperation, Seed_Box.Text, (int)K_value.Value);
            sw.Stop();
            DecryptionDecompressionTime.Text = sw.Elapsed.ToString();
            ImageOperations.DisplayImage(ImageMatrixAfterOperation, pictureBox2);
        }

        private void Seed_Box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '1') && !(e.KeyChar == '0') && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool validateInputs(bool validateImage = true)
        {
            if (validateImage)
            {
                if (OriginalImageMatrix == null)
                {
                    MessageBox.Show("Open Image, Please.");
                    return false;
                }
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

        private void K_value_ValueChanged(object sender, EventArgs e)
        {
            if (Seed_Box.Text.Length <= K_value.Value)
            {
                MessageBox.Show("Tap Position Must Be Less Than The Length Of The Initial Seed.");
                K_value.Value = Seed_Box.Text.Length - 1;
            }
        }
    }
}