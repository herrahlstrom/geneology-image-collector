using System.Linq;

namespace GenPhoto.Parser.Test;

[TestClass]
public class MetaParserTest
{
    [TestMethod]
    [DataRow("Källor\\Haga (O)\\C 7 (1905-1909) - år 1906 - sida 96 - A0024429_00102.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Year, ImageMetaKey.Page, ImageMetaKey.Reference)]
    [DataRow("Källor\\Fristad (P)\\A I 3 (1797-1811) - sida 36.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Statistiska-Centralbyrån-SCB--1940-års-folkräkning-H1AA-404-1940-Bild-1730-Sida-30.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Year, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Bergum-O,-P-AI-1-1820-1821-Bild-19-Sida-31.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Eda-S-AIIa-4-1906-1914-Bild-331-Sida-745.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Fristad-P-C-4-1785-1824-Bild-42-Sida-75.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Göteborgs-Amiralitetsvarvsförsamling-O-AI-3-1805-1827-Bild-31-Sida-53.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\AD\\Göteborgs-rådhusrätt-och-magistrat.-Första-avdelningen-efter-1901.-O-EIIIa-64-1918-Bild-950-Sida-592.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\Fässberg (O)\\A II a 19 (1910-1916) - sida 16 - 00082603_00022.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Page, ImageMetaKey.Reference)]
    [DataRow("Källor\\AD\\Vendel-C-AII-10-1921-1926-Bild-3080-Sida-628.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    [DataRow("Källor\\Gunnarskog (S)\\C 12 (1874-1904).jpg", ImageMetaKey.Repository, ImageMetaKey.Volume)]
    [DataRow("Källor\\Tierp (C)\\A I 15 c (1861-1865) - sida 662 - C0005211_00139.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Page, ImageMetaKey.Reference)]
    [DataRow("Källor\\AD\\Lindome-N,-O-AIIa-6-1923-1940-Bild-2250-Sida-572.jpg", ImageMetaKey.Repository, ImageMetaKey.Volume, ImageMetaKey.Image, ImageMetaKey.Page)]
    public void TestParseFromPath(string path, params ImageMetaKey[] expectedMetaKeys)
    {
        var result = MetaParser.GetMetaFromPath(path);
        var expectedResult = expectedMetaKeys.Select(x => x.ToString()).ToList();

        result.Select(x => x.Key).Should().BeEquivalentTo(expectedMetaKeys);
    }
}