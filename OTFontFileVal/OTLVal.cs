using System;
using System.Diagnostics;

using OTFontFile;
using OTFontFileVal;

namespace OTFontFile.OTL
{
    interface I_OTLValidate
    {
        bool Validate(Validator v, string sIdentity, OTTable table);
    }

    public class ScriptListTable_val : ScriptListTable, I_OTLValidate
    {
        public ScriptListTable_val(ushort offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;
            bool bScriptListOk = true;

            // check that ScriptRecord array doesn't extend past end of table

            if (m_offsetScriptListTable + (uint)FieldOffsets.ScriptRecords + 6*ScriptCount > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_ScriptListTable_E_ScriptRecordArray_pastEOT, table.m_tag, sIdentity);
                bScriptListOk = false;
                bRet = false;
            }

            // check that ScriptRecord array is in alphabetical order
            if (ScriptCount > 1)
            {
                for (uint i=0; i<ScriptCount-1; i++)
                {
                    ScriptRecord srCurr = GetScriptRecord(i);
                    ScriptRecord srNext = GetScriptRecord(i+1);
                    if (srCurr.ScriptTag >= srNext.ScriptTag)
                    {
                        v.Error(T.T_NULL, E._OTL_ScriptListTable_E_ScriptRecordArray_order, table.m_tag, sIdentity);
                        bScriptListOk = false;
                        bRet = false;
                    }
                }
            }

            // check each ScriptRecord
            for (uint i=0; i<ScriptCount; i++)
            {
                // check the tag
                ScriptRecord sr = GetScriptRecord(i);
                if (!sr.ScriptTag.IsValid())
                {
                    v.Error(T.T_NULL, E._OTL_ScriptListTable_E_ScriptRecord_tag, table.m_tag, sIdentity + ", ScriptRecord[" + i + "]");
                    bScriptListOk = false;
                    bRet = false;
                }

                // check the offset
                if (m_offsetScriptListTable + sr.ScriptTableOffset > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_ScriptListTable_E_ScriptRecord_offset, table.m_tag, sIdentity + ", ScriptRecord[" + i + "]");
                    bScriptListOk = false;
                    bRet = false;
                }

                // validate the ScriptTable
                ScriptTable_val st = GetScriptTable_val(sr);
                bRet &= st.Validate(v, sIdentity + ", ScriptRecord[" + i + "](" + sr.ScriptTag + "), ScriptTable", table);
            }

            if (bScriptListOk)
            {
                v.Pass(T.T_NULL, P._OTL_ScriptListTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }

        public ScriptTable_val GetScriptTable_val(ScriptRecord sr)
        {
            return new ScriptTable_val((ushort)(m_offsetScriptListTable + sr.ScriptTableOffset), m_bufTable);
        }
    }

    public class ScriptTable_val : ScriptTable, I_OTLValidate
    {
        public ScriptTable_val(ushort offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }


        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            bool bScriptTableOk = true;

            // check the DefaultLangSys offset
            if (DefaultLangSysOffset != 0)
            {
                if (m_offsetScriptTable + DefaultLangSysOffset > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_ScriptTable_E_DefaultLangSysOffset, table.m_tag, sIdentity);
                    bScriptTableOk = false;
                    bRet = false;
                }
                else
                {
                    // check the DefaultLangSys table
                    LangSysTable_val lst = GetDefaultLangSysTable_val();
                    bRet &= lst.Validate(v, sIdentity + ", DefaultLangSysTable", table);
                }
            }

