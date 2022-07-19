using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

using OTFontFile;
using NS_ValCommon;
using OTFontFileVal;

using Compat;

namespace FontValidator
{
    public class CmdLineInterface : Driver.DriverCallbacks
    {
        string []             m_sFiles;
        ReportFileDestination m_ReportFileDestination;
        bool                  m_bOpenReportFiles;
        string                m_sReportFixedDir;
        ValidatorParameters   m_vp;
        OTFileVal             m_curOTFileVal;
        List<string>          m_reportFiles = new List<string>();
        List<string>          m_captions = new List<string>();
        bool m_verbose;
        bool m_report2stdout;
        static string version = "2.1.6";

        static void ErrOut( string s ) 
        {
            Console.WriteLine( s );
        }

        static void StdOut( string s )  {
            Console.WriteLine( s );
        }

        // ================================================================
        // Callbacks for Driver.DriverCallbacks interface
        // ================================================================
        public void OnException( Exception e )
        {
            ErrOut( "Error: " + e.Message );
            DeleteTempFiles();
        }
        public void OnReportsReady()
        {
            StdOut( "Reports are ready!" );
        }
        public void OnBeginRasterTest( string label )
        {
            if (m_verbose == true && m_vp.IsTestingRaster())
                StdOut( "Begin Raster Test: " + label );
        }

        public void OnBeginTableTest( DirectoryEntry de )
        {
            string name = ( string )de.tag;
            if (m_verbose == true && m_vp.IsTestingTable(name))
                StdOut( "Table Test: " + name );
        }
        public void OnTestProgress( object oParam )
        {
            string s = ( string )oParam;
            if (s == null) {
                s = "";
            }
            if (m_verbose == true)
                StdOut( "Progress: " + s );
        }

