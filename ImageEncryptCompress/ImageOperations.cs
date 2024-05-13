
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.IO.Compression;
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
        public string Identifier;
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

        public static void CompressImage(RGBPixel[,] image, string outputFilePath)
        {
          
            Dictionary<byte, int> frequenciesRed = new Dictionary<byte, int>();
            Dictionary<byte, int> frequenciesGreen = new Dictionary<byte, int>();
            Dictionary<byte, int> frequenciesBlue = new Dictionary<byte, int>();
            foreach (var pixel in image)
            {
                int value;

                if (frequenciesRed.TryGetValue(pixel.red, out value))
                    frequenciesRed[pixel.red] = value + 1;
                else
                    frequenciesRed[pixel.red] = 1;

                if (frequenciesGreen.TryGetValue(pixel.green, out value))
                    frequenciesGreen[pixel.green] = value + 1;
                else
                    frequenciesGreen[pixel.green] = 1;

                if (frequenciesBlue.TryGetValue(pixel.blue, out value))
                    frequenciesBlue[pixel.blue] = value + 1;
                else
                    frequenciesBlue[pixel.blue] = 1;
            }
         
            HuffmanNode rootRed = BuildHuffmanTree(frequenciesRed);
            HuffmanNode rootGreen = BuildHuffmanTree(frequenciesGreen);
            HuffmanNode rootBlue = BuildHuffmanTree(frequenciesBlue);
            Dictionary<byte, string> huffmanCodesRed = BuildHuffmanCodes(rootRed);
            Dictionary<byte, string> huffmanCodesGreen = BuildHuffmanCodes(rootGreen);
            Dictionary<byte, string> huffmanCodesBlue = BuildHuffmanCodes(rootBlue);
            int width = GetWidth(image);
            int height = GetHeight(image);
            int w = 0;
            List<byte> encodedBytes = new List<byte>();
            byte currentByte=0;
            int count = 0;
           
            foreach (var pixel in image)
            {
                string code = huffmanCodesRed[pixel.red];
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentByte |= (byte)(1 << (7 - count));
                    }
                    count++;
                    if (count == 8)
                    {
                        encodedBytes.Add(currentByte);
                        currentByte = 0;
                        count = 0;
                    }

                }
                code = huffmanCodesGreen[pixel.green];
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentByte |= (byte)(1 << (7 - count));

                    }
                    count++;
                    if (count == 8)
                    {
                        encodedBytes.Add(currentByte);
                        currentByte = 0;
                        count = 0;
                    }
                }
                code = huffmanCodesBlue[pixel.blue];
                foreach (char bit in code)
                {
                    if (bit == '1')
                    {
                        currentByte |= (byte)(1 << (7 - count));

                    }
                    count++;
                    if (count == 8)
                    {

                        encodedBytes.Add(currentByte);
                        currentByte = 0;
                        count = 0;
                    }
                }
                w++;
            }

            if (currentByte != 0)
            {
                encodedBytes.Add(currentByte);
            }
       
            using (FileStream file = File.Open(outputFilePath, FileMode.Create))
            {
               
                    using (BinaryWriter writer = new BinaryWriter(file))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(writer.BaseStream, rootRed);
                        formatter.Serialize(writer.BaseStream, rootGreen);
                        formatter.Serialize(writer.BaseStream, rootBlue);
                        writer.Write(GetHeight(image));
                        writer.Write(GetWidth(image));
                        writer.Write(encodedBytes.Count);
                        writer.Write(encodedBytes.ToArray());
                    }
                
            }

            MessageBox.Show("Image compression completed and .bin file is saved.");
        }

        public static RGBPixel[,] Decompress(string path)
        {
            HuffmanNode rootRed, rootGreen, rootBlue;
            int width, height;
            byte[] encodedBytes, encodedBytesGreen, encodedBytesBlue;

            using (FileStream file = File.Open(path, FileMode.Open))
            {
              
                    using (BinaryReader reader = new BinaryReader(file))
                    {
                        // Deserialize Huffman tree
                        IFormatter formatter = new BinaryFormatter();
                        rootRed = (HuffmanNode)formatter.Deserialize(file);
                        rootGreen = (HuffmanNode)formatter.Deserialize(file);
                        rootBlue = (HuffmanNode)formatter.Deserialize(file);

                        // Read height and width
                        height = reader.ReadInt32();
                        width = reader.ReadInt32();
                        int encodedBytesLength = reader.ReadInt32();
                        encodedBytes = reader.ReadBytes(encodedBytesLength);
                    }
                
            }

            RGBPixel[,] image = new RGBPixel[height, width];
            //Dictionary<string, byte> huffmanCodes = ReBuildHuffmanCodes(root);
            // Decode Huffman-encoded bits and reconstruct image pixels
            int h = 0, w = 0;
            HuffmanNode current = rootRed;
            int count = 0;
            foreach (byte b in encodedBytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool bit = ((b >> i) & 1) == 1;

                    if (current.Left == null && current.Right == null)
                    {
                        if (count % 3 == 0)
                        {
                            image[h, w].red = current.Value;

                        }
                        else if (count % 3 == 1)
                        {
                            image[h, w].green = current.Value;
                        }
                        else
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
                        }

                       
                        count++;
                        if (count % 3 == 0)
                        {
                            current = rootRed;
                        }
                      
                       else if (count % 3 == 1)
                        {
                            current = rootGreen;
                        }
                        
                        else 
                        {
                            current = rootBlue;
                        }
                    }
                    if (bit)
                        current = current.Right;
                    else
                        current = current.Left;
                }
            }

            return image;
        }

        public static Dictionary<string, byte> ReBuildHuffmanCodes(HuffmanNode root)
        {
            Dictionary<string, byte> codes = new Dictionary<string, byte>();
            ReBuildHuffmanCodesRecursive(root, "", codes);
            return codes;
        }

        private static void ReBuildHuffmanCodesRecursive(HuffmanNode node, string code, Dictionary<string, byte> codes)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                codes.Add(code, node.Value);
                return;
            }

            ReBuildHuffmanCodesRecursive(node.Left, code + "0", codes);
            ReBuildHuffmanCodesRecursive(node.Right, code + "1", codes);
        }

        public static HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencies)
        {
            int identifierCounter = 0;
            var priorityQueue = new SortedDictionary<(int Frequency, string Identifier), HuffmanNode>();

            foreach (var kvp in frequencies)
            {
                priorityQueue.Add((kvp.Value, identifierCounter.ToString()), new HuffmanNode { Value = kvp.Key, Frequency = kvp.Value, Identifier = identifierCounter.ToString() });
                identifierCounter++; // Increment the counter for the next identifier
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
                    Identifier = identifierCounter.ToString()
                };

                priorityQueue.Add((merged.Frequency, merged.Identifier), merged);
                identifierCounter++; // Increment the counter for the next identifier
            }

            return priorityQueue.First().Value;
        }

        public static Dictionary<byte, string> BuildHuffmanCodes(HuffmanNode root)
        {
            var codes = new Dictionary<byte, string>();
            BuildHuffmanCodesRecursive(root, "", codes);
            return codes;
        }

        private static void BuildHuffmanCodesRecursive(HuffmanNode node, string code, Dictionary<byte, string> codes)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                codes.Add(node.Value, code);
                return;
            }

            BuildHuffmanCodesRecursive(node.Left, code + "0", codes);
            BuildHuffmanCodesRecursive(node.Right, code + "1", codes);
        }

        public static string[] Generatekey(string init_sead, int tap_position)
        {
            string Initial_seed = string.Copy(init_sead);

            string compelet_key = null;

            for (int i = 0; i < 8; i++)
            {


                char leftBit = Initial_seed[0];
                char tapBit = Initial_seed[Initial_seed.Length - tap_position - 1];
                string key = Convert.ToString(leftBit ^ tapBit);

                Initial_seed = Initial_seed.Substring(1) + key;
                compelet_key = compelet_key + key;

            }
            string[] final = new string[2];
            final[0] = compelet_key;
            final[1] = Initial_seed;
            return final;

        }

        public static RGBPixel[,] Encrypt(RGBPixel[,] SourceImage, string init_seed, int tap_pos)
        {
            
            RGBPixel[,] Image = new RGBPixel[GetHeight(SourceImage), GetWidth(SourceImage)];
            Array.Copy(SourceImage, Image, SourceImage.Length);
            string[] seed_key = new string[2];
            string initSeed = init_seed;

            for (int i = 0; i < GetHeight(Image); i++)
            {
                for (int j = 0; j < GetWidth(Image); j++)
                {
                    seed_key = Generatekey(initSeed, tap_pos);

                    Image[i, j].red = Convert.ToByte(Image[i, j].red ^ Convert.ToInt32(seed_key[0], 2));

                    initSeed = seed_key[1];
                    seed_key = Generatekey(initSeed, tap_pos);

                    Image[i, j].green = Convert.ToByte(Image[i, j].green ^ Convert.ToInt32(seed_key[0], 2));

                    initSeed = seed_key[1];
                    seed_key = Generatekey(initSeed, tap_pos);

                    Image[i, j].blue = Convert.ToByte(Image[i, j].blue ^ Convert.ToInt32(seed_key[0], 2));

                    initSeed = seed_key[1];

                    //int blue_key = Generatekey(copy, tap_pos) ^ Image[i, j].blue;
                    //Image[i, j].blue = Convert.ToByte(blue_key);

                    //int green_key = Generatekey(copy, tap_pos) ^ Image[i, j].green;
                    //Image[i, j].green = Convert.ToByte(green_key);

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
           /* CompressImage(Buffer, "D://study//algo/.bin");*/
            //afterDecompression = Decompress(path);
            //sw.Stop();
           // return Decompress("D://study//algo/.bin");
            return Buffer;
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
