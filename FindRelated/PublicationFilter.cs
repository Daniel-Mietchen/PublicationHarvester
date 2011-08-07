﻿/*
 *                                FindRelated
 *              Copyright (c) 2003-2011 Stellman & Greene Consulting
 *      Developed for Joshua Zivin and Pierre Azoulay, Columbia University
 *            http://www.stellman-greene.com/PublicationHarvester
 *
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Software 
 * Foundation; either version 2 of the License, or (at your option) any later 
 * version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT 
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with 
 * this program (GPL.txt); if not, write to the Free Software Foundation, Inc., 51 
 * Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.StellmanGreene.PubMed;

namespace Com.StellmanGreene.FindRelated
{
    /// <summary>
    /// Filters to include/exclude publications
    /// </summary>
    class PublicationFilter
    {
        /// <summary>
        /// Only allow publications from the same journal (null or empty string disables the filter)
        /// </summary>
        public bool SameJournal { get; private set; }

        /// <summary>
        /// Lower bound for the publication window (pubdate-t2) from 0 to +10 (null value disables the filter)
        /// </summary>
        public int? PubWindowLowerBound { get; private set; }

        /// <summary>
        /// Upper bound for the publication window (pubdate+t1) from 0 to +10 (null value disables the filter)
        /// </summary>
        public int? PubWindowUpperBound { get; private set; }

        /// <summary>
        /// Only include link rankings up to this value (null value disables the filter)
        /// </summary>
        public int? MaximumLinkRanking { get; private set; }

        /// <summary>
        /// Collection of publication categories to exclude (null value disables the filter)
        /// </summary>
        public IEnumerable<int> ExcludeCategories { get; private set; }

        private readonly PublicationTypes publicationTypes;

        /// <summary>
        /// Create a publication filter to test whether publications match criteria
        /// </summary>
        /// <param name="publicationTypes">PublicationTypes object for the current database</param>
        /// <param name="sameJournal">Only allow publications from the same journal (null or empty string disables the filter)</param>
        /// <param name="pubWindowLowerBound">Lower bound for the publication window (pubdate-t2) from 0 to +10 (null value disables the filter)</param>
        /// <param name="pubWindowUpperBound">Upper bound for the publication window (pubdate+t1) from 0 to +10 (null value disables the filter)</param>
        /// <param name="maximumLinkRanking">Only include link rankings up to this value (null value disables the filter)</param>
        /// <param name="excludeCategories">Collection of publication categories to exclude (null value disables the filter)</param>
        public PublicationFilter(PublicationTypes publicationTypes, bool sameJournal, int? pubWindowLowerBound, int? pubWindowUpperBound, 
            int? maximumLinkRanking, IEnumerable<int> excludeCategories)
        {
            this.publicationTypes = publicationTypes;

            SameJournal = sameJournal;

            if (pubWindowLowerBound.HasValue && (pubWindowLowerBound < 0 || pubWindowLowerBound > 10))
                throw new ArgumentException("pubWindowLowerBound", "Publication window lower bound must be between 0 and 10 or have no value");
            PubWindowLowerBound = PubWindowLowerBound;

            if (pubWindowUpperBound.HasValue && (pubWindowUpperBound < 0 || pubWindowUpperBound > 10))
                throw new ArgumentException("pubWindowUpperBound", "Publication window lower bound must be between 0 and 10 or have no value        ");
            PubWindowUpperBound = pubWindowUpperBound;

            if (maximumLinkRanking < 0)
                throw new ArgumentException("maximumLinkRanking", "Maximum link ranking must be >= 0");
            MaximumLinkRanking = maximumLinkRanking;

            ExcludeCategories = excludeCategories;
        }

        /// <summary>
        /// Check a publication against the filter
        /// </summary>
        /// <param name="publication">Publication to check</param>
        /// <param name="linkRanking">Link ranking for the publication</param>
        /// <param name="referencePublication">Reference publication to compare against</param>
        /// <returns>True if the publication matches the filter, false otherwise</returns>
        public bool FilterPublication(Publication publication, int linkRanking, Publication referencePublication)
        {
            if (SameJournal && (publication.Journal != referencePublication.Journal))
                return false;

            if (PubWindowLowerBound.HasValue && (referencePublication.Year - PubWindowLowerBound > publication.Year))
                return false;

            if (PubWindowUpperBound.HasValue && (referencePublication.Year + PubWindowUpperBound < publication.Year))
                return false;

            if (linkRanking > MaximumLinkRanking)
                return false;

            if ((ExcludeCategories != null)
                && (!ExcludeCategories.Contains(publicationTypes.GetCategoryNumber(publication.PubType))))
                return false;

            return true;
        }

        public override string ToString()
        {
            return String.Format(
@"Filter values:
Same Journal: {0}
Publication window lower bound: {1}
Publication window upper bound: {2}
Maximum link ranking: {3}
Exclude pubtype 'bins': {4}",
                            SameJournal,
                            PubWindowLowerBound == null ? "filter not set" : PubWindowLowerBound.ToString(),
                            PubWindowUpperBound == null ? "filter not set" : PubWindowLowerBound.ToString(),
                            MaximumLinkRanking == null ? "filter not set" : PubWindowLowerBound.ToString(),
                            ((ExcludeCategories == null) || (ExcludeCategories.Count() == 0)) ? "filter not set" : String.Join(", ", ExcludeCategories));
        }
    }
}
