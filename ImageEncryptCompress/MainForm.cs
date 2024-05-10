using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox3);
        //}

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ImageMatrix = ImageOperations.Encrypt(ImageMatrix , Seed_Box.Text,(int) K_value.Value);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txtGaussSigma_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}