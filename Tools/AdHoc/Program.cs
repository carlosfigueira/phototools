using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdHoc
{
    class Program
    {
        static void Main(string[] args)
        {
            var images = new[] { "capa.png", "dupla 1.png", "dupla 2.png" };
            foreach (var image in images)
            {
                Console.WriteLine(image);
                var imgFile = Path.Combine(@"C:\temp\deleteme\ttt", image);
                var bitmap = (Bitmap)Bitmap.FromFile(imgFile);
                foreach (var propertyItem in bitmap.PropertyItems)
                {
                    Console.Write("  {0}: ", GetPropertyName(propertyItem));
                    switch (propertyItem.Type)
                    {
                        case 1:
                            Console.Write("<byte array size {0}>", propertyItem.Len);
                            if (propertyItem.Len < 10)
                            {
                                Console.Write(" ({0})", string.Join(",", propertyItem.Value.Take(propertyItem.Len).Select(b => string.Format("{0:X2}", (int)b))));
                            }

                            break;
                        case 2:
                            Console.Write("<string> {0}", Encoding.ASCII.GetString(propertyItem.Value, 0, propertyItem.Len - 1));
                            break;
                        case 4:
                            Console.Write("<uint> {0}", GetUIntValue(propertyItem, 0));
                            break;
                        case 5:
                            Console.Write("<uint pairs>: ");
                            List<string> temp = new List<string>();
                            for (int i = 0; i < propertyItem.Len; i += 8)
                            {
                                temp.Add(string.Format("{0}/{1}", GetUIntValue(propertyItem, i), GetUIntValue(propertyItem, i + 4)));
                            }
                            Console.Write(string.Join(", ", temp));
                            break;
                        default:
                            Console.Write("<type {0}, len {1}>", propertyItem.Type, propertyItem.Len);
                            break;
                    }
                    Console.WriteLine();
                }
                bitmap.Dispose();
            }
        }

        static uint GetUIntValue(PropertyItem item, int index)
        {
            uint result = item.Value[index + 3];
            result = result << 8;
            result = result | item.Value[index + 2];
            result = result << 8;
            result = result | item.Value[index + 1];
            result = result << 8;
            result = result | item.Value[index + 0];
            return result;
        }

        static string GetPropertyName(PropertyItem item)
        {
            switch (item.Id)
            {
                case 0x0000:
                    return "PropertyTagGpsVer";
                case 0x0001:
                    return "PropertyTagGpsLatitudeRef";
                case 0x0002:
                    return "PropertyTagGpsLatitude";
                case 0x0003:
                    return "PropertyTagGpsLongitudeRef";
                case 0x0004:
                    return "PropertyTagGpsLongitude";
                case 0x0005:
                    return "PropertyTagGpsAltitudeRef";
                case 0x0006:
                    return "PropertyTagGpsAltitude";
                case 0x0007:
                    return "PropertyTagGpsGpsTime";
                case 0x0008:
                    return "PropertyTagGpsGpsSatellites";
                case 0x0009:
                    return "PropertyTagGpsGpsStatus";
                case 0x000A:
                    return "PropertyTagGpsGpsMeasureMode";
                case 0x000B:
                    return "PropertyTagGpsGpsDop";
                case 0x000C:
                    return "PropertyTagGpsSpeedRef";
                case 0x000D:
                    return "PropertyTagGpsSpeed";
                case 0x000E:
                    return "PropertyTagGpsTrackRef";
                case 0x000F:
                    return "PropertyTagGpsTrack";
                case 0x0010:
                    return "PropertyTagGpsImgDirRef";
                case 0x0011:
                    return "PropertyTagGpsImgDir";
                case 0x0012:
                    return "PropertyTagGpsMapDatum";
                case 0x0013:
                    return "PropertyTagGpsDestLatRef";
                case 0x0014:
                    return "PropertyTagGpsDestLat";
                case 0x0015:
                    return "PropertyTagGpsDestLongRef";
                case 0x0016:
                    return "PropertyTagGpsDestLong";
                case 0x0017:
                    return "PropertyTagGpsDestBearRef";
                case 0x0018:
                    return "PropertyTagGpsDestBear";
                case 0x0019:
                    return "PropertyTagGpsDestDistRef";
                case 0x001A:
                    return "PropertyTagGpsDestDist";
                case 0x00FE:
                    return "PropertyTagNewSubfileType";
                case 0x00FF:
                    return "PropertyTagSubfileType";
                case 0x0100:
                    return "PropertyTagImageWidth";
                case 0x0101:
                    return "PropertyTagImageHeight";
                case 0x0102:
                    return "PropertyTagBitsPerSample";
                case 0x0103:
                    return "PropertyTagCompression";
                case 0x0106:
                    return "PropertyTagPhotometricInterp";
                case 0x0107:
                    return "PropertyTagThreshHolding";
                case 0x0108:
                    return "PropertyTagCellWidth";
                case 0x0109:
                    return "PropertyTagCellHeight";
                case 0x010A:
                    return "PropertyTagFillOrder";
                case 0x010D:
                    return "PropertyTagDocumentName";
                case 0x010E:
                    return "PropertyTagImageDescription";
                case 0x010F:
                    return "PropertyTagEquipMake";
                case 0x0110:
                    return "PropertyTagEquipModel";
                case 0x0111:
                    return "PropertyTagStripOffsets";
                case 0x0112:
                    return "PropertyTagOrientation";
                case 0x0115:
                    return "PropertyTagSamplesPerPixel";
                case 0x0116:
                    return "PropertyTagRowsPerStrip";
                case 0x0117:
                    return "PropertyTagStripBytesCount";
                case 0x0118:
                    return "PropertyTagMinSampleValue";
                case 0x0119:
                    return "PropertyTagMaxSampleValue";
                case 0x011A:
                    return "PropertyTagXResolution";
                case 0x011B:
                    return "PropertyTagYResolution";
                case 0x011C:
                    return "PropertyTagPlanarConfig";
                case 0x011D:
                    return "PropertyTagPageName";
                case 0x011E:
                    return "PropertyTagXPosition";
                case 0x011F:
                    return "PropertyTagYPosition";
                case 0x0120:
                    return "PropertyTagFreeOffset";
                case 0x0121:
                    return "PropertyTagFreeByteCounts";
                case 0x0122:
                    return "PropertyTagGrayResponseUnit";
                case 0x0123:
                    return "PropertyTagGrayResponseCurve";
                case 0x0124:
                    return "PropertyTagT4Option";
                case 0x0125:
                    return "PropertyTagT6Option";
                case 0x0128:
                    return "PropertyTagResolutionUnit";
                case 0x0129:
                    return "PropertyTagPageNumber";
                case 0x012D:
                    return "PropertyTagTransferFunction";
                case 0x0131:
                    return "PropertyTagSoftwareUsed";
                case 0x0132:
                    return "PropertyTagDateTime";
                case 0x013B:
                    return "PropertyTagArtist";
                case 0x013C:
                    return "PropertyTagHostComputer";
                case 0x013D:
                    return "PropertyTagPredictor";
                case 0x013E:
                    return "PropertyTagWhitePoint";
                case 0x013F:
                    return "PropertyTagPrimaryChromaticities";
                case 0x0140:
                    return "PropertyTagColorMap";
                case 0x0141:
                    return "PropertyTagHalftoneHints";
                case 0x0142:
                    return "PropertyTagTileWidth";
                case 0x0143:
                    return "PropertyTagTileLength";
                case 0x0144:
                    return "PropertyTagTileOffset";
                case 0x0145:
                    return "PropertyTagTileByteCounts";
                case 0x014C:
                    return "PropertyTagInkSet";
                case 0x014D:
                    return "PropertyTagInkNames";
                case 0x014E:
                    return "PropertyTagNumberOfInks";
                case 0x0150:
                    return "PropertyTagDotRange";
                case 0x0151:
                    return "PropertyTagTargetPrinter";
                case 0x0152:
                    return "PropertyTagExtraSamples";
                case 0x0153:
                    return "PropertyTagSampleFormat";
                case 0x0154:
                    return "PropertyTagSMinSampleValue";
                case 0x0155:
                    return "PropertyTagSMaxSampleValue";
                case 0x0156:
                    return "PropertyTagTransferRange";
                case 0x0200:
                    return "PropertyTagJPEGProc";
                case 0x0201:
                    return "PropertyTagJPEGInterFormat";
                case 0x0202:
                    return "PropertyTagJPEGInterLength";
                case 0x0203:
                    return "PropertyTagJPEGRestartInterval";
                case 0x0205:
                    return "PropertyTagJPEGLosslessPredictors";
                case 0x0206:
                    return "PropertyTagJPEGPointTransforms";
                case 0x0207:
                    return "PropertyTagJPEGQTables";
                case 0x0208:
                    return "PropertyTagJPEGDCTables";
                case 0x0209:
                    return "PropertyTagJPEGACTables";
                case 0x0211:
                    return "PropertyTagYCbCrCoefficients";
                case 0x0212:
                    return "PropertyTagYCbCrSubsampling";
                case 0x0213:
                    return "PropertyTagYCbCrPositioning";
                case 0x0214:
                    return "PropertyTagREFBlackWhite";
                case 0x0301:
                    return "PropertyTagGamma";
                case 0x0302:
                    return "PropertyTagICCProfileDescriptor";
                case 0x0303:
                    return "PropertyTagSRGBRenderingIntent";
                case 0x0320:
                    return "PropertyTagImageTitle";
                case 0x5001:
                    return "PropertyTagResolutionXUnit";
                case 0x5002:
                    return "PropertyTagResolutionYUnit";
                case 0x5003:
                    return "PropertyTagResolutionXLengthUnit";
                case 0x5004:
                    return "PropertyTagResolutionYLengthUnit";
                case 0x5005:
                    return "PropertyTagPrintFlags";
                case 0x5006:
                    return "PropertyTagPrintFlagsVersion";
                case 0x5007:
                    return "PropertyTagPrintFlagsCrop";
                case 0x5008:
                    return "PropertyTagPrintFlagsBleedWidth";
                case 0x5009:
                    return "PropertyTagPrintFlagsBleedWidthScale";
                case 0x500A:
                    return "PropertyTagHalftoneLPI";
                case 0x500B:
                    return "PropertyTagHalftoneLPIUnit";
                case 0x500C:
                    return "PropertyTagHalftoneDegree";
                case 0x500D:
                    return "PropertyTagHalftoneShape";
                case 0x500E:
                    return "PropertyTagHalftoneMisc";
                case 0x500F:
                    return "PropertyTagHalftoneScreen";
                case 0x5010:
                    return "PropertyTagJPEGQuality";
                case 0x5011:
                    return "PropertyTagGridSize";
                case 0x5012:
                    return "PropertyTagThumbnailFormat";
                case 0x5013:
                    return "PropertyTagThumbnailWidth";
                case 0x5014:
                    return "PropertyTagThumbnailHeight";
                case 0x5015:
                    return "PropertyTagThumbnailColorDepth";
                case 0x5016:
                    return "PropertyTagThumbnailPlanes";
                case 0x5017:
                    return "PropertyTagThumbnailRawBytes";
                case 0x5018:
                    return "PropertyTagThumbnailSize";
                case 0x5019:
                    return "PropertyTagThumbnailCompressedSize";
                case 0x501A:
                    return "PropertyTagColorTransferFunction";
                case 0x501B:
                    return "PropertyTagThumbnailData";
                case 0x5020:
                    return "PropertyTagThumbnailImageWidth";
                case 0x5021:
                    return "PropertyTagThumbnailImageHeight";
                case 0x5022:
                    return "PropertyTagThumbnailBitsPerSample";
                case 0x5023:
                    return "PropertyTagThumbnailCompression";
                case 0x5024:
                    return "PropertyTagThumbnailPhotometricInterp";
                case 0x5025:
                    return "PropertyTagThumbnailImageDescription";
                case 0x5026:
                    return "PropertyTagThumbnailEquipMake";
                case 0x5027:
                    return "PropertyTagThumbnailEquipModel";
                case 0x5028:
                    return "PropertyTagThumbnailStripOffsets";
                case 0x5029:
                    return "PropertyTagThumbnailOrientation";
                case 0x502A:
                    return "PropertyTagThumbnailSamplesPerPixel";
                case 0x502B:
                    return "PropertyTagThumbnailRowsPerStrip";
                case 0x502C:
                    return "PropertyTagThumbnailStripBytesCount";
                case 0x502D:
                    return "PropertyTagThumbnailResolutionX";
                case 0x502E:
                    return "PropertyTagThumbnailResolutionY";
                case 0x502F:
                    return "PropertyTagThumbnailPlanarConfig";
                case 0x5030:
                    return "PropertyTagThumbnailResolutionUnit";
                case 0x5031:
                    return "PropertyTagThumbnailTransferFunction";
                case 0x5032:
                    return "PropertyTagThumbnailSoftwareUsed";
                case 0x5033:
                    return "PropertyTagThumbnailDateTime";
                case 0x5034:
                    return "PropertyTagThumbnailArtist";
                case 0x5035:
                    return "PropertyTagThumbnailWhitePoint";
                case 0x5036:
                    return "PropertyTagThumbnailPrimaryChromaticities";
                case 0x5037:
                    return "PropertyTagThumbnailYCbCrCoefficients";
                case 0x5038:
                    return "PropertyTagThumbnailYCbCrSubsampling";
                case 0x5039:
                    return "PropertyTagThumbnailYCbCrPositioning";
                case 0x503A:
                    return "PropertyTagThumbnailRefBlackWhite";
                case 0x503B:
                    return "PropertyTagThumbnailCopyRight";
                case 0x5090:
                    return "PropertyTagLuminanceTable";
                case 0x5091:
                    return "PropertyTagChrominanceTable";
                case 0x5100:
                    return "PropertyTagFrameDelay";
                case 0x5101:
                    return "PropertyTagLoopCount";
                case 0x5102:
                    return "PropertyTagGlobalPalette";
                case 0x5103:
                    return "PropertyTagIndexBackground";
                case 0x5104:
                    return "PropertyTagIndexTransparent";
                case 0x5110:
                    return "PropertyTagPixelUnit";
                case 0x5111:
                    return "PropertyTagPixelPerUnitX";
                case 0x5112:
                    return "PropertyTagPixelPerUnitY";
                case 0x5113:
                    return "PropertyTagPaletteHistogram";
                case 0x8298:
                    return "PropertyTagCopyright";
                case 0x829A:
                    return "PropertyTagExifExposureTime";
                case 0x829D:
                    return "PropertyTagExifFNumber";
                case 0x8769:
                    return "PropertyTagExifIFD";
                case 0x8773:
                    return "PropertyTagICCProfile";
                case 0x8822:
                    return "PropertyTagExifExposureProg";
                case 0x8824:
                    return "PropertyTagExifSpectralSense";
                case 0x8825:
                    return "PropertyTagGpsIFD";
                case 0x8827:
                    return "PropertyTagExifISOSpeed";
                case 0x8828:
                    return "PropertyTagExifOECF";
                case 0x9000:
                    return "PropertyTagExifVer";
                case 0x9003:
                    return "PropertyTagExifDTOrig";
                case 0x9004:
                    return "PropertyTagExifDTDigitized";
                case 0x9101:
                    return "PropertyTagExifCompConfig";
                case 0x9102:
                    return "PropertyTagExifCompBPP";
                case 0x9201:
                    return "PropertyTagExifShutterSpeed";
                case 0x9202:
                    return "PropertyTagExifAperture";
                case 0x9203:
                    return "PropertyTagExifBrightness";
                case 0x9204:
                    return "PropertyTagExifExposureBias";
                case 0x9205:
                    return "PropertyTagExifMaxAperture";
                case 0x9206:
                    return "PropertyTagExifSubjectDist";
                case 0x9207:
                    return "PropertyTagExifMeteringMode";
                case 0x9208:
                    return "PropertyTagExifLightSource";
                case 0x9209:
                    return "PropertyTagExifFlash";
                case 0x920A:
                    return "PropertyTagExifFocalLength";
                case 0x927C:
                    return "PropertyTagExifMakerNote";
                case 0x9286:
                    return "PropertyTagExifUserComment";
                case 0x9290:
                    return "PropertyTagExifDTSubsec";
                case 0x9291:
                    return "PropertyTagExifDTOrigSS";
                case 0x9292:
                    return "PropertyTagExifDTDigSS";
                case 0xA000:
                    return "PropertyTagExifFPXVer";
                case 0xA001:
                    return "PropertyTagExifColorSpace";
                case 0xA002:
                    return "PropertyTagExifPixXDim";
                case 0xA003:
                    return "PropertyTagExifPixYDim";
                case 0xA004:
                    return "PropertyTagExifRelatedWav";
                case 0xA005:
                    return "PropertyTagExifInterop";
                case 0xA20B:
                    return "PropertyTagExifFlashEnergy";
                case 0xA20C:
                    return "PropertyTagExifSpatialFR";
                case 0xA20E:
                    return "PropertyTagExifFocalXRes";
                case 0xA20F:
                    return "PropertyTagExifFocalYRes";
                case 0xA210:
                    return "PropertyTagExifFocalResUnit";
                case 0xA214:
                    return "PropertyTagExifSubjectLoc";
                case 0xA215:
                    return "PropertyTagExifExposureIndex";
                case 0xA217:
                    return "PropertyTagExifSensingMethod";
                case 0xA300:
                    return "PropertyTagExifFileSource";
                case 0xA301:
                    return "PropertyTagExifSceneType";
                case 0xA302:
                    return "PropertyTagExifCfaPattern";
                default:
                    return "Unknown (" + item.Id + ")";
            }
        }
    }
}
