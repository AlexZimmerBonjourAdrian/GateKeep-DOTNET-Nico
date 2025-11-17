using QRCoder;
using System.IO;

namespace GateKeep.Infrastructure.QrCodes
{
  public class QrCodeGenerator
  {
    public byte[] Generate(string content, int width = 250, int height = 250)
    {
      using var qrGenerator = new QRCodeGenerator();
      // ECCLevel.L = corrección de errores baja (genera QR más simple con menos módulos)
      using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);
      using var qrCode = new PngByteQRCode(qrCodeData);
      
      // Aumentar pixelsPerModule para que los módulos sean más grandes y fáciles de escanear
      // Para un JWT largo, necesitamos módulos más grandes
      int pixelsPerModule = Math.Max(3, width / 80); // Módulos más grandes
      
      return qrCode.GetGraphic(pixelsPerModule);
    }
  }
}
