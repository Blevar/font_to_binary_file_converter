using System;
using System.Drawing;
using System.IO;

class BitmapGenerator
{
    static void Main()
    {
        string[] fontTypes = { "Arial", "Times New Roman", "Courier New", "Verdana", "Tahoma", "Calibri", "Georgia", "Impact", "Comic Sans MS", "Trebuchet MS", "OCR-A", "Lucida Console", "Cyberpunk", "Major Mono Display", "Roboto Mono", "Digital dream", "Consolas", "Sankofa Display" };
        int maxBitmapWidth = 32;
        int maxBitmapHeight = 128;

        foreach (string fontType in fontTypes)
        {
            string fontFolder = fontType.Replace(" ", "_");
            if (!Directory.Exists(fontFolder))
            {
                Directory.CreateDirectory(fontFolder);
            }

            int baseFontSize = CalculateMaxFontSizeForAllDigits(fontType, maxBitmapWidth, maxBitmapHeight);
            int fontSize = baseFontSize;

            // Adjust font size until left and right padding columns are minimal
            fontSize = AdjustFontSizeUntilPaddingIsMinimal(6, fontType, fontSize, maxBitmapWidth, maxBitmapHeight);

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Current font size: {fontSize}");
                GenerateAndDisplayBitmapForDigit(fontType, fontSize, maxBitmapWidth, maxBitmapHeight);
                Console.WriteLine("Press +/- to adjust size or Enter to accept this size.");

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key == ConsoleKey.OemPlus)
                {
                    fontSize++;
                    fontSize = AdjustFontSizeUntilPaddingIsMinimal(6, fontType, fontSize, maxBitmapWidth, maxBitmapHeight);
                }
                else if (key == ConsoleKey.OemMinus)
                {
                    fontSize = Math.Max(1, fontSize - 1);
                    fontSize = AdjustFontSizeUntilPaddingIsMinimal(6, fontType, fontSize, maxBitmapWidth, maxBitmapHeight);
                }
            }

