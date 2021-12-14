using System.Collections.Generic;

namespace com.samwalz.unity_ui.misc
{
    public interface ISearchProvider
    {
        /// <summary>
        /// Searches for items that match a given search term
        /// </summary>
        /// <param name="searchTerm">term to search for</param>
        /// <param name="results">found items</param>
        /// <param name="maxResults">maximum number of search results, less or equal 0: unlimited</param>
        /// <returns>true, if at least 1 item found</returns>
        bool Search(string searchTerm, ref List<string> results, int maxResults = 0);
        /// <summary>
        /// Searches for first item that match a given search term
        /// </summary>
        /// <param name="searchTerm">term to search for</param>
        /// <param name="result">found item</param>
        /// <returns>true, if at least 1 item found</returns>
        bool SearchFirst(string searchTerm, out string result);
    }
}