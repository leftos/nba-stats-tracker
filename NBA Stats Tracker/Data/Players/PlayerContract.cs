using System;
using System.Collections.Generic;
using System.Linq;

namespace NBA_Stats_Tracker.Data.Players
{
    [Serializable]
    public class PlayerContract
    {
        public List<int> ContractSalaryPerYear { get; set; }
        public PlayerContractOption Option { get; set; }

        public PlayerContract()
        {
            Option = PlayerContractOption.None;
            ContractSalaryPerYear = new List<int> {0};
        }

        public int GetYears()
        {
            return ContractSalaryPerYear.Count;
        }

        public int GetTotal()
        {
            return ContractSalaryPerYear.Sum();
        }

        public double GetAverage()
        {
            return ContractSalaryPerYear.Average();
        }

        public new string ToString()
        {
            return string.Format("{0} year contract, {1} total, {2:0,0} per year on average", GetYears(), GetTotal(), GetAverage());
        }

        public int TryGetSalary(int year)
        {
            if (ContractSalaryPerYear.Count >= year)
            {
                return ContractSalaryPerYear[year - 1];
            }
            else
            {
                return 0;
            }
        }
    }
}