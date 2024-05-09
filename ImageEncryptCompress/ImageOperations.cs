using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
    public class HuffmanNode
    {
        public int Value { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }
        public string Identifier { get; set; } // Unique identifier for handling duplicates
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
        public static RGBPixel[,] DecompressImage(string inputFilePath, int imageWidth, int imageHeight)
        {
            // Read the compressed data from the input file
            List<bool> encodedBits = new List<bool>();
            using (FileStream input = new FileStream(inputFilePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(input))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        foreach (byte b in buffer)
                        {
                            for (int i = 7; i >= 0; i--)
                            {
                                encodedBits.Add((b & (1 << i)) != 0);
                            }
                        }
                    }
                }
            }

            // Rebuild Huffman tree
            HuffmanNode root = RebuildHuffmanTree(encodedBits);

            // Decode the encoded bits using the Huffman tree
            List<int> pixelValues = DecodeHuffmanEncodedData(encodedBits, root);

            // Convert pixel values back to RGBPixel array
            RGBPixel[,] image = new RGBPixel[imageWidth, imageHeight];
            int pixelIndex = 0;
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    int pixelValue = pixelValues[pixelIndex++];
                    image[i, j] = new RGBPixel
                    {
                        red = (byte)((pixelValue >> 16) & 0xFF),
                        green = (byte)((pixelValue >> 8) & 0xFF),
                        blue = (byte)(pixelValue & 0xFF)
                    };
                }
            }

            return image;
        }
        private static HuffmanNode RebuildHuffmanTree(List<bool> encodedBits)
        {
            // Initialize root node
            HuffmanNode root = new HuffmanNode();
            HuffmanNode currentNode = root;

            // Traverse the encoded bits to build the tree
            foreach (bool bit in encodedBits)
            {
                if (bit)
                {
                    if (currentNode.Right == null)
                        currentNode.Right = new HuffmanNode();
                    currentNode = currentNode.Right;
                }
                else
                {
                    if (currentNode.Left == null)
                        currentNode.Left = new HuffmanNode();
                    currentNode = currentNode.Left;
                }
            }

            // Assign a dummy value to all leaf nodes
            AssignValuesToLeafNodes(root);

            return root;
        }

        // Assign a dummy value to all leaf nodes
        private static void AssignValuesToLeafNodes(HuffmanNode node)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
                node.Value = 1; // Dummy value indicating a leaf node

            AssignValuesToLeafNodes(node.Left);
            AssignValuesToLeafNodes(node.Right);
        }
        private static List<int> DecodeHuffmanEncodedData(List<bool> encodedBits, HuffmanNode root)
        {
            List<int> decodedData = new List<int>();
            HuffmanNode currentNode = root;

            // Traverse the Huffman tree to decode the encoded bits
            foreach (bool bit in encodedBits)
            {
                currentNode = bit ? currentNode.Right : currentNode.Left;

                if (currentNode.Left == null && currentNode.Right == null)
                {
                    // Reached a leaf node, add its value to the decoded data
                    decodedData.Add(currentNode.Value);
                    currentNode = root; // Reset to the root for the next iteration
                }
            }

            return decodedData;
        }
        public static HuffmanNode BuildHuffmanTree(Dictionary<int, int> frequencies)
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
                    Left = firstPair.Value,
                    Right = secondPair.Value,
                    Frequency = firstPair.Value.Frequency + secondPair.Value.Frequency,
                    Identifier = identifierCounter.ToString()
                };

                priorityQueue.Add((merged.Frequency, merged.Identifier), merged);
                identifierCounter++; // Increment the counter for the next identifier
            }

            return priorityQueue.First().Value;
        }
        public static Dictionary<int, string> BuildHuffmanCodes(HuffmanNode root)
        {
            var codes = new Dictionary<int, string>();
            BuildHuffmanCodesRecursive(root, "", codes);
            return codes;
        }

        private static void BuildHuffmanCodesRecursive(HuffmanNode node, string code, Dictionary<int, string> codes)
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
        public static void CompressImage(RGBPixel[,] image, string outputFilePath)
        {
            Dictionary<int, int> frequencies = new Dictionary<int, int>();
            for (int i = 0; i < 3; i++)
            {


                foreach (var pixel in image)
                {

                    int pixelValue;
                    if (i == 0)
                    {
                      pixelValue=  pixel.red;
                    }
                    else if(i==1){
                        pixelValue = pixel.green;
                    }
                    else
                    {
                        pixelValue = pixel.blue;
                    }
                    if (frequencies.ContainsKey(pixelValue))
                        frequencies[pixelValue]++;
                    else
                        frequencies[pixelValue] = 1;
                }
            }

            HuffmanNode root =BuildHuffmanTree(frequencies);

           
            Dictionary<int, string> huffmanCodes = BuildHuffmanCodes(root);

            
            List<bool> encodedBits = new List<bool>();
            for (int i = 0; i < 3; i++)
            {
                foreach (var pixel in image)
                {
                    int pixelValue;
                    if (i == 0)
                    {
                        pixelValue = pixel.red;
                    }
                    else if (i == 1)
                    {
                        pixelValue = pixel.green;
                    }
                    else
                    {
                        pixelValue = pixel.blue;
                    }
                    string code = huffmanCodes[pixelValue];
                    foreach (char bit in code)
                    {
                        encodedBits.Add(bit == '1');
                    }
                }
            }

            // Convert bits to bytes
            List<byte> encodedBytes = new List<byte>();
            for (int i = 0; i < encodedBits.Count; i += 8)
            {
                byte currentByte = 0;
                for (int j = 0; j < 8 && i + j < encodedBits.Count; j++)
                {
                    if (encodedBits[i + j])
                    {
                        currentByte |= (byte)(1 << (7 - j));
                    }
                }
                encodedBytes.Add(currentByte);
            }

            // Write compressed data to the output file
            using (FileStream output = new FileStream(outputFilePath, FileMode.Create))
            {
                output.Write(encodedBytes.ToArray(), 0, encodedBytes.Count);
            }

            Console.WriteLine("Image compression completed.");
        
    }
        static string Generatekey(string init_sead, int tap_position, int k)
        {
            string Initial_seed = string.Copy(init_sead);
            while (k > 0)
            {
                k--;

                char leftBit = Initial_seed[0];
                char tapBit = Initial_seed[Initial_seed.Length - 1 - tap_position];
                Initial_seed = Initial_seed.Insert(Initial_seed.Length, (leftBit ^ tapBit).ToString());
                Initial_seed = Initial_seed.Remove(0, 1);
            }
            return Initial_seed;
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
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
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
            CompressImage(Buffer, "D://study//algo/.bin");
     /*      return DecompressImage("D://study//algo/compressedImage.bin", Width, Height);*/
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
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }
}
