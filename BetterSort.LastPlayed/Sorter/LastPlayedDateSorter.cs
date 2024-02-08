using BetterSort.Common.External;
using BetterSort.Common.Models;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSort.LastPlayed.Sorter {
  public record class LevelPlayData(DateTime Time, PlayedMap? Map);

  public class LastPlayedDateSorter(IClock clock, SiraLog logger) : ISortFilter {

    /// <summary>
    /// Level id to play data.
    /// </summary>
    public Dictionary<string, LevelPlayData> LastPlays = [];

    private readonly IClock _clock = clock;
    private readonly SiraLog _logger = logger;
    private bool _isSelected = false;
    private IEnumerable<ILevelPreview>? _triggeredLevels;

    public event Action<ISortFilterResult?> OnResultChanged = delegate { };

    public string Name => "Last played";

    public void NotifyChange(IEnumerable<ILevelPreview>? newLevels, bool isSelected = false) {
      _isSelected = isSelected;
      _logger.Debug($"NotifyChange called: newLevels.Count: {newLevels.Count()}, isSelected: {isSelected}");

      if (newLevels == null) {
        return;
      }

      _triggeredLevels = newLevels;
      Sort();
    }

    private void Sort() {
      if (!_isSelected) {
        return;
      }

      if (LastPlays == null) {
        throw new InvalidOperationException($"Precondition: {nameof(LastPlays)} should not be null.");
      }

      var ordered = _triggeredLevels
        .OrderByDescending(x => LastPlays.TryGetValue(x.LevelId, out var data) ? data.Time : new DateTime(0))
        .ToList();
      var legend = DateLegendMaker.GetLegend(ordered, _clock.Now, LastPlays);
      OnResultChanged(new SortFilterResult(ordered, legend));
      _logger.Info($"Sort finished, ordered[0].Name: {(ordered.Count == 0 ? null : ordered[0].SongName)}");
    }
  }
}
