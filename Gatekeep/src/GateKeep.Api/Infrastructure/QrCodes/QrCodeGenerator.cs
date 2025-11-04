using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace GateKeep.Infrastructure.QrCodes
{
  public class QrCodeGenerator
  {
    [SupportedOSPlatform("windows")]
    public byte[] Generate(string content, int width = 250, int height = 250)
    {
      var writer = new BarcodeWriter()
      {
        Format = BarcodeFormat.QR_CODE,
        Options = new EncodingOptions
        {
          Width = width,
          Height = height,
          Margin = 1
        },
      };

      using var bitmap = writer.Write(content);
      using var stream = new MemoryStream();
      bitmap.Save(stream, ImageFormat.Png);
      return stream.ToArray();
    }
  }
}
