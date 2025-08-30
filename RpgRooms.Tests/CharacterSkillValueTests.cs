using System.Collections.Generic;
using RpgRooms.Core.Domain.Entities;
using Xunit;

public class CharacterSkillValueTests
{
    public static IEnumerable<object[]> AllSkills => new[]
    {
        new object[] { "Acrobatics" },
        new object[] { "Animal Handling" },
        new object[] { "Arcana" },
        new object[] { "Athletics" },
        new object[] { "Deception" },
        new object[] { "History" },
        new object[] { "Insight" },
        new object[] { "Intimidation" },
        new object[] { "Investigation" },
        new object[] { "Medicine" },
        new object[] { "Nature" },
        new object[] { "Perception" },
        new object[] { "Performance" },
        new object[] { "Persuasion" },
        new object[] { "Religion" },
        new object[] { "Sleight of Hand" },
        new object[] { "Stealth" },
        new object[] { "Survival" },
    };

    [Theory]
    [MemberData(nameof(AllSkills))]
    public void ProficiencyAffectsSkillValue(string skill)
    {
        var character = new Character
        {
            Level = 1,
            Str = 10,
            Dex = 10,
            Con = 10,
            Int = 10,
            Wis = 10,
            Cha = 10,
            SkillProficiencies = new List<SkillProficiency>()
        };

        Assert.Equal(0, character.GetSkillValue(skill));

        character.SkillProficiencies.Add(new SkillProficiency { Name = skill });

        Assert.Equal(2, character.GetSkillValue(skill));
    }
}

