# Font-Validator

Font Validator is a tool for testing fonts prior to release. 
It was initially developed by Microsoft, to ensure that fonts meet Microsoft's high quality standards and perform exceptionally well on Microsoft's platform.

In 2015 the source code was published under the MIT license ([see release discussion](http://typedrawers.com/discussion/1222/microsoft-font-validator-lives))

Portions of this software are copyright © 2015-2018 The FreeType Project (www.freetype.org).  All rights reserved.

Portions of this software are copyright © 2015-2018 The SharpFont Project (https://github.com/Robmaister/SharpFont).  All rights reserved.

## Usage

`FontVal.exe` is the GUI, and `FontValidator.exe` shows usage and example if run without arguments; both should be self-explanatory. 
Prepend with `mono` if runs on non-Windows systems.

The GUI's built-in help requires a CHM viewer, which defaults to [chmsee](https://github.com/jungleji/chmsee) on GNU+Linux, or via env variable `MONO_HELP_VIEWER` 

The GUI on X11/mono needs the env variable `MONO_WINFORMS_XIM_STYLE=disabled` set to work around [Bug 28047 - Forms on separare threads -- Fatal errors/crashes](https://bugzilla.xamarin.com/show_bug.cgi?id=28047)

## Binary Downloads

Since Release 2.0, binaries (`*.dmg` for Mac OS X, `*-bin-net2.zip` or `*-bin-net4.zip` for MS .NET/mono) are available from
[Binary Downloads](https://sourceforge.net/projects/hp-pxl-jetready/files/Microsoft%20Font%20Validator/).
From Release 2.1 onwards, gpg-signed binaries for Ubuntu Linux are also available. There is an additional and simplified location for
Binary downloads at the [Releases link above this page](https://github.com/HinTak/Font-Validator/releases).

Please consider [donating to the effort](https://sourceforge.net/p/hp-pxl-jetready/donate/), if you use the binaries.

## Build Instructions

Font Validator was developed with Visual Studio C# by Microsoft; when it was released under MIT license in autumn 2015, VC# project files were not released. Instead, building with Mono's mcs C# compiler (http://www.mono-project.com/) was added.
To build with mono instead of Microsoft C\# simply run:

    make

The usable binaries are then available from the `bin` directory.

If one is making major changes (adding new tests, or new error/warning codes):

    make gendoc

(Plus a few extra manual steps.)

To delete the newly generated binaries:

    make clean

As of Feb 2016, building with Microsoft Visual C# was partially re-implemented. See https://github.com/HinTak/Font-Validator/issues/8 for current status on this.

The rasterer-dependent tests (HDMX/LTSH/VDMX) requires an enhancement which first appears in FreeType 2.6.1. 
Linux users can use `LD_LIBRARY_PATH` env to load newer library than system's.

The bundled win64 FreeType dll was built with an additional win64-specific patch, `freetype-win64.patch`.

Currently the CHM Help file requires MS Help Workshop to build, so is bundled in the bin/ directory.
`fval.xsl` is also rarely changed, so duplicated there. 

SharpFont requires xbuild from monodevelop to build. 
<https://github.com/Robmaister/SharpFont>

## Roadmap

### Missing/broken Parts

As of Release 2.0 (July 18 2016), all the withheld parts not released by Microsoft were re-implemented.
Release 2.0 run well on non-windows, and is substantially faster also.
Existing users of the increasingly dated 1.0 release from 2003 are encouraged to upgrade.
There are a number of known disagreements and issues which are gradually being filed and addressed.
[README-hybrid.txt](README-hybrid.txt) is now of historical interests only.

* The DSIG test (DSIG_VerifySignature) does not validate trusted certificate chain yet.

* Many "Required field missing" in GenerateFValData/OurData.xml

* Issues mentioned in "FDK/Technical Documentation/MSFontValidatorIssues.htm"

* Many post-2nd (i.e. 2009) edition changes, such as CBLC/CBDT and other new tables.

See [README-extra.txt](README-extra.txt) for a list of other interesting or non-essential tasks.

### Caveats

The 3 Rasterer-dependent metrics tests (LTSH/HDMX/VDMX) with a FreeType backend are known to behave somewhat differently compared to the MS Font Scaler backend. 
In particular:

HDMX: differ by up to two pixels (i.e. either side)

LTSH: FreeType backend shows a lot more non-linearity than an MS backend; the result with MS backend should be a sub-set of FreeType's, however.

VDMX: The newer code has a built-in 10% tolerance, so the newer FreeType backend result should be a sub-set of (the older) MS result. Also, note that MS 2003 binary seems to be wrong for non-square resolutions, so results differ by design.

On the other hand, the FreeType backend is up to 5x faster (assuming single-thread), and support CFF rastering. It is not known whether the MS backend is multi-threaded, but the FreeType backend is currently single-threaded.

Incomplete CFF checks:

    val_CFF.cs   I.CFF_I_NotValidated
    val_head.cs: I._TEST_I_NotForCFF head_MinMaxValues
    val_hhea.cs: I._TEST_I_NotForCFF hhea_MinMax
    val_OS2.cs:  I._TEST_I_NotForCFF OS/2_xAvgCharWidth

### Annoyances

Table order is case-insensitive sorted in GUI, but case-sensitive sorted in output, both should be sorted consistently.

GUI allows in-memory reports, so CMD does not warn nor abort when output location is invalid, and wastes time producing no output.
Only `-report-dir` aborts on that; no workaround to `-report-in-font-dir` nor temp dir yet.
