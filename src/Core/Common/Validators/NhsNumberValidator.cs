using System.Text.RegularExpressions;
using Core.Common.Models;
using FluentValidation;

namespace Core.Common.Validators
{
    public class NhsNumberValidator : AbstractValidator<NhsNumber>
    {
        private const int NhsNumberLength = 10;
        private const int RemainderConstant = 11;
        private static readonly int[] Weightings = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        public NhsNumberValidator()
        {
            RuleFor(x => x.Value)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("NHS Number cannot be null.")
                .NotEmpty().WithMessage("NHS Number cannot be empty.")
                .Must(IsInputValid!).WithMessage("NHS Number must be 10 digits long and contain only numbers.")
                .Must(IsValid!).WithMessage("NHS Number is not valid.");
        }

        private static bool IsValid(string nhsNumber)
        {
            var characters = ExplodeNumber(nhsNumber);
            var checkSum = ExtractChecksum(characters);
            var weightedTotal = CalculateWeightedTotal(characters);
            var remainder = CalculateRemainder(weightedTotal);
            return RemainderAndChecksumMatch(remainder, checkSum);
        }

        private static List<int> ExplodeNumber(string number)
        {
            var characters = number.Select(x => new string(x, 1)).ToList();
            return characters.Select(int.Parse).ToList();
        }

        private static int ExtractChecksum(IReadOnlyList<int> characters)
        {
            return characters[NhsNumberLength - 1];
        }

        private static int CalculateWeightedTotal(IReadOnlyCollection<int> characters)
        {
            var position = 0;
            var lastCharacter = characters.Count;

            return characters.TakeWhile(_ => position != lastCharacter - 1).Sum(character => character * Weightings[position++]);
        }

        private static int CalculateRemainder(int weightedTotal)
        {
            return weightedTotal % RemainderConstant;
        }

        private static bool RemainderAndChecksumMatch(int remainder, int checkSum)
        {
            var actual = remainder == 0 ? 0 : RemainderConstant - remainder;
            return actual == checkSum;
        }

        private static bool IsInputValid(string nhsNumber)
        {
            return nhsNumber.Length == NhsNumberLength && Regex.IsMatch(nhsNumber, "^\\d+$");
        }
    }
}