
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>

    public struct RGBPixel
    {
        public byte red, green, blue;
    }

    [Serializable]
    public class HuffmanNode
    {
        public byte Value;
        public int Frequency;
        public HuffmanNode Left;
        public HuffmanNode Right;

        public static void WriteTree(BinaryWriter writer, HuffmanNode node)
        {
            if (node == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            writer.Write(node.Value);
            WriteTree(writer, node.Left);
            WriteTree(writer, node.Right);
        }

        public static HuffmanNode ReadTree(BinaryReader reader)
        {
            bool isNotNull = reader.ReadBoolean();
            if (!isNotNull)
            {
                return null;
            }

            byte value = reader.ReadByte();
    
            HuffmanNode node = new HuffmanNode { Value = value };

            node.Left = ReadTree(reader);
            node.Right = ReadTree(reader);

            return node;
        }
    }


    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        /// 

        public static (HuffmanNode RootRed, HuffmanNode RootGreen, HuffmanNode RootBlue, List<byte> EncodedBytesRed, List<byte> EncodedBytesGreen, List<byte> EncodedBytesBlue) CompressImage(RGBPixel[,] image)
        {
            int[] frequenciesRed = new int[256];
            int[] frequenciesGreen = new int[256];
            int[] frequenciesBlue = new int[256];
            foreach (var pixel in image)
            {
                    frequenciesRed[pixel.red]++;
                    frequenciesGreen[pixel.green]++;
                    frequenciesBlue[pixel.blue]++;
            }

            HuffmanNode rootRed = BuildHuffmanTree(frequenciesRed);
            HuffmanNode rootGreen = BuildHuffmanTree(frequenciesGreen);
            HuffmanNode rootBlue = BuildHuffmanTree(frequenciesBlue);
            string[] huffmanCodesRed=new string[256];
            string[] huffmanCodesGreen = new string[256];
            string[] huffmanCodesBlue = new string[256];
            BuildHuffmanCodes(rootRed,"",huffmanCodesRed);
            BuildHuffmanCodes(rootGreen,"",huffmanCodesGreen);
            BuildHuffmanCodes(rootBlue,"",huffmanCodesBlue);
            List<byte> encodedBytesRed = new List<byte>();
            List<byte> encodedBytesGreen = new List<byte>();
            List<byte> encodedBytesBlue = new List<byte>();
            int countRed = 0, countGreen = 0, countBlue = 0;
            byte currentRedByte = 0, currentGreenByte = 0, currentBlueByte = 0;
            int w = 0;
            foreach (var pixel in image)
            {
                string code = huffmanCodesRed[pixel.red];
                
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentRedByte |= (byte)(1 << (7 - countRed));
                    }
                    countRed++;
                    if (countRed == 8)
                    {
                        encodedBytesRed.Add(currentRedByte);
                        currentRedByte = 0;
                        countRed = 0;
                    }

                }
              
                
                code = huffmanCodesGreen[pixel.green];
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentGreenByte |= (byte)(1 << (7 - countGreen));

                    }
                    countGreen++;
                    if (countGreen == 8)
                    {
                        encodedBytesGreen.Add(currentGreenByte);
                        currentGreenByte = 0;
                        countGreen = 0;
                    }
                }
                code = huffmanCodesBlue[pixel.blue];
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentBlueByte |= (byte)(1 << (7 - countBlue));

                    }
                    countBlue++;
                    if (countBlue == 8)
                    {

                        encodedBytesBlue.Add(currentBlueByte);
                        currentBlueByte = 0;
                        countBlue = 0;
                    }
                }
                w++;
            }

            if (countRed > 0)
                encodedBytesRed.Add(currentRedByte);
            if (countGreen > 0)
                encodedBytesGreen.Add(currentGreenByte);
            if (countBlue > 0)
                encodedBytesBlue.Add(currentBlueByte);
          
            return (rootRed, rootGreen, rootBlue, encodedBytesRed, encodedBytesGreen, encodedBytesBlue);
        }

        public static RGBPixel[,] Decompress(HuffmanNode rootRed, HuffmanNode rootGreen, HuffmanNode rootBlue, int height, int width, byte[] encodedBytesRed, byte[] encodedBytesGreen, byte[] encodedBytesBlue)
        {
            RGBPixel[,] image = new RGBPixel[height, width];
            int h = 0, w = 0;
            HuffmanNode current = rootRed;
            
            foreach (byte b in encodedBytesRed)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool bit = ((b >> i) & 1) == 1;

                    if (current.Left == null && current.Right == null)
                    {
                        image[h, w].red = current.Value;
                        w++;
                        if (w == width)
                        {
                            w = 0;
                            h++;
                            if (h == height)
                            {
                               
                                break;
                            }
                            
                        }
                        current = rootRed;
                    }
                    if (bit)
                        current = current.Right;
                    else
                        current = current.Left;
                }
              
            }

            byte bb = encodedBytesRed[encodedBytesRed.Length-1];
            for (int i = 7; i >= 0; i--)
            {
                bool bit = ((bb >> i) & 1) == 1;

                if (current.Left == null && current.Right == null)
                {
                    image[height - 1, width - 1].red = current.Value;
                
                    current = rootRed;
                    break;
                }
                if (bit)
                    current = current.Right;
                else
                    current = current.Left;
            }
 
            
       
            h = 0;
            w = 0;
            current = rootGreen;
            foreach (byte b in encodedBytesGreen)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool bit = ((b >> i) & 1) == 1;

                    if (current.Left == null && current.Right == null)
                    {
                        image[h, w].green = current.Value;

                        w++;
                        if (w == width)
                        {
                            w = 0;
                            h++;
                            if (h == height)
                                break;
                        }

                        current = rootGreen;
                    }
                    if (bit)
                        current = current.Right;
                    else
                        current = current.Left;
                }
            }
            for (int i = 7; i >= 0; i--)
            {
                bool bit = ((bb >> i) & 1) == 1;

                if (current.Left == null && current.Right == null)
                {
                    image[height - 1, width - 1].green = current.Value;

                    current = rootRed;
                    break;
                }
                if (bit)
                    current = current.Right;
                else
                    current = current.Left;
            }
    

       

            h = 0;
            w = 0;
            current = rootBlue;
            foreach (byte b in encodedBytesBlue)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool bit = ((b >> i) & 1) == 1;

                    if (current.Left == null && current.Right == null)
                    {
                        image[h, w].blue = current.Value;

                        w++;
                        if (w == width)
                        {
                            w = 0;
                            h++;
                            if (h == height)
                                break;
                        }

                        current = rootBlue;
                    }
                    if (bit)
                        current = current.Right;
                    else
                        current = current.Left;
                }
            }
            for (int i = 7; i >= 0; i--)
            {
                bool bit = ((bb >> i) & 1) == 1;

                if (current.Left == null && current.Right == null)
                {
                    image[height - 1, width - 1].blue = current.Value;

                    current = rootRed;
                    break;
                }
                if (bit)
                    current = current.Right;
                else
                    current = current.Left;
            }
        
         

            return image;
        }

        public static HuffmanNode BuildHuffmanTree(int[] frequencies)
        {

            int identifierCounter = 0;
            var priorityQueue = new SortedDictionary<(int Frequency, int Identifier), HuffmanNode>();

           for (int i = 0;i<256;i++)
            {
                if (frequencies[i] != 0)
                {
                    priorityQueue.Add((frequencies[i], identifierCounter), new HuffmanNode { Value = (byte)i, Frequency = frequencies[i] });
                    identifierCounter++;
                }
            }

            while (priorityQueue.Count > 1)
            {
              
                var firstPair = priorityQueue.First();
                priorityQueue.Remove(firstPair.Key);
                var secondPair = priorityQueue.First();
                priorityQueue.Remove(secondPair.Key);


                var merged = new HuffmanNode
                {
                    Right = firstPair.Value,
                    Left = secondPair.Value,
                    Frequency = firstPair.Value.Frequency + secondPair.Value.Frequency,
                };

                priorityQueue.Add((merged.Frequency,identifierCounter ), merged);
                identifierCounter++; 
            }

            return priorityQueue.First().Value;
        }

        private static void BuildHuffmanCodes(HuffmanNode node, string code, string[] codes)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                codes[node.Value]=code;
                return;
            }

            BuildHuffmanCodes(node.Left, code + "0", codes);
            BuildHuffmanCodes(node.Right, code + "1", codes);
        }

        public static string[] Generatekey(string init_sead, int tap_position)
        {
            char[] Initial_seed = string.Copy(init_sead).ToArray();
            char[] compelet_key = new char[8];
            int n = Initial_seed.Length;
            for (int i = 0; i < 8; i++)
            {
                bool leftBit = Initial_seed[0]-'0'==1;
                bool tapBit = Initial_seed[Initial_seed.Length - tap_position - 1]-'0'==1;

                bool key = leftBit ^ tapBit;
               for(int j = 0; j <n-1 ; j++)
                {
                    Initial_seed[j] = Initial_seed[j+1];
                }
                
                if (key)
                {
                    Initial_seed[n - 1] = '1' ;

                    compelet_key[i] =  '1' ;
                }
                else
                {
                    Initial_seed[n - 1] = '0';

                    compelet_key[i] = '0';
                }
            }
            string[] final = new string[2];
            string compelet = new string(compelet_key);
            string init = new string(Initial_seed);
            final[0] = compelet;
            final[1] = init;
            return final;

        }

        public static RGBPixel[,] Encrypt(RGBPixel[,] SourceImage, string init_seed, int tap_pos)
        {
            RGBPixel[,] Image = new RGBPixel[GetHeight(SourceImage), GetWidth(SourceImage)];
            Array.Copy(SourceImage, Image, SourceImage.Length);
            string[] seed_key = new string[2];
            string initSeed = init_seed;
            int height = GetHeight(Image);
            int width = GetWidth(Image);
            for (int i = 0; i <height ; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    seed_key = Generatekey(initSeed, tap_pos);
                    Image[i, j].red = Convert.ToByte(Image[i, j].red ^ Convert.ToByte(seed_key[0], 2));
                    initSeed = seed_key[1];

                    seed_key = Generatekey(initSeed, tap_pos);
                    Image[i, j].green = Convert.ToByte(Image[i, j].green ^ Convert.ToByte(seed_key[0], 2));
                    initSeed = seed_key[1];

                    seed_key = Generatekey(initSeed, tap_pos);
                    Image[i, j].blue = Convert.ToByte(Image[i, j].blue ^ Convert.ToByte(seed_key[0], 2));
                    initSeed = seed_key[1];
                }
            }

            //CompressImage(Image, "C:/Users/Gabesky/Downloads/OUTPUT/OUTPUT/.bin");
            return Image;
        }

        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;
                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    MainForm.ImagePixelFormat = PixelFormat.Format24bppRgb;
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    MainForm.ImagePixelFormat = PixelFormat.Format32bppRgb;
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    MainForm.ImagePixelFormat = PixelFormat.Format8bppIndexed;
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            //compress decompress save file
            //string path = File.ReadAllText("path.txt");
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //CompressImage(Buffer, path);
            //afterDecompression = Decompress(path);
            //sw.Stop();
            return Buffer;
            //return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        //public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        //{
        //    int Height = GetHeight(ImageMatrix);
        //    int Width = GetWidth(ImageMatrix);

        //    RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
        //    RGBPixel[,] Filtered = new RGBPixel[Height, Width];


        //    // Create Filter in Spatial Domain:
        //    //=================================
        //    //make the filter ODD size
        //    if (filterSize % 2 == 0) filterSize++;

        //    double[] Filter = new double[filterSize];

        //    //Compute Filter in Spatial Domain :
        //    //==================================
        //    double Sum1 = 0;
        //    int HalfSize = filterSize / 2;
        //    for (int y = -HalfSize; y <= HalfSize; y++)
        //    {
        //        //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
        //        Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
        //        Sum1 += Filter[y + HalfSize];
        //    }
        //    for (int y = -HalfSize; y <= HalfSize; y++)
        //    {
        //        Filter[y + HalfSize] /= Sum1;
        //    }

        //    //Filter Original Image Vertically:
        //    //=================================
        //    int ii, jj;
        //    RGBPixelD Sum;
        //    RGBPixel Item1;
        //    RGBPixelD Item2;

        //    for (int j = 0; j < Width; j++)
        //        for (int i = 0; i < Height; i++)
        //        {
        //            Sum.red = 0;
        //            Sum.green = 0;
        //            Sum.blue = 0;
        //            for (int y = -HalfSize; y <= HalfSize; y++)
        //            {
        //                ii = i + y;
        //                if (ii >= 0 && ii < Height)
        //                {
        //                    Item1 = ImageMatrix[ii, j];
        //                    Sum.red += Filter[y + HalfSize] * Item1.red;
        //                    Sum.green += Filter[y + HalfSize] * Item1.green;
        //                    Sum.blue += Filter[y + HalfSize] * Item1.blue;
        //                }
        //            }
        //            VerFiltered[i, j] = Sum;
        //        }

        //    //Filter Resulting Image Horizontally:
        //    //===================================
        //    for (int i = 0; i < Height; i++)
        //        for (int j = 0; j < Width; j++)
        //        {
        //            Sum.red = 0;
        //            Sum.green = 0;
        //            Sum.blue = 0;
        //            for (int x = -HalfSize; x <= HalfSize; x++)
        //            {
        //                jj = j + x;
        //                if (jj >= 0 && jj < Width)
        //                {
        //                    Item2 = VerFiltered[i, jj];
        //                    Sum.red += Filter[x + HalfSize] * Item2.red;
        //                    Sum.green += Filter[x + HalfSize] * Item2.green;
        //                    Sum.blue += Filter[x + HalfSize] * Item2.blue;
        //                }
        //            }
        //            Filtered[i, j].red = (byte)Sum.red;
        //            Filtered[i, j].green = (byte)Sum.green;
        //            Filtered[i, j].blue = (byte)Sum.blue;
        //        }

        //    return Filtered;
        //}


    }
}
