namespace GenPhoto.Data.Models;

public class PersonImage
{
    public Guid ImageId { get; set; }

    public Guid PersonId { get; set; }

    public static object[] GetKey(Guid imageId, Guid personId) => new object[] { imageId, personId };
}