        public void OnCloseReportFile( string sReportFile )
        {
            // Maybe check that it exists? The GUI
            // can use memory stream for temporary XML display;
            // Not applicable to CMD.
            StdOut( "Complete: " + sReportFile );
            // copy the xsl file to the same directory as the report
            // 
            // This has to be done for each file because the if we are
            // putting the report on the font's directory, there may
            // be a different directory for each font.
            Driver.CopyXslFile( sReportFile );

            if ( m_report2stdout == true && m_ReportFileDestination == ReportFileDestination.TempFiles )
            {
                // build the dest filename
                FileInfo fi = new FileInfo(sReportFile);
                string sDestDir  = fi.DirectoryName;
                string sDestFile = sDestDir + Path.DirectorySeparatorChar + "fval2txt.xsl";

                if ( !File.Exists( sDestFile ) )
                    File.WriteAllBytes(sDestFile, Compat.Xsl.fval2txt);

                var xslTrans = new XslCompiledTransform();
                xslTrans.Load(sDestFile);
                string sTXTFile = sReportFile.Replace(".report.xml", ".report.txt");
                if ( sTXTFile != sReportFile )
                    try
                    {
                        xslTrans.Transform(sReportFile, sTXTFile);
                    }
                    catch (Exception e)
                    {
                        ErrOut( "xslTrans.Transform failure: " + e.Message );
                    }

                using (StreamReader sr = new StreamReader(sTXTFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
        }

        public void OnOpenReportFile( string sReportFile, string fpath )
        {
            m_captions.Add( fpath );
            m_reportFiles.Add( sReportFile );
        }

        public void OnCancel()
        {
            DeleteTempFiles();
        }

        public void OnOTFileValChange( OTFileVal fontFile )
        {
            m_curOTFileVal = fontFile;
        }

        public string GetReportFileName( string sFontFile )
        {
            string sReportFile = null;
            switch ( m_ReportFileDestination )
            {
                case ReportFileDestination.UserDesktop:
                    sReportFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +
                        Path.DirectorySeparatorChar +
                        Path.GetFileName(sFontFile) + ".report.xml";
                    break;
                case ReportFileDestination.TempFiles:
                    string sTemp = Path.GetTempFileName();
                    sReportFile = sTemp + ".report.xml";
                    while ( File.Exists( sReportFile ) )
                    {
                        sTemp = Path.GetTempFileName();
                        sReportFile = sTemp + ".report.xml";
                    }
                    File.Move(sTemp, sReportFile);
                    break;
                case ReportFileDestination.FixedDir:
                    sReportFile = m_sReportFixedDir + Path.DirectorySeparatorChar +
                        Path.GetFileName(sFontFile) + ".report.xml";
                    break;
                case ReportFileDestination.SameDirAsFont:
                    sReportFile = sFontFile + ".report.xml";
                    break;
            }
            return sReportFile;
        }

        public void OnBeginFontTest( string fname, int nth, int nFonts )
        {
            string label = fname + " (file " + (nth+1) + " of " + nFonts + ")";
            StdOut( label );
        }

        public void DeleteTempFiles()
        {
            if ( m_ReportFileDestination == ReportFileDestination.TempFiles )
            {
                for ( int i = 0; i < m_reportFiles.Count; i++ ) {
                    File.Delete( m_reportFiles[i] );
                }
            }
        }

        public int DoIt( )
        {

            Validator v = new Validator();
            m_vp.SetupValidator( v );
            OTFontFileVal.Driver driver = new OTFontFileVal.Driver( this );
            return driver.RunValidation( v, m_sFiles );
        }

        public CmdLineInterface( ValidatorParameters vp, 
                                 string [] sFilenames, 
                                 ReportFileDestination rfd, 
                                 bool bOpenReportFiles, 
                                 string sReportFixedDir,
                                 bool verbose,
                                 bool report2stdout)
        {
            m_vp = vp;
            m_sFiles = sFilenames;
            m_ReportFileDestination = rfd;
            m_bOpenReportFiles = bOpenReportFiles;
            m_sReportFixedDir = sReportFixedDir;
            m_verbose = verbose;
            m_report2stdout = report2stdout;
        }

        static void Usage()
        {
            Console.WriteLine( "Usage: FontValidator [options]" );
            Console.WriteLine( "" );
            Console.WriteLine( "Usage: FontValidator script.py args1 args2 ..." );
            Console.WriteLine( "    ( Python mode when first arg ends with \".py\" )" );
            Console.WriteLine( "" );
            Console.WriteLine( "Version: {0}", version);
            Console.WriteLine( "" );

            Console.WriteLine( "Options:" );
            Console.WriteLine( "-file          <fontfile>      (multiple allowed)" );
            Console.WriteLine( "+table         <table-include> (multible allowed)" );
            Console.WriteLine( "-table         <table-skip>    (multiple allowed)" );
            Console.WriteLine( "-all-tables    (\"+all-tables\" is an alias)" );
            Console.WriteLine( "-only-tables" );
            Console.WriteLine( "-quiet" );
            Console.WriteLine( "-test-parms    <test-parms>.py" );
            //Console.WriteLine( "+raster-tests  (no-op, default)" );
            Console.WriteLine( "-no-raster-tests" );
            Console.WriteLine( "-report-dir    <reportDir>" );
            Console.WriteLine( "-report-stdout                 (=\"-stdout\", implies -quiet)" );
            Console.WriteLine( "-temporary-reports" );
            Console.WriteLine( "-report-in-font-dir" );
            Console.WriteLine( "-version" );

            Console.WriteLine( "" );
            Console.WriteLine( "Valid table names (note the space after \"CFF \" and \"cvt \"):" );

            string [] allTables = TableManager.GetKnownOTTableTypes();
            Console.Write(allTables[0]);
            for ( int k = 1; k < allTables.Length; k++ )
                Console.Write(",{0}", allTables[k]);
            Console.WriteLine( "" );
            Console.WriteLine( "" );

            Console.WriteLine( "Example:" );
            Console.WriteLine( "  FontValidator -file arial.ttf -file times.ttf -table 'OS/2' -table DSIG -report-dir ~/Desktop");
            Console.WriteLine( "  FontValidator ttx-l-example.py arial.ttf");
        }

        static int Main( string[] args )
        {
            bool err = false;
            bool verbose = true;
            bool report2stdout = false;
            string reportDir = null;
            ReportFileDestination rfd = ReportFileDestination.UserDesktop;
            List<string> sFileList = new List<string>();
            ValidatorParameters vp = new ValidatorParameters();
            
            if (args.Length == 0) {
                Usage();
                return 0;
            }

            if ( args[0].EndsWith(".py") ) {
                // Not try/catch.
                EmbeddedIronPython.RunScriptWithArgs(args);
                return 0;
            }

            vp.SetRasterTesting();
            for ( int i = 0; i < args.Length; i++ ) {
                if ( "-file" == args[i] ) {
                    i++;
                    if ( i < args.Length ) {
                        sFileList.Add( args[i] );
                    } else {
                        ErrOut( "Argument required for \"" + args[i-1] + "\"" );
                        err = true;
                    }
                }
                else if ( "+table" == args[i] ) {
                    i++;
                    if ( i < args.Length ) {
                        vp.AddTable( args[i] );
                    } else {
                        ErrOut( "Argument required for \"" + args[i-1] + "\"" );
                        err = true;
                    }
                }
                else if ( "-table" == args[i] ) {
                    i++;
                    if ( i < args.Length ) {
                        int n = vp.RemoveTableFromList( args[i] );
                        if ( 0 == n ) {
                            ErrOut( "Table \"" + args[i] + "\" not found" );
                            err = true;
                        }
                    } else {
                        ErrOut( "Argument required for \"" + args[i-1] + "\"" );
                        err = true;
                    }
                }
                else if ( "-all-tables" == args[i] || "+all-tables" == args[i] ) {
                    vp.SetAllTables();
                }
                else if ( "-only-tables" == args[i] ) {
                    vp.ClearTables();
                }
                else if ( "-quiet" == args[i] ) {
                    verbose = false;
                }
                else if ( "-stdout" == args[i] || "-report-stdout" == args[i] ) {
                    verbose = false;
                    report2stdout = true;
                    rfd = ReportFileDestination.TempFiles;
                }
                else if ( "-test-parms" == args[i] ) {
                    i++;
                    try
                    {
                        vp = (ValidatorParameters) EmbeddedIronPython.RunPythonMethod( args[i], "validation_parameters", "GetValue" );
                    }
                    catch (Exception e)
                    {
                        ErrOut( "Setting -test-parms failure: " + e.Message );
                        err = true;
                    }
                }
                else if ( "+raster-tests" == args[i] ) {
                    // default
                }
                else if ( "-no-raster-tests" == args[i] ) {
                    vp.SetNoRasterTesting();
                }
                else if ( "-report-dir" == args[i] ) {
                    i++;
                    if ( i < args.Length ) {
                        reportDir = args[i];
                        rfd = ReportFileDestination.FixedDir;
                        // Try writing to the directory to see if it works.
                        using (FileStream fs = File.Create(
                                                           Path.Combine(
                                                                        reportDir,
                                                                        Path.GetRandomFileName()
                                                                        ),
                                                           1, // bufferSize
                                                           FileOptions.DeleteOnClose)
                               )
                        { };
                        // exception should throw & abort on failure
                    } else {
                        ErrOut( "Argument required for \"" + args[i-1] + "\"" );
                        err = true;
                    }
                    
                }
                else if ( "-report-in-font-dir" == args[i] ) {
                    rfd = ReportFileDestination.SameDirAsFont;
                }
                else if ( "-temporary-reports" == args[i] ) {
                    rfd = ReportFileDestination.TempFiles;
                }
                else if ( "-version" == args[i] ) {
                    Console.WriteLine( "Version: {0}", version);
                    return 0; /* terminates success */
                }
                else {
                    ErrOut( "Unknown argument: \"" + args[i] + "\"" );
                    err = true;
                }
            }
            if ( err ) {
                Usage();
                return 1;
            }

            CmdLineInterface cmd = new 
                CmdLineInterface( vp, sFileList.ToArray(), rfd, false, 
                                  reportDir , verbose, report2stdout);
            return cmd.DoIt();
        }
    }
}
