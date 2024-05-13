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
        public static PixelFormat ImagePixelFormat;
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
                MessageBox.Show("File opened successfully.");
            }
        }

        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox3);
        //}

        private void SaveFileButton_Click(object sender, EventArgs e)
        {
            if (ImageMatrixAfterOperation == null)
            {
                MessageBox.Show("Make operation, please.");
                return;
            }

            int width = ImageOperations.GetWidth(ImageMatrixAfterOperation);
            int height = ImageOperations.GetHeight(ImageMatrixAfterOperation);

            using (Bitmap bitmap = new Bitmap(width, height, ImagePixelFormat))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                try
                {
                    unsafe
                    {
                        byte* ptr = (byte*)bmpData.Scan0;

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                RGBPixel pixel = ImageMatrixAfterOperation[y, x];
                                int bytesPerPixel = Image.GetPixelFormatSize(ImagePixelFormat) / 8;
                                int index = y * bmpData.Stride + x * bytesPerPixel;
                                ptr[index] = pixel.blue;
                                if (bytesPerPixel >= 3)
                                {
                                    ptr[index + 1] = pixel.green;
                                    ptr[index + 2] = pixel.red;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        MessageBox.Show("File already exists. Please enter a non-existing file name.");
                        return;
                    }

                    bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                    MessageBox.Show("Image saved successfully.");
                }
            }
        }

        private void EncryptDecryptButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Encrypt(OriginalImageMatrix, Seed_Box.Text, (int)K_value.Value);
            sw.Stop();
            TimeSpan timeSpan = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
            EncryptDecryptTime.Text = timeSpan.ToString(@"hh\:mm\:ss\.ff");
            ImageOperations.DisplayImage(ImageMatrixAfterOperation, pictureBox2);
        }

        private void EncryptCompressButton_Click(object sender, EventArgs e)
        {
            if (!validateInputs())
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Encrypt(OriginalImageMatrix, Seed_Box.Text, (int)K_value.Value);
            ImageOperations.CompressImage(OriginalImageMatrix, $"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin");
            sw.Stop();
            TimeSpan timeSpan = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
            EncryptionCompressionTime.Text = timeSpan.ToString(@"hh\:mm\:ss\.ff");
            ImageOperations.DisplayImage(ImageMatrixAfterOperation, pictureBox2);
        }

        private void DecryptDecompressButton_Click(object sender, EventArgs e)
        {
            if (fileExtension != ".bin")
            {
                MessageBox.Show("Enter .bin file!");
                return;
            }

            MessageBox.Show("Entered biniary initial seed and tap position will be ignored!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ImageMatrixAfterOperation = ImageOperations.Decompress($"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin");
            ImageMatrixAfterOperation = ImageOperations.Encrypt(ImageMatrixAfterOperation, Seed_Box.Text, (int)K_value.Value);
            sw.Stop();
            TimeSpan timeSpan = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
            DecryptionDecompressionTime.Text = timeSpan.ToString(@"hh\:mm\:ss\.ff");
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
                MessageBox.Show("Tap Position Must Be Less Than The Length Of The Initial Seed.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try
                {
                    K_value.Value = Seed_Box.Text.Length - 1;
                }
                catch { K_value.Value = 0; }
            }
        }


    }
}