using SkiaSharp;
using Svg.Skia;

namespace maria.Helpers;

// يرسم نفس رسومات SVG الموجودة في elevator.component.html ويحوّلها إلى PNG بحجم 400×400
// NuGet: Svg.Skia
public static class ElevatorShapeRenderer
{
    private const string Head =
        "<svg width=\"400\" height=\"400\" viewBox=\"0 0 400 400\" xmlns=\"http://www.w3.org/2000/svg\" " +
        "xmlns:xlink=\"http://www.w3.org/1999/xlink\" font-family=\"Times New Roman\"><g transform=\"translate(200,200)\">";
    private const string Tail = "</g></svg>";

    // البئر: square / square-semicircle / circle
    public static byte[] Well(string? shapeType, decimal? width, decimal? height, decimal? radius,
                              decimal? liftWidth, decimal? rightWidth, decimal? centerWidth)
    {
        string body = shapeType switch
        {
            "square-semicircle" =>
                "<circle cx=\"-50\" cy=\"-90\" r=\"60\" fill=\"none\" stroke=\"blue\" stroke-width=\"3\"/>" +
                "<line stroke=\"red\" stroke-width=\"2\" stroke-dasharray=\"4,2\" x1=\"-110\" y1=\"-90\" x2=\"10\" y2=\"-90\"/>" +
                $"<text x=\"-50\" y=\"-170\" text-anchor=\"middle\" font-size=\"18\">D={N(radius)}</text>",
            "circle" =>
                "<circle cx=\"0\" cy=\"0\" r=\"80\" fill=\"none\" stroke=\"blue\" stroke-width=\"3\"/>" +
                $"<text x=\"0\" y=\"-100\" text-anchor=\"middle\" font-size=\"18\">D={N(radius)}</text>",
            _ =>
                $"<image x=\"-115\" y=\"-160\" width=\"170\" height=\"120\" xlink:href=\"{ElevatorShapeAssets.Square}\" preserveAspectRatio=\"xMidYMid meet\"/>" +
                $"<text x=\"-20\" y=\"-170\" text-anchor=\"middle\" font-size=\"18\" fill=\"blue\">W={N(width)}</text>" +
                $"<text x=\"85\" y=\"-114\" font-size=\"18\" fill=\"red\">D ={N(height)}</text>" +
                $"<text x=\"-80\" y=\"-70\" text-anchor=\"middle\" fill=\"green\" font-size=\"20\">{N(liftWidth)}</text>" +
                $"<text x=\"17\" y=\"-70\" text-anchor=\"middle\" fill=\"green\" font-size=\"20\">{N(rightWidth)}</text>" +
                $"<text x=\"-35\" y=\"-15\" text-anchor=\"middle\" fill=\"green\" font-size=\"20\">{N(centerWidth)}</text>",
        };
        return Render(body);
    }

    // فتحة الباب
    public static byte[] Direction(decimal? directionWidth, decimal? directionHeight) => Render(
        $"<image xlink:href=\"{ElevatorShapeAssets.Door}\" x=\"-80\" y=\"-160\" width=\"75\" height=\"100\" preserveAspectRatio=\"none\"/>" +
        "<line stroke=\"blue\" stroke-width=\"2\" x1=\"-80\" y1=\"-55\" x2=\"-5\" y2=\"-55\"/>" +
        $"<text x=\"-40\" y=\"-40\" text-anchor=\"middle\" font-size=\"18\" fill=\"blue\">W={N(directionWidth)}</text>" +
        "<line x1=\"5\" y1=\"-160\" x2=\"5\" y2=\"-60\" stroke=\"red\" stroke-width=\"2\"/>" +
        $"<text x=\"25\" y=\"-110\" text-anchor=\"middle\" font-size=\"18\" fill=\"red\">H={N(directionHeight)}</text>");

    // المساحة المتوفرة للتركيب
    public static byte[] Resizable(decimal? width, decimal? depth) => Render(
        $"<image x=\"-115\" y=\"-160\" width=\"150\" height=\"100\" xlink:href=\"{ElevatorShapeAssets.Area}\" preserveAspectRatio=\"xMidYMid meet\"/>" +
        "<line x1=\"-115\" y1=\"-40\" x2=\"35\" y2=\"-40\" stroke=\"black\" stroke-width=\"2\"/>" +
        $"<text x=\"-40\" y=\"-20\" text-anchor=\"middle\" font-size=\"18\" fill=\"blue\">W={N(width)}</text>" +
        "<line x1=\"60\" y1=\"-160\" x2=\"60\" y2=\"-60\" stroke=\"black\" stroke-width=\"2\"/>" +
        $"<text x=\"95\" y=\"-110\" text-anchor=\"middle\" font-size=\"18\" fill=\"red\">D={N(depth)}</text>");

    private static string N(decimal? v) => (v ?? 0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private static byte[] Render(string body)
    {
        using var svg = new SKSvg();
        svg.FromSvg(Head + body + Tail);

        using var bitmap = new SKBitmap(400, 400);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            if (svg.Picture != null) canvas.DrawPicture(svg.Picture);
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
