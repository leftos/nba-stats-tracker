#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace NBA_Stats_Tracker.Data.Players.Contracts
{
    [Serializable]
    public class PlayerContract
    {
        public PlayerContract()
        {
            Option = PlayerContractOption.None;
            ContractSalaryPerYear = new List<int>();
        }

        public List<int> ContractSalaryPerYear { get; set; }
        public PlayerContractOption Option { get; set; }

        public int GetYears()
        {
            return ContractSalaryPerYear.Count;
        }

        public int GetYearsMinusOption()
        {
            int total = GetYears();
            switch (Option)
            {
                case PlayerContractOption.None:
                    return total;
                case PlayerContractOption.Team:
                case PlayerContractOption.Player:
                    return total - 1;
                case PlayerContractOption.Team2Yr:
                    return total - 2;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetYearsDesc()
        {
            int total = ContractSalaryPerYear.Count;
            switch (Option)
            {
                case PlayerContractOption.Player:
                    return String.Format("{0}+1 years (Player Option)", GetYearsMinusOption());
                case PlayerContractOption.Team:
                    return String.Format("{0}+1 years (Team Option)", GetYearsMinusOption());
                case PlayerContractOption.Team2Yr:
                    return String.Format("{0}+2 years (Team Option)", GetYearsMinusOption());
                default:
                    if (total == 0)
                        return "Not signed";
                    else if (total == 1)
                        return String.Format("{0} year", total);
                    else
                        return String.Format("{0} years", total);
            }
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
            return string.Format("{0}, {1:C} total, {2:C} per year on average", GetYearsDesc(), GetTotal(), GetAverage());
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