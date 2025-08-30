using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RpgRooms.Core.Domain.Entities;

namespace RpgRooms.Core.Application.Services;

public static class DiceRoller
{
    public record RollResult(int Total, string Detail);

    public static RollResult Roll(string expr, Character character)
    {
        if (string.IsNullOrWhiteSpace(expr))
            throw new ArgumentException("Expression cannot be empty", nameof(expr));

        var detailParts = new List<string>();
        var total = 0;
        var normalized = expr.Replace(" ", string.Empty).ToUpperInvariant();
        normalized = normalized.Replace("-", "+-");
        var tokens = normalized.Split('+', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var term = token;
            var sign = 1;
            if (term.StartsWith('-'))
            {
                sign = -1;
                term = term[1..];
            }

            int value;
            string partDetail;
            var match = Regex.Match(term, "^(\\d*)D(\\d+)$");
            if (match.Success)
            {
                var count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var sides = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                var rolls = new List<int>();
                for (int i = 0; i < count; i++)
                    rolls.Add(Random.Shared.Next(1, sides + 1));
                value = rolls.Sum();
                partDetail = $"{(sign == 1 ? string.Empty : "-")}{count}d{sides}({string.Join(',', rolls)})";
            }
            else if (IsAbility(term))
            {
                var ability = term[0] + term[1..].ToLowerInvariant();
                value = character.GetAbilityModifier(ability);
                partDetail = $"{(sign == 1 ? string.Empty : "-")}{term}({value})";
            }
            else if (term == "PB")
            {
                value = character.GetProficiencyBonus();
                partDetail = $"{(sign == 1 ? string.Empty : "-")}PB({value})";
            }
            else if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                value = number;
                partDetail = $"{(sign == 1 ? string.Empty : "-")}{number}";
            }
            else
            {
                throw new ArgumentException($"Invalid term '{term}' in expression", nameof(expr));
            }
            total += sign * value;
            detailParts.Add(partDetail);
        }
        var detail = string.Join(" + ", detailParts).Replace("+-", "-");
        return new RollResult(total, detail);
    }

    private static bool IsAbility(string token)
        => token is "STR" or "DEX" or "CON" or "INT" or "WIS" or "CHA";
}

