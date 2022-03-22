using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RSSMS.DataService.ViewModels.Floors
{
    public class FloorInSpaceViewModel : IComparable<FloorInSpaceViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Usage { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }

        public int CompareTo([AllowNull] FloorInSpaceViewModel other)
        {
            int posMinus = Name.IndexOf(" - ");
            if (posMinus == -1) return -1;
            string characterPart = Name.Substring(0, posMinus);
            string NumericPart = Name.Substring(posMinus + 3);

            int posMinus2 = other.Name.IndexOf(" - ");
            if (posMinus2 == -1) return 1;
            string characterPart2 = other.Name.Substring(0, posMinus);
            string NumericPart2 = other.Name.Substring(posMinus + 3);

            if (characterPart.CompareTo(characterPart2) == 0)
            {
                int num1 = int.Parse(NumericPart);
                int num2 = int.Parse(NumericPart2);
                return num1.CompareTo(num2);
            }
            else
            {
                return characterPart.CompareTo(characterPart2);
            }
        }
    }
}
