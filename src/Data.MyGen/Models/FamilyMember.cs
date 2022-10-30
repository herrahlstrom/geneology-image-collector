using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class FamilyMember
{
    public Family Family { get; set; }

    [Column("family")]
    public int FamilyId { get; set; }

    public FamilyMemberType MemberType { get; set; }

    [Column("member_type")]
    public int MemberTypeId { get; set; }

    public Person Person { get; set; }

    [Column("person")]
    public int PersonId { get; set; }
}