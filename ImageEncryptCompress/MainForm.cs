using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
            string filePath = $"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin";

            ImageMatrixAfterOperation = ImageOperations.Encrypt(OriginalImageMatrix, Seed_Box.Text, (int)K_value.Value);

            var (RedR, GreenR, BlueR, RedEB, GreeeenEB, BlueEB) = ImageOperations.CompressImage(ImageMatrixAfterOperation);

            WriteBinaryFile(filePath, RedR, GreenR, BlueR, RedEB, GreeeenEB, BlueEB);

            MessageBox.Show("Image compression completed and .bin file is saved.");

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

            string filePath = $"{pathWithoutFileName}\\{fileNameWithoutExtension}.bin";

            var (RedR, GreenR, BlueR, height, width, TapP, InitS, RedEB, GreenEB, BlueEB) = ReadBinaryFile(filePath);

            ImageMatrixAfterOperation = ImageOperations.Decompress(RedR, GreenR, BlueR, height, width, RedEB, GreenEB, BlueEB);
            ImageMatrixAfterOperation = ImageOperations.Encrypt(ImageMatrixAfterOperation, InitS, TapP);
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

        private void WriteBinaryFile(string filePath, HuffmanNode rootRed, HuffmanNode rootGreen, HuffmanNode rootBlue, List<byte> encodedBytesRed, List<byte> encodedBytesGreen, List<byte> encodedBytesBlue)
        {
            using (FileStream file = File.Open(filePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writer.BaseStream, rootRed);
                    formatter.Serialize(writer.BaseStream, rootGreen);
                    formatter.Serialize(writer.BaseStream, rootBlue);

                    writer.Write(ImageOperations.GetHeight(OriginalImageMatrix));
                    writer.Write(ImageOperations.GetWidth(OriginalImageMatrix));

                    writer.Write(Convert.ToInt32(K_value.Value));
                    writer.Write(Seed_Box.Text);

                    writer.Write(encodedBytesRed.Count);
                    writer.Write(encodedBytesRed.ToArray());

                    writer.Write(encodedBytesGreen.Count);
                    writer.Write(encodedBytesGreen.ToArray());

                    writer.Write(encodedBytesBlue.Count);
                    writer.Write(encodedBytesBlue.ToArray());

                    writer.Dispose();
                    file.Dispose();
                }
            }
        }

        private (HuffmanNode rootRed, HuffmanNode rootGreen, HuffmanNode rootBlue, int height, int width, int tab_position, string init_seed, byte[] encodedBytesRed, byte[] encodedBytesGreen, byte[] encodedBytesBlue) ReadBinaryFile(string filePath)
        {

            HuffmanNode rootRed, rootGreen, rootBlue;
            int height, width, tab_position;
            string init_seed;
            byte[] encodedBytesRed, encodedBytesGreen, encodedBytesBlue;

            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    IFormatter formatter = new BinaryFormatter();
                    rootRed = (HuffmanNode)formatter.Deserialize(file);
                    rootGreen = (HuffmanNode)formatter.Deserialize(file);
                    rootBlue = (HuffmanNode)formatter.Deserialize(file);

                    height = reader.ReadInt32();
                    width = reader.ReadInt32();

                    tab_position = reader.ReadInt32();
                    init_seed = reader.ReadString();

                    int encodedBytesRedLength = reader.ReadInt32();
                    encodedBytesRed = reader.ReadBytes(encodedBytesRedLength);

                    int encodedBytesGreenLength = reader.ReadInt32();
                    encodedBytesGreen = reader.ReadBytes(encodedBytesGreenLength);

                    int encodedBytesBlueLength = reader.ReadInt32();
                    encodedBytesBlue = reader.ReadBytes(encodedBytesBlueLength);

                    reader.Dispose();
                    file.Dispose();
                }
            }
            return (rootRed, rootGreen, rootBlue, height, width, tab_position, init_seed, encodedBytesRed, encodedBytesGreen, encodedBytesBlue);
        }

    }
}