            for (int i = 0; i <= 9; i++)
            {
                GenerateBitmapForDigit(i, fontType, fontSize, maxBitmapWidth, maxBitmapHeight, fontFolder);
            }
            GenerateBitmapForColon(fontType, fontSize, maxBitmapWidth, maxBitmapHeight, fontFolder);
        }
    }

    static int CalculateMaxFontSizeForAllDigits(string fontType, int maxWidth, int maxHeight)
    {
        int maxFontSize = 1;
        using (Bitmap tempBitmap = new Bitmap(maxWidth, maxHeight))
        using (Graphics g = Graphics.FromImage(tempBitmap))
        {
            foreach (char digit in "0123456789")
            {
                int fontSize = 1;
                while (true)
                {
                    Font font = new Font(fontType, fontSize, FontStyle.Bold);
                    SizeF textSize = g.MeasureString(digit.ToString(), font);
                    if (textSize.Width > maxWidth || textSize.Height > maxHeight)
                    {
                        break;
                    }
                    fontSize++;
                }
                maxFontSize = Math.Max(maxFontSize, fontSize - 1);
            }
        }
        return maxFontSize;
    }

    static int AdjustFontSizeUntilPaddingIsMinimal(int digit, string fontType, int fontSize, int maxBitmapWidth, int maxBitmapHeight)
    {
        while (true)
        {
            Bitmap bitmap = GenerateBitmapForDigitPreview(digit, fontType, fontSize, maxBitmapWidth, maxBitmapHeight);
            (int leftPadding, int rightPadding) = CalculatePadding(bitmap);

            if (leftPadding == 0 && rightPadding == 0)
            {
                bitmap.Dispose();
                break;
            }

            fontSize++;
            bitmap.Dispose();
        }
        return fontSize;
    }

    static Bitmap GenerateBitmapForDigitPreview(int digit, string fontType, int fontSize, int maxBitmapWidth, int maxBitmapHeight)
    {
        Bitmap bitmap = new Bitmap(maxBitmapWidth, maxBitmapHeight);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Black);
            Font font = new Font(fontType, fontSize, FontStyle.Bold);
            SizeF textSize = g.MeasureString(digit.ToString(), font);
            float x = (bitmap.Width - textSize.Width) / 2;
            float y = (bitmap.Height - textSize.Height) / 2;
            g.DrawString(digit.ToString(), font, Brushes.White, new PointF(x, y));
        }

        return CenterBitmapByRemovingColumns(bitmap);
    }

    static (int leftPadding, int rightPadding) CalculatePadding(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        int leftZeroColumns = 0;
        int rightZeroColumns = 0;

        // Find left zero columns
        for (int x = 0; x < width; x++)
        {
            bool hasNonZeroPixel = false;
            for (int y = 0; y < height; y++)
            {
                if (bitmap.GetPixel(x, y).R == 255)
                {
                    hasNonZeroPixel = true;
                    break;
                }
            }
            if (hasNonZeroPixel)
                break;
            leftZeroColumns++;
        }

        // Find right zero columns
        for (int x = width - 1; x >= 0; x--)
        {
            bool hasNonZeroPixel = false;
            for (int y = 0; y < height; y++)
            {
                if (bitmap.GetPixel(x, y).R == 255)
                {
                    hasNonZeroPixel = true;
                    break;
                }
            }
            if (hasNonZeroPixel)
                break;
            rightZeroColumns++;
        }

        return (leftZeroColumns, rightZeroColumns);
    }

    static void GenerateAndDisplayBitmapForDigit(string fontType, int initialFontSize, int maxBitmapWidth, int maxBitmapHeight)
    {
        int fontSize = initialFontSize;
        while (true)
        {
            Bitmap originalBitmap = new Bitmap(maxBitmapWidth, maxBitmapHeight);
            using (Graphics g = Graphics.FromImage(originalBitmap))
            {
                g.Clear(Color.Black);
                Font font = new Font(fontType, fontSize, FontStyle.Bold);
                SizeF textSize = g.MeasureString("6", font);
                float x = (originalBitmap.Width - textSize.Width) / 2;
                float y = (originalBitmap.Height - textSize.Height) / 2;
                g.DrawString("6", font, Brushes.White, new PointF(x, y));
            }

            Bitmap adjustedBitmap = CenterBitmapByRemovingColumns(originalBitmap);
            DisplayBitmapInConsole(adjustedBitmap);

            Console.WriteLine($"Current font size: {fontSize}");
            Console.WriteLine("Press +/- to adjust size or Enter to accept this size.");

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Enter)
            {
                break;
            }
            else if (key == ConsoleKey.OemPlus)
            {
                fontSize++;
            }
            else if (key == ConsoleKey.OemMinus)
            {
                fontSize = Math.Max(1, fontSize - 1);
            }
        }
    }

    static Bitmap CenterBitmapByRemovingColumns(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        int leftZeroColumns = 0;
        int rightZeroColumns = 0;

        // Find left zero columns
        for (int x = 0; x < width; x++)
        {
            bool hasNonZeroPixel = false;
            for (int y = 0; y < height; y++)
            {
                if (bitmap.GetPixel(x, y).R == 255)
                {
                    hasNonZeroPixel = true;
                    break;
                }
            }
            if (hasNonZeroPixel)
                break;
            leftZeroColumns++;
        }

        // Find right zero columns
        for (int x = width - 1; x >= 0; x--)
        {
            bool hasNonZeroPixel = false;
            for (int y = 0; y < height; y++)
            {
                if (bitmap.GetPixel(x, y).R == 255)
                {
                    hasNonZeroPixel = true;
                    break;
                }
            }
            if (hasNonZeroPixel)
                break;
            rightZeroColumns++;
        }

        // Adjust columns if difference is greater than 1
        while (Math.Abs(leftZeroColumns - rightZeroColumns) > 1)
        {
            if (leftZeroColumns > rightZeroColumns)
            {
                // Remove one column from the left
                leftZeroColumns--;
                Bitmap newBitmap = new Bitmap(width - 1, height);
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(Color.Black);
                    g.DrawImage(bitmap, -1, 0);
                }
                bitmap = newBitmap;
                width--;
            }
            else
            {
                // Remove one column from the right
                rightZeroColumns--;
                Bitmap newBitmap = new Bitmap(width - 1, height);
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(Color.Black);
                    g.DrawImage(bitmap, 0, 0);
                }
                bitmap = newBitmap;
                width--;
            }

            // Recalculate zero columns
            (leftZeroColumns, rightZeroColumns) = CalculatePadding(bitmap);
        }

        return bitmap;
    }

    static void DisplayBitmapInConsole(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            string row = "";
            for (int x = 0; x < bitmap.Width; x++)
            {
                row += (bitmap.GetPixel(x, y).R == 255) ? "1" : "0";
            }
            if (row.Contains("1"))  // Only display rows that contain at least one "1"
            {
                Console.WriteLine(row);
            }
        }
    }

    static void GenerateBitmapForDigit(int digit, string fontType, int fontSize, int maxBitmapWidth, int maxBitmapHeight, string folder)
    {
        Bitmap originalBitmap = new Bitmap(maxBitmapWidth, maxBitmapHeight);
        using (Graphics g = Graphics.FromImage(originalBitmap))
        {
            g.Clear(Color.Black);
            Font font = new Font(fontType, fontSize, FontStyle.Bold);
            SizeF textSize = g.MeasureString(digit.ToString(), font);
            float x = (originalBitmap.Width - textSize.Width) / 2;
            float y = (originalBitmap.Height - textSize.Height) / 2;
            g.DrawString(digit.ToString(), font, Brushes.White, new PointF(x, y));
        }

        Bitmap adjustedBitmap = CenterBitmapByRemovingColumns(originalBitmap);
        Bitmap scaledBitmap = ScaleBitmapToFill(adjustedBitmap, maxBitmapWidth, maxBitmapHeight);
        Bitmap rotatedBitmap = RotateBitmap(scaledBitmap);
        SaveBitmapAsBinary(rotatedBitmap, Path.Combine(folder, $"digit_{digit}_128x32.bin"));
    }

    static void GenerateBitmapForColon(string fontType, int fontSize, int maxBitmapWidth, int maxBitmapHeight, string folder)
    {
        Bitmap originalBitmap = new Bitmap(maxBitmapWidth, maxBitmapHeight);
        using (Graphics g = Graphics.FromImage(originalBitmap))
        {
            g.Clear(Color.Black);
            Font font = new Font(fontType, fontSize, FontStyle.Bold);
            SizeF textSize = g.MeasureString(":", font);
            float x = (originalBitmap.Width - textSize.Width) / 2;
            float y = (originalBitmap.Height - textSize.Height) / 2;
            g.DrawString(":", font, Brushes.White, new PointF(x, y));
        }

        Bitmap adjustedBitmap = CenterBitmapByRemovingColumns(originalBitmap);
        Bitmap scaledBitmap = ScaleBitmapToFill(adjustedBitmap, maxBitmapWidth, maxBitmapHeight);
        Bitmap rotatedBitmap = RotateBitmap(scaledBitmap);
        SaveBitmapAsBinary(rotatedBitmap, Path.Combine(folder, "colon_128x32.bin"));
    }

    static Bitmap ScaleBitmapToFill(Bitmap original, int targetWidth, int targetHeight)
    {
        // Calculate scale ratios
        float widthRatio = (float)targetWidth / original.Width;
        float heightRatio = (float)targetHeight / original.Height;
        float scaleRatio = Math.Max(widthRatio, heightRatio);

        // Calculate new dimensions
        int newWidth = (int)(original.Width * scaleRatio);
        int newHeight = (int)(original.Height * scaleRatio);

        // Create a new bitmap with scaled size
        Bitmap scaled = new Bitmap(targetWidth, targetHeight);
        using (Graphics g = Graphics.FromImage(scaled))
        {
            g.Clear(Color.Black);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, (targetWidth - newWidth) / 2, (targetHeight - newHeight) / 2, newWidth, newHeight);
        }
        return scaled;
    }

    static Bitmap RotateBitmap(Bitmap original)
    {
        Bitmap rotated = new Bitmap(original.Height, original.Width);
        for (int x = 0; x < original.Width; x++)
        {
            for (int y = 0; y < original.Height; y++)
            {
                rotated.SetPixel(original.Height - 1 - y, x, original.GetPixel(x, y));
            }
        }
        return rotated;
    }

    static void SaveBitmapAsBinary(Bitmap bitmap, string fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x += 8)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (x + bit < bitmap.Width && bitmap.GetPixel(x + bit, y).R == 255)
                        {
                            b |= (byte)(1 << (7 - bit));
                        }
                    }
                    fs.WriteByte(b);
                }
            }
        }
    }
}
