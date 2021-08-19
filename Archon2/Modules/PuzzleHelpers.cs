using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Archon2.Modules
{
    public static class PuzzleHelpers
    {
        public static (Image, int x, int y) Generate(Image input, int size, Color background)
        {
            var blank = new Image<Argb32>(input.Width, input.Height);
            blank.Mutate(b =>
            {
                var pS = input.Width / size;
                b.BackgroundColor(background)
                    .DrawImage(input, 1)
                    .Fill(Color.White, new Rectangle(pS * (size - 1), pS * (size - 1), pS, pS));
            });

            return (blank, size - 1, size - 1);
        }

        /// <param name = "direction" > 0 = Up, 1=Right, 2=Down, 3=Left</param>
        /// <returns></returns>
        public static (Image, int x, int y) Swap(Image input, int x, int y, int size, int direction)
        {
            if (y == 0 && direction == 0) { return (input, x, y); }
            if (y == size - 1 && direction == 2) { return (input, x, y); }
            if (x == 0 && direction == 3) { return (input, x, y); }
            if (x == size - 1 && direction == 1) { return (input, x, y); }

            var nX = direction == 1 ? x + 1 : direction == 3 ? x - 1 : x;
            var nY = direction == 2 ? y + 1 : direction == 0 ? y - 1 : y;

            var pS = input.Width / size;
            var i1 = input.Clone(c => { c.Crop(new Rectangle(x * pS, y * pS, pS, pS)); });
            var i2 = input.Clone(c => { c.Crop(new Rectangle(nX * pS, nY * pS, pS, pS)); });

            input.Mutate(i => { i.DrawImage(i1, new Point(nX * pS, nY * pS), 1); });
            input.Mutate(i => { i.DrawImage(i2, new Point(x * pS, y * pS), 1); });
            return (input, nX, nY);
        }
    }
}
