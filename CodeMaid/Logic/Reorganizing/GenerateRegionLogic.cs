﻿#region CodeMaid is Copyright 2007-2014 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License version 3 as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2014 Steve Cadwallader.

using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Model.CodeItems;
using SteveCadwallader.CodeMaid.Properties;
using System.Collections.Generic;
using System.Linq;

namespace SteveCadwallader.CodeMaid.Logic.Reorganizing
{
    /// <summary>
    /// A class for encapsulating the logic of generating regions.
    /// </summary>
    internal class GenerateRegionLogic
    {
        #region Fields

        private readonly CodeMaidPackage _package;
        private readonly RegionComparerByName _regionComparerByName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="GenerateRegionLogic" /> class.
        /// </summary>
        private static GenerateRegionLogic _instance;

        /// <summary>
        /// Gets an instance of the <see cref="GenerateRegionLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="GenerateRegionLogic" /> class.</returns>
        internal static GenerateRegionLogic GetInstance(CodeMaidPackage package)
        {
            return _instance ?? (_instance = new GenerateRegionLogic(package));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateRegionLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private GenerateRegionLogic(CodeMaidPackage package)
        {
            _package = package;
            _regionComparerByName = new RegionComparerByName();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the enumerable set of regions to be removed based on the specified code items.
        /// </summary>
        /// <param name="codeItems">The code items.</param>
        /// <returns>An enumerable set of regions to be removed.</returns>
        public IEnumerable<CodeItemRegion> GetRegionsToRemove(IEnumerable<BaseCodeItem> codeItems)
        {
            var regionsToKeep = ComposeRegionList(codeItems);

            var existingRegions = codeItems.OfType<CodeItemRegion>();
            var regionsToRemove = existingRegions.Except(regionsToKeep, _regionComparerByName);

            return regionsToRemove;
        }

        public void InsertRegions(SetCodeItems codeItems)
        {
            var regionsToExist = ComposeRegionList(codeItems);

            var existingRegions = codeItems.OfType<CodeItemRegion>();
            var regionsToInsert = regionsToExist.Except(existingRegions);

            foreach (var region in regionsToInsert)
            {
                //TODO: Create the region.
            }
        }

        private IEnumerable<CodeItemRegion> ComposeRegionList(IEnumerable<BaseCodeItem> codeItems)
        {
            var regions = new List<CodeItemRegion>();

            if (Settings.Default.Reorganizing_RegionsInsertEvenIfEmpty)
            {
                regions.AddRange(GeneratePossibleRegionList());
            }

            return regions;
        }

        private IEnumerable<CodeItemRegion> GeneratePossibleRegionList()
        {
            var regions = new List<CodeItemRegion>();

            var types = MemberTypeSettingHelper.AllSettings.GroupBy(x => x.Order).Select(y => new List<MemberTypeSetting>(y)).OrderBy(z => z[0].Order);
            foreach (var type in types)
            {
                if (Settings.Default.Reorganizing_RegionsIncludeAccessLevel)
                {
                    foreach (var accessModifier in GetAccessModifiers())
                    {
                        regions.Add(new CodeItemRegion { Name = accessModifier + " " + type[0].EffectiveName });
                    }
                }
                else
                {
                    regions.Add(new CodeItemRegion { Name = type[0].EffectiveName });
                }
            }

            return regions;
        }

        private IEnumerable<string> GetAccessModifiers()
        {
            return new[] { "Public", "Internal", "Protected Internal", "Protected", "Private" };
        }

        #endregion Methods
    }
}