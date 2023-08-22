using Buffer;
using System.Drawing;
using System.Drawing.Imaging;

const int width = 200, height = 100;
const bool useOnlyChars = false;
const int frameDelayMS = 50;

const string AsciiChars = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@";

ConsoleBuffer buff = new ConsoleBuffer(width,height);

//FileStream gifStream = File.OpenRead("cat-space.gif");
FileStream gifStream = File.OpenRead("bob.gif");
//FileStream gifStream = File.OpenRead("horse.gif");

List<Bitmap> gif = new List<Bitmap>(GetImages(gifStream));


//resize to fit console size
for (int i = 0; i < gif.Count; i++)
{
    gif[i] = ResizeBitmap(gif[i], width, height);
}

int curFrame = 0;
while (true)
{
    curFrame = ++curFrame >= gif.Count ? 0 : curFrame;

    for (int i = 0; i < gif[curFrame].Width; i++)
    {
        for (int j = 0; j < gif[curFrame].Height; j++)
        {
            (char ch, short color) pixel = PixelToAscii(gif[curFrame].GetPixel(i, j), useOnlyChars);
            buff.WriteToBuff(i, j, pixel.ch, pixel.color);
        }
    }
    buff.Swap();
    Thread.Sleep(frameDelayMS);
}

#region Utils

IEnumerable<Bitmap> GetImages(Stream stream)
{
    using (var gifImage = Image.FromStream(stream))
    {
        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        var frameCount = gifImage.GetFrameCount(dimension);
        for (var index = 0; index < frameCount; index++)
        {
            gifImage.SelectActiveFrame(dimension, index);
            yield return (Bitmap)gifImage.Clone();
        }
    }
}

Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
{
    Bitmap result = new Bitmap(width, height);
    using (Graphics g = Graphics.FromImage(result))
    {
        g.DrawImage(bmp, 0, 0, width, height);
    }
    return result;
}

(char ch, short color) PixelToAscii(Color pixel, bool charOnly)
{
    int avg = (pixel.R + pixel.G + pixel.B) / 3;
    int charIndex = Map(avg, 0, 255, AsciiChars.Length-1, 0);
    char ch = AsciiChars[charIndex];

    int index = (pixel.R > 128 | pixel.G > 128 | pixel.B > 128) ? 8 : 0; // Bright bit
    index |= (pixel.R > 64) ? 4 : 0; // Red bit
    index |= (pixel.G > 64) ? 2 : 0; // Green bit
    index |= (pixel.B > 64) ? 1 : 0; // Blue bit
    return (ch, (short)(charOnly ? index:(index * 16 + index)));
}

int Map(int x, int in_min, int in_max, int out_min, int out_max)
{
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

#endregion