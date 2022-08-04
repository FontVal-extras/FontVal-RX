// Copyright (c) Hin-Tak Leung

// All rights reserved.

// MIT License

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the ""Software""), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Text;
using OTFontFile;
using OTFontFile.Rasterizer;

namespace Compat
{
    public class RX
    {
        private static int verbose;

        static int Main( string[] args )
        {
            if (args.Length == 0) {
                Console.WriteLine("RX fontfile");
                return 0;
            }

            OTFile f = new OTFile();
            string filename = null;
            verbose = 0;

            for ( int i = 0; i < args.Length; i++ ) {
                if ( "-v" == args[i] )
                    verbose++;
                else
                    filename = args[i];
            }

            if ( !f.open(filename) )
            {
                    Console.WriteLine("Error: Cannot open {0} as font file", filename);
                    return 0;
            }

            if ( f.GetNumFonts() != 1 )
                Console.WriteLine("{0} contains {1} member fonts", filename, f.GetNumFonts() );

            RasterInterf2 ri = new RasterInterf2();
            for (uint iFont = 0; iFont < f.GetNumFonts() ; iFont++)
            {
                OTFont fn = f.GetFont(iFont);
                ri.RasterNewSfnt(fn.GetFile().GetFileStream(), fn.GetFontIndexInFile());
            }
            return 0;
        }
    }
}
