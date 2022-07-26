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
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

#if USING_COMPAT_OTFontFile_Rasterizer
using TrueType = Avalon.Media.Text.TrueType;
#endif

namespace OTFontFile.Rasterizer
{
    public class RasterInterf2 : RasterInterf
    {
        /* shadowing RasterInterf.RastTest */
        public new bool RastTest (int resX, int resY, int[] arrPointSizes,
                                  float stretchX, float stretchY,
                                  float rotation, float skew,
                                  float[,] matrix,
                                  bool setBW, bool setGrayscale, bool setCleartype, uint CTFlags,
                                  RastTestErrorDelegate pRastTestErrorDelegate,
                                  UpdateProgressDelegate pUpdateProgressDelegate)
        {
            return true;
        }
        /* shadowing RasterInterf.CalcDevMetrics */
        /*
        public new DevMetricsData CalcDevMetrics (int Huge_calcHDMX, int Huge_calcLTSH, int Huge_calcVDMX,
                                                  ushort numGlyphs,
                                                  byte[] phdmxPointSizes, ushort maxHdmxPointSize,
                                                  byte uchPixelHeightRangeStart, byte uchPixelHeightRangeEnd,
                                                  ushort[] pVDMXxResolution, ushort[] pVDMXyResolution,
                                                  ushort cVDMXResolutions, UpdateProgressDelegate pUpdateProgressDelegate)
        {
            this.m_bCancelCalcDevMetrics = false;
            DevMetricsData data = new DevMetricsData {
                hdmxData = null,
                ltshData = null,
                vdmxData = null
            };
            return null;
        }
        */
        /* shadowing RasterInterf.RenderSize() */
        public new bool RenderSize(int resX, int resY, int nPointSize, float stretchX, float stretchY, float rotation, float skew, float[,] matrix)
        {
            return true;
        }
        /* shadowing RasterInterf.CheckPointDeltaAndRenderOutline() */
        /*
        public unsafe new bool CheckPointDeltaAndRenderOutline(void modopt(CallConvCdecl) *(fnt_LocalGraphicStateType*, byte*) traceFunc, ushort nGlyphID)
        {
            return true;
        }
        */
#if USING_COMPAT_OTFontFile_Rasterizer
        private static RasterInterf _Rasterizer;
        private static TrueType.RasterInterf m_Rasterizer;
        private DevMetricsData m_DevMetricsData;
        private bool m_UserCancelledTest = false;

        public delegate void RastTestErrorDelegate (string sStringName, string sDetails);

        public delegate void UpdateProgressDelegate (string s);

        private RasterInterf ()
        {
                m_Rasterizer = new TrueType.RasterInterf();
        }

        static public RasterInterf getInstance()
        {
            if (_Rasterizer == null)
            {
                _Rasterizer = new RasterInterf ();
            }

            return _Rasterizer;
        }

        public bool RastTest (int resX, int resY, int[] arrPointSizes,
                             float stretchX, float stretchY,
                             float rotation, float skew,
                             float[,] matrix,
                             bool setBW, bool setGrayscale, bool setCleartype, uint CTFlags,
                             RastTestErrorDelegate pRastTestErrorDelegate,
                             UpdateProgressDelegate pUpdateProgressDelegate,
                             int numGlyphs)
        {
            TrueType.RasterInterf.RastTestErrorDelegate m_pRastTestErrorDelegate =
                new TrueType.RasterInterf.RastTestErrorDelegate(pRastTestErrorDelegate);
            TrueType.RasterInterf.UpdateProgressDelegate m_pUpdateProgressDelegate =
                new TrueType.RasterInterf.UpdateProgressDelegate(pUpdateProgressDelegate);
            bool ms = m_Rasterizer.RastTest(resX, resY, arrPointSizes,
                                         stretchX, stretchY,
                                         rotation, skew,
                                         matrix,
                                         m_pRastTestErrorDelegate,
                                         m_pUpdateProgressDelegate
                                         );
            return ms;
        }

        public DevMetricsData CalcDevMetrics (int Huge_calcHDMX, int Huge_calcLTSH, int Huge_calcVDMX,
                                              ushort numGlyphs,
                                              byte[] phdmxPointSizes, ushort maxHdmxPointSize,
                                              byte uchPixelHeightRangeStart, byte uchPixelHeightRangeEnd,
                                              ushort[] pVDMXxResolution, ushort[] pVDMXyResolution,
                                              ushort cVDMXResolutions, UpdateProgressDelegate pUpdateProgressDelegate)
        {
            if ( Huge_calcHDMX == 0 && Huge_calcLTSH == 0 && Huge_calcVDMX == 0 )
                return null;

            TrueType.RasterInterf.UpdateProgressDelegate m_pUpdateProgressDelegate =
                new TrueType.RasterInterf.UpdateProgressDelegate(pUpdateProgressDelegate);
            m_DevMetricsData = (DevMetricsData) m_Rasterizer.CalcDevMetrics(Huge_calcHDMX, Huge_calcLTSH, Huge_calcVDMX,
                                                                            numGlyphs,
                                                                            phdmxPointSizes, maxHdmxPointSize,
                                                                            uchPixelHeightRangeStart, uchPixelHeightRangeEnd,
                                                                            pVDMXxResolution, pVDMXyResolution,
                                                                            cVDMXResolutions, m_pUpdateProgressDelegate
                                                                            );
            return m_DevMetricsData;
        }

        public ushort RasterNewSfnt (FileStream fontFileStream, uint faceIndex)
        {
            m_UserCancelledTest = false;

                m_Rasterizer.RasterNewSfnt (fontFileStream, faceIndex);

            return 1; //Not used by caller
        }

        public void CancelRastTest ()
        {
            m_Rasterizer.CancelRastTest ();
        }

        public void CancelCalcDevMetrics ()
        {
            m_UserCancelledTest = true;
            m_Rasterizer.CancelCalcDevMetrics ();
        }

        public int GetRastErrorCount ()
        {
            int result = 0;
                result += m_Rasterizer.GetRastErrorCount ();
            return result;
        }

        public class DevMetricsData : TrueType.RasterInterf.DevMetricsData
        {
            //public HDMX hdmxData;
            //public LTSH ltshData;
            //public VDMX vdmxData;
        }

        // These structures largely have their OTSPEC meanings,
        // except there is no need to store array lengths separately
        // as .NET arrays know their own lengths.

        public class HDMX
        {
            public HDMX_DeviceRecord[] Records;
        }

        public class HDMX_DeviceRecord
        {
            public byte[] Widths;
        }

        public class LTSH
        {
            public byte[] yPels; // byte[numGlyphs] 1 default
        }

        public class VDMX
        {
            public VDMX_Group[] groups;
        }

        public class VDMX_Group
        {
            public VDMX_Group_vTable[] entry;
        }

        public class VDMX_Group_vTable
        {
            public ushort yPelHeight;
            public short yMax;
            public short yMin;
        }
#endif
    }
}