            // check the LansgSysRecord array length
            if (m_offsetScriptTable + (uint)FieldOffsets.LangSysRecord + LangSysCount * 6 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_ScriptTable_E_LangSysRecordArray_pastEOT, table.m_tag, sIdentity);
                bScriptTableOk = false;
                bRet = false;
            }

            // check that the LangSysRecord array is sorted alphabetically
            if (LangSysCount > 1)
            {
                for (uint i=0; i<LangSysCount-1; i++)
                {
                    LangSysRecord ThisLsr = GetLangSysRecord(i);
                    LangSysRecord NextLsr = GetLangSysRecord(i+1);
                    if (ThisLsr.LangSysTag >= NextLsr.LangSysTag)
                    {
                        v.Error(T.T_NULL, E._OTL_ScriptTable_E_LangSysRecordArray_order, table.m_tag, sIdentity);
                        bScriptTableOk = false;
                        bRet = false;
                    }
                }
            }

            // check each LangSysRecord
            for (uint i=0; i<LangSysCount; i++)
            {
                LangSysRecord lsr = GetLangSysRecord(i);
                
                // check the tag
                if (!lsr.LangSysTag.IsValid())
                {
                    v.Error(T.T_NULL, E._OTL_ScriptTable_E_LangSysRecord_tag, table.m_tag, sIdentity + ", LangSysRecord[" + i + "]");
                    bScriptTableOk = false;
                    bRet = false;
                }

                // check the offset
                if (m_offsetScriptTable + lsr.LangSysOffset > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_ScriptTable_E_LangSysRecord_offset, table.m_tag, sIdentity + ", LangSysRecord[" + i + "]");
                    bScriptTableOk = false;
                    bRet = false;
                }

                // validate the langsys table
                LangSysTable_val lst = GetLangSysTable_val(lsr);
                bRet &= lst.Validate(v, sIdentity + ", LangSysRecord[" + i + "], LangSysTable", table);
            }

            if (bScriptTableOk)
            {
                v.Pass(T.T_NULL, P._OTL_ScriptTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }

        public LangSysTable_val GetDefaultLangSysTable_val()
        {
            LangSysTable_val lst = null;
            if (DefaultLangSysOffset != 0)
            {
                lst = new LangSysTable_val((ushort)(m_offsetScriptTable + DefaultLangSysOffset), m_bufTable);;
            }
            return lst;
        }

        public LangSysTable_val GetLangSysTable_val(LangSysRecord lsr)
        {
            return new LangSysTable_val((ushort)(m_offsetScriptTable + lsr.LangSysOffset), m_bufTable);
        }
    }

    public class LangSysTable_val : LangSysTable, I_OTLValidate
    {
        public LangSysTable_val(ushort offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            if (LookupOrder != 0)
            {
                v.Error(T.T_NULL, E._OTL_LangSysTable_E_LookupOrder, table.m_tag, sIdentity);
                bRet = false;
            }

            if (m_offsetLangSysTable + (uint)FieldOffsets.FeatureIndexArray + FeatureCount*2 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_LangSysTable_E_FeatureIndexArray_pastEOT, table.m_tag, sIdentity);
                bRet = false;
            }

            if (bRet)
            {
                v.Pass(T.T_NULL, P._OTL_LangSysTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }
    }

    public class FeatureListTable_val : FeatureListTable, I_OTLValidate
    {
        public FeatureListTable_val(ushort offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            bool bFeatureListOk = true;

            // check the FeatureRecord array length
            if (m_offsetFeatureListTable + (uint)FieldOffsets.FeatureRecordArray + FeatureCount*6 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_FeatureListTable_E_FeatureRecordArray_pastEOT, table.m_tag, sIdentity);
                bFeatureListOk = false;
                bRet = false;
            }

            // check that the FeatureRecord array is sorted alphabetically
            if (FeatureCount > 1)
            {
                for (uint i=0; i<FeatureCount-1; i++)
                {
                    FeatureRecord frCurr = GetFeatureRecord(i);
                    FeatureRecord frNext = GetFeatureRecord(i+1);
                    if (frCurr.FeatureTag > frNext.FeatureTag)
                    {
                        v.Error(T.T_NULL, E._OTL_FeatureListTable_E_FeatureRecordArray_order, table.m_tag, sIdentity);
                        bFeatureListOk = false;
                        bRet = false;
                        break;
                    }
                }
            }

            // check each FeatureRecord
            for (uint i=0; i<FeatureCount; i++)
            {
                FeatureRecord fr = GetFeatureRecord(i);
                if (fr != null)
                {
                    // check tag
                    if (!fr.FeatureTag.IsValid())
                    {
                        v.Error(T.T_NULL, E._OTL_FeatureListTable_E_FeatureRecord_tag, table.m_tag, sIdentity + ", FeatureRecord[" + i + "]");
                        bFeatureListOk = false;
                        bRet = false;
                    }
                    else if (!IsKnownFeatureTag(fr.FeatureTag))
                    {
                        v.Warning(T.T_NULL, W._OTL_FeatureListTable_W_FeatureRecord_tag, table.m_tag, sIdentity + ", FeatureRecord[" + i + "], tag = '" + fr.FeatureTag + "'");
                    }

                    // check offset
                    if (m_offsetFeatureListTable + fr.FeatureTableOffset > m_bufTable.GetLength())
                    {
                        v.Error(T.T_NULL, E._OTL_FeatureListTable_E_FeatureRecord_offset, table.m_tag, sIdentity + ", FeatureRecord[" + i + "]");
                        bFeatureListOk = false;
                        bRet = false;
                    }
                    else
                    {
                        // validate the feature table
                        FeatureTable_val ft = GetFeatureTable_val(fr);
                        bRet &= ft.Validate(v, sIdentity + ", FeatureRecord[" + i + "]" + "(" + (string)fr.FeatureTag +  ")" + ", FeatureTable", table);
                    }
                }
                else
                {
                    bFeatureListOk = false;
                    bRet = false;
                }
            }


            if (bFeatureListOk)
            {
                v.Pass(T.T_NULL, P._OTL_FeatureListTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }

        public bool IsKnownFeatureTag(OTTag tag)
        {
            string [] sTags = 
            {
                "aalt", // Access All Alternates"
                "abvf", // Above-base Forms"
                "abvm", // Above-base Mark Positioning"
                "abvs", // Above-base Substitutions"
                "afrc", // Alternative Fractions"
                "akhn", // Akhands"
                "blwf", // Below-base Forms"
                "blwm", // Below-base Mark Positioning"
                "blws", // Below-base Substitutions"
                "calt", // Contextual Alternates"
                "case", // Case-Sensitive Forms"
                "ccmp", // Glyph Composition / Decomposition"
                "cfar", // Conjunct Form After Ro"
                "cjct", // Conjunct Forms"
                "clig", // Contextual Ligatures"
                "cpct", // Centered CJK Punctuation"
                "cpsp", // Capital Spacing"
                "cswh", // Contextual Swash"
                "curs", // Cursive Positioning"
                "cv01", // Character Variants"
                "cv02", // Character Variants"
                "cv03", // Character Variants"
                "cv04", // Character Variants"
                "cv05", // Character Variants"
                "cv06", // Character Variants"
                "cv07", // Character Variants"
                "cv08", // Character Variants"
                "cv09", // Character Variants"
                "cv10", // Character Variants"
                "cv11", // Character Variants"
                "cv12", // Character Variants"
                "cv13", // Character Variants"
                "cv14", // Character Variants"
                "cv15", // Character Variants"
                "cv16", // Character Variants"
                "cv17", // Character Variants"
                "cv18", // Character Variants"
                "cv19", // Character Variants"
                "cv20", // Character Variants"
                "cv21", // Character Variants"
                "cv22", // Character Variants"
                "cv23", // Character Variants"
                "cv24", // Character Variants"
                "cv25", // Character Variants"
                "cv26", // Character Variants"
                "cv27", // Character Variants"
                "cv28", // Character Variants"
                "cv29", // Character Variants"
                "cv30", // Character Variants"
                "cv31", // Character Variants"
                "cv32", // Character Variants"
                "cv33", // Character Variants"
                "cv34", // Character Variants"
                "cv35", // Character Variants"
                "cv36", // Character Variants"
                "cv37", // Character Variants"
                "cv38", // Character Variants"
                "cv39", // Character Variants"
                "cv40", // Character Variants"
                "cv41", // Character Variants"
                "cv42", // Character Variants"
                "cv43", // Character Variants"
                "cv44", // Character Variants"
                "cv45", // Character Variants"
                "cv46", // Character Variants"
                "cv47", // Character Variants"
                "cv48", // Character Variants"
                "cv49", // Character Variants"
                "cv50", // Character Variants"
                "cv51", // Character Variants"
                "cv52", // Character Variants"
                "cv53", // Character Variants"
                "cv54", // Character Variants"
                "cv55", // Character Variants"
                "cv56", // Character Variants"
                "cv57", // Character Variants"
                "cv58", // Character Variants"
                "cv59", // Character Variants"
                "cv60", // Character Variants"
                "cv61", // Character Variants"
                "cv62", // Character Variants"
                "cv63", // Character Variants"
                "cv64", // Character Variants"
                "cv65", // Character Variants"
                "cv66", // Character Variants"
                "cv67", // Character Variants"
                "cv68", // Character Variants"
                "cv69", // Character Variants"
                "cv70", // Character Variants"
                "cv71", // Character Variants"
                "cv72", // Character Variants"
                "cv73", // Character Variants"
                "cv74", // Character Variants"
                "cv75", // Character Variants"
                "cv76", // Character Variants"
                "cv77", // Character Variants"
                "cv78", // Character Variants"
                "cv79", // Character Variants"
                "cv80", // Character Variants"
                "cv81", // Character Variants"
                "cv82", // Character Variants"
                "cv83", // Character Variants"
                "cv84", // Character Variants"
                "cv85", // Character Variants"
                "cv86", // Character Variants"
                "cv87", // Character Variants"
                "cv88", // Character Variants"
                "cv89", // Character Variants"
                "cv90", // Character Variants"
                "cv91", // Character Variants"
                "cv92", // Character Variants"
                "cv93", // Character Variants"
                "cv94", // Character Variants"
                "cv95", // Character Variants"
                "cv96", // Character Variants"
                "cv97", // Character Variants"
                "cv98", // Character Variants"
                "cv99", // Character Variants"
                "c2pc", // Petite Capitals From Capitals"
                "c2sc", // Small Capitals From Capitals"
                "dist", // Distances"
                "dlig", // Discretionary Ligatures"
                "dnom", // Denominators"
                "dtls", // Dotless Forms"
                "expt", // Expert Forms"
                "falt", // Final Glyph on Line Alternates"
                "fin2", // Terminal Forms #2"
                "fin3", // Terminal Forms #3"
                "fina", // Terminal Forms"
                "flac", // Flattened accent forms"
                "frac", // Fractions"
                "fwid", // Full Widths"
                "half", // Half Forms"
                "haln", // Halant Forms"
                "halt", // Alternate Half Widths"
                "hist", // Historical Forms"
                "hkna", // Horizontal Kana Alternates"
                "hlig", // Historical Ligatures"
                "hngl", // Hangul"
                "hojo", // Hojo Kanji Forms (JIS X 0212-1990 Kanji Forms)"
                "hwid", // Half Widths"
                "init", // Initial Forms"
                "isol", // Isolated Forms"
                "ital", // Italics"
                "jalt", // Justification Alternates"
                "jp78", // JIS78 Forms"
                "jp83", // JIS83 Forms"
                "jp90", // JIS90 Forms"
                "jp04", // JIS2004 Forms"
                "kern", // Kerning"
                "lfbd", // Left Bounds"
                "liga", // Standard Ligatures"
                "ljmo", // Leading Jamo Forms"
                "lnum", // Lining Figures"
                "locl", // Localized Forms"
                "ltra", // Left-to-right alternates"
                "ltrm", // Left-to-right mirrored forms"
                "mark", // Mark Positioning"
                "med2", // Medial Forms #2"
                "medi", // Medial Forms"
                "mgrk", // Mathematical Greek"
                "mkmk", // Mark to Mark Positioning"
                "mset", // Mark Positioning via Substitution"
                "nalt", // Alternate Annotation Forms"
                "nlck", // NLC Kanji Forms"
                "nukt", // Nukta Forms"
                "numr", // Numerators"
                "onum", // Oldstyle Figures"
                "opbd", // Optical Bounds"
                "ordn", // Ordinals"
                "ornm", // Ornaments"
                "palt", // Proportional Alternate Widths"
                "pcap", // Petite Capitals"
                "pkna", // Proportional Kana"
                "pnum", // Proportional Figures"
                "pref", // Pre-Base Forms"
                "pres", // Pre-base Substitutions"
                "pstf", // Post-base Forms"
                "psts", // Post-base Substitutions"
                "pwid", // Proportional Widths"
                "qwid", // Quarter Widths"
                "rand", // Randomize"
                "rclt", // Required Contextual Alternates"
                "rkrf", // Rakar Forms"
                "rlig", // Required Ligatures"
                "rphf", // Reph Forms"
                "rtbd", // Right Bounds"
                "rtla", // Right-to-left alternates"
                "rtlm", // Right-to-left mirrored forms"
                "ruby", // Ruby Notation Forms"
                "salt", // Stylistic Alternates"
                "sinf", // Scientific Inferiors"
                "size", // Optical size"
                "smcp", // Small Capitals"
                "smpl", // Simplified Forms"
                "ss01", // Stylistic Set 1"
                "ss02", // Stylistic Set 2"
                "ss03", // Stylistic Set 3"
                "ss04", // Stylistic Set 4"
                "ss05", // Stylistic Set 5"
                "ss06", // Stylistic Set 6"
                "ss07", // Stylistic Set 7"
                "ss08", // Stylistic Set 8"
                "ss09", // Stylistic Set 9"
                "ss10", // Stylistic Set 10"
                "ss11", // Stylistic Set 11"
                "ss12", // Stylistic Set 12"
                "ss13", // Stylistic Set 13"
                "ss14", // Stylistic Set 14"
                "ss15", // Stylistic Set 15"
                "ss16", // Stylistic Set 16"
                "ss17", // Stylistic Set 17"
                "ss18", // Stylistic Set 18"
                "ss19", // Stylistic Set 19"
                "ss20", // Stylistic Set 20"
                "ssty", // Math script style alternates"
                "stch", // Stretching Glyph Decomposition"
                "subs", // Subscript"
                "sups", // Superscript"
                "swsh", // Swash"
                "titl", // Titling"
                "tjmo", // Trailing Jamo Forms"
                "tnam", // Traditional Name Forms"
                "tnum", // Tabular Figures"
                "trad", // Traditional Forms"
                "twid", // Third Widths"
                "unic", // Unicase"
                "valt", // Alternate Vertical Metrics"
                "vatu", // Vattu Variants"
                "vert", // Vertical Writing"
                "vhal", // Alternate Vertical Half Metrics"
                "vjmo", // Vowel Jamo Forms"
                "vkna", // Vertical Kana Alternates"
                "vkrn", // Vertical Kerning"
                "vpal", // Proportional Alternate Vertical Metrics"
                "vrt2", // Vertical Alternates and Rotation"
                "zero", // Slashed Zero"
                // TODO: The 4 below are missing in the 2015 spec
                // accept them but emit warning if these are used.
                "dflt", // Default Processing"
                "dpng", // Diphthongs"
                "jajp", // Japanese Forms"
                "rtbd", // Right-to-left Alternates"
            };

            for (uint i=0; i<sTags.Length; i++)
            {
                if ((string)tag == sTags[i])
                    return true;
            }

            return false;
        }


        public FeatureTable_val GetFeatureTable_val(FeatureRecord fr)
        {
            return new FeatureTable_val((ushort)(m_offsetFeatureListTable + fr.FeatureTableOffset), m_bufTable);
        }
    }

    public class FeatureTable_val : FeatureTable, I_OTLValidate
    {
        public FeatureTable_val(ushort offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            if (sIdentity.EndsWith("(size), FeatureTable"))
            {
                // The "size" feature is special in having non-null FeatureParams

                // TODO
            }
            else
            {
                // check that FeatureParams is null
                if (FeatureParams != 0)
                {
                    v.Error(T.T_NULL, E._OTL_FeatureTable_E_FeatureParams_nonnull, table.m_tag, sIdentity);
                    bRet = false;
                }
            }

            // check LookupListIndex array length
            if (m_offsetFeatureTable + (uint)FieldOffsets.LookupListIndexArray + LookupCount * 2 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_FeatureTable_E_LookupListIndexArray_pastEOT, table.m_tag, sIdentity);
                bRet = false;
            }

            if (bRet)
            {
                v.Pass(T.T_NULL, P._OTL_FeatureTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }
    }

    public class LookupListTable_val : LookupListTable, I_OTLValidate
    {
        public LookupListTable_val(ushort offset, MBOBuffer bufTable, OTTag tag) : base(offset, bufTable, tag)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;
            bool bLookupListOk = true;

            // check that the Lookup array doesn't extend past end of table
            if (m_offsetLookupListTable + (uint)FieldOffsets.LookupArray + LookupCount * 2 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_LookupListTable_E_LookupArray_pastEOT, table.m_tag, sIdentity);
                bLookupListOk = false;
                bRet = false;
            }

            // check that each offset is within the table
            for (uint i=0; i<LookupCount; i++)
            {
                if (m_offsetLookupListTable + GetLookupOffset(i) > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_LookupListTable_E_Lookup_offset, table.m_tag, sIdentity + ", Lookup[" + i + "]");
                    bLookupListOk = false;
                    bRet = false;
                }
            }

            // validate each lookup table
            for (uint i=0; i<LookupCount; i++)
            {
                LookupTable_val lt = GetLookupTable_val(i);
                if (lt != null)
                {
                    bRet &= lt.Validate(v, sIdentity + ", Lookup[" + i + "]", table);
                }
                else
                {
                    bLookupListOk = false;
                    bRet = false;
                }
            }

            if (bLookupListOk)
            {
                v.Pass(T.T_NULL, P._OTL_LookupListTable_P_valid, table.m_tag, sIdentity);
            }

            return bRet;
        }

        public LookupTable_val GetLookupTable_val(uint i)
        {
            LookupTable_val lt = null;

            if (i < LookupCount)
            {
                ushort offset = (ushort)(m_offsetLookupListTable + GetLookupOffset(i));
                if (offset + 6 <= m_bufTable.GetLength()) // minimum lookuptable with zero entries is six bytes
                {
                    lt = new LookupTable_val(offset, m_bufTable, m_tag);
                }
            }

            return lt;
        }
    }

    public class LookupTable_val : LookupTable, I_OTLValidate
    {
        public LookupTable_val(ushort offset, MBOBuffer bufTable, OTTag tag) : base(offset, bufTable, tag)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            // check the LookupType
            if (((string)table.m_tag == "GPOS") && LookupType > 9 
                || ((string)table.m_tag == "GSUB" && LookupType > 8))
            {
                v.Error(T.T_NULL, E._OTL_LookupTable_E_LookupType, table.m_tag, sIdentity + ", LookupType = " + LookupType);
                bRet = false;
            }

            // check LookupFlag reserved bits are clear
            if ((LookupFlag & 0x00E0) != 0)
            {
                v.Error(T.T_NULL, E._OTL_LookupTable_E_LookupFlag_reserved, table.m_tag, sIdentity);
                bRet = false;
            }

            // check Subtable offset array doesn't extend past end of table
            if (m_offsetLookupTable + (uint)FieldOffsets.SubTableOffsetArray + SubTableCount*2 > m_bufTable.GetLength())
            {
                v.Error(T.T_NULL, E._OTL_LookupTable_E_SubtableArray_pastEOT, table.m_tag, sIdentity);
                bRet = false;
            }

            // check Subtable offsets don't point past end of table
            for (uint i=0; i<SubTableCount; i++)
            {
                // verify that the subtable offset is accessible, if not error was already reported
                if (m_offsetLookupTable + (uint)FieldOffsets.SubTableOffsetArray + i*2 + 2 <= m_bufTable.GetLength())
                {
                    if (m_offsetLookupTable + GetSubTableOffset(i) > m_bufTable.GetLength())
                    {
                        v.Error(T.T_NULL, E._OTL_LookupTable_E_SubtableArray_offset, table.m_tag, sIdentity + ", SubTable[" + i + "]");
                        bRet = false;
                    }
                }
            }

            // way too many lookup tables to justify this pass message
            //if (bRet)
            //{
            //    v.Pass("_OTL_LookupTable_P_valid", table.m_tag, sIdentity);
            //}


            // validate each subtable
            for (uint i=0; i<SubTableCount; i++)
            {
                // verify that the subtable offset is accessible, if not error was already reported
                if (m_offsetLookupTable + (uint)FieldOffsets.SubTableOffsetArray + i*2 + 2 <= m_bufTable.GetLength())
                {
                    // verify subtable offset is valid
                    if (m_offsetLookupTable + GetSubTableOffset(i) <= m_bufTable.GetLength())
                    {
                        SubTable st = GetSubTable(i);
                        if (st != null)
                        {
                            I_OTLValidate iv = (I_OTLValidate)st;
                            bRet &= iv.Validate(v, sIdentity + ", SubTable[" + i + "]", table);
                        }
                        else
                        {
                            v.Warning(T.T_NULL, W._TEST_W_OtherErrorsInTable, table.m_tag, "unable to validate subtable: " + sIdentity + ", SubTable[" + i + "]");
                        }
                    }
                    else
                    {
                        v.Warning(T.T_NULL, W._TEST_W_OtherErrorsInTable, table.m_tag, "unable to validate subtable: " + sIdentity + ", SubTable[" + i + "]");
                    }
                }
            }


            return bRet;
        }

        public override SubTable GetSubTable(uint i)
        {
            if (i >= SubTableCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            SubTable st = null;
            uint stOffset = m_offsetLookupTable + (uint)GetSubTableOffset(i);

            if ((string)m_tag == "GPOS")
            {
                switch (LookupType)
                {
                    case 1: st = new val_GPOS.SinglePos_val      (stOffset, m_bufTable); break;
                    case 2: st = new val_GPOS.PairPos_val        (stOffset, m_bufTable); break;
                    case 3: st = new val_GPOS.CursivePos_val     (stOffset, m_bufTable); break;
                    case 4: st = new val_GPOS.MarkBasePos_val    (stOffset, m_bufTable); break;
                    case 5: st = new val_GPOS.MarkLigPos_val     (stOffset, m_bufTable); break;
                    case 6: st = new val_GPOS.MarkMarkPos_val    (stOffset, m_bufTable); break;
                    case 7: st = new val_GPOS.ContextPos_val     (stOffset, m_bufTable); break;
                    case 8: st = new val_GPOS.ChainContextPos_val(stOffset, m_bufTable); break;
                    case 9: st = new val_GPOS.ExtensionPos_val   (stOffset, m_bufTable); break;
                }
            }
            else if ((string)m_tag == "GSUB")
            {
                switch (LookupType)
                {
                    case 1: st = new val_GSUB.SingleSubst_val      (stOffset, m_bufTable); break;
                    case 2: st = new val_GSUB.MultipleSubst_val    (stOffset, m_bufTable); break;
                    case 3: st = new val_GSUB.AlternateSubst_val   (stOffset, m_bufTable); break;
                    case 4: st = new val_GSUB.LigatureSubst_val    (stOffset, m_bufTable); break;
                    case 5: st = new val_GSUB.ContextSubst_val     (stOffset, m_bufTable); break;
                    case 6: st = new val_GSUB.ChainContextSubst_val(stOffset, m_bufTable); break;
                    case 7: st = new val_GSUB.ExtensionSubst_val   (stOffset, m_bufTable); break;
                    case 8: st = new val_GSUB.ReverseChainSubst_val(stOffset, m_bufTable); break;
                }
            }
            else
            {
                throw new InvalidOperationException("unknown table type");
            }

            return st;
        }
    }

    public class CoverageTable_val : CoverageTable, I_OTLValidate
    {
        public CoverageTable_val(uint offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            if (CoverageFormat == 1)
            {
                if (m_offsetCoverageTable + (uint)FieldOffsets1.GlyphArray + F1GlyphCount*2 > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_CoverageTable_E_GlyphArrayPastEOT, table.m_tag, sIdentity);
                    bRet = false;
                }
            }
            else if (CoverageFormat == 2)
            {
                if (m_offsetCoverageTable + (uint)FieldOffsets2.RangeRecordArray + F2RangeCount*6 > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_CoverageTable_E_RangeRecordArrayPastEOT, table.m_tag, sIdentity);
                    bRet = false;
                }
            }
            else
            {
                v.Error(T.T_NULL, E._OTL_CoverageTable_E_Format, table.m_tag, sIdentity + ", format = " + CoverageFormat.ToString());
                bRet = false;
            }

            // way too many coverage tables to justify this pass message
            //if (bRet)
            //{
            //    v.Pass("_OTL_CoverageTable_P_valid", table.m_tag, sIdentity);
            //}

            return bRet;
        }
    }

    public class ClassDefTable_val : ClassDefTable, I_OTLValidate
    {
        public ClassDefTable_val(uint offset, MBOBuffer bufTable) : base(offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            if (ClassFormat == 1)
            {
                ClassDefFormat1_val cdf1 = GetClassDefFormat1_val();
                bRet &= cdf1.Validate(v, sIdentity + "(Fmt1)", table);
            }
            else if (ClassFormat == 2)
            {
                ClassDefFormat2_val cdf2 = GetClassDefFormat2_val();
                bRet &= cdf2.Validate(v, sIdentity + "(Fmt2)", table);
            }
            else
            {
                v.Error(T.T_NULL, E._OTL_ClassDefinitionTable_E_Format, table.m_tag, sIdentity + ", format = " + ClassFormat.ToString());
                bRet = false;
            }

            // way too many ClassDefTables to justify this pass message
            //if (bRet)
            //{
            //    v.Pass("_OTL_ClassDefinitionTable_P_valid", table.m_tag, sIdentity);
            //}

            return bRet;
        }

        /************************
         * nested classes
         */

        class ClassDefFormat1_val : ClassDefFormat1, I_OTLValidate
        {
            public ClassDefFormat1_val(uint offset, MBOBuffer bufTable) : base(offset, bufTable)
            {
            }

            public bool Validate(Validator v, string sIdentity, OTTable table)
            {
                bool bRet = true;

                if (m_offsetClassDefFormat1 + (uint)FieldOffsets.ClassValueArray + GlyphCount*2 > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_ClassDefinitionTable_E_GlyphArrayPastEOT, table.m_tag, sIdentity);
                    bRet = false;
                }

                return bRet;
            }


        }

        class ClassDefFormat2_val : ClassDefFormat2, I_OTLValidate
        {
            public ClassDefFormat2_val(uint offset, MBOBuffer bufTable) : base(offset, bufTable)
            {
            }

            public bool Validate(Validator v, string sIdentity, OTTable table)
            {
                bool bRet = true;

                // check that ClassRangeRecord array doesn't extend past end of table
                if (m_offsetClassDefFormat2 + (uint)FieldOffsets.ClassRangeRecordArray + ClassRangeCount*6 > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_ClassDefinitionTable_E_RangeRecordArrayPastEOT, table.m_tag, sIdentity);
                    bRet = false;
                }

                // check that ClassRangeRecord array is in sorted order
                if (ClassRangeCount > 1)
                {
                    for (uint i=0; i<ClassRangeCount-1; i++)
                    {
                        ClassRangeRecord ThisCrr = GetClassRangeRecord(i);
                        ClassRangeRecord NextCrr = GetClassRangeRecord(i+1);
                        if (ThisCrr.Start >= NextCrr.Start)
                        {
                            v.Error(T.T_NULL, E._OTL_ClassDefinitionTable_E_RangeRecordArray_order, table.m_tag, sIdentity);
                            bRet = false;
                            
                            // temporary debug code
                            /*
                            v.DebugMsg("ClassRangeCount = " + ClassRangeCount, tag);
                            for (uint j=0; j<ClassRangeCount; j++)
                            {
                                ClassRangeRecord crr = GetClassRangeRecord(j);
                                v.DebugMsg("ClassRangeRecord[" + j + "].Start = " + crr.Start, tag);
                            }
                            */

                            break;

                        }
                    }
                }

                return bRet;
            }
        }

        ClassDefFormat1_val GetClassDefFormat1_val()
        {
            return new ClassDefFormat1_val(m_offsetClassDefTable, m_bufTable);
        }

        ClassDefFormat2_val GetClassDefFormat2_val()
        {
            return new ClassDefFormat2_val(m_offsetClassDefTable, m_bufTable);
        }
    }

    public class DeviceTable_val : DeviceTable, I_OTLValidate
    {
        public DeviceTable_val(uint offset, MBOBuffer bufTable) : base (offset, bufTable)
        {
        }

        public bool Validate(Validator v, string sIdentity, OTTable table)
        {
            bool bRet = true;

            // check StartSize for unreasonable values
            if (StartSize > 16384)
            {
                v.Warning(T.T_NULL, W._OTL_DeviceTable_W_StartSize, table.m_tag, sIdentity + ", StartSize = " + StartSize);
            }

            // check EndSize for unreasonable values
            if (EndSize > 16384)
            {
                v.Warning(T.T_NULL, W._OTL_DeviceTable_W_EndSize, table.m_tag, sIdentity + ", EndSize = " + EndSize);
            }

            // check that StartSize <= EndSize
            if (StartSize > EndSize)
            {
                v.Error(T.T_NULL, E._OTL_DeviceTable_E_sizes, table.m_tag, sIdentity);
                bRet = false;
            }

            // check DeltaFormat is 1, 2, 3, or VARIATION_INDEX
            if ( (DeltaFormat < 1 || DeltaFormat > 3) && (DeltaFormat != 0x8000) )
            {
                v.Error(T.T_NULL, E._OTL_DeviceTable_E_DeltaFormat, table.m_tag, sIdentity + ", DeltaFormat = " + DeltaFormat);
                bRet = false;
            }
            else
            {

                if (DeltaFormat >= 1 || DeltaFormat <= 3) {
                // check that DeltaValue array doesn't extend past the end of the table
                int nSizes = EndSize - StartSize + 1;
                int nValuesPerUint = 16 >> DeltaFormat;
                int nUints = nSizes / nValuesPerUint;
                if (nSizes % nValuesPerUint != 0) nUints++;
                if (m_offsetDeviceTable + (uint)FieldOffsets.DeltaValueArray + nUints > m_bufTable.GetLength())
                {
                    v.Error(T.T_NULL, E._OTL_DeviceTable_E_DeltaValueArray_pastEOT, table.m_tag, sIdentity);
                    bRet = false;
                }
                }
                // TODO: VARIATION_INDEX check
            }

            // way too many device tables to justify this pass message
            //if (bRet)
            //{
            //    v.Pass("_OTL_DeviceTable_P_valid", table.m_tag, sIdentity);
            //}

            return bRet;
        }
                
    }
}
