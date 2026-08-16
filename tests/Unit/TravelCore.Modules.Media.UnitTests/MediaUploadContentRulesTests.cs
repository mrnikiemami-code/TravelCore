using System.Text;
using TravelCore.Modules.Media.Domain;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaUploadContentRulesTests
{
    private static readonly byte[] Png1x1 =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
        0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
        0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D,
        0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
        0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public void Allowlist_AcceptsRasterTypes(string contentType)
    {
        Assert.Equal(
            contentType,
            MediaUploadContentRules.NormalizeAndRequireAllowedContentType(contentType));
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("image/svg")]
    [InlineData("application/pdf")]
    [InlineData("image/avif")]
    public void Allowlist_RejectsSvgAndNonAllowlisted(string contentType)
    {
        Assert.Throws<ArgumentException>(() =>
            MediaUploadContentRules.NormalizeAndRequireAllowedContentType(contentType));
    }

    [Theory]
    [InlineData("logo.svg")]
    [InlineData("x.SVGZ")]
    public void FileName_RejectsSvgExtension(string fileName)
    {
        Assert.Throws<ArgumentException>(() => MediaUploadContentRules.ValidateFileName(fileName));
    }

    [Fact]
    public void FileName_AllowsPngEvenIfRenamedFromSvgNamePattern()
    {
        MediaUploadContentRules.ValidateFileName("logo.png");
    }

    [Fact]
    public void Payload_RejectsSvgXmlEvenWhenDeclaredAsPng()
    {
        var svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");
        Assert.Throws<ArgumentException>(() =>
            MediaUploadContentRules.ValidatePayload(svg, "image/png"));
    }

    [Fact]
    public void Payload_AcceptsPngMagic()
    {
        MediaUploadContentRules.ValidatePayload(Png1x1, "image/png");
    }

    [Fact]
    public void Size_RejectsOverMax()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MediaUploadContentRules.NormalizeAndRequireSize(100, maxBytes: 50));
    }

    [Fact]
    public void MarkFailed_SetsFailedStatus()
    {
        var now = NodaTime.Instant.FromUtc(2026, 8, 16, 10, 0);
        var asset = MediaAsset.Create("image/png", 10, now);
        var later = now.Plus(NodaTime.Duration.FromSeconds(5));
        asset.MarkFailed(later);
        Assert.Equal(MediaAssetStatus.Failed, asset.Status);
        Assert.Equal(later, asset.UpdatedAt);
    }
}
