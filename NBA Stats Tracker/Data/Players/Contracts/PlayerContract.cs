#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

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
            return string.Format("{0} year contract, {1:C} total, {2:C} per year on average", GetYears(), GetTotal(), GetAverage());
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