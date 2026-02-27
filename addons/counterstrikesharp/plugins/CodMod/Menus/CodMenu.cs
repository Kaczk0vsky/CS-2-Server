using System;
using System.Collections.Generic;
using CounterStrikeSharp.API.Core;

namespace CodMod
{
    public class CodMenu
    {
        public string Title { get; }
        public string ModuleName => "Cod Mod";

        private List<(string Text, Action<CCSPlayerController> Action)> Options { get; } = new();

        private int _currentIndex;
        public bool IsOpen { get; private set; }

        public CodMenu(string title)
        {
            Title = title;
        }

        public void AddOption(string optionText, Action<CCSPlayerController> action)
        {
            Options.Add((optionText, action));
        }

        public void Open(CCSPlayerController player)
        {
            Open(player, 0);
        }

        public void Open(CCSPlayerController player, int selectedIndex)
        {
            IsOpen = true;
            _currentIndex = Options.Count == 0
                ? 0
                : Math.Clamp(selectedIndex, 0, Options.Count - 1);
            PrintToPlayer(player);
        }

        public void Close(CCSPlayerController player)
        {
            IsOpen = false;
        }

        /// <summary>
        /// Re-send the menu HTML to the given player. Useful for keeping the window
        /// visible while the menu remains open.
        /// </summary>
        public void Refresh(CCSPlayerController player) => PrintToPlayer(player);

        private const int PageSize = 4;

        private void PrintToPlayer(CCSPlayerController player)
        {
            int total = Options.Count;

            // Sliding window: keep selected item centred, clamp to valid range
            int pageStart = Math.Min(
                Math.Max(0, _currentIndex - PageSize / 2),
                Math.Max(0, total - PageSize));
            int pageEnd = Math.Min(pageStart + PageSize, total);

            var html = "<center>" +
                       $"<h2><font color='yellow'>{Title}</font></h2>";

            if (pageStart > 0)
                html += "<font color='gray'>▲</font><br/>";

            for (int i = pageStart; i < pageEnd; i++)
            {
                var color = i == _currentIndex ? "#c2c204" : "white";
                html += $"<font color='{color}'>{Options[i].Text}</font><br/>";
            }

            if (pageEnd < total)
                html += "<font color='gray'>▼</font><br/>";

            html += "</center>";

            player.PrintToCenterHtml(html, 90);
        }

        public void NextOption(CCSPlayerController player)
        {
            if (!IsOpen || Options.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % Options.Count;
            PrintToPlayer(player);
        }

        public void PreviousOption(CCSPlayerController player)
        {
            if (!IsOpen || Options.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + Options.Count) % Options.Count;
            PrintToPlayer(player);
        }

        public void SelectCurrent(CCSPlayerController player)
        {
            if (!IsOpen || Options.Count == 0) return;
            var action = Options[_currentIndex].Action;
            Close(player);
            action?.Invoke(player);
        }
    }